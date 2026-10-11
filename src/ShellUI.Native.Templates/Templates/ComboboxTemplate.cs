using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class ComboboxTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "combobox",
        DisplayName = "Combobox",
        Description = "Searchable select with a filter field",
        Category = ComponentCategory.Form,
        FilePath = "Combobox.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "combobox", "select", "search", "autocomplete" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Combobox — a Select with a search field: h-10 trigger, and a floating panel with a filter
// input above the option list (check on the selected option, ""no results"" when nothing matches).
// Usage: <ui:Combobox Placeholder=""Select framework..."" Value=""{Binding Framework}"" /> then set
// ItemsSource in code or by binding.
public partial class Combobox : ContentView, IShellPopup
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(Combobox), string.Empty, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((Combobox)b).OnValueChanged());

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IList<string>), typeof(Combobox), null,
            propertyChanged: (b, o, n) => ((Combobox)b).RebuildItems());

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(Combobox), ""Select..."",
            propertyChanged: (b, o, n) => ((Combobox)b).UpdateTrigger());

    public static readonly BindableProperty SearchPlaceholderProperty =
        BindableProperty.Create(nameof(SearchPlaceholder), typeof(string), typeof(Combobox), ""Search..."",
            propertyChanged: (b, o, n) => ((Combobox)b)._search.Placeholder = (string)n);

    public static readonly BindableProperty EmptyTextProperty =
        BindableProperty.Create(nameof(EmptyText), typeof(string), typeof(Combobox), ""No results found."",
            propertyChanged: (b, o, n) => ((Combobox)b)._empty.Text = (string)n);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IList<string>? ItemsSource
    {
        get => (IList<string>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string SearchPlaceholder
    {
        get => (string)GetValue(SearchPlaceholderProperty);
        set => SetValue(SearchPlaceholderProperty, value);
    }

    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public bool IsOpen { get; private set; }

    public event EventHandler<string>? ValueChanged;

    private const double ItemHeight = 32;
    private const double MaxListHeight = 240;

    private readonly Border _trigger;
    private readonly Label _value;
    private readonly Border _panel;
    private readonly Entry _search;
    private readonly VerticalStackLayout _list;
    private readonly Grid _listArea;
    private readonly Label _empty;
    private readonly List<(string Text, Border Row, Icon Check)> _rows = new();
    private ShellPopupHandle? _handle;

    public Combobox()
    {
        _value = new Label { FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.TailTruncation };
        var triggerRow = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        triggerRow.Add(_value, 0, 0);
        triggerRow.Add(new Icon { Name = IconName.ChevronsUpDown, Size = 16, Token = ShellToken.MutedForeground }, 1, 0);

        _trigger = new Border
        {
            Content = triggerRow,
            HeightRequest = 40,
            Padding = new Thickness(12, 0),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        _trigger.Token(Border.StrokeProperty, ShellToken.Input);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (!IsEnabled) return; ShellFocus.FocusPressed(this); SetOpen(!IsOpen); };
        _trigger.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, () => { if (IsEnabled) SetOpen(!IsOpen); });

        _search = new Entry
        {
            Placeholder = SearchPlaceholder,
            FontSize = 14,
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center,
            ClearButtonVisibility = ClearButtonVisibility.Never,
            IsTextPredictionEnabled = false,
            IsSpellCheckEnabled = false
        };
        _search.Token(Entry.TextColorProperty, ShellToken.PopoverForeground);
        _search.Token(Entry.PlaceholderColorProperty, ShellToken.MutedForeground);
        ShellPlatform.StripNativeChrome(_search);
        ShellFocus.Track(_search);
        _search.TextChanged += (_, _) => Filter();
        // Enter picks the first match.
        _search.Completed += (_, _) =>
        {
            var first = _rows.FirstOrDefault(r => r.Row.IsVisible);
            if (first.Row != null) Pick(first.Text);
        };

        var searchRow = new Grid
        {
            HeightRequest = 40,
            Padding = new Thickness(12, 0),
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }
        };
        searchRow.Add(new Icon { Name = IconName.Search, Size = 16, Token = ShellToken.MutedForeground }, 0, 0);
        searchRow.Add(_search, 1, 0);

        var divider = new BoxView { HeightRequest = 1, BackgroundColor = Colors.Transparent };
        divider.Token(BoxView.ColorProperty, ShellToken.Border);

        _list = new VerticalStackLayout { Spacing = 0 };
        _empty = new Label
        {
            Text = EmptyText,
            FontSize = 14,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            IsVisible = false
        };
        _empty.Token(Label.TextColorProperty, ShellToken.MutedForeground);
        // Fixed height (set in RebuildItems): the panel keeps its size and position while filtering.
        _listArea = new Grid { Margin = new Thickness(4), Children = { new ScrollView { Content = _list }, _empty } };

        _panel = new Border
        {
            Content = new VerticalStackLayout { Spacing = 0, Children = { searchRow, divider, _listArea } },
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            Shadow = ShellPopups.PanelShadow()
        };
        _panel.Token(VisualElement.BackgroundColorProperty, ShellToken.Popover);
        _panel.Token(Border.StrokeProperty, ShellToken.Border);
        // Taps on the panel's own chrome must not fall through to the click-outside catcher.
        _panel.GestureRecognizers.Add(new TapGestureRecognizer());

        Content = _trigger;
        RebuildItems();
    }

    public void SetOpen(bool open)
    {
        if (IsOpen == open) return;
        IsOpen = open;
        _trigger.Token(Border.StrokeProperty, open ? ShellToken.Ring : ShellToken.Input);
        if (open)
        {
            _search.Text = string.Empty;
            Filter();
            ShellPopups.Opened(this);
            _handle = ShellPortal.ShowPopup(_trigger, _panel, new ShellPopupOptions
            {
                MatchAnchorWidth = true,
                Owner = this,
                OnDismiss = Close
            });
            // Type-to-filter straight away where there is a hardware keyboard; on touch devices
            // the soft keyboard would cover the list, so the user taps the field to search.
            if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
                Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(150), () => { if (IsOpen) _search.Focus(); });
        }
        else
        {
            ShellPopups.Closed(this);
            if (_search.IsFocused) _search.Unfocus();
            var handle = _handle;
            _handle = null;
            if (handle != null) _ = handle.CloseAsync();
        }
    }

    public void Close() => SetOpen(false);

    private void Pick(string text)
    {
        Value = text;
        SetOpen(false);
    }

    private void OnValueChanged()
    {
        UpdateTrigger();
        UpdateChecks();
        ValueChanged?.Invoke(this, Value ?? string.Empty);
    }

    private void UpdateTrigger()
    {
        var hasValue = !string.IsNullOrEmpty(Value);
        _value.Text = hasValue ? Value : Placeholder;
        _value.Token(Label.TextColorProperty, hasValue ? ShellToken.Foreground : ShellToken.MutedForeground);
    }

    private void RebuildItems()
    {
        _list.Children.Clear();
        _rows.Clear();
        var items = ItemsSource;
        _listArea.HeightRequest = Math.Min(Math.Max(items?.Count ?? 0, 2) * ItemHeight, MaxListHeight);
        if (items != null)
        {
            foreach (var text in items)
                _list.Children.Add(CreateItem(text));
        }
        UpdateTrigger();
        UpdateChecks();
        Filter();
    }

    private View CreateItem(string text)
    {
        var label = new Label { Text = text, FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.TailTruncation };
        label.Token(Label.TextColorProperty, ShellToken.PopoverForeground);
        var check = new Icon { Name = IconName.Check, Size = 16, IsVisible = false };
        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        row.Add(label, 0, 0);
        row.Add(check, 1, 0);

        var item = new Border
        {
            Content = row,
            HeightRequest = ItemHeight,
            Padding = new Thickness(8, 0),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusSm },
            BackgroundColor = Colors.Transparent
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => item.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        pointer.PointerExited += (_, _) => { item.ClearValue(VisualElement.BackgroundColorProperty); item.BackgroundColor = Colors.Transparent; };
        item.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => Pick(text);
        item.GestureRecognizers.Add(tap);

        _rows.Add((text, item, check));
        return item;
    }

    private void UpdateChecks()
    {
        foreach (var (text, _, check) in _rows)
            check.IsVisible = text == Value;
    }

    private void Filter()
    {
        var query = _search.Text?.Trim() ?? string.Empty;
        var any = false;
        foreach (var (text, row, _) in _rows)
        {
            row.IsVisible = query.Length == 0 || text.Contains(query, StringComparison.OrdinalIgnoreCase);
            any |= row.IsVisible;
        }
        _empty.IsVisible = !any;
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace YourProjectNamespace.Components.UI;

// Combobox — a Select with a search field: h-10 trigger, and a floating panel with a filter
// input above the option list (check on the selected option, ""no results"" when nothing matches).
// Type to filter, Up / Down to move, Enter to pick, Escape to close.
// Usage: <ui:Combobox Placeholder=""Select framework..."" Value=""{Binding Framework}"" /> then set
// ItemsSource in code or by binding.
public class Combobox : Border
{
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<Combobox, string?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<IList<string>?> ItemsSourceProperty =
        AvaloniaProperty.Register<Combobox, IList<string>?>(nameof(ItemsSource));

    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<Combobox, string?>(nameof(Placeholder), ""Select..."");

    public static readonly StyledProperty<string?> SearchPlaceholderProperty =
        AvaloniaProperty.Register<Combobox, string?>(nameof(SearchPlaceholder), ""Search..."");

    public static readonly StyledProperty<string?> EmptyTextProperty =
        AvaloniaProperty.Register<Combobox, string?>(nameof(EmptyText), ""No results found."");

    private const double ItemHeight = 32;
    private const double MaxListHeight = 240;

    private readonly Border _trigger;
    private readonly TextBlock _value;
    private readonly Border _panel;
    private readonly TextBox _search;
    private readonly StackPanel _list = new();
    private readonly Panel _listArea;
    private readonly TextBlock _empty;
    private readonly List<(string Text, Border Row, Icon Check)> _rows = new();
    private readonly ShellAnchoredPopup _popup;
    private int _active = -1;

    static Combobox()
    {
        ValueProperty.Changed.AddClassHandler<Combobox>((c, _) =>
        {
            c.UpdateTrigger();
            c.UpdateChecks();
            c.ValueChanged?.Invoke(c, c.Value ?? string.Empty);
        });
        ItemsSourceProperty.Changed.AddClassHandler<Combobox>((c, _) => c.RebuildItems());
        PlaceholderProperty.Changed.AddClassHandler<Combobox>((c, _) => c.UpdateTrigger());
        SearchPlaceholderProperty.Changed.AddClassHandler<Combobox>((c, e) => c._search.PlaceholderText = (string?)e.NewValue);
        EmptyTextProperty.Changed.AddClassHandler<Combobox>((c, e) => c._empty.Text = (string?)e.NewValue);
        IsEnabledProperty.Changed.AddClassHandler<Combobox>((c, _) => c.Opacity = c.IsEnabled ? 1.0 : 0.5);
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IList<string>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
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

    public event EventHandler<string>? ValueChanged;

    public Combobox()
    {
        _value = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        var chevrons = new Icon { Kind = IconName.ChevronsUpDown, Size = 16, Token = ShellToken.MutedForeground };
        Grid.SetColumn(chevrons, 1);
        _trigger = new Border
        {
            Child = new Grid { ColumnDefinitions = new ColumnDefinitions(""*,Auto""), ColumnSpacing = 8, Children = { _value, chevrons } },
            Height = 40,
            Padding = new Thickness(12, 0),
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
                ColumnDefinitions = new ColumnDefinitions(""Auto,*""),
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
    }

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
            if (_active >= 0) Pick(_rows[_active].Text);
        }
        else if (e.Key == Key.Down) MoveActive(1);
        else if (e.Key == Key.Up) MoveActive(-1);
        else return;
        e.Handled = true;
    }

    private void Pick(string text)
    {
        Value = text;
        SetOpen(false);
    }

    private void UpdateTrigger()
    {
        var hasValue = !string.IsNullOrEmpty(Value);
        _value.Text = hasValue ? Value : Placeholder ?? string.Empty;
        _value.Token(TextBlock.ForegroundProperty, hasValue ? ShellToken.Foreground : ShellToken.MutedForeground);
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
        UpdateTrigger();
        UpdateChecks();
        Filter();
    }

    private Control CreateItem(string text, int index)
    {
        var label = new TextBlock { Text = text, FontSize = 14, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        label.Token(TextBlock.ForegroundProperty, ShellToken.PopoverForeground);
        var check = new Icon { Kind = IconName.Check, Size = 16, IsVisible = false, Token = ShellToken.PopoverForeground };
        Grid.SetColumn(check, 1);

        var item = new Border
        {
            Child = new Grid { ColumnDefinitions = new ColumnDefinitions(""*,Auto""), ColumnSpacing = 8, Children = { label, check } },
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
            Pick(text);
            e.Handled = true;
        };

        _rows.Add((text, item, check));
        return item;
    }

    private void UpdateChecks()
    {
        foreach (var (text, _, check) in _rows)
            check.IsVisible = text == Value;
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
"
    };
}
