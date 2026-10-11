using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// App sidebar — a w-64 column (bg-card, border on its inner edge) with a header, scrolling
// groups of items and a footer. Collapsed, it is a w-14 rail that shows the icons only. Put it in
// a column next to your content, or inside a Sheet on phones.
//   <ui:Sidebar x:Name="AppSidebar" SelectedValue="{Binding Page}">
//       <ui:Sidebar.Header> ...any view... </ui:Sidebar.Header>
//       <ui:SidebarGroup Label="Platform">
//           <ui:SidebarItem Value="home" Text="Home" Icon="House" />
//           <ui:SidebarItem Value="inbox" Text="Inbox" Icon="Inbox" Badge="12" />
//       </ui:SidebarGroup>
//       <ui:Sidebar.Footer> ... </ui:Sidebar.Footer>
//   </ui:Sidebar>
// Collapse it with IsCollapsed or Toggle().
[ContentProperty(nameof(Groups))]
public partial class Sidebar : ContentView
{
    public static readonly BindableProperty IsCollapsedProperty =
        BindableProperty.Create(nameof(IsCollapsed), typeof(bool), typeof(Sidebar), false, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((Sidebar)b).OnCollapsedChanged());

    // The active item's Value.
    public static readonly BindableProperty SelectedValueProperty =
        BindableProperty.Create(nameof(SelectedValue), typeof(string), typeof(Sidebar), null, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((Sidebar)b).OnSelectedValueChanged());

    public static readonly BindableProperty ExpandedWidthProperty =
        BindableProperty.Create(nameof(ExpandedWidth), typeof(double), typeof(Sidebar), 256d,
            propertyChanged: (b, o, n) => ((Sidebar)b).ApplyWidth(animate: false));

    public static readonly BindableProperty CollapsedWidthProperty =
        BindableProperty.Create(nameof(CollapsedWidth), typeof(double), typeof(Sidebar), 56d,
            propertyChanged: (b, o, n) => ((Sidebar)b).ApplyWidth(animate: false));

    // Which edge of the window it sits on; the border is drawn on the other edge.
    public static readonly BindableProperty SideProperty =
        BindableProperty.Create(nameof(Side), typeof(SidebarSide), typeof(Sidebar), SidebarSide.Left,
            propertyChanged: (b, o, n) => ((Sidebar)b).UpdateEdge());

    public static readonly BindableProperty HeaderProperty =
        BindableProperty.Create(nameof(Header), typeof(View), typeof(Sidebar), null,
            propertyChanged: (b, o, n) => ((Sidebar)b).UpdateSlots());

    public static readonly BindableProperty FooterProperty =
        BindableProperty.Create(nameof(Footer), typeof(View), typeof(Sidebar), null,
            propertyChanged: (b, o, n) => ((Sidebar)b).UpdateSlots());

    public bool IsCollapsed
    {
        get => (bool)GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    public string? SelectedValue
    {
        get => (string?)GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public double ExpandedWidth
    {
        get => (double)GetValue(ExpandedWidthProperty);
        set => SetValue(ExpandedWidthProperty, value);
    }

    public double CollapsedWidth
    {
        get => (double)GetValue(CollapsedWidthProperty);
        set => SetValue(CollapsedWidthProperty, value);
    }

    public SidebarSide Side
    {
        get => (SidebarSide)GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    public View? Header
    {
        get => (View?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public View? Footer
    {
        get => (View?)GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public IList<IView> Groups => _groups.Children;

    public event EventHandler<bool>? IsCollapsedChanged;
    public event EventHandler<string?>? SelectedValueChanged;

    private const string WidthAnimation = "SidebarWidth";

    private readonly ContentView _header;
    private readonly ContentView _footer;
    private readonly VerticalStackLayout _groups;
    private readonly BoxView _edge;

    public Sidebar()
    {
        _header = new ContentView { Padding = new Thickness(8), IsVisible = false };
        _footer = new ContentView { Padding = new Thickness(8), IsVisible = false };
        _groups = new VerticalStackLayout { Spacing = 8, Padding = new Thickness(8) };
        _groups.ChildAdded += (_, _) => Refresh();
        _edge = new BoxView { WidthRequest = 1, BackgroundColor = Colors.Transparent };
        _edge.Token(BoxView.ColorProperty, ShellToken.Border);

        var column = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) }
        };
        column.Add(_header, 0, 0);
        column.Add(new ScrollView { Content = _groups, VerticalScrollBarVisibility = ScrollBarVisibility.Never }, 0, 1);
        column.Add(_footer, 0, 2);

        var root = new Grid { IsClippedToBounds = true, Children = { column, _edge } };
        root.Token(VisualElement.BackgroundColorProperty, ShellToken.Card);
        Content = root;
        WidthRequest = ExpandedWidth;
        HorizontalOptions = LayoutOptions.Start;
        UpdateEdge();
        Loaded += (_, _) => Refresh();
    }

    public void Toggle() => IsCollapsed = !IsCollapsed;

    private void OnCollapsedChanged()
    {
        Refresh();
        ApplyWidth(animate: true);
        IsCollapsedChanged?.Invoke(this, IsCollapsed);
    }

    private void OnSelectedValueChanged()
    {
        Refresh();
        SelectedValueChanged?.Invoke(this, SelectedValue);
    }

    private void ApplyWidth(bool animate)
    {
        var target = IsCollapsed ? CollapsedWidth : ExpandedWidth;
        this.AbortAnimation(WidthAnimation);
        if (!animate || Handler is null || Width <= 0)
        {
            WidthRequest = target;
            return;
        }
        new Animation(v => WidthRequest = v, Width, target)
            .Commit(this, WidthAnimation, 16, 200, Easing.CubicOut);
    }

    private void UpdateEdge() =>
        _edge.HorizontalOptions = Side == SidebarSide.Left ? LayoutOptions.End : LayoutOptions.Start;

    private void UpdateSlots()
    {
        _header.Content = Header;
        _header.IsVisible = Header != null;
        _footer.Content = Footer;
        _footer.IsVisible = Footer != null;
    }

    // Groups and items don't track the sidebar's state: the sidebar pushes it to them.
    private void Refresh()
    {
        foreach (var group in this.FindDescendantsOfType<SidebarGroup>())
            group.Apply(IsCollapsed);
        foreach (var item in this.FindDescendantsOfType<SidebarItem>())
            item.Apply(isActive: !string.IsNullOrEmpty(item.Value) && item.Value == SelectedValue, collapsed: IsCollapsed);
    }
}

// A labelled group of SidebarItems — the label is text-xs font-medium in the muted foreground.
[ContentProperty(nameof(Items))]
public partial class SidebarGroup : ContentView
{
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(SidebarGroup), string.Empty,
            propertyChanged: (b, o, n) => ((SidebarGroup)b).Update());

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public IList<IView> Items => _items.Children;

    private readonly Microsoft.Maui.Controls.Label _label;
    private readonly VerticalStackLayout _items;
    private bool _collapsed;

    public SidebarGroup()
    {
        _label = new Microsoft.Maui.Controls.Label
        {
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            HeightRequest = 28,
            Padding = new Thickness(8, 0),
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        };
        _label.Token(Microsoft.Maui.Controls.Label.TextColorProperty, ShellToken.MutedForeground);
        _items = new VerticalStackLayout { Spacing = 2 };
        Content = new VerticalStackLayout { Spacing = 0, Children = { _label, _items } };
        Update();
    }

    internal void Apply(bool collapsed)
    {
        _collapsed = collapsed;
        Update();
    }

    private void Update()
    {
        _label.Text = Label ?? string.Empty;
        _label.IsVisible = !_collapsed && !string.IsNullOrEmpty(Label);
    }
}

// One row of a Sidebar — h-9 rounded-md px-2 text-sm: icon, text and an optional badge;
// hover:bg-accent, and bg-accent font-medium when active. Collapsed, only the icon shows.
public partial class SidebarItem : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(SidebarItem), string.Empty,
            propertyChanged: (b, o, n) => ((SidebarItem)b).Update());

