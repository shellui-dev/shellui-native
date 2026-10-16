using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// A menu docked to a screen edge: a small handle that slides out a rail of icon tiles — tools and
// shortcuts that shouldn't take a tab in the bottom nav. Put it over the page (same cell, after
// the content); only the handle and the open rail take input. A tap outside, Escape, the handle
// or a tile closes it.
//   <Grid>
//       <ScrollViewer>...</ScrollViewer>
//       <ui:EdgeMenu Side="Right" Position="0.4">
//           <ui:EdgeMenuItem Icon="Calendar" Text="Calendar" Clicked="OnCalendar" />
//           <ui:EdgeMenuItem Icon="Camera" Text="Camera" />
//       </ui:EdgeMenu>
//   </Grid>
// Default: rounded-l-2xl border bg-popover shadow-lg, tiles hover:bg-accent.
// Solid: bg-primary text-primary-foreground, like a dark dock.
public class EdgeMenu : Border
{
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<EdgeMenu, bool>(nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<EdgeMenuSide> SideProperty =
        AvaloniaProperty.Register<EdgeMenu, EdgeMenuSide>(nameof(Side), EdgeMenuSide.Right);

    // Where the handle sits along the edge: 0 top, 0.5 middle, 1 bottom.
    public static readonly StyledProperty<double> PositionProperty =
        AvaloniaProperty.Register<EdgeMenu, double>(nameof(Position), 0.5);

    public static readonly StyledProperty<EdgeMenuVariant> VariantProperty =
        AvaloniaProperty.Register<EdgeMenu, EdgeMenuVariant>(nameof(Variant));

    // Dims the page while the menu is open.
    public static readonly StyledProperty<bool> DimProperty =
        AvaloniaProperty.Register<EdgeMenu, bool>(nameof(Dim));

    // Picking a tile closes the menu.
    public static readonly StyledProperty<bool> CloseOnSelectProperty =
        AvaloniaProperty.Register<EdgeMenu, bool>(nameof(CloseOnSelect), true);

    // The highlighted tile's Value (optional: tiles can be plain actions).
    public static readonly StyledProperty<string?> SelectedValueProperty =
        AvaloniaProperty.Register<EdgeMenu, string?>(nameof(SelectedValue), defaultBindingMode: BindingMode.TwoWay);

    private const int Duration = 220;
    private static readonly Easing Ease = new CubicEaseOut();

    private readonly Border _backdrop;
    private readonly StackPanel _dock;
    private readonly Border _handle;
    private readonly Icon _chevron;
    private readonly Border _panel;
    private readonly StackPanel _list;
    private bool _byUser;

    static EdgeMenu()
    {
        IsOpenProperty.Changed.AddClassHandler<EdgeMenu>((m, _) => m.OnOpenChanged());
        SideProperty.Changed.AddClassHandler<EdgeMenu>((m, _) => m.ApplyLook());
        VariantProperty.Changed.AddClassHandler<EdgeMenu>((m, _) => m.ApplyLook());
        PositionProperty.Changed.AddClassHandler<EdgeMenu>((m, _) => m.PlaceDock());
        DimProperty.Changed.AddClassHandler<EdgeMenu>((m, _) => m.Slide(animate: false));
        SelectedValueProperty.Changed.AddClassHandler<EdgeMenu>((m, _) =>
        {
            m.Refresh();
            if (m._byUser) m.SelectedValueChanged?.Invoke(m, m.SelectedValue);
        });
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public EdgeMenuSide Side
    {
        get => GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    public double Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public EdgeMenuVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public bool Dim
    {
        get => GetValue(DimProperty);
        set => SetValue(DimProperty, value);
    }

    public bool CloseOnSelect
    {
        get => GetValue(CloseOnSelectProperty);
        set => SetValue(CloseOnSelectProperty, value);
    }

    public string? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    // The EdgeMenuItem children.
    [Content]
    public Controls Items { get; } = new();

    // Raised on every open and close, like the other overlays.
    public event EventHandler<bool>? IsOpenChanged;

    // User input only: setting SelectedValue from code or a binding doesn't raise it.
    public event EventHandler<string?>? SelectedValueChanged;

    public EdgeMenu()
    {
        // Covers the page while open, so a tap outside closes the menu instead of reaching the page.
        _backdrop = new Border { IsVisible = false, Opacity = 0 };
        _backdrop.PointerPressed += (_, e) =>
        {
            IsOpen = false;
            e.Handled = true;
        };

        _chevron = new Icon { Size = 16 };
        _handle = new Border
        {
            Child = _chevron,
            Width = 28,
            Height = 64,
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = new Cursor(StandardCursorType.Hand),
            Focusable = true
        };
        ShellFocus.Ring(_handle, _handle);
        _handle.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;
            IsOpen = !IsOpen;
            e.Handled = true;
        };
        _handle.KeyDown += (_, e) =>
        {
            if (e.Key is not (Key.Enter or Key.Space)) return;
            IsOpen = !IsOpen;
            e.Handled = true;
        };

        _list = new StackPanel { Spacing = 6 };
        _panel = new Border
        {
            Child = new ScrollViewer { Content = _list, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Hidden },
            Padding = new Thickness(8),
            BorderThickness = new Thickness(1)
        };
        _dock = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Top };

        Child = new Panel { Children = { _backdrop, _dock } };
        // Only the handle and the open rail take input: the rest of this control is see-through.
        Background = null;
        ClipToBounds = true;
        Items.CollectionChanged += (_, _) => Rebuild();
        SizeChanged += (_, _) => PlaceDock();
        _dock.SizeChanged += (_, _) => PlaceDock();
        _panel.SizeChanged += (_, _) => Slide(animate: false);
        ApplyLook();
    }

    private List<EdgeMenuItem> ItemList => Items.OfType<EdgeMenuItem>().ToList();

    public void Open() => IsOpen = true;
    public void Close() => IsOpen = false;
    public void Toggle() => IsOpen = !IsOpen;

    // A tap or Enter / Space on a tile.
    internal void Pick(EdgeMenuItem item)
    {
        if (!string.IsNullOrEmpty(item.Value))
        {
            var outer = _byUser;
            _byUser = true;
            try { SelectedValue = item.Value; }
            finally { _byUser = outer; }
        }
        if (CloseOnSelect) IsOpen = false;
    }

    // Up and Down move between tiles.
    internal void Step(EdgeMenuItem from, int direction)
    {
        var items = ItemList.Where(i => i.IsEffectivelyEnabled).ToList();
        var index = items.IndexOf(from) + direction;
        if (index >= 0 && index < items.Count) items[index].Focus(NavigationMethod.Directional);
    }

    private void Rebuild()
    {
        _list.Children.Clear();
        foreach (var item in ItemList) _list.Children.Add(item);
        Refresh();
    }

    private void Refresh()
    {
        foreach (var item in ItemList)
            item.Apply(!string.IsNullOrEmpty(item.Value) && item.Value == SelectedValue, Variant);
    }

    private void ApplyLook()
    {
        var right = Side == EdgeMenuSide.Right;
        var solid = Variant == EdgeMenuVariant.Solid;
        _dock.Children.Clear();
        if (right) _dock.Children.AddRange(new Control[] { _handle, _panel });
        else _dock.Children.AddRange(new Control[] { _panel, _handle });
        _dock.HorizontalAlignment = right ? HorizontalAlignment.Right : HorizontalAlignment.Left;

        // Rounded on the inner side only; the outer side runs into the screen edge.
        _handle.CornerRadius = right ? new CornerRadius(12, 0, 0, 12) : new CornerRadius(0, 12, 12, 0);
        _panel.CornerRadius = right ? new CornerRadius(16, 0, 0, 16) : new CornerRadius(0, 16, 16, 0);
        _panel.BorderThickness = solid ? default : right ? new Thickness(1, 1, 0, 1) : new Thickness(0, 1, 1, 1);
        _panel.BoxShadow = new BoxShadows(new BoxShadow { OffsetY = 10, Blur = 24, Color = ShellTheme.Shadow(0.15) });

        _handle.Token(BackgroundProperty, solid ? ShellToken.Primary : ShellToken.Popover);
        _handle.BorderThickness = solid ? default : right ? new Thickness(1, 1, 0, 1) : new Thickness(0, 1, 1, 1);
        _handle.Token(BorderBrushProperty, ShellToken.Border);
        _chevron.Token = solid ? ShellToken.PrimaryForeground : ShellToken.PopoverForeground;
        _panel.Token(BackgroundProperty, solid ? ShellToken.Primary : ShellToken.Popover);
        _panel.Token(BorderBrushProperty, ShellToken.Border);
        Refresh();
        PlaceDock();
        Slide(animate: false);
    }

    // The dock's top follows Position, kept inside this control.
    private void PlaceDock()
    {
        var room = Bounds.Height - _dock.DesiredSize.Height;
        _dock.Margin = new Thickness(0, Math.Max(0, room * Math.Clamp(Position, 0, 1)), 0, 0);
        _panel.MaxHeight = Math.Max(0, Bounds.Height - 32);
    }

    private void OnOpenChanged()
    {
        if (IsOpen) ShellDismiss.Push(this, Close, this);
        else ShellDismiss.Remove(this);
        Slide(animate: IsLoaded);
        // Focus goes to the first tile on open and back to the handle on close.
        if (IsOpen) ItemList.FirstOrDefault(i => i.IsEffectivelyEnabled)?.Focus();
        else if (IsKeyboardFocusWithin) _handle.Focus();
        IsOpenChanged?.Invoke(this, IsOpen);
    }

    // Closed, the dock is pushed out by the panel's width so only the handle shows.
    private void Slide(bool animate)
    {
        var width = _panel.Bounds.Width > 0 ? _panel.Bounds.Width : 90;
        var shift = IsOpen ? 0 : Side == EdgeMenuSide.Right ? width : -width;
        var transform = FormattableString.Invariant($"translateX({shift}px)");
        if (animate) _ = ShellMotion.To(_dock, 1, transform, Duration, Ease);
        else ShellMotion.Set(_dock, 1, transform);
        // The panel fades too, so its shadow doesn't show at the edge while closed.
        if (animate) _ = ShellMotion.To(_panel, IsOpen ? 1 : 0, "scale(1)", Duration, Ease);
        else ShellMotion.Set(_panel, IsOpen ? 1 : 0, "scale(1)");

        _chevron.Kind = IsOpen == (Side == EdgeMenuSide.Right) ? IconName.ChevronRight : IconName.ChevronLeft;
        AutomationProperties.SetName(_handle, IsOpen ? "Close menu" : "Open menu");
        _panel.IsHitTestVisible = IsOpen;

        _backdrop.IsVisible = IsOpen;
        if (Dim) _backdrop.Background = new SolidColorBrush(ShellTheme.Shadow(0.4));
        else _backdrop.Background = Brushes.Transparent;
        if (animate && IsOpen) _ = ShellMotion.To(_backdrop, 1, "scale(1)", Duration, Ease);
        else ShellMotion.Set(_backdrop, IsOpen ? 1 : 0, "scale(1)");
    }
}

// A tile in an EdgeMenu — an icon over a short label (rounded-xl, 72 x 60). A tab stop: Enter or
// Space picks it, Up and Down move between tiles.
public class EdgeMenuItem : Border, IShellFocusable
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<EdgeMenuItem, string?>(nameof(Text));

