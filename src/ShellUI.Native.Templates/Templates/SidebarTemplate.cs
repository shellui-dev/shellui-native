using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class SidebarTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "sidebar",
        DisplayName = "Sidebar",
        Description = "App sidebar with groups and items that collapses to an icon rail",
        Category = ComponentCategory.Navigation,
        FilePath = "Sidebar.cs",
        Dependencies = new List<string> { "shell", "icon", "element-extensions" },
        Tags = new List<string> { "sidebar", "navigation", "menu", "layout" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// App sidebar — a w-64 column (bg-card, border on its inner edge) with a header, scrolling
// groups of items and a footer. Collapsed, it is a w-14 rail that shows the icons only. Put it in
// a column next to your content, or inside a Sheet on phones.
//   <ui:Sidebar x:Name=""AppSidebar"" SelectedValue=""{Binding Page}"">
//       <ui:Sidebar.Header> ...any view... </ui:Sidebar.Header>
//       <ui:SidebarGroup Label=""Platform"">
//           <ui:SidebarItem Value=""home"" Text=""Home"" Icon=""House"" />
//           <ui:SidebarItem Value=""inbox"" Text=""Inbox"" Icon=""Inbox"" Badge=""12"" />
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
    // User input only: setting SelectedValue from code or a binding doesn't raise it.
    public event EventHandler<string?>? SelectedValueChanged;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    private const string WidthAnimation = ""SidebarWidth"";

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

    // A click or key on an item.
    internal void SelectByUser(string? value) => ByUser(() => SelectedValue = value);

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    private void OnCollapsedChanged()
    {
        Refresh();
        ApplyWidth(animate: true);
        IsCollapsedChanged?.Invoke(this, IsCollapsed);
    }

    private void OnSelectedValueChanged()
    {
        Refresh();
        if (_byUser) SelectedValueChanged?.Invoke(this, SelectedValue);
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
        if (!string.IsNullOrEmpty(Value) && this.FindParentOfType<Sidebar>() is { } sidebar) sidebar.SelectByUser(Value);
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
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace YourProjectNamespace.Components.UI;

// App sidebar — a w-64 column (bg-card, border on its inner edge) with a header, scrolling
// groups of items and a footer. Collapsed, it is a w-14 rail that shows the icons only. Put it in
// a column next to your content.
//   <ui:Sidebar x:Name=""AppSidebar"" SelectedValue=""{Binding Page}"">
//       <ui:Sidebar.Header> ...any control... </ui:Sidebar.Header>
//       <ui:SidebarGroup Label=""Platform"">
//           <ui:SidebarItem Value=""home"" Text=""Home"" Icon=""House"" />
//           <ui:SidebarItem Value=""inbox"" Text=""Inbox"" Icon=""Inbox"" Badge=""12"" />
//       </ui:SidebarGroup>
//       <ui:Sidebar.Footer> ... </ui:Sidebar.Footer>
//   </ui:Sidebar>
// Collapse it with IsCollapsed or Toggle().
public class Sidebar : Border
{
    public static readonly StyledProperty<bool> IsCollapsedProperty =
        AvaloniaProperty.Register<Sidebar, bool>(nameof(IsCollapsed), defaultBindingMode: BindingMode.TwoWay);

    // The active item's Value.
    public static readonly StyledProperty<string?> SelectedValueProperty =
        AvaloniaProperty.Register<Sidebar, string?>(nameof(SelectedValue), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> ExpandedWidthProperty =
        AvaloniaProperty.Register<Sidebar, double>(nameof(ExpandedWidth), 256);

    public static readonly StyledProperty<double> CollapsedWidthProperty =
        AvaloniaProperty.Register<Sidebar, double>(nameof(CollapsedWidth), 56);

    // Which edge of the window it sits on; the border is drawn on the other edge.
    public static readonly StyledProperty<SidebarSide> SideProperty =
        AvaloniaProperty.Register<Sidebar, SidebarSide>(nameof(Side));

    public static readonly StyledProperty<Control?> HeaderProperty =
        AvaloniaProperty.Register<Sidebar, Control?>(nameof(Header));

    public static readonly StyledProperty<Control?> FooterProperty =
        AvaloniaProperty.Register<Sidebar, Control?>(nameof(Footer));

    private readonly Border _header = new() { Padding = new Thickness(8), IsVisible = false };
    private readonly Border _footer = new() { Padding = new Thickness(8), IsVisible = false };
    private readonly StackPanel _groups = new() { Spacing = 8, Margin = new Thickness(8) };
    private readonly Border _edge = new() { Width = 1 };

    static Sidebar()
    {
        IsCollapsedProperty.Changed.AddClassHandler<Sidebar>((s, _) =>
        {
            s.Refresh();
            s.Width = s.IsCollapsed ? s.CollapsedWidth : s.ExpandedWidth;
            s.IsCollapsedChanged?.Invoke(s, s.IsCollapsed);
        });
        SelectedValueProperty.Changed.AddClassHandler<Sidebar>((s, _) =>
        {
            s.Refresh();
            if (s._byUser) s.SelectedValueChanged?.Invoke(s, s.SelectedValue);
        });
        ExpandedWidthProperty.Changed.AddClassHandler<Sidebar>((s, _) => s.SnapWidth());
        CollapsedWidthProperty.Changed.AddClassHandler<Sidebar>((s, _) => s.SnapWidth());
        SideProperty.Changed.AddClassHandler<Sidebar>((s, _) => s.UpdateEdge());
        HeaderProperty.Changed.AddClassHandler<Sidebar>((s, _) => s.UpdateSlots());
        FooterProperty.Changed.AddClassHandler<Sidebar>((s, _) => s.UpdateSlots());
    }

    public bool IsCollapsed
    {
        get => GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    public string? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public double ExpandedWidth
    {
        get => GetValue(ExpandedWidthProperty);
        set => SetValue(ExpandedWidthProperty, value);
    }

    public double CollapsedWidth
    {
        get => GetValue(CollapsedWidthProperty);
        set => SetValue(CollapsedWidthProperty, value);
    }

    public SidebarSide Side
    {
        get => GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    public Control? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public Control? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    // The SidebarGroup children.
    [Content]
    public Controls Groups => _groups.Children;

    public event EventHandler<bool>? IsCollapsedChanged;
    // User input only: setting SelectedValue from code or a binding doesn't raise it.
    public event EventHandler<string?>? SelectedValueChanged;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    public Sidebar()
    {
        _groups.Children.CollectionChanged += (_, _) => Refresh();
        _edge.Token(BackgroundProperty, ShellToken.Border);
        var scroll = new ScrollViewer { Content = _groups, VerticalScrollBarVisibility = ScrollBarVisibility.Hidden, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        Grid.SetRow(scroll, 1);
        Grid.SetRow(_footer, 2);

        Child = new Panel
        {
            Children =
            {
                new Grid { RowDefinitions = new RowDefinitions(""Auto,*,Auto""), Children = { _header, scroll, _footer } },
                _edge
            }
        };
        ClipToBounds = true;
        HorizontalAlignment = HorizontalAlignment.Left;
        Width = ExpandedWidth;
        this.Token(BackgroundProperty, ShellToken.Card);
        UpdateEdge();
    }

    public void Toggle() => IsCollapsed = !IsCollapsed;

    // A click or key on an item.
    internal void SelectByUser(string? value) => ByUser(() => SelectedValue = value);

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    // The width animates only once the sidebar is on screen, so it doesn't slide in at startup.
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Refresh();
        Transitions = new Transitions
        {
            new DoubleTransition { Property = WidthProperty, Duration = TimeSpan.FromMilliseconds(200), Easing = new CubicEaseOut() }
        };
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Transitions = null;
    }

    private void SnapWidth()
    {
        var transitions = Transitions;
        Transitions = null;
        Width = IsCollapsed ? CollapsedWidth : ExpandedWidth;
        Transitions = transitions;
    }

    private void UpdateEdge() =>
        _edge.HorizontalAlignment = Side == SidebarSide.Left ? HorizontalAlignment.Right : HorizontalAlignment.Left;

    private void UpdateSlots()
    {
        _header.Child = Header;
        _header.IsVisible = Header != null;
        _footer.Child = Footer;
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
public class SidebarGroup : Border
{
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<SidebarGroup, string?>(nameof(Label));

    private readonly TextBlock _label;
    private readonly StackPanel _items = new() { Spacing = 2 };
    private bool _collapsed;

    static SidebarGroup()
    {
        LabelProperty.Changed.AddClassHandler<SidebarGroup>((g, _) => g.Update());
    }

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    [Content]
    public Controls Items => _items.Children;

    public SidebarGroup()
    {
        _label = new TextBlock { FontSize = 12, FontWeight = FontWeight.Medium, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        _label.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        var header = new Border { Child = _label, Height = 28, Padding = new Thickness(8, 0) };
        Child = new StackPanel { Children = { header, _items } };
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
        if (_label.Parent is Control header) header.IsVisible = !_collapsed && !string.IsNullOrEmpty(Label);
    }
}

// One row of a Sidebar — h-9 rounded-md px-2 text-sm: icon, text and an optional badge;
// hover:bg-accent, and bg-accent font-medium when active. Collapsed, only the icon shows. A tab
// stop that activates with Enter or Space.
public class SidebarItem : Border, IShellFocusable
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<SidebarItem, string?>(nameof(Text));

    // Identifies the item for Sidebar.SelectedValue.
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<SidebarItem, string?>(nameof(Value));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<SidebarItem, IconName>(nameof(Icon));

    // Short text at the end of the row, e.g. a count.
    public static readonly StyledProperty<string?> BadgeProperty =
        AvaloniaProperty.Register<SidebarItem, string?>(nameof(Badge));

    private readonly Grid _grid;
    private readonly Icon _icon;
    private readonly TextBlock _label;
    private readonly TextBlock _badge;
    private bool _active;
    private bool _collapsed;

    static SidebarItem()
    {
        TextProperty.Changed.AddClassHandler<SidebarItem>((i, _) => i.Update());
        IconProperty.Changed.AddClassHandler<SidebarItem>((i, _) => i.Update());
        BadgeProperty.Changed.AddClassHandler<SidebarItem>((i, _) => i.Update());
        IsEnabledProperty.Changed.AddClassHandler<SidebarItem>((i, _) => i.Update());
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

    public string? Badge
    {
        get => GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }

    public event EventHandler? Clicked;

    public SidebarItem()
    {
        _icon = new Icon { Size = 16, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        _label = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        Grid.SetColumn(_label, 1);
        _badge = new TextBlock { FontSize = 12, VerticalAlignment = VerticalAlignment.Center, IsVisible = false };
        _badge.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        Grid.SetColumn(_badge, 2);

        _grid = new Grid { ColumnDefinitions = new ColumnDefinitions(""Auto,*,Auto""), ColumnSpacing = 8, Children = { _icon, _label, _badge } };
        Child = _grid;
        Height = 36;
        Padding = new Thickness(8, 0);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
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

    // Picks the item in code, as a click or Enter/Space would.
    public void Press()
    {
        if (!IsEffectivelyEnabled) return;
        if (!string.IsNullOrEmpty(Value) && this.FindParentOfType<Sidebar>() is { } sidebar) sidebar.SelectByUser(Value);
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    internal void Apply(bool isActive, bool collapsed)
    {
        _active = isActive;
        _collapsed = collapsed;
        Update();
    }

    private void Update()
    {
        _label.Text = Text ?? string.Empty;
        _label.FontWeight = _active ? FontWeight.Medium : FontWeight.Normal;
        _label.IsVisible = !_collapsed;
        _icon.Kind = Icon;
        _icon.IsVisible = Icon != IconName.None;
        _badge.Text = Badge ?? string.Empty;
        _badge.IsVisible = !_collapsed && !string.IsNullOrEmpty(Badge);
        // Collapsed, the icon column takes the row so the icon sits in the middle of the rail.
        _grid.ColumnDefinitions[0].Width = _collapsed ? GridLength.Star : GridLength.Auto;
        _grid.ColumnDefinitions[1].Width = _collapsed ? new GridLength(0) : GridLength.Star;
        _grid.ColumnSpacing = _collapsed ? 0 : 8;
        AutomationProperties.SetName(this, Text ?? string.Empty);

        var foreground = _active ? ShellToken.AccentForeground : ShellToken.Foreground;
        _label.Token(TextBlock.ForegroundProperty, foreground);
        _icon.Token = foreground;

        if (_active || (IsPointerOver && IsEffectivelyEnabled)) this.Token(BackgroundProperty, ShellToken.Accent);
        else
        {
            // Transparent, not null, so the whole row stays hit-testable.
            this.ClearToken(BackgroundProperty);
            Background = Brushes.Transparent;
        }
        Opacity = IsEnabled ? 1.0 : 0.5;
    }
}

public enum SidebarSide { Left, Right }
"
    };
}
