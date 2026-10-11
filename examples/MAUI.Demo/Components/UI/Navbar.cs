using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

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
        SemanticProperties.SetDescription(_menuButton, IsMenuOpen ? "Close menu" : "Open menu");
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