    // Identifies the tile for EdgeMenu.SelectedValue (optional).
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<EdgeMenuItem, string?>(nameof(Value));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<EdgeMenuItem, IconName>(nameof(Icon));

    private readonly Icon _icon;
    private readonly TextBlock _label;
    private bool _active;
    private EdgeMenuVariant _variant;

    static EdgeMenuItem()
    {
        TextProperty.Changed.AddClassHandler<EdgeMenuItem>((i, _) => i.Update());
        IconProperty.Changed.AddClassHandler<EdgeMenuItem>((i, _) => i.Update());
        IsEnabledProperty.Changed.AddClassHandler<EdgeMenuItem>((i, _) => i.Update());
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IconName Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsActive => _active;

    // Raised on every tap or Enter / Space.
    public event EventHandler? Clicked;

    public EdgeMenuItem()
    {
        _icon = new Icon { Size = 20 };
        _label = new TextBlock { FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        Child = new StackPanel
        {
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { _icon, _label }
        };
        Width = 72;
        Height = 60;
        Padding = new Thickness(4, 0);
        CornerRadius = new CornerRadius(12);
        Cursor = new Cursor(StandardCursorType.Hand);
        ShellFocus.Ring(this, this);
        Update();
    }

    // Solid tiles mix their color from the theme, so they follow it by hand.
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ShellTheme.ThemeChanged += OnThemeChanged;
        Update();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ShellTheme.ThemeChanged -= OnThemeChanged;
    }

    private void OnThemeChanged(object? sender, EventArgs e) => Update();

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        Update();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        Update();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (IsEffectivelyEnabled) _ = ShellMotion.To(this, Opacity, "scale(0.95)", 80, new CubicEaseOut());
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _ = ShellMotion.To(this, Opacity, "scale(1)", 150, new CubicEaseOut());
        if (e.InitialPressMouseButton != MouseButton.Left) return;
        Press();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.Key)
        {
            case Key.Enter or Key.Space:
                Press();
                break;
            case Key.Up or Key.Down:
                this.FindParentOfType<EdgeMenu>()?.Step(this, e.Key == Key.Down ? 1 : -1);
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    // Picks the tile in code, as a tap or Enter / Space would.
    public void Press()
    {
        if (!IsEffectivelyEnabled) return;
        Clicked?.Invoke(this, EventArgs.Empty);
        this.FindParentOfType<EdgeMenu>()?.Pick(this);
    }

    internal void Apply(bool isActive, EdgeMenuVariant variant)
    {
        _active = isActive;
        _variant = variant;
        Update();
    }

    private void Update()
    {
        _icon.Kind = Icon;
        _label.Text = Text ?? string.Empty;
        _label.IsVisible = !string.IsNullOrEmpty(Text);
        AutomationProperties.SetName(this, Text);

        var hovered = IsPointerOver && IsEffectivelyEnabled;
        if (_variant == EdgeMenuVariant.Solid)
        {
            // Tiles a shade lighter than the dock; brighter when hovered or active.
            var shade = _active ? 0.2 : hovered ? 0.14 : 0.08;
            this.ClearToken(BackgroundProperty);
            Background = new SolidColorBrush(ShellTheme.Get(ShellToken.PrimaryForeground, shade));
            _icon.Token = ShellToken.PrimaryForeground;
            _label.Token(TextBlock.ForegroundProperty, ShellToken.PrimaryForeground);
            _label.Opacity = _active || hovered ? 1 : 0.75;
        }
        else
        {
            if (_active || hovered) this.Token(BackgroundProperty, ShellToken.Accent);
            else
            {
                this.ClearToken(BackgroundProperty);
                Background = Brushes.Transparent;
            }
            var foreground = _active || hovered ? ShellToken.AccentForeground : ShellToken.MutedForeground;
            _icon.Token = _active || hovered ? ShellToken.AccentForeground : ShellToken.PopoverForeground;
            _label.Token(TextBlock.ForegroundProperty, foreground);
            _label.Opacity = 1;
        }
        _label.FontWeight = _active ? FontWeight.SemiBold : FontWeight.Normal;
        Opacity = IsEnabled ? 1.0 : 0.5;
    }
}

public enum EdgeMenuSide { Right, Left }

public enum EdgeMenuVariant { Default, Solid }
