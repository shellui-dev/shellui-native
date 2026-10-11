using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Top navigation bar — h-14 border-b bg-background: a brand (icon and title), links, and actions
// at the end. Narrower than CollapseBelow, the links move into a menu that a button opens under
// the bar.
//   <ui:Navbar Title="ShellUI" Icon="Sparkles" SelectedValue="{Binding Page}">
//       <ui:NavbarItem Value="docs" Text="Docs" />
//       <ui:NavbarItem Value="components" Text="Components" />
//       <ui:Navbar.Actions>
//           <ui:Button Icon="Search" Size="Icon" Variant="Ghost" />
//       </ui:Navbar.Actions>
//   </ui:Navbar>
public class Navbar : Border
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Navbar, string?>(nameof(Title));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<Navbar, IconName>(nameof(Icon));

    // Replaces the icon and title.
    public static readonly StyledProperty<Control?> BrandProperty =
        AvaloniaProperty.Register<Navbar, Control?>(nameof(Brand));

    // Shown at the end of the bar (buttons, an avatar, a theme toggle).
    public static readonly StyledProperty<Control?> ActionsProperty =
        AvaloniaProperty.Register<Navbar, Control?>(nameof(Actions));

    // The active item's Value.
    public static readonly StyledProperty<string?> SelectedValueProperty =
        AvaloniaProperty.Register<Navbar, string?>(nameof(SelectedValue), defaultBindingMode: BindingMode.TwoWay);

    // Below this width the links collapse into the menu. 0 never collapses.
    public static readonly StyledProperty<double> CollapseBelowProperty =
        AvaloniaProperty.Register<Navbar, double>(nameof(CollapseBelow), 640);

    public static readonly StyledProperty<bool> IsMenuOpenProperty =
        AvaloniaProperty.Register<Navbar, bool>(nameof(IsMenuOpen), defaultBindingMode: BindingMode.TwoWay);

    private readonly Icon _brandIcon;
    private readonly TextBlock _title;
    private readonly StackPanel _brandRow;
    private readonly Border _brand;
    private readonly StackPanel _links;
    private readonly Border _actions;
    private readonly Border _menuButton;
    private readonly Icon _menuIcon;
    private readonly StackPanel _menu;
    private bool _collapsed;

    static Navbar()
    {
        TitleProperty.Changed.AddClassHandler<Navbar>((n, _) => n.UpdateBrand());
        IconProperty.Changed.AddClassHandler<Navbar>((n, _) => n.UpdateBrand());
        BrandProperty.Changed.AddClassHandler<Navbar>((n, _) => n.UpdateBrand());
        ActionsProperty.Changed.AddClassHandler<Navbar>((n, e) => n._actions.Child = e.NewValue as Control);
        CollapseBelowProperty.Changed.AddClassHandler<Navbar>((n, _) => n.UpdateLayout(force: false));
        IsMenuOpenProperty.Changed.AddClassHandler<Navbar>((n, _) => n.UpdateLayout(force: false));
        SelectedValueProperty.Changed.AddClassHandler<Navbar>((n, _) =>
        {
            n.Refresh();
            n.SelectedValueChanged?.Invoke(n, n.SelectedValue);
        });
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IconName Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public Control? Brand
    {
        get => GetValue(BrandProperty);
        set => SetValue(BrandProperty, value);
    }

    public Control? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    public string? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public double CollapseBelow
    {
        get => GetValue(CollapseBelowProperty);
        set => SetValue(CollapseBelowProperty, value);
    }

    public bool IsMenuOpen
    {
        get => GetValue(IsMenuOpenProperty);
        set => SetValue(IsMenuOpenProperty, value);
    }

    // The NavbarItem children.
    [Content]
    public Controls Items { get; } = new();

    public event EventHandler<string?>? SelectedValueChanged;

    public Navbar()
    {
        _brandIcon = new Icon { Size = 20, Token = ShellToken.Foreground, VerticalAlignment = VerticalAlignment.Center, IsVisible = false };
        _title = new TextBlock { FontSize = 16, FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center };
        _title.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        _brandRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { _brandIcon, _title } };
        _brand = new Border { VerticalAlignment = VerticalAlignment.Center, Child = _brandRow };
        _links = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(16, 0, 0, 0) };
        Grid.SetColumn(_links, 1);
        _actions = new Border { VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(_actions, 2);

        _menuIcon = new Icon { Kind = IconName.Menu, Size = 18, Token = ShellToken.Foreground, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        _menuButton = new Border
        {
            Child = _menuIcon,
            Width = 36,
            Height = 36,
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            Background = Brushes.Transparent,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4, 0, 0, 0),
            Cursor = new Cursor(StandardCursorType.Hand),
            IsVisible = false
        };
        Grid.SetColumn(_menuButton, 3);
        ShellFocus.Ring(_menuButton, _menuButton);
        _menuButton.PointerEntered += (_, _) => _menuButton.Token(BackgroundProperty, ShellToken.Accent);
        _menuButton.PointerExited += (_, _) =>
        {
            _menuButton.ClearToken(BackgroundProperty);
            _menuButton.Background = Brushes.Transparent;
        };
        _menuButton.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;
            IsMenuOpen = !IsMenuOpen;
            e.Handled = true;
        };
        _menuButton.KeyDown += (_, e) =>
        {
            if (e.Key is not (Key.Enter or Key.Space)) return;
            IsMenuOpen = !IsMenuOpen;
            e.Handled = true;
        };

        var bar = new Grid
        {
            Height = 56,
            Margin = new Thickness(16, 0),
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto"),
            Children = { _brand, _links, _actions, _menuButton }
        };
        _menu = new StackPanel { Spacing = 2, Margin = new Thickness(12, 0, 12, 12), IsVisible = false };
        var edge = new Border { Height = 1 };
        edge.Token(BackgroundProperty, ShellToken.Border);

        Child = new StackPanel { Children = { bar, _menu, edge } };
        this.Token(BackgroundProperty, ShellToken.Background);
        Items.CollectionChanged += (_, _) => UpdateLayout(force: true);
        SizeChanged += (_, _) => UpdateLayout(force: false);
        UpdateBrand();
    }

    private List<NavbarItem> ItemList => Items.OfType<NavbarItem>().ToList();

    internal void Select(NavbarItem item)
    {
        if (!string.IsNullOrEmpty(item.Value)) SelectedValue = item.Value;
        IsMenuOpen = false;
    }

    private void UpdateBrand()
    {
        _brandIcon.Kind = Icon;
        _brandIcon.IsVisible = Icon != IconName.None;
        _title.Text = Title ?? string.Empty;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _brand.Child = Brand ?? _brandRow;
    }

    // The links live in the bar or, when collapsed, in the menu under it.
    private void UpdateLayout(bool force)
    {
        var items = ItemList;
        var width = Bounds.Width;
        var collapsed = CollapseBelow > 0 && width > 0 && width < CollapseBelow && items.Count > 0;
        if (collapsed != _collapsed || force)
        {
            _collapsed = collapsed;
            _links.Children.Clear();
            _menu.Children.Clear();
            foreach (var item in items)
            {
                if (collapsed) _menu.Children.Add(item);
                else _links.Children.Add(item);
            }
        }
        _menuButton.IsVisible = _collapsed;
        _menu.IsVisible = _collapsed && IsMenuOpen;
        _menuIcon.Kind = IsMenuOpen && _collapsed ? IconName.X : IconName.Menu;
        AutomationProperties.SetName(_menuButton, IsMenuOpen ? "Close menu" : "Open menu");
        Refresh();
    }

    // Items don't track the bar's state: the bar pushes it to them.
    private void Refresh()
    {
        foreach (var item in ItemList)
            item.Apply(isActive: !string.IsNullOrEmpty(item.Value) && item.Value == SelectedValue, inMenu: _collapsed);
    }
}

