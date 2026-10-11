using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class NavbarTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "navbar",
        DisplayName = "Navbar",
        Description = "Top navigation bar that collapses its links into a menu",
        Category = ComponentCategory.Navigation,
        FilePath = "Navbar.cs",
        Dependencies = new List<string> { "shell", "icon", "element-extensions" },
        Tags = new List<string> { "navbar", "navigation", "header", "menu" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Top navigation bar — h-14 border-b bg-background: a brand (icon and title), links, and actions
// at the end. Narrower than CollapseBelow, the links move into a menu that a button opens under
// the bar.
//   <ui:Navbar Title=""ShellUI"" Icon=""Sparkles"" SelectedValue=""{Binding Page}"">
//       <ui:NavbarItem Value=""docs"" Text=""Docs"" />
//       <ui:NavbarItem Value=""components"" Text=""Components"" />
//       <ui:Navbar.Actions>
//           <ui:Button Icon=""Search"" Size=""Icon"" Variant=""Ghost"" />
//       </ui:Navbar.Actions>
//   </ui:Navbar>
[ContentProperty(nameof(Items))]
public partial class Navbar : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(Navbar), string.Empty,
            propertyChanged: (b, o, n) => ((Navbar)b).UpdateBrand());

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(Navbar), IconName.None,
            propertyChanged: (b, o, n) => ((Navbar)b).UpdateBrand());

    // Replaces the icon and title.
    public static readonly BindableProperty BrandProperty =
        BindableProperty.Create(nameof(Brand), typeof(View), typeof(Navbar), null,
            propertyChanged: (b, o, n) => ((Navbar)b).UpdateBrand());

    // Shown at the end of the bar (buttons, an avatar, a theme toggle).
    public static readonly BindableProperty ActionsProperty =
        BindableProperty.Create(nameof(Actions), typeof(View), typeof(Navbar), null,
            propertyChanged: (b, o, n) => ((Navbar)b)._actions.Content = n as View);

    // The active item's Value.
    public static readonly BindableProperty SelectedValueProperty =
        BindableProperty.Create(nameof(SelectedValue), typeof(string), typeof(Navbar), null, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((Navbar)b).OnSelectedValueChanged());

    // Below this width the links collapse into the menu. 0 never collapses.
    public static readonly BindableProperty CollapseBelowProperty =
        BindableProperty.Create(nameof(CollapseBelow), typeof(double), typeof(Navbar), 640d,
            propertyChanged: (b, o, n) => ((Navbar)b).UpdateLayout());

    public static readonly BindableProperty IsMenuOpenProperty =
        BindableProperty.Create(nameof(IsMenuOpen), typeof(bool), typeof(Navbar), false, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((Navbar)b).UpdateLayout());

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IconName Icon
    {
        get => (IconName)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public View? Brand
    {
        get => (View?)GetValue(BrandProperty);
        set => SetValue(BrandProperty, value);
    }

    public View? Actions
    {
        get => (View?)GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    public string? SelectedValue
    {
        get => (string?)GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public double CollapseBelow
    {
        get => (double)GetValue(CollapseBelowProperty);
        set => SetValue(CollapseBelowProperty, value);
    }

    public bool IsMenuOpen
    {
        get => (bool)GetValue(IsMenuOpenProperty);
        set => SetValue(IsMenuOpenProperty, value);
    }

    public IList<NavbarItem> Items => _items;

    public event EventHandler<string?>? SelectedValueChanged;

    private readonly ObservableCollection<NavbarItem> _items = new();
    private readonly Icon _brandIcon;
    private readonly Label _title;
    private readonly ContentView _brand;
    private readonly HorizontalStackLayout _brandRow;
    private readonly HorizontalStackLayout _links;
    private readonly ContentView _actions;
    private readonly Border _menuButton;
    private readonly Icon _menuIcon;
    private readonly VerticalStackLayout _menu;
    private bool _collapsed;

    public Navbar()
    {
        _brandIcon = new Icon { Size = 20, Token = ShellToken.Foreground, VerticalOptions = LayoutOptions.Center, IsVisible = false };
        _title = new Label { FontSize = 16, FontAttributes = FontAttributes.Bold, VerticalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.NoWrap };
        _title.Token(Label.TextColorProperty, ShellToken.Foreground);
        _brandRow = new HorizontalStackLayout { Spacing = 8, Children = { _brandIcon, _title } };
        _brand = new ContentView { VerticalOptions = LayoutOptions.Center, Content = _brandRow };
        _links = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(16, 0, 0, 0) };
        _actions = new ContentView { VerticalOptions = LayoutOptions.Center };

        _menuIcon = new Icon { Name = IconName.Menu, Size = 18, Token = ShellToken.Foreground };
        _menuButton = new Border
        {
            Content = _menuIcon,
            WidthRequest = 36,
            HeightRequest = 36,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(4, 0, 0, 0),
            IsVisible = false
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => _menuButton.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        pointer.PointerExited += (_, _) => { _menuButton.ClearValue(VisualElement.BackgroundColorProperty); _menuButton.BackgroundColor = Colors.Transparent; };
        _menuButton.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => IsMenuOpen = !IsMenuOpen;
        _menuButton.GestureRecognizers.Add(tap);

        var bar = new Grid
        {
            HeightRequest = 56,
            Padding = new Thickness(16, 0),
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto)
            }
        };
        bar.Add(_brand, 0, 0);
        bar.Add(_links, 1, 0);
        bar.Add(_actions, 2, 0);
        bar.Add(_menuButton, 3, 0);

        _menu = new VerticalStackLayout { Spacing = 2, Padding = new Thickness(12, 0, 12, 12), IsVisible = false };
        var edge = new BoxView { HeightRequest = 1, BackgroundColor = Colors.Transparent };
        edge.Token(BoxView.ColorProperty, ShellToken.Border);

        var root = new VerticalStackLayout { Spacing = 0, Children = { bar, _menu, edge } };
        root.Token(VisualElement.BackgroundColorProperty, ShellToken.Background);
        Content = root;

        _items.CollectionChanged += (_, _) => UpdateLayout(force: true);
        SizeChanged += (_, _) => UpdateLayout();
        UpdateBrand();
    }

    internal void Select(NavbarItem item)
    {
        if (!string.IsNullOrEmpty(item.Value)) SelectedValue = item.Value;
        IsMenuOpen = false;
    }

    private void OnSelectedValueChanged()
    {
        Refresh();
        SelectedValueChanged?.Invoke(this, SelectedValue);
    }

    private void UpdateBrand()
    {
        _brandIcon.Name = Icon;
        _brandIcon.IsVisible = Icon != IconName.None;
        _title.Text = Title ?? string.Empty;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _brand.Content = Brand ?? _brandRow;
    }

    private void UpdateLayout() => UpdateLayout(force: false);

    // The links live in the bar or, when collapsed, in the menu under it.
    private void UpdateLayout(bool force)
    {
        var collapsed = CollapseBelow > 0 && Width > 0 && Width < CollapseBelow && _items.Count > 0;
        if (collapsed != _collapsed || force)
        {
            _collapsed = collapsed;
            _links.Children.Clear();
            _menu.Children.Clear();
            foreach (var item in _items)
            {
                if (collapsed) _menu.Children.Add(item);
                else _links.Children.Add(item);
            }
        }
        _menuButton.IsVisible = _collapsed;
        _menu.IsVisible = _collapsed && IsMenuOpen;
        _menuIcon.Name = IsMenuOpen && _collapsed ? IconName.X : IconName.Menu;
        SemanticProperties.SetDescription(_menuButton, IsMenuOpen ? ""Close menu"" : ""Open menu"");
        Refresh();
    }

    // Items don't track the bar's state: the bar pushes it to them.
    private void Refresh()
    {
        foreach (var item in _items)
            item.Apply(isActive: !string.IsNullOrEmpty(item.Value) && item.Value == SelectedValue, inMenu: _collapsed);
    }
}

