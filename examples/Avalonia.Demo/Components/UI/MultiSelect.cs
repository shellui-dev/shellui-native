using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace AvaloniaDemo.Components.UI;

// Multi-select — a min-h-10 trigger that shows the picked options as removable chips, and a
// floating panel with a search field and a checked list. Picking toggles an option and leaves
// the panel open; click outside or press Escape to close it. Type to filter, Up / Down to move,
// Enter to toggle.
// Usage: <ui:MultiSelect Placeholder="Select frameworks..." Values="{Binding Frameworks}" /> then
// set ItemsSource in code or by binding.
public class MultiSelect : Border
{
    public static readonly StyledProperty<IList<string>?> ItemsSourceProperty =
        AvaloniaProperty.Register<MultiSelect, IList<string>?>(nameof(ItemsSource));

    public static readonly StyledProperty<IList<string>?> ValuesProperty =
        AvaloniaProperty.Register<MultiSelect, IList<string>?>(nameof(Values), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<MultiSelect, string?>(nameof(Placeholder), "Select...");

    public static readonly StyledProperty<string?> SearchPlaceholderProperty =
        AvaloniaProperty.Register<MultiSelect, string?>(nameof(SearchPlaceholder), "Search...");

    public static readonly StyledProperty<string?> EmptyTextProperty =
        AvaloniaProperty.Register<MultiSelect, string?>(nameof(EmptyText), "No results found.");

    private const double ItemHeight = 32;
    private const double MaxListHeight = 240;

    private readonly Border _trigger;
    private readonly WrapLayout _chips;
    private readonly TextBlock _placeholder;
    private readonly Border _panel;
    private readonly TextBox _search;
    private readonly StackPanel _list = new();
    private readonly Panel _listArea;
    private readonly TextBlock _empty;
    private readonly List<(string Text, Border Row, Icon Check)> _rows = new();
    private readonly ShellAnchoredPopup _popup;
    private int _active = -1;

    static MultiSelect()
    {
        ItemsSourceProperty.Changed.AddClassHandler<MultiSelect>((m, _) => m.RebuildItems());
        ValuesProperty.Changed.AddClassHandler<MultiSelect>((m, e) => m.OnValuesReplaced(e.OldValue, e.NewValue));
        PlaceholderProperty.Changed.AddClassHandler<MultiSelect>((m, e) => m._placeholder.Text = (string?)e.NewValue);
        SearchPlaceholderProperty.Changed.AddClassHandler<MultiSelect>((m, e) => m._search.PlaceholderText = (string?)e.NewValue);
        EmptyTextProperty.Changed.AddClassHandler<MultiSelect>((m, e) => m._empty.Text = (string?)e.NewValue);
        IsEnabledProperty.Changed.AddClassHandler<MultiSelect>((m, _) => m.Opacity = m.IsEnabled ? 1.0 : 0.5);
    }

    public IList<string>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public IList<string>? Values
    {
        get => GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string? SearchPlaceholder
    {
        get => GetValue(SearchPlaceholderProperty);
        set => SetValue(SearchPlaceholderProperty, value);
    }

    public string? EmptyText
    {
        get => GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public bool IsOpen => _popup.IsOpen;

    public event EventHandler<IReadOnlyList<string>>? ValuesChanged;

    public MultiSelect()
    {
        _placeholder = new TextBlock { Text = Placeholder, FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 0, 0) };
        _placeholder.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        _chips = new WrapLayout { Spacing = 4, LineSpacing = 4, VerticalAlignment = VerticalAlignment.Center };
        var chevrons = new Icon { Kind = IconName.ChevronsUpDown, Size = 16, Token = ShellToken.MutedForeground, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(chevrons, 1);

        _trigger = new Border
        {
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                ColumnSpacing = 8,
                Children = { new Panel { Children = { _placeholder, _chips } }, chevrons }
            },
            MinHeight = 40,
            Padding = new Thickness(8, 6, 12, 6),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        _trigger.Token(BorderBrushProperty, ShellToken.Input);
        _trigger.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left || !IsEffectivelyEnabled) return;
            SetOpen(!IsOpen);
            e.Handled = true;
        };

        _search = new TextBox { FontSize = 14, PlaceholderText = SearchPlaceholder, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center };
        ShellPlatform.StripNativeChrome(_search);
        _search.TextChanged += (_, _) => Filter();
        _search.KeyDown += OnSearchKeyDown;
        Grid.SetColumn(_search, 1);
        var searchRow = new Border
        {
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                ColumnSpacing = 8,
                Children = { new Icon { Kind = IconName.Search, Size = 16, Token = ShellToken.MutedForeground, VerticalAlignment = VerticalAlignment.Center }, _search }
            },
            Height = 40,
            Padding = new Thickness(12, 0)
        };

        var divider = new Border { Height = 1 };
        divider.Token(BackgroundProperty, ShellToken.Border);

        _empty = new TextBlock { Text = EmptyText, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsVisible = false };
        _empty.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        // Fixed height (set in RebuildItems): the panel keeps its size and position while filtering.
        _listArea = new Panel { Margin = new Thickness(4), Children = { new ScrollViewer { Content = _list }, _empty } };

        _panel = new Border
        {
            Child = new StackPanel { Children = { searchRow, divider, _listArea } },
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            BoxShadow = ShellPopups.PanelShadow()
        };
        _panel.Token(BackgroundProperty, ShellToken.Popover).Token(BorderBrushProperty, ShellToken.Border);

        _popup = new ShellAnchoredPopup(_trigger);
        _popup.Popup.Child = _panel;
        _popup.Closed += (_, _) =>
        {
            _trigger.Token(BorderBrushProperty, ShellToken.Input);
            // Focus goes back to the trigger instead of being lost with the panel.
            if (_search.IsFocused) Focus();
        };

        Child = new Panel { Children = { _trigger, _popup.Popup } };
        ShellFocus.Ring(this, _trigger);
        RebuildItems();
        RebuildChips();
    }