// A link in a Navbar — text-sm font-medium, muted until hovered or active. A tab stop that
// activates with Enter or Space.
public class NavbarItem : Border, IShellFocusable
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<NavbarItem, string?>(nameof(Text));

    // Identifies the item for Navbar.SelectedValue.
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<NavbarItem, string?>(nameof(Value));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<NavbarItem, IconName>(nameof(Icon));

    private readonly Icon _icon;
    private readonly TextBlock _label;
    private bool _active;

    static NavbarItem()
    {
        TextProperty.Changed.AddClassHandler<NavbarItem>((i, _) => i.Update());
        IconProperty.Changed.AddClassHandler<NavbarItem>((i, _) => i.Update());
        IsEnabledProperty.Changed.AddClassHandler<NavbarItem>((i, _) => i.Update());
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

    public event EventHandler? Clicked;

    public NavbarItem()
    {
        _icon = new Icon { Size = 16, IsVisible = false, VerticalAlignment = VerticalAlignment.Center };
        _label = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
        Child = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center, Children = { _icon, _label } };
        Height = 36;
        Padding = new Thickness(12, 0);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        HorizontalAlignment = HorizontalAlignment.Left;
        Cursor = new Cursor(StandardCursorType.Hand);
        ShellFocus.Ring(this, this);
        Update();
    }

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

    // Picks the link in code, as a click or Enter/Space would.
    public void Press()
    {
        if (!IsEffectivelyEnabled) return;
        this.FindParentOfType<Navbar>()?.Select(this);
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    internal void Apply(bool isActive, bool inMenu)
    {
        _active = isActive;
        // In the menu each link is a full-width row.
        HorizontalAlignment = inMenu ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;
        Update();
    }

    private void Update()
    {
        _label.Text = Text ?? string.Empty;
        _label.FontWeight = _active ? FontWeight.Medium : FontWeight.Normal;
        _icon.Kind = Icon;
        _icon.IsVisible = Icon != IconName.None;

        var hovered = IsPointerOver && IsEffectivelyEnabled;
        var foreground = _active || hovered ? ShellToken.Foreground : ShellToken.MutedForeground;
        _label.Token(TextBlock.ForegroundProperty, foreground);
        _icon.Token = foreground;

        if (_active) this.Token(BackgroundProperty, ShellToken.Accent);
        else if (hovered) this.Token(BackgroundProperty, ShellToken.Muted);
        else
        {
            // Transparent, not null, so the whole link stays hit-testable.
            this.ClearToken(BackgroundProperty);
            Background = Brushes.Transparent;
        }
        Opacity = IsEnabled ? 1.0 : 0.5;
    }
}
