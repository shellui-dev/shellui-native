using System;
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

namespace AvaloniaDemo.Components.UI;

// App sidebar — a w-64 column (bg-card, border on its inner edge) with a header, scrolling
// groups of items and a footer. Collapsed, it is a w-14 rail that shows the icons only. Put it in
// a column next to your content.
//   <ui:Sidebar x:Name="AppSidebar" SelectedValue="{Binding Page}">
//       <ui:Sidebar.Header> ...any control... </ui:Sidebar.Header>
//       <ui:SidebarGroup Label="Platform">
//           <ui:SidebarItem Value="home" Text="Home" Icon="House" />
//           <ui:SidebarItem Value="inbox" Text="Inbox" Icon="Inbox" Badge="12" />
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
            s.SelectedValueChanged?.Invoke(s, s.SelectedValue);
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
    public event EventHandler<string?>? SelectedValueChanged;

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
                new Grid { RowDefinitions = new RowDefinitions("Auto,*,Auto"), Children = { _header, scroll, _footer } },
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

        _grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"), ColumnSpacing = 8, Children = { _icon, _label, _badge } };
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
        if (!string.IsNullOrEmpty(Value) && this.FindParentOfType<Sidebar>() is { } sidebar) sidebar.SelectedValue = Value;
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
