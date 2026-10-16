using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Mobile bottom navigation — a row of icon tabs pinned to the bottom of the screen, the phone
// take on a sidebar. Place it at the bottom of the page; on a phone the bar's color runs under
// the home indicator by itself (BottomInset). With ScrollSource set, scrolling down slides the
// bar away and scrolling up brings it back; put the bar over the content (same cell, aligned to
// the bottom) so the content can use the space.
//   <ui:BottomNav SelectedValue="{Binding Page}" Variant="Floating" Labels="Active">
//       <ui:BottomNavItem Value="home" Icon="House" Text="Home" />
//       <ui:BottomNavItem Value="inbox" Icon="Inbox" Text="Inbox" Badge="3" />
//       <ui:BottomNavItem Icon="Plus" Text="New" Prominent="True" Clicked="OnNew" />
//       <ui:BottomNavItem Value="me" Text="Me" Dot="True">
//           <ui:BottomNavItem.IconContent><ui:Avatar Size="Sm" Fallback="SS" /></ui:BottomNavItem.IconContent>
//       </ui:BottomNavItem>
//   </ui:BottomNav>
// Default: h-16 border-t bg-background; a line in ActiveToken slides to the active item.
// Pill: h-20; a bg-secondary pill grows behind the active icon (Material 3).
// Floating: an inset rounded-full bar with a shadow; a bg-primary pill slides to the active icon.
// Lifted: the active icon rises into a ringed bg-primary circle above the bar, and the circle
// slides from item to item.
// Expanding: a compact floating capsule of icons; the active one widens into a pill with its label.
public class BottomNav : Border
{
    // The active item's Value.
    public static readonly StyledProperty<string?> SelectedValueProperty =
        AvaloniaProperty.Register<BottomNav, string?>(nameof(SelectedValue), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<BottomNavVariant> VariantProperty =
        AvaloniaProperty.Register<BottomNav, BottomNavVariant>(nameof(Variant));

    // Which items show their label. Expanding always labels the active item only.
    public static readonly StyledProperty<BottomNavLabels> LabelsProperty =
        AvaloniaProperty.Register<BottomNav, BottomNavLabels>(nameof(Labels));

    // The active item's color in the Default variant (e.g. Primary for a brand-colored bar).
    public static readonly StyledProperty<ShellToken> ActiveTokenProperty =
        AvaloniaProperty.Register<BottomNav, ShellToken>(nameof(ActiveToken), ShellToken.Foreground);

    // The sliding line (Default) or pill (Floating). Without it the active item shows by color alone.
    public static readonly StyledProperty<bool> ShowIndicatorProperty =
        AvaloniaProperty.Register<BottomNav, bool>(nameof(ShowIndicator), true);

    // Space under the bar for the home indicator: the full-width bars fill it with their color,
    // the floating ones sit above it. NaN (the default) follows the platform's safe area where the
    // window draws edge to edge, and is 0 elsewhere (desktop).
    public static readonly StyledProperty<double> BottomInsetProperty =
        AvaloniaProperty.Register<BottomNav, double>(nameof(BottomInset), double.NaN);

    // A scroll viewer to follow: scrolling it down hides the bar, scrolling up shows it.
    public static readonly StyledProperty<ScrollViewer?> ScrollSourceProperty =
        AvaloniaProperty.Register<BottomNav, ScrollViewer?>(nameof(ScrollSource));

    // Slid out of view (by ScrollSource, or from code).
    public static readonly StyledProperty<bool> IsHiddenProperty =
        AvaloniaProperty.Register<BottomNav, bool>(nameof(IsHidden), defaultBindingMode: BindingMode.TwoWay);

    internal const int Duration = 200;
    internal static readonly Easing Ease = new CubicEaseOut();

    // How far a prominent item's circle rises above a full-width bar.
    internal const double Raise = 20;

    private readonly Grid _row = new();
    private readonly Border _indicator = new() { IsHitTestVisible = false, Opacity = 0, HorizontalAlignment = HorizontalAlignment.Left };
    private readonly Panel _layer;
    private readonly Border _bar = new();
    private readonly Panel _root = new();
    private bool _placed;
    private double _safeArea;
    private IInsetsManager? _insets;
    private double _scrolled;

    static BottomNav()
    {
        SelectedValueProperty.Changed.AddClassHandler<BottomNav>((n, _) =>
        {
            n.Refresh(animate: n.IsLoaded);
            if (n._byUser) n.SelectedValueChanged?.Invoke(n, n.SelectedValue);
        });
        VariantProperty.Changed.AddClassHandler<BottomNav>((n, _) => n.Rebuild());
        LabelsProperty.Changed.AddClassHandler<BottomNav>((n, _) => n.Refresh(animate: false));
        ActiveTokenProperty.Changed.AddClassHandler<BottomNav>((n, _) => n.Rebuild());
        ShowIndicatorProperty.Changed.AddClassHandler<BottomNav>((n, _) => n.Refresh(animate: false));
        BottomInsetProperty.Changed.AddClassHandler<BottomNav>((n, _) => n.Rebuild());
        ScrollSourceProperty.Changed.AddClassHandler<BottomNav>((n, e) => n.Follow(e.OldValue as ScrollViewer, e.NewValue as ScrollViewer));
        IsHiddenProperty.Changed.AddClassHandler<BottomNav>((n, _) => n.Slide(animate: n.IsLoaded));
    }

    public string? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public BottomNavVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public BottomNavLabels Labels
    {
        get => GetValue(LabelsProperty);
        set => SetValue(LabelsProperty, value);
    }

    public ShellToken ActiveToken
    {
        get => GetValue(ActiveTokenProperty);
        set => SetValue(ActiveTokenProperty, value);
    }

    public bool ShowIndicator
    {
        get => GetValue(ShowIndicatorProperty);
        set => SetValue(ShowIndicatorProperty, value);
    }

    public double BottomInset
    {
        get => GetValue(BottomInsetProperty);
        set => SetValue(BottomInsetProperty, value);
    }

    public ScrollViewer? ScrollSource
    {
        get => GetValue(ScrollSourceProperty);
        set => SetValue(ScrollSourceProperty, value);
    }

    public bool IsHidden
    {
        get => GetValue(IsHiddenProperty);
        set => SetValue(IsHiddenProperty, value);
    }

    // The BottomNavItem children.
    [Content]
    public Controls Items { get; } = new();

    // User input only: setting SelectedValue from code or a binding doesn't raise it.
    public event EventHandler<string?>? SelectedValueChanged;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    public BottomNav()
    {
        _layer = new Panel { Children = { _indicator, _row } };
        _bar.Token(BorderBrushProperty, ShellToken.Border);
        _bar.Token(BackgroundProperty, ShellToken.Background);
        Child = _root;
        VerticalAlignment = VerticalAlignment.Bottom;
        Items.CollectionChanged += (_, _) => Rebuild();
        // The indicator's place depends on the bar's width.
        _layer.SizeChanged += (_, _) => PlaceIndicator(animate: false);
        SizeChanged += (_, _) => UpdateSafeArea();
        Rebuild();
    }

    // The bottom inset in use: BottomInset, or the safe area under this bar.
    private double Inset => double.IsNaN(BottomInset) ? _safeArea : BottomInset;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _insets = TopLevel.GetTopLevel(this)?.InsetsManager;
        if (_insets != null) _insets.SafeAreaChanged += OnSafeAreaChanged;
        UpdateSafeArea();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_insets != null) _insets.SafeAreaChanged -= OnSafeAreaChanged;
        _insets = null;
    }

    private void OnSafeAreaChanged(object? sender, SafeAreaChangedArgs e) => UpdateSafeArea();

    // Only the part of the system's bottom area that this bar actually sits on: nothing when the
    // window doesn't draw under it, or when something is laid out below this bar.
    private void UpdateSafeArea()
    {
        var safe = 0.0;
        if (_insets is { DisplaysEdgeToEdge: true } insets && TopLevel.GetTopLevel(this) is { } top
            && this.TranslatePoint(new Point(0, Bounds.Height), top) is { } bottom)
            safe = Math.Max(0, insets.SafeAreaPadding.Bottom - (top.Bounds.Height - bottom.Y));
        if (Math.Abs(safe - _safeArea) < 0.5) return;
        _safeArea = safe;
        if (double.IsNaN(BottomInset)) Rebuild();
    }

    private void Follow(ScrollViewer? old, ScrollViewer? source)
    {
        if (old != null) old.ScrollChanged -= OnScrollChanged;
        if (source != null) source.ScrollChanged += OnScrollChanged;
        _scrolled = 0;
    }

    // Hides after 24 px of scrolling down (past the first 48), shows after 8 px up or at the top,
    // so a jittery finger doesn't make it flicker.
    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer source || e.OffsetDelta.Y == 0) return;
        var delta = e.OffsetDelta.Y;
        _scrolled = Math.Sign(delta) == Math.Sign(_scrolled) ? _scrolled + delta : delta;
        if (source.Offset.Y <= 0) IsHidden = false;
        else if (_scrolled > 24 && source.Offset.Y > 48) IsHidden = true;
        else if (_scrolled < -8) IsHidden = false;
    }

    // Slides the whole bar down out of view and fades it, so nothing shows in a frame that doesn't clip.
    private void Slide(bool animate)
    {
        var transform = IsHidden ? FormattableString.Invariant($"translateY({Bounds.Height}px)") : "translateY(0px)";
        if (animate) _ = ShellMotion.To(_root, IsHidden ? 0 : 1, transform, Duration, Ease);
        else ShellMotion.Set(_root, IsHidden ? 0 : 1, transform);
        IsHitTestVisible = !IsHidden;
    }

    private List<BottomNavItem> ItemList => Items.OfType<BottomNavItem>().ToList();

    private bool Floats => Variant is BottomNavVariant.Floating or BottomNavVariant.Expanding;

    // A lifted bar always leaves room above it for the circle.
    private bool Raised(List<BottomNavItem> items) =>
        !Floats && (Variant == BottomNavVariant.Lifted || items.Any(i => i.Prominent));

    private bool IsSelected(BottomNavItem item) =>
        !item.Prominent && !string.IsNullOrEmpty(item.Value) && item.Value == SelectedValue;

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    // A prominent item is an action (compose, pay), not a place, so it never becomes active.
    internal void Select(BottomNavItem item)
    {
        if (!item.Prominent && !string.IsNullOrEmpty(item.Value)) ByUser(() => SelectedValue = item.Value);
    }

    // Left and Right move to the neighbouring destination and select it, as in a tab strip.
    internal void Step(BottomNavItem from, int direction)
    {
        var items = ItemList.Where(i => i.IsEffectivelyEnabled && !i.Prominent).ToList();
        var index = items.IndexOf(from) + direction;
        if (index < 0 || index >= items.Count) return;
        items[index].Focus(NavigationMethod.Directional);
        Select(items[index]);
    }

    // An item became (or stopped being) prominent, which changes the bar's layout.
    internal void InvalidateItems() => Rebuild();

    private void Rebuild()
    {
        var items = ItemList;
        _row.Children.Clear();
        _row.ColumnDefinitions.Clear();
        for (var i = 0; i < items.Count; i++)
        {
            // Equal columns, except in the capsule, where each item is as wide as it needs.
            _row.ColumnDefinitions.Add(new ColumnDefinition(Variant == BottomNavVariant.Expanding ? GridLength.Auto : GridLength.Star));
            Grid.SetColumn(items[i], i);
            _row.Children.Add(items[i]);
        }
        Layout(items);
        _placed = false;
        Refresh(animate: false);
    }

    /* Full-width bars: the bar's background is a sibling under the items, so a prominent item's
       circle can rise above its top edge and still be inside this control (touch only reaches a
       view within its bounds). The floating bars hold their items. */
    private void Layout(List<BottomNavItem> items)
    {
        _root.Children.Clear();
        _bar.Child = null;
        var raise = Raised(items) ? Raise : 0;
        var height = Variant switch { BottomNavVariant.Pill => 80.0, BottomNavVariant.Expanding => 60.0, _ => 64.0 };

        _bar.BorderThickness = Floats ? new Thickness(1) : new Thickness(0, 1, 0, 0);
        _bar.BoxShadow = Floats ? new BoxShadows(new BoxShadow { OffsetY = 10, Blur = 24, Color = ShellTheme.Shadow(0.12) }) : default;
        _bar.CornerRadius = Floats ? new CornerRadius(height / 2) : default;
        _row.ColumnSpacing = Variant == BottomNavVariant.Expanding ? 4 : 0;

        if (Floats)
        {
            var padding = Variant == BottomNavVariant.Expanding ? 8 : 6;
            _bar.Height = height;
            _bar.Margin = new Thickness(16, 0, 16, 16 + Inset);
            _bar.Padding = new Thickness(padding - 1);
            // The capsule is as wide as its items; the bar no wider than a phone.
            _bar.HorizontalAlignment = Variant == BottomNavVariant.Expanding ? HorizontalAlignment.Center : HorizontalAlignment.Stretch;
            _bar.MaxWidth = Variant == BottomNavVariant.Floating ? 480 : double.PositiveInfinity;
            _bar.VerticalAlignment = VerticalAlignment.Bottom;
            _layer.Margin = default;
            _layer.Height = double.NaN;
            _bar.Child = _layer;
            _root.Children.Add(_bar);
        }
        else
        {
            _bar.Height = height + Inset;
            _bar.Margin = new Thickness(0, raise, 0, 0);
            _bar.Padding = default;
            _bar.HorizontalAlignment = HorizontalAlignment.Stretch;
            _bar.MaxWidth = double.PositiveInfinity;
            _bar.VerticalAlignment = VerticalAlignment.Top;
            _layer.Margin = new Thickness(0, 0, 0, Inset);
            _layer.Height = height + raise;
            _root.Children.Add(_bar);
            _root.Children.Add(_layer);
        }

        // Each item lays itself out in the part of the row that is the bar. A raised item's label
        // lines up with the others', which are centred in that part (an item is 50 tall).
        var top = Variant == BottomNavVariant.Pill ? 12.0 : 0;
        var bottom = Variant == BottomNavVariant.Pill ? 16.0 : 0;
        var labelBottom = bottom + (height - top - bottom - 50) / 2;
        foreach (var item in items)
            item.Place(Floats ? default : new Thickness(0, raise + top, 0, bottom), labelBottom);

        _indicator.VerticalAlignment = VerticalAlignment.Top;
        _indicator.BorderThickness = default;
        switch (Variant)
        {
            case BottomNavVariant.Floating:
                // Wraps the active icon only, so a long label stays outside it.
                _indicator.CornerRadius = new CornerRadius(16);
                _indicator.Height = 32;
                _indicator.Token(BackgroundProperty, ShellToken.Primary);
                break;
            case BottomNavVariant.Lifted:
                // Centred 6 px under the bar's top edge, with a ring in the bar's color.
                _indicator.CornerRadius = new CornerRadius(LiftedCircle / 2);
                _indicator.Height = LiftedCircle;
                _indicator.Margin = new Thickness(0, raise + 6 - LiftedCircle / 2, 0, 0);
                _indicator.BorderThickness = new Thickness(4);
                _indicator.Token(BorderBrushProperty, ShellToken.Background);
                _indicator.Token(BackgroundProperty, ShellToken.Primary);
                break;
            case BottomNavVariant.Default:
                _indicator.CornerRadius = new CornerRadius(0, 0, 2, 2);
                _indicator.Height = 2;
                _indicator.Margin = new Thickness(0, raise, 0, 0);
                _indicator.Token(BackgroundProperty, ActiveToken);
                break;
        }
    }

    internal const double LiftedCircle = 52;

    // Items don't track the bar's state: the bar pushes it to them.
    private void Refresh(bool animate)
    {
        foreach (var item in ItemList)
            item.Apply(IsSelected(item), Variant, Labels, ActiveToken, ShowIndicator, animate);
        PlaceIndicator(animate);
    }

    // Default, Floating and Lifted have equal columns, so the indicator moves by whole columns.
    private void PlaceIndicator(bool animate)
    {
        var items = ItemList;
        var index = items.FindIndex(IsSelected);
        var width = _layer.Bounds.Width;
        if (Variant is BottomNavVariant.Pill or BottomNavVariant.Expanding || !ShowIndicator || index < 0 || width <= 0)
        {
            _indicator.Transitions = null;
            _indicator.Opacity = 0;
            _placed = false;
            return;
        }

        var column = width / items.Count;
        _indicator.Width = Variant switch
        {
            BottomNavVariant.Floating => Math.Min(56, column),
            BottomNavVariant.Lifted => LiftedCircle,
            _ => Math.Min(32, column)
        };
        // The Floating pill sits behind the icon: at the top of a labelled item, centred otherwise
        // (the 52 px slot holds the 50 px icon and label).
        if (Variant == BottomNavVariant.Floating)
            _indicator.Margin = new Thickness(0, Labels == BottomNavLabels.Never || string.IsNullOrEmpty(items[index].Text) ? 10 : 1, 0, 0);
        var x = column * index + (column - _indicator.Width) / 2;
        var transform = FormattableString.Invariant($"translateX({x}px)");
        // Slide only between two items; the first placement (or a resize) jumps.
        if (animate && _placed) _ = ShellMotion.To(_indicator, 1, transform, Duration, Ease);
        else ShellMotion.Set(_indicator, 1, transform);
        _placed = true;
    }
}

