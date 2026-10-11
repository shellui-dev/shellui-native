using System;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Menu bar — h-10 rounded-md border bg-background p-1: a row of menus, each opening a panel of
// items under its title. Clicking outside, picking an item or pressing Escape closes it. The
// titles are tab stops: Enter, Space or Down opens a menu, and Left / Right move between them.
//   <ui:Menubar>
//       <ui:MenubarMenu Title="File">
//           <ui:MenubarItem Text="New tab" Shortcut="Ctrl+T" Clicked="OnNewTab" />
//           <ui:MenubarSeparator />
//           <ui:MenubarItem Text="Print" Icon="FileText" Clicked="OnPrint" />
//       </ui:MenubarMenu>
//       <ui:MenubarMenu Title="Edit"> ... </ui:MenubarMenu>
//   </ui:Menubar>
public class Menubar : Border
{
    private readonly StackPanel _row = new() { Orientation = Orientation.Horizontal, Spacing = 4, VerticalAlignment = VerticalAlignment.Center };

    // The MenubarMenu children.
    [Content]
    public Controls Menus => _row.Children;

    public Menubar()
    {
        Child = _row;
        Height = 40;
        Padding = new Thickness(3);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        HorizontalAlignment = HorizontalAlignment.Left;
        this.Token(BackgroundProperty, ShellToken.Background).Token(BorderBrushProperty, ShellToken.Border);
        _row.Children.CollectionChanged += (_, _) =>
        {
            foreach (var menu in _row.Children.OfType<MenubarMenu>()) menu.Bar = this;
        };
    }

    // Moves keyboard focus to the menu before or after `from`, wrapping around.
    internal void FocusNeighbour(MenubarMenu from, int direction)
    {
        var menus = _row.Children.OfType<MenubarMenu>().Where(m => m.IsEffectivelyEnabled).ToList();
        var index = menus.IndexOf(from);
        if (index < 0 || menus.Count < 2) return;
        menus[(index + direction + menus.Count) % menus.Count].Focus(NavigationMethod.Directional);
    }
}

// One menu of a Menubar: the title in the bar (rounded-sm px-3 text-sm font-medium) and the
// panel that opens under it (min-w-[12rem] rounded-md border bg-popover p-1 shadow-md).
public class MenubarMenu : Border, IShellFocusable
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<MenubarMenu, string?>(nameof(Title));

    private readonly TextBlock _label;
    private readonly Border _trigger;
    private readonly StackPanel _stack = new();
    private readonly ShellAnchoredPopup _popup;

    static MenubarMenu()
    {
        TitleProperty.Changed.AddClassHandler<MenubarMenu>((m, _) => m.Update());
        IsEnabledProperty.Changed.AddClassHandler<MenubarMenu>((m, _) => m.Opacity = m.IsEnabled ? 1.0 : 0.5);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // The MenubarItem and MenubarSeparator children.
    [Content]
    public Controls Items => _stack.Children;

    public bool IsOpen => _popup.IsOpen;

    public event EventHandler? Opened;
    public event EventHandler? Closed;

    internal Menubar? Bar { get; set; }

    public MenubarMenu()
    {
        _label = new TextBlock { FontSize = 14, FontWeight = FontWeight.Medium, VerticalAlignment = VerticalAlignment.Center };
        _label.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        _trigger = new Border
        {
            Child = _label,
            Height = 32,
            Padding = new Thickness(12, 0),
            CornerRadius = new CornerRadius(ShellTheme.RadiusSm),
            // Transparent, not null, so the whole title takes the pointer.
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand)
        };

        var panel = new Border
        {
            Child = _stack,
            Padding = new Thickness(4),
            MinWidth = 192,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            BoxShadow = ShellPopups.PanelShadow()
        };
        panel.Token(BackgroundProperty, ShellToken.Popover).Token(BorderBrushProperty, ShellToken.Border);

        _popup = new ShellAnchoredPopup(_trigger);
        _popup.Popup.Child = panel;
        _popup.Closed += (_, _) =>
        {
            Update();
            Closed?.Invoke(this, EventArgs.Empty);
        };

        Child = new Panel { Children = { _trigger, _popup.Popup } };
        VerticalAlignment = VerticalAlignment.Center;
        ShellFocus.Ring(this, _trigger);
        Update();
    }

    public void SetOpen(bool open)
    {
        if (open == _popup.IsOpen) return;
        if (!open)
        {
            _popup.Close();
            return;
        }
        foreach (var item in _stack.Children.OfType<MenubarItem>()) item.Menu = this;
        _popup.Open();
        Update();
        Opened?.Invoke(this, EventArgs.Empty);
    }

    public void Close() => SetOpen(false);

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
        if (!ReferenceEquals(e.Source, this)) return;
        switch (e.Key)
        {
            case Key.Enter or Key.Space or Key.Down:
                SetOpen(true);
                // Into the panel, so Tab and Enter work on the items.
                _stack.Children.OfType<MenubarItem>().FirstOrDefault(i => i.IsEffectivelyEnabled)?.Focus(NavigationMethod.Directional);
                break;
            case Key.Left:
                Bar?.FocusNeighbour(this, -1);
                break;
            case Key.Right:
                Bar?.FocusNeighbour(this, 1);
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    private void Update()
    {
        _label.Text = Title ?? string.Empty;
        AutomationProperties.SetName(this, Title ?? string.Empty);
        if (IsOpen || (IsPointerOver && IsEffectivelyEnabled)) _trigger.Token(BackgroundProperty, ShellToken.Accent);
        else
        {
            _trigger.ClearToken(BackgroundProperty);
            _trigger.Background = Brushes.Transparent;
        }
    }
}

