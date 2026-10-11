using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Context menu — right-click the trigger (long-press on touch, or the Menu key / Shift+F10) to
// open a menu at the pointer. Clicking outside, picking an item or pressing Escape closes it.
//   <ui:ContextMenu>
//       <ui:ContextMenuTrigger> ...any control... </ui:ContextMenuTrigger>
//       <ui:ContextMenuContent>
//           <ui:ContextMenuItem Text="Copy" Icon="Copy" Shortcut="Ctrl+C" Clicked="OnCopy" />
//           <ui:ContextMenuSeparator />
//           <ui:ContextMenuItem Text="Delete" Icon="Trash2" IsDestructive="True" Clicked="OnDelete" />
//       </ui:ContextMenuContent>
//   </ui:ContextMenu>
public class ContextMenu : ShellPopoverHost
{
    private bool _atPoint;

    public ContextMenu()
    {
        // The trigger usually is a surface (a card, a row): let it fill its container.
        HorizontalAlignment = HorizontalAlignment.Stretch;
        // Clicks on the trigger reach it while the menu is open, so a second right-click moves the menu.
        Popup.OverlayInputPassThroughElement = this;
    }

    protected override bool IsContent(Control child) => child is ContextMenuContent;
    protected override double Offset => _atPoint ? 0 : 4;

    // Opens the menu at a point relative to this control, or below the trigger when null.
    public void OpenAt(Point? point)
    {
        _atPoint = point.HasValue;
        Popup.PlacementRect = point is { } at ? new Rect(at, new Size(1, 1)) : null;
        if (IsOpen) Popup.VerticalOffset = Offset;
        else IsOpen = true;
    }
}

// The surface that opens the enclosing ContextMenu.
public class ContextMenuTrigger : Border
{
    public ContextMenuTrigger()
    {
        // Transparent, not null, so empty areas of the surface take the right-click too.
        Background = Brushes.Transparent;
        AddHandler(ContextRequestedEvent, OnContextRequested);
    }

    private void OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (this.FindParentOfType<ContextMenu>() is not { } menu) return;
        menu.OpenAt(e.TryGetPosition(menu, out var point) ? point : null);
        e.Handled = true;
    }

    // A long-press on touch opens the menu under the trigger.
    protected override void OnHolding(HoldingRoutedEventArgs e)
    {
        base.OnHolding(e);
        if (e.HoldingState != HoldingState.Started || this.FindParentOfType<ContextMenu>() is not { IsOpen: false } menu) return;
        menu.OpenAt(null);
        e.Handled = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            this.FindParentOfType<ContextMenu>()?.Close();
    }
}

// Menu panel — min-w-[13rem] rounded-md border bg-popover p-1 shadow-md. XAML children go to Items.
public class ContextMenuContent : Border
{
    private readonly StackPanel _stack = new();

    [Content]
    public Controls Items => _stack.Children;

    public ContextMenuContent()
    {
        Child = _stack;
        Padding = new Thickness(4);
        MinWidth = 208;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        BoxShadow = ShellPopups.PanelShadow();
        this.Token(BackgroundProperty, ShellToken.Popover).Token(BorderBrushProperty, ShellToken.Border);
    }
}

// Menu row — rounded-sm px-2 py-1.5 text-sm, hover:bg-accent, optional icon and shortcut hint.
public class ContextMenuItem : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<ContextMenuItem, string?>(nameof(Text));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<ContextMenuItem, IconName>(nameof(Icon));

    // Keyboard hint shown at the end of the row (display only).
    public static readonly StyledProperty<string?> ShortcutProperty =
        AvaloniaProperty.Register<ContextMenuItem, string?>(nameof(Shortcut));

    public static readonly StyledProperty<bool> IsDestructiveProperty =
        AvaloniaProperty.Register<ContextMenuItem, bool>(nameof(IsDestructive));

    private readonly Icon _icon;
    private readonly TextBlock _label;
    private readonly TextBlock _shortcut;

    static ContextMenuItem()
    {
        TextProperty.Changed.AddClassHandler<ContextMenuItem>((i, _) => i.Update());
        IconProperty.Changed.AddClassHandler<ContextMenuItem>((i, _) => i.Update());
        ShortcutProperty.Changed.AddClassHandler<ContextMenuItem>((i, _) => i.Update());
        IsDestructiveProperty.Changed.AddClassHandler<ContextMenuItem>((i, _) => i.Update());
        IsEnabledProperty.Changed.AddClassHandler<ContextMenuItem>((i, _) => i.Opacity = i.IsEnabled ? 1.0 : 0.5);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IconName Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Shortcut
    {
        get => GetValue(ShortcutProperty);
        set => SetValue(ShortcutProperty, value);
    }

    public bool IsDestructive
    {
        get => GetValue(IsDestructiveProperty);
        set => SetValue(IsDestructiveProperty, value);
    }

    public event EventHandler? Clicked;

    public ContextMenuItem()
    {
        _icon = new Icon { Size = 16, IsVisible = false, VerticalAlignment = VerticalAlignment.Center };
        _label = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
        _shortcut = new TextBlock { FontSize = 12, VerticalAlignment = VerticalAlignment.Center, IsVisible = false };
        _shortcut.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"), ColumnSpacing = 8 };
        Grid.SetColumn(_label, 1);
        Grid.SetColumn(_shortcut, 2);
        grid.Children.Add(_icon);
        grid.Children.Add(_label);
        grid.Children.Add(_shortcut);

        Child = grid;
        Height = 32;
        Padding = new Thickness(8, 0);
        CornerRadius = new CornerRadius(ShellTheme.RadiusSm);
        Background = Brushes.Transparent;
        Cursor = new Cursor(StandardCursorType.Hand);
        ShellFocus.Ring(this, this);
        Update();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (IsEffectivelyEnabled) this.Token(BackgroundProperty, ShellToken.Accent);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        this.ClearToken(BackgroundProperty);
        Background = Brushes.Transparent;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left) return;
        Press();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is not (Key.Enter or Key.Space)) return;
        Press();
        e.Handled = true;
    }

    // Picks the item in code, as a click or Enter/Space would.
    public void Press()
    {
        if (!IsEffectivelyEnabled) return;
        this.FindParentOfType<ContextMenu>()?.Close();
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        var color = IsDestructive ? ShellToken.Destructive : ShellToken.PopoverForeground;
        _icon.Kind = Icon;
        _icon.IsVisible = Icon != IconName.None;
        _icon.Token = color;
        _label.Text = Text ?? string.Empty;
        _label.Token(TextBlock.ForegroundProperty, color);
        _shortcut.Text = Shortcut ?? string.Empty;
        _shortcut.IsVisible = !string.IsNullOrEmpty(Shortcut);
    }
}

// Divider between groups of items — -mx-1 my-1 h-px bg-border.
public class ContextMenuSeparator : Border
{
    public ContextMenuSeparator()
    {
        Height = 1;
        Margin = new Thickness(-4, 4);
        this.Token(BackgroundProperty, ShellToken.Border);
    }
}