    // Identifies the item for Sidebar.SelectedValue.
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(SidebarItem), null);

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(SidebarItem), IconName.None,
            propertyChanged: (b, o, n) => ((SidebarItem)b).Update());

    // Short text at the end of the row, e.g. a count.
    public static readonly BindableProperty BadgeProperty =
        BindableProperty.Create(nameof(Badge), typeof(string), typeof(SidebarItem), string.Empty,
            propertyChanged: (b, o, n) => ((SidebarItem)b).Update());

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

    public string Badge
    {
        get => (string)GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }

    public event EventHandler? Clicked;

    private readonly Border _row;
    private readonly Grid _grid;
    private readonly Icon _icon;
    private readonly Label _label;
    private readonly Label _badge;
    private bool _active;
    private bool _collapsed;
    private bool _hovered;

    public SidebarItem()
    {
        _icon = new Icon { Size = 16, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
        _label = new Label { FontSize = 14, VerticalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center, LineBreakMode = LineBreakMode.TailTruncation };
        _badge = new Label { FontSize = 12, VerticalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center, IsVisible = false };
        _badge.Token(Label.TextColorProperty, ShellToken.MutedForeground);

        _grid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)
            }
        };
        _grid.Add(_icon, 0, 0);
        _grid.Add(_label, 1, 0);
        _grid.Add(_badge, 2, 0);

        _row = new Border
        {
            Content = _grid,
            HeightRequest = 36,
            Padding = new Thickness(8, 0),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { _hovered = true; Update(); };
        pointer.PointerExited += (_, _) => { _hovered = false; Update(); };
        _row.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { ShellFocus.FocusPressed(this); Press(); };
        _row.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, Press);

        Content = _row;
        Update();
    }

    private void Press()
    {
        if (!IsEnabled) return;
        if (!string.IsNullOrEmpty(Value) && this.FindParentOfType<Sidebar>() is { } sidebar) sidebar.SelectedValue = Value;
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    internal void Apply(bool isActive, bool collapsed)
    {
        _active = isActive;
        _collapsed = collapsed;
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
        _label.IsVisible = !_collapsed;
        _icon.Name = Icon;
        _icon.IsVisible = Icon != IconName.None;
        _badge.Text = Badge ?? string.Empty;
        _badge.IsVisible = !_collapsed && !string.IsNullOrEmpty(Badge);
        // Collapsed, the icon column takes the row so the icon sits in the middle of the rail.
        _grid.ColumnDefinitions[0].Width = _collapsed ? GridLength.Star : GridLength.Auto;
        _grid.ColumnDefinitions[1].Width = _collapsed ? new GridLength(0) : GridLength.Star;
        _grid.ColumnSpacing = _collapsed ? 0 : 8;
        SemanticProperties.SetDescription(this, Text ?? string.Empty);

        var foreground = _active ? ShellToken.AccentForeground : ShellToken.Foreground;
        _label.Token(Label.TextColorProperty, foreground);
        _icon.Token = foreground;

        if (_active || (_hovered && IsEnabled))
            _row.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        else
        {
            _row.ClearValue(VisualElement.BackgroundColorProperty);
            _row.BackgroundColor = Colors.Transparent;
        }
    }
}

public enum SidebarSide { Left, Right }