    private IReadOnlyList<string> Current => (IReadOnlyList<string>?)Values?.ToList() ?? Array.Empty<string>();

    public void SetOpen(bool open)
    {
        if (open == _popup.IsOpen) return;
        if (!open)
        {
            _popup.Close();
            return;
        }
        _search.Text = string.Empty;
        Filter();
        // The panel matches the trigger's width.
        _panel.Width = _trigger.Bounds.Width;
        _trigger.Token(BorderBrushProperty, ShellToken.Ring);
        _popup.Open();
        Dispatcher.UIThread.Post(() =>
        {
            if (IsOpen) _search.Focus();
        }, DispatcherPriority.Loaded);
    }

    public void Close() => SetOpen(false);

    // Adds the option when it isn't picked, removes it when it is.
    public void Toggle(string text)
    {
        // A new list each time, so a two-way binding sees the change.
        var next = new List<string>(Current);
        if (!next.Remove(text)) next.Add(text);
        Values = next;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        // Only keys pressed on the closed trigger: the search field handles its own.
        if (IsOpen || !ReferenceEquals(e.Source, this) || e.Key is not (Key.Enter or Key.Space or Key.Down)) return;
        SetOpen(true);
        e.Handled = true;
    }

    private void OnSearchKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (_active >= 0) Toggle(_rows[_active].Text);
        }
        else if (e.Key == Key.Down) MoveActive(1);
        else if (e.Key == Key.Up) MoveActive(-1);
        else return;
        e.Handled = true;
    }

    private void OnValuesReplaced(object? oldValue, object? newValue)
    {
        if (oldValue is INotifyCollectionChanged before) before.CollectionChanged -= OnValuesMutated;
        if (newValue is INotifyCollectionChanged after) after.CollectionChanged += OnValuesMutated;
        OnValuesMutated(this, null);
    }

    private void OnValuesMutated(object? sender, NotifyCollectionChangedEventArgs? e)
    {
        RebuildChips();
        UpdateChecks();
        ValuesChanged?.Invoke(this, Current);
    }

    private void RebuildChips()
    {
        _chips.Children.Clear();
        var current = Current;
        foreach (var value in current)
            _chips.Children.Add(CreateChip(value));
        _placeholder.IsVisible = current.Count == 0;
    }

    private Control CreateChip(string value)
    {
        var label = new TextBlock { Text = value, FontSize = 12, FontWeight = FontWeight.Medium, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        label.Token(TextBlock.ForegroundProperty, ShellToken.SecondaryForeground);
        var close = new Icon { Kind = IconName.X, Size = 12, Token = ShellToken.MutedForeground, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        // 12px icon in a 20px click target. Its clicks stop here, so they don't toggle the panel.
        var remove = new Border { Width = 20, Height = 20, Background = Brushes.Transparent, Child = close };
        AutomationProperties.SetName(remove, $"Remove {value}");
        remove.PointerEntered += (_, _) => close.Token = ShellToken.Destructive;
        remove.PointerExited += (_, _) => close.Token = ShellToken.MutedForeground;
        remove.PointerPressed += (_, e) => e.Handled = true;
        remove.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;
            if (IsEffectivelyEnabled) Toggle(value);
            e.Handled = true;
        };

        var chip = new Border
        {
            Child = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 2, Children = { label, remove } },
            Padding = new Thickness(8, 0, 2, 0),
            Height = 24,
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd)
        };
        chip.Token(BackgroundProperty, ShellToken.Secondary);
        return chip;
    }

    private void RebuildItems()
    {
        _list.Children.Clear();
        _rows.Clear();
        _active = -1;
        var items = ItemsSource;
        _listArea.Height = Math.Min(Math.Max(items?.Count ?? 0, 2) * ItemHeight, MaxListHeight);
        if (items != null)
        {
            foreach (var text in items)
                _list.Children.Add(CreateItem(text, _rows.Count));
        }
        UpdateChecks();
        Filter();
    }

    private Control CreateItem(string text, int index)
    {
        var check = new Icon { Kind = IconName.Check, Size = 16, Opacity = 0, Token = ShellToken.PopoverForeground, VerticalAlignment = VerticalAlignment.Center };
        var label = new TextBlock { Text = text, FontSize = 14, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        label.Token(TextBlock.ForegroundProperty, ShellToken.PopoverForeground);
        Grid.SetColumn(label, 1);

        var item = new Border
        {
            Child = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*"), ColumnSpacing = 8, Children = { check, label } },
            Height = ItemHeight,
            Padding = new Thickness(8, 0),
            CornerRadius = new CornerRadius(ShellTheme.RadiusSm),
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        // The pointer and the arrow keys move the same highlight.
        item.PointerMoved += (_, _) => SetActive(index);
        item.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;
            Toggle(text);
            e.Handled = true;
        };

        _rows.Add((text, item, check));
        return item;
    }

    private void UpdateChecks()
    {
        var current = Current;
        // The check keeps its column when hidden, so option labels stay aligned.
        foreach (var (text, _, check) in _rows)
            check.Opacity = current.Contains(text) ? 1 : 0;
    }

    private void Filter()
    {
        var query = _search.Text?.Trim() ?? string.Empty;
        var first = -1;
        for (var i = 0; i < _rows.Count; i++)
        {
            var (text, row, _) = _rows[i];
            row.IsVisible = query.Length == 0 || text.Contains(query, StringComparison.OrdinalIgnoreCase);
            if (row.IsVisible && first < 0) first = i;
        }
        _empty.IsVisible = first < 0;
        SetActive(first);
    }

    private void MoveActive(int direction)
    {
        for (var i = _active + direction; i >= 0 && i < _rows.Count; i += direction)
        {
            if (!_rows[i].Row.IsVisible) continue;
            SetActive(i);
            _rows[i].Row.BringIntoView();
            return;
        }
    }

    private void SetActive(int index)
    {
        if (index == _active) return;
        _active = index;
        for (var i = 0; i < _rows.Count; i++)
        {
            var row = _rows[i].Row;
            if (i == index) row.Token(BackgroundProperty, ShellToken.Accent);
            else
            {
                row.ClearToken(BackgroundProperty);
                row.Background = Brushes.Transparent;
            }
        }
    }
}
