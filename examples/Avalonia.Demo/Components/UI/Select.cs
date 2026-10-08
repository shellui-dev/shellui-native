using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Select — shadcn-style trigger (h-10 rounded-md border, chevrons icon) and a list that floats
// below it, with a check on the selected item. A click outside or Escape closes it.
// Usage: <ui:Select Placeholder="Pick a country" /> then set ItemsSource in code or by binding.
public class Select : Border
{
    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<Select, int>(nameof(SelectedIndex), -1, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<IList<string>?> ItemsSourceProperty =
        AvaloniaProperty.Register<Select, IList<string>?>(nameof(ItemsSource));

    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<Select, string?>(nameof(Placeholder), "Select...");

    private const double ItemHeight = 32;
    private const double MaxListHeight = 280;

    private readonly Border _trigger;
    private readonly TextBlock _value;
    private readonly StackPanel _list = new();
    private readonly Border _panel;
    private readonly ShellAnchoredPopup _popup;

    static Select()
    {
        SelectedIndexProperty.Changed.AddClassHandler<Select>((s, _) =>
        {
            s.UpdateTrigger();
            s.UpdateItemStates();
            s.SelectedIndexChanged?.Invoke(s, EventArgs.Empty);
        });
        ItemsSourceProperty.Changed.AddClassHandler<Select>((s, _) => s.RebuildItems());
        PlaceholderProperty.Changed.AddClassHandler<Select>((s, _) => s.UpdateTrigger());
        IsEnabledProperty.Changed.AddClassHandler<Select>((s, _) => s.Opacity = s.IsEnabled ? 1.0 : 0.5);
    }

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
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

    public string? SelectedItem => SelectedIndex >= 0 && ItemsSource != null && SelectedIndex < ItemsSource.Count
        ? ItemsSource[SelectedIndex] : null;

    public bool IsOpen => _popup.IsOpen;

    public event EventHandler? SelectedIndexChanged;

    public Select()
    {
        _value = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        var chevrons = new Icon { Kind = IconName.ChevronsUpDown, Size = 16, Token = ShellToken.MutedForeground };
        Grid.SetColumn(chevrons, 1);
        _trigger = new Border
        {
            Child = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), ColumnSpacing = 8, Children = { _value, chevrons } },
            Height = 40,
            Padding = new Thickness(12, 0),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        _trigger.Token(BorderBrushProperty, ShellToken.Input);

        _panel = new Border
        {
            Child = new ScrollViewer { Content = _list, MaxHeight = MaxListHeight },
            Padding = new Thickness(4),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            BoxShadow = ShellPopups.PanelShadow()
        };
        _panel.Token(BackgroundProperty, ShellToken.Popover).Token(BorderBrushProperty, ShellToken.Border);

        _popup = new ShellAnchoredPopup(_trigger);
        _popup.Popup.Child = _panel;
        _popup.Closed += (_, _) => _trigger.Token(BorderBrushProperty, ShellToken.Input);

        Child = new Panel { Children = { _trigger, _popup.Popup } };
        ShellFocus.Ring(this, _trigger);
        UpdateTrigger();
    }

    public void SetOpen(bool open)
    {
        if (open == _popup.IsOpen) return;
        if (!open)
        {
            _popup.Close();
            return;
        }
        // The list matches the trigger's width (shadcn's w-[--radix-select-trigger-width]).
        _panel.Width = _trigger.Bounds.Width;
        _trigger.Token(BorderBrushProperty, ShellToken.Ring);
        _popup.Open();
    }

    public void Close() => SetOpen(false);

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left || !IsEffectivelyEnabled) return;
        SetOpen(!IsOpen);
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is not (Key.Enter or Key.Space or Key.Down)) return;
        SetOpen(true);
        e.Handled = true;
    }

    private void UpdateTrigger()
    {
        var item = SelectedItem;
        _value.Text = item ?? Placeholder ?? string.Empty;
        _value.Token(TextBlock.ForegroundProperty, item != null ? ShellToken.Foreground : ShellToken.MutedForeground);
    }

    private void RebuildItems()
    {
        _list.Children.Clear();
        if (ItemsSource is { } items)
            for (var i = 0; i < items.Count; i++)
                _list.Children.Add(CreateItem(items[i], i));
        UpdateTrigger();
        UpdateItemStates();
    }

    private Control CreateItem(string text, int index)
    {
        var label = new TextBlock { Text = text, FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
        label.Token(TextBlock.ForegroundProperty, ShellToken.PopoverForeground);
        var check = new Icon { Kind = IconName.Check, Size = 16, IsVisible = false, Token = ShellToken.PopoverForeground };
        Grid.SetColumn(check, 1);

        var item = new Border
        {
            Child = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), ColumnSpacing = 8, Children = { label, check } },
            Height = ItemHeight,
            Padding = new Thickness(8, 0),
            CornerRadius = new CornerRadius(ShellTheme.RadiusSm),
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand),
            Tag = check
        };
        item.PointerEntered += (_, _) => item.Token(BackgroundProperty, ShellToken.Accent);
        item.PointerExited += (_, _) => { item.ClearToken(BackgroundProperty); item.Background = Brushes.Transparent; };
        item.PointerReleased += (_, e) =>
        {
            SelectedIndex = index;
            SetOpen(false);
            e.Handled = true;
        };
        return item;
    }

    private void UpdateItemStates()
    {
        for (var i = 0; i < _list.Children.Count; i++)
            if (_list.Children[i].Tag is Icon check)
                check.IsVisible = i == SelectedIndex;
    }
}