// Menu row — rounded-sm px-2 py-1.5 text-sm, hover:bg-accent, optional icon and shortcut hint.
// A tab stop that activates with Enter or Space; Up and Down move between the rows.
public class MenubarItem : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<MenubarItem, string?>(nameof(Text));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<MenubarItem, IconName>(nameof(Icon));

    // Keyboard hint shown at the end of the row (display only).
    public static readonly StyledProperty<string?> ShortcutProperty =
        AvaloniaProperty.Register<MenubarItem, string?>(nameof(Shortcut));

    public static readonly StyledProperty<bool> IsDestructiveProperty =
        AvaloniaProperty.Register<MenubarItem, bool>(nameof(IsDestructive));

    private readonly Icon _icon;
    private readonly TextBlock _label;
    private readonly TextBlock _shortcut;

    static MenubarItem()
    {
        TextProperty.Changed.AddClassHandler<MenubarItem>((i, _) => i.Update());
        IconProperty.Changed.AddClassHandler<MenubarItem>((i, _) => i.Update());
        ShortcutProperty.Changed.AddClassHandler<MenubarItem>((i, _) => i.Update());
        IsDestructiveProperty.Changed.AddClassHandler<MenubarItem>((i, _) => i.Update());
        IsEnabledProperty.Changed.AddClassHandler<MenubarItem>((i, _) => i.Opacity = i.IsEnabled ? 1.0 : 0.5);
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

    internal MenubarMenu? Menu { get; set; }

    public MenubarItem()
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
        switch (e.Key)
        {
            case Key.Enter or Key.Space:
                Press();
                break;
            case Key.Up or Key.Down:
                if (Parent is not Panel rows) return;
                var items = rows.Children.OfType<MenubarItem>().Where(i => i.IsEffectivelyEnabled).ToList();
                var index = items.IndexOf(this);
                if (index < 0) return;
                items[(index + (e.Key == Key.Down ? 1 : -1) + items.Count) % items.Count].Focus(NavigationMethod.Directional);
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    // Picks the item in code, as a click or Enter/Space would.
    public void Press()
    {
        if (!IsEffectivelyEnabled) return;
        Menu?.Close();
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
public class MenubarSeparator : Border
{
    public MenubarSeparator()
    {
        Height = 1;
        Margin = new Thickness(-4, 4);
        this.Token(BackgroundProperty, ShellToken.Border);
    }
}