// A link in a Navbar — text-sm font-medium, muted until hovered or active.
public partial class NavbarItem : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(NavbarItem), string.Empty,
            propertyChanged: (b, o, n) => ((NavbarItem)b).Update());

    // Identifies the item for Navbar.SelectedValue.
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(NavbarItem), null);

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(NavbarItem), IconName.None,
            propertyChanged: (b, o, n) => ((NavbarItem)b).Update());

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string? Value
    {
        get => (string?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IconName Icon
    {
        get => (IconName)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public event EventHandler? Clicked;

    private readonly Border _border;
    private readonly Icon _icon;
    private readonly Label _label;
    private bool _active;
    private bool _hovered;

    public NavbarItem()
    {
        _icon = new Icon { Size = 16, IsVisible = false, VerticalOptions = LayoutOptions.Center };
        _label = new Label { FontSize = 14, VerticalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center, LineBreakMode = LineBreakMode.NoWrap };
        _border = new Border
        {
            Content = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center, Children = { _icon, _label } },
            HeightRequest = 36,
            Padding = new Thickness(12, 0),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { _hovered = true; Update(); };
        pointer.PointerExited += (_, _) => { _hovered = false; Update(); };
        _border.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { ShellFocus.FocusPressed(this); Press(); };
        _border.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, Press);

        Content = _border;
        Update();
    }

    private void Press()
    {
        if (!IsEnabled) return;
        this.FindParentOfType<Navbar>()?.Select(this);
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    internal void Apply(bool isActive, bool inMenu)
    {
        _active = isActive;
        // In the menu each link is a full-width row.
        HorizontalOptions = inMenu ? LayoutOptions.Fill : LayoutOptions.Start;
        Update();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName)
            Opacity = IsEnabled ? 1.0 : 0.5;
    }

    private void Update()
    {
        _label.Text = Text ?? string.Empty;
        _label.FontAttributes = _active ? FontAttributes.Bold : FontAttributes.None;
        _icon.Name = Icon;
        _icon.IsVisible = Icon != IconName.None;

        var foreground = _active || _hovered ? ShellToken.Foreground : ShellToken.MutedForeground;
        _label.Token(Label.TextColorProperty, foreground);
        _icon.Token = foreground;

        if (_active)
            _border.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        else if (_hovered && IsEnabled)
            _border.Token(VisualElement.BackgroundColorProperty, ShellToken.Muted);
        else
        {
            _border.ClearValue(VisualElement.BackgroundColorProperty);
            _border.BackgroundColor = Colors.Transparent;
        }
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
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

namespace YourProjectNamespace.Components.UI;

// Top navigation bar — h-14 border-b bg-background: a brand (icon and title), links, and actions
// at the end. Narrower than CollapseBelow, the links move into a menu that a button opens under
// the bar.
//   <ui:Navbar Title=""ShellUI"" Icon=""Sparkles"" SelectedValue=""{Binding Page}"">
//       <ui:NavbarItem Value=""docs"" Text=""Docs"" />
//       <ui:NavbarItem Value=""components"" Text=""Components"" />
//       <ui:Navbar.Actions>
//           <ui:Button Icon=""Search"" Size=""Icon"" Variant=""Ghost"" />
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
            ColumnDefinitions = new ColumnDefinitions(""Auto,*,Auto,Auto""),
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
        AutomationProperties.SetName(_menuButton, IsMenuOpen ? ""Close menu"" : ""Open menu"");
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
"
    };
}