// One destination in a BottomNav — an icon over a short label, with an optional count badge or
// dot. A tab stop: Enter or Space selects it, Left and Right move along the bar. Prominent makes
// it a round action button (compose, pay) that raises Clicked and never becomes active; in a
// full-width bar it rises above the bar's edge.
public class BottomNavItem : Border, IShellFocusable
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<BottomNavItem, string?>(nameof(Text));

    // Identifies the item for BottomNav.SelectedValue.
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<BottomNavItem, string?>(nameof(Value));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<BottomNavItem, IconName>(nameof(Icon));

    // Shown instead of Icon while the item is active (e.g. MailOpen for Mail).
    public static readonly StyledProperty<IconName> ActiveIconProperty =
        AvaloniaProperty.Register<BottomNavItem, IconName>(nameof(ActiveIcon));

    // Replaces the icon with any control, such as an Avatar.
    public static readonly StyledProperty<Control?> IconContentProperty =
        AvaloniaProperty.Register<BottomNavItem, Control?>(nameof(IconContent));

    // A count over the icon ("3", "99+"). Empty hides it.
    public static readonly StyledProperty<string?> BadgeProperty =
        AvaloniaProperty.Register<BottomNavItem, string?>(nameof(Badge));

    // A small dot over the icon, for "something new" without a count.
    public static readonly StyledProperty<bool> DotProperty =
        AvaloniaProperty.Register<BottomNavItem, bool>(nameof(Dot));

    public static readonly StyledProperty<bool> ProminentProperty =
        AvaloniaProperty.Register<BottomNavItem, bool>(nameof(Prominent));

    private readonly Icon _icon;
    private readonly Border _glyph;
    private readonly Border _pill;
    private readonly Border _circle;
    private readonly Border _backdrop;
    private readonly Panel _iconHost;
    private readonly TextBlock _label;
    private readonly Border _labelHost;
    private readonly StackPanel _stack;
    private readonly Border _badge;
    private readonly TextBlock _badgeText;
    private bool _active;
    private BottomNavVariant _variant;
    private BottomNavLabels _labels;
    private ShellToken _activeToken = ShellToken.Foreground;
    private bool _indicator = true;
    private Thickness _inset;
    private double _labelBottom;

    static BottomNavItem()
    {
        TextProperty.Changed.AddClassHandler<BottomNavItem>((i, _) => i.Update(animate: false));
        IconProperty.Changed.AddClassHandler<BottomNavItem>((i, _) => i.Update(animate: false));
        ActiveIconProperty.Changed.AddClassHandler<BottomNavItem>((i, _) => i.Update(animate: false));
        IconContentProperty.Changed.AddClassHandler<BottomNavItem>((i, _) => i.Update(animate: false));
        ProminentProperty.Changed.AddClassHandler<BottomNavItem>((i, _) =>
        {
            i.Update(animate: false);
            i.FindParentOfType<BottomNav>()?.InvalidateItems();
        });
        BadgeProperty.Changed.AddClassHandler<BottomNavItem>((i, _) => i.UpdateBadge());
        DotProperty.Changed.AddClassHandler<BottomNavItem>((i, _) => i.UpdateBadge());
        IsEnabledProperty.Changed.AddClassHandler<BottomNavItem>((i, _) => i.Update(animate: false));
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

    public IconName ActiveIcon
    {
        get => GetValue(ActiveIconProperty);
        set => SetValue(ActiveIconProperty, value);
    }

    public Control? IconContent
    {
        get => GetValue(IconContentProperty);
        set => SetValue(IconContentProperty, value);
    }

    public string? Badge
    {
        get => GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }

    public bool Dot
    {
        get => GetValue(DotProperty);
        set => SetValue(DotProperty, value);
    }

    public bool Prominent
    {
        get => GetValue(ProminentProperty);
        set => SetValue(ProminentProperty, value);
    }

    public bool IsActive => _active;

    // Raised on every press, including on the item that is already active (e.g. scroll to top).
    public event EventHandler? Clicked;

    public BottomNavItem()
    {
        _icon = new Icon { Size = 22 };
        _glyph = new Border { Child = _icon, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        _pill = new Border
        {
            Width = 56,
            Height = 32,
            CornerRadius = new CornerRadius(16),
            Opacity = 0,
            IsHitTestVisible = false
        };
        _pill.Token(BackgroundProperty, ShellToken.Secondary);
        // A ring in the bar's color sets the raised button off from the bar's edge.
        _circle = new Border { IsVisible = false, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        _circle.Token(BackgroundProperty, ShellToken.Primary);
        _circle.Token(BorderBrushProperty, ShellToken.Background);

        _badgeText = new TextBlock { FontSize = 10, FontWeight = FontWeight.SemiBold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        _badgeText.Token(TextBlock.ForegroundProperty, ShellToken.DestructiveForeground);
        _badge = new Border
        {
            Child = _badgeText,
            BorderThickness = new Thickness(2),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            IsHitTestVisible = false,
            IsVisible = false
        };
        _badge.Token(BackgroundProperty, ShellToken.Destructive);
        _badge.Token(BorderBrushProperty, ShellToken.Background);
        _iconHost = new Panel { Children = { _pill, _circle, _glyph, _badge } };

        _label = new TextBlock
        {
            FontSize = 12,
            FontWeight = FontWeight.Medium,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        _labelHost = new Border { Child = _label, ClipToBounds = true, VerticalAlignment = VerticalAlignment.Center };
        _stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, Children = { _iconHost, _labelHost } };
        // Expanding: the active item's rounded-full bg-primary.
        _backdrop = new Border { Opacity = 0, IsHitTestVisible = false, CornerRadius = new CornerRadius(22) };
        _backdrop.Token(BackgroundProperty, ShellToken.Primary);

        Child = new Panel { Children = { _backdrop, _stack } };
        Background = Brushes.Transparent;
        Cursor = new Cursor(StandardCursorType.Hand);
        ShellFocus.Ring(this, this);
        // Text measured before the item is in a window misses the inherited font, so the
        // capsule's label is sized again once it is.
        Loaded += (_, _) => SizeLabel(animate: false);
        Update(animate: false);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        UpdateColors();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        UpdateColors();
        Release();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!IsEffectivelyEnabled) return;
        // A small press-in.
        if (Prominent) _ = ShellMotion.To(_circle, 1, "scale(0.92)", 80, BottomNav.Ease);
        else _ = ShellMotion.To(_glyph, 1, GlyphTransform(0.9), 80, BottomNav.Ease);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        Release();
        if (e.InitialPressMouseButton != MouseButton.Left) return;
        Press();
        e.Handled = true;
    }

    private void Release()
    {
        _ = ShellMotion.To(_circle, 1, "scale(1)", BottomNav.Duration, BottomNav.Ease);
        _ = ShellMotion.To(_glyph, 1, GlyphTransform(1), BottomNav.Duration, BottomNav.Ease);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.Key)
        {
            case Key.Enter or Key.Space:
                Press();
                break;
            case Key.Left or Key.Right:
                this.FindParentOfType<BottomNav>()?.Step(this, e.Key == Key.Right ? 1 : -1);
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    // Picks the item in code, as a tap or Enter/Space would.
    public void Press()
    {
        if (!IsEffectivelyEnabled) return;
        this.FindParentOfType<BottomNav>()?.Select(this);
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    // The part of the item's height that is the bar (the rest is room for a raised circle).
    internal void Place(Thickness inset, double labelBottom)
    {
        _inset = inset;
        _labelBottom = labelBottom;
        Update(animate: false);
    }

    internal void Apply(bool isActive, BottomNavVariant variant, BottomNavLabels labels, ShellToken activeToken, bool indicator, bool animate)
    {
        var changed = isActive != _active;
        _active = isActive;
        _variant = variant;
        _labels = labels;
        _activeToken = activeToken;
        _indicator = indicator;
        Update(animate && changed);
    }

    // Lifted: the active icon sits in the bar's circle.
    private bool Lifts => _variant == BottomNavVariant.Lifted && _active && _indicator && !Prominent;

    private bool Expanding => _variant == BottomNavVariant.Expanding;
    private bool Floats => _variant is BottomNavVariant.Floating or BottomNavVariant.Expanding;

    // A prominent item keeps its label under the raised circle; in a floating bar it is just the circle.
    private bool HasLabel => !string.IsNullOrEmpty(Text) && (Prominent ? !Floats : Expanding || _labels != BottomNavLabels.Never);

    private bool ShowsLabel => HasLabel && (Prominent || !(Expanding || _labels == BottomNavLabels.Active) || _active);

    private static string GlyphTransform(double scale) => FormattableString.Invariant($"scale({scale})");

    // Moves the whole icon slot, so a badge goes with its icon.
    private string SlotTransform()
    {
        // With labels on the active item only, the others drop their icon to the middle (half the
        // label and its spacing). Lifted: up from the middle of the 32 px slot (7 px down in the
        // 64 px bar) into the circle.
        var drop = Lifts ? -17 : !Prominent && !Expanding && _labels == BottomNavLabels.Active && !_active && HasLabel ? 9 : 0;
        return FormattableString.Invariant($"translateY({drop}px)");
    }

    private void Update(bool animate)
    {
        _icon.Kind = _active && ActiveIcon != IconName.None ? ActiveIcon : Icon;
        _icon.Size = Prominent ? 24 : 22;
        _glyph.Child = IconContent ?? _icon;
        _label.Text = Text ?? string.Empty;
        _labelHost.IsVisible = HasLabel;

        // Expanding lays the icon and label side by side in a pill; the others stack them.
        _stack.Orientation = Expanding ? Orientation.Horizontal : Orientation.Vertical;
        _stack.Spacing = Expanding ? 0 : 2;
        var raised = Prominent && !Floats;
        // In the capsule the pill fills the item and the content sits inside it.
        _stack.Margin = Expanding ? new Thickness(Prominent ? 0 : 12, 0) : default;
        if (Expanding)
        {
            Padding = default;
            Height = 44;
        }
        else
        {
            // A raised item uses the whole height, its label lined up with the others'.
            Padding = raised ? new Thickness(0, 0, 0, _labelBottom) : _inset;
            Height = double.NaN;
        }
        _stack.VerticalAlignment = raised ? VerticalAlignment.Bottom : VerticalAlignment.Center;
        // The floating bars' slots are pill-shaped, so the focus ring follows them.
        CornerRadius = new CornerRadius(Floats ? 26 : ShellTheme.RadiusMd);

        // The icon slot: 56 x 32 (room for the Pill), 24 x 24 in the capsule, the circle's size
        // for a prominent item.
        var circle = Expanding ? 44.0 : Floats ? 48.0 : 56.0;
        _iconHost.Width = Prominent ? circle : Expanding ? 24 : 56;
        _iconHost.Height = Prominent ? circle : Expanding ? 24 : 32;
        _circle.IsVisible = Prominent;
        _circle.Width = _circle.Height = circle;
        _circle.CornerRadius = new CornerRadius(circle / 2);
        _circle.BorderThickness = new Thickness(raised ? 4 : 0);

        UpdateColors();
        UpdateBadge();

        var pill = _variant == BottomNavVariant.Pill && _active && !Prominent;
        var labelOpacity = ShowsLabel ? 1.0 : 0.0;
        // A label that appears rises into place.
        var labelTransform = ShowsLabel || Expanding ? "translateY(0px)" : "translateY(4px)";
        if (animate)
        {
            _ = ShellMotion.To(_iconHost, 1, SlotTransform(), BottomNav.Duration, BottomNav.Ease);
            _ = ShellMotion.To(_label, labelOpacity, labelTransform, BottomNav.Duration, BottomNav.Ease);
            _ = ShellMotion.To(_pill, pill ? 1 : 0, pill ? "scaleX(1)" : "scaleX(0.4)", BottomNav.Duration, BottomNav.Ease);
            _ = ShellMotion.To(_backdrop, Expanding && _active ? 1 : 0, "scale(1)", BottomNav.Duration, BottomNav.Ease);
        }
        else
        {
            ShellMotion.Set(_iconHost, 1, SlotTransform());
            ShellMotion.Set(_glyph, 1, GlyphTransform(1));
            ShellMotion.Set(_label, labelOpacity, labelTransform);
            ShellMotion.Set(_pill, pill ? 1 : 0, pill ? "scaleX(1)" : "scaleX(0.4)");
            ShellMotion.Set(_backdrop, Expanding && _active ? 1 : 0, "scale(1)");
            ShellMotion.Set(_circle, 1, "scale(1)");
        }
        SizeLabel(animate);
        Opacity = IsEnabled ? 1.0 : 0.5;
    }

    // In the capsule the label's width opens from 0, which widens the item and the bar with it.
    private void SizeLabel(bool animate)
    {
        _labelHost.Transitions = null;
        if (!Expanding)
        {
            _labelHost.Width = double.NaN;
            _labelHost.Padding = default;
            return;
        }
        _labelHost.Padding = new Thickness(8, 0, 0, 0);
        _label.Measure(Size.Infinity);
        var width = _active ? _label.DesiredSize.Width + 8 : 0;
        if (animate)
        {
            // From the current width (NaN means it was laid out at its natural size).
            if (double.IsNaN(_labelHost.Width)) _labelHost.Width = _labelHost.Bounds.Width;
            _labelHost.Transitions = new Transitions
            {
                new DoubleTransition { Property = WidthProperty, Duration = TimeSpan.FromMilliseconds(BottomNav.Duration), Easing = BottomNav.Ease }
            };
        }
        _labelHost.Width = width;
    }

    private void UpdateColors()
    {
        var hovered = IsPointerOver && IsEffectivelyEnabled;
        var rest = hovered ? ShellToken.Foreground : ShellToken.MutedForeground;
        ShellToken icon, label;
        if (Prominent) (icon, label) = (ShellToken.PrimaryForeground, rest);
        else if (!_active) icon = label = rest;
        else if (Lifts) (icon, label) = (ShellToken.PrimaryForeground, ShellToken.Foreground);
        else
            (icon, label) = _variant switch
            {
                BottomNavVariant.Pill => (ShellToken.SecondaryForeground, ShellToken.Foreground),
                BottomNavVariant.Floating => (_indicator ? ShellToken.PrimaryForeground : ShellToken.Foreground, ShellToken.Foreground),
                BottomNavVariant.Expanding => (ShellToken.PrimaryForeground, ShellToken.PrimaryForeground),
                _ => (_activeToken, _activeToken)
            };
        _icon.Token = icon;
        _label.Token(TextBlock.ForegroundProperty, label);
        _label.FontWeight = _active ? FontWeight.SemiBold : FontWeight.Medium;
    }

    private void UpdateBadge()
    {
        var count = !string.IsNullOrEmpty(Badge);
        _badge.IsVisible = (count || Dot) && !Prominent;
        // On an active pill or circle the ring takes its color, so the badge doesn't look clipped.
        var onPill = _active && _indicator && _variant is BottomNavVariant.Floating or BottomNavVariant.Lifted;
        _badge.Token(BorderBrushProperty,
            _active && _variant == BottomNavVariant.Pill ? ShellToken.Secondary
            : onPill || (_active && Expanding) ? ShellToken.Primary
            : ShellToken.Background);
        _badgeText.Text = Badge ?? string.Empty;
        _badgeText.IsVisible = count;
        // Over the top-right corner of the 22 px icon, which sits in the middle of the slot.
        var slotWidth = Expanding ? 24 : 56;
        var iconTop = ((Expanding ? 24 : 32) - 22) / 2.0;
        if (count)
        {
            _badge.MinWidth = 20;
            _badge.Height = 20;
            _badge.Padding = new Thickness(4, 0);
            _badge.CornerRadius = new CornerRadius(10);
            _badge.Margin = new Thickness(slotWidth / 2.0 + 2, iconTop - 9, 0, 0);
        }
        else
        {
            _badge.MinWidth = 12;
            _badge.Height = 12;
            _badge.Padding = default;
            _badge.CornerRadius = new CornerRadius(6);
            _badge.Margin = new Thickness(slotWidth / 2.0 + 5, iconTop - 2, 0, 0);
        }
        UpdateName();
    }

    // Screen readers hear the badge with the label: "Inbox, 3 new".
    private void UpdateName() => AutomationProperties.SetName(this,
        !string.IsNullOrEmpty(Badge) ? $"{Text}, {Badge} new" : Dot ? $"{Text}, new" : Text);
}

public enum BottomNavVariant { Default, Pill, Floating, Lifted, Expanding }

// Which items show their label: all of them, only the active one (the others are icon-only), or none.
public enum BottomNavLabels { Always, Active, Never }
