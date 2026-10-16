using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// A menu docked to a screen edge: a small handle that slides out a rail of icon tiles — tools and
// shortcuts that shouldn't take a tab in the bottom nav. Put it over the page (same cell, after
// the content); only the handle and the open rail take input. A tap outside, Escape / back, the
// handle or a tile closes it.
//   <Grid>
//       <ScrollView>...</ScrollView>
//       <ui:EdgeMenu Side="Right" Position="0.4">
//           <ui:EdgeMenuItem Icon="Calendar" Text="Calendar" Clicked="OnCalendar" />
//           <ui:EdgeMenuItem Icon="Camera" Text="Camera" />
//       </ui:EdgeMenu>
//   </Grid>
// Default: rounded-l-2xl border bg-popover shadow-lg, tiles hover:bg-accent.
// Solid: bg-primary text-primary-foreground, like a dark dock.
[ContentProperty(nameof(Items))]
public partial class EdgeMenu : ContentView
{
    public static readonly BindableProperty IsOpenProperty =
        BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(EdgeMenu), false, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((EdgeMenu)b).OnOpenChanged());

    public static readonly BindableProperty SideProperty =
        BindableProperty.Create(nameof(Side), typeof(EdgeMenuSide), typeof(EdgeMenu), EdgeMenuSide.Right,
            propertyChanged: (b, o, n) => ((EdgeMenu)b).ApplyLook());

    // Where the handle sits along the edge: 0 top, 0.5 middle, 1 bottom.
    public static readonly BindableProperty PositionProperty =
        BindableProperty.Create(nameof(Position), typeof(double), typeof(EdgeMenu), 0.5,
            propertyChanged: (b, o, n) => ((EdgeMenu)b).PlaceDock());

    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(EdgeMenuVariant), typeof(EdgeMenu), EdgeMenuVariant.Default,
            propertyChanged: (b, o, n) => ((EdgeMenu)b).ApplyLook());

    // Dims the page while the menu is open.
    public static readonly BindableProperty DimProperty =
        BindableProperty.Create(nameof(Dim), typeof(bool), typeof(EdgeMenu), false,
            propertyChanged: (b, o, n) => ((EdgeMenu)b).Slide(animate: false));

    // Picking a tile closes the menu.
    public static readonly BindableProperty CloseOnSelectProperty =
        BindableProperty.Create(nameof(CloseOnSelect), typeof(bool), typeof(EdgeMenu), true);

    // The highlighted tile's Value (optional: tiles can be plain actions).
    public static readonly BindableProperty SelectedValueProperty =
        BindableProperty.Create(nameof(SelectedValue), typeof(string), typeof(EdgeMenu), null, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((EdgeMenu)b).OnSelectedValueChanged());

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public EdgeMenuSide Side
    {
        get => (EdgeMenuSide)GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    public double Position
    {
        get => (double)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public EdgeMenuVariant Variant
    {
        get => (EdgeMenuVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public bool Dim
    {
        get => (bool)GetValue(DimProperty);
        set => SetValue(DimProperty, value);
    }

    public bool CloseOnSelect
    {
        get => (bool)GetValue(CloseOnSelectProperty);
        set => SetValue(CloseOnSelectProperty, value);
    }

    public string? SelectedValue
    {
        get => (string?)GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public IList<EdgeMenuItem> Items => _items;

    // Raised on every open and close, like the other overlays.
    public event EventHandler<bool>? IsOpenChanged;

    // User input only: setting SelectedValue from code or a binding doesn't raise it.
    public event EventHandler<string?>? SelectedValueChanged;

    private const uint Duration = 220;

    private readonly ObservableCollection<EdgeMenuItem> _items = new();
    private readonly Grid _backdrop;
    private readonly HorizontalStackLayout _dock;
    private readonly Border _handle;
    private readonly Icon _chevron;
    private readonly Border _panel;
    private readonly VerticalStackLayout _list;
    private bool _byUser;

    public EdgeMenu()
    {
        // Covers the page while open, so a tap outside closes the menu instead of reaching the page.
        _backdrop = new Grid { IsVisible = false, Opacity = 0, BackgroundColor = Colors.Transparent };
        var outside = new TapGestureRecognizer();
        outside.Tapped += (_, _) => IsOpen = false;
        _backdrop.GestureRecognizers.Add(outside);

        _chevron = new Icon { Size = 16 };
        _handle = new Border
        {
            Content = _chevron,
            WidthRequest = 28,
            HeightRequest = 64,
            StrokeThickness = 1,
            VerticalOptions = LayoutOptions.Center
        };
        var toggle = new TapGestureRecognizer();
        toggle.Tapped += (_, _) => { ShellFocus.FocusPressed(_handle); IsOpen = !IsOpen; };
        _handle.GestureRecognizers.Add(toggle);
        ShellFocus.MakeFocusable(_handle, () => IsOpen = !IsOpen);

        _list = new VerticalStackLayout { Spacing = 6 };
        _panel = new Border
        {
            Content = new ScrollView { Content = _list, VerticalScrollBarVisibility = ScrollBarVisibility.Never },
            Padding = new Thickness(8),
            StrokeThickness = 1,
            Shadow = new Shadow { Brush = new SolidColorBrush(Colors.Black), Offset = new Point(0, 10), Radius = 24, Opacity = 0.15f }
        };
        _dock = new HorizontalStackLayout { VerticalOptions = LayoutOptions.Start };

        Content = new Grid { Children = { _backdrop, _dock } };
        // Only the handle and the open rail take input: the rest of this view lets taps through.
        InputTransparent = true;
        CascadeInputTransparent = false;
        IsClippedToBounds = true;
        _items.CollectionChanged += (_, _) => Rebuild();
        SizeChanged += (_, _) => PlaceDock();
        _dock.SizeChanged += (_, _) => PlaceDock();
        _panel.SizeChanged += (_, _) => Slide(animate: false);
        ApplyLook();
    }

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

    private void OnSelectedValueChanged()
    {
        Refresh();
        if (_byUser) SelectedValueChanged?.Invoke(this, SelectedValue);
    }

    private void Rebuild()
    {
        _list.Children.Clear();
        foreach (var item in _items) _list.Children.Add(item);
        Refresh();
    }

    private void Refresh()
    {
        foreach (var item in _items)
            item.Apply(!string.IsNullOrEmpty(item.Value) && item.Value == SelectedValue, Variant);
    }

    private void ApplyLook()
    {
        var right = Side == EdgeMenuSide.Right;
        var solid = Variant == EdgeMenuVariant.Solid;
        _dock.Children.Clear();
        _dock.Children.Add(right ? _handle : _panel);
        _dock.Children.Add(right ? _panel : _handle);
        _dock.HorizontalOptions = right ? LayoutOptions.End : LayoutOptions.Start;

        // Rounded on the inner side only; the outer side runs into the screen edge.
        _handle.StrokeShape = new RoundRectangle { CornerRadius = right ? new CornerRadius(12, 0, 12, 0) : new CornerRadius(0, 12, 0, 12) };
        _panel.StrokeShape = new RoundRectangle { CornerRadius = right ? new CornerRadius(16, 0, 16, 0) : new CornerRadius(0, 16, 0, 16) };
        _panel.StrokeThickness = solid ? 0 : 1;
        _handle.StrokeThickness = solid ? 0 : 1;

        _handle.Token(VisualElement.BackgroundColorProperty, solid ? ShellToken.Primary : ShellToken.Popover);
        _handle.Token(Border.StrokeProperty, ShellToken.Border);
        _chevron.Token = solid ? ShellToken.PrimaryForeground : ShellToken.PopoverForeground;
        _panel.Token(VisualElement.BackgroundColorProperty, solid ? ShellToken.Primary : ShellToken.Popover);
        _panel.Token(Border.StrokeProperty, ShellToken.Border);
        Refresh();
        PlaceDock();
        Slide(animate: false);
    }

    // The dock's top follows Position, kept inside this view.
    private void PlaceDock()
    {
        if (Height <= 0) return;
        var room = Height - _dock.Height;
        _dock.Margin = new Thickness(0, Math.Max(0, room * Math.Clamp(Position, 0, 1)), 0, 0);
        _panel.MaximumHeightRequest = Math.Max(0, Height - 32);
    }

    private void OnOpenChanged()
    {
        if (IsOpen) ShellDismiss.Push(this, Close);
        else ShellDismiss.Remove(this);
        Slide(animate: IsLoaded);
        IsOpenChanged?.Invoke(this, IsOpen);
    }

    // Closed, the dock is pushed out by the panel's width so only the handle shows.
    private void Slide(bool animate)
    {
        var width = _panel.Width > 0 ? _panel.Width : 90;
        var shift = IsOpen ? 0 : Side == EdgeMenuSide.Right ? width : -width;
        if (animate)
        {
            _ = _dock.TranslateToAsync(shift, 0, Duration, Easing.CubicOut);
            // The panel fades too, so its shadow doesn't show at the edge while closed.
            _ = _panel.FadeToAsync(IsOpen ? 1 : 0, Duration, Easing.CubicOut);
        }
        else
        {
            _dock.TranslationX = shift;
            _panel.Opacity = IsOpen ? 1 : 0;
        }
        _panel.InputTransparent = !IsOpen;

        _chevron.Name = IsOpen == (Side == EdgeMenuSide.Right) ? IconName.ChevronRight : IconName.ChevronLeft;
        SemanticProperties.SetDescription(_handle, IsOpen ? "Close menu" : "Open menu");

        _backdrop.IsVisible = IsOpen;
        _backdrop.BackgroundColor = Dim ? Colors.Black.WithAlpha(0.4f) : Colors.Transparent;
        if (animate && IsOpen) _ = _backdrop.FadeToAsync(1, Duration, Easing.CubicOut);
        else _backdrop.Opacity = IsOpen ? 1 : 0;
    }
}

// A tile in an EdgeMenu — an icon over a short label (rounded-xl, 72 x 60). Tap or Enter / Space
// picks it.
public partial class EdgeMenuItem : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(EdgeMenuItem), null,
            propertyChanged: (b, o, n) => ((EdgeMenuItem)b).Update());

    // Identifies the tile for EdgeMenu.SelectedValue (optional).
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(EdgeMenuItem), null);

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(EdgeMenuItem), IconName.None,
            propertyChanged: (b, o, n) => ((EdgeMenuItem)b).Update());

    public string? Text
    {
        get => (string?)GetValue(TextProperty);
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

    public bool IsActive => _active;

    // Raised on every tap or Enter / Space.
    public event EventHandler? Clicked;

    private readonly Border _tile;
    private readonly Icon _icon;
    private readonly Label _label;
    private bool _active;
    private bool _hovered;
    private EdgeMenuVariant _variant;

    public EdgeMenuItem()
    {
        _icon = new Icon { Size = 20 };
        _label = new Label { FontSize = 11, HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1 };
        _tile = new Border
        {
            Content = new VerticalStackLayout
            {
                Spacing = 4,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children = { _icon, _label }
            },
            WidthRequest = 72,
            HeightRequest = 60,
            Padding = new Thickness(4, 0),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 12 }
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { _hovered = true; Update(); };
        pointer.PointerExited += (_, _) => { _hovered = false; Update(); _ = _tile.ScaleToAsync(1, 150, Easing.CubicOut); };
        // A small press-in.
        pointer.PointerPressed += (_, _) => { if (IsEnabled) _ = _tile.ScaleToAsync(0.95, 80, Easing.CubicOut); };
        pointer.PointerReleased += (_, _) => _ = _tile.ScaleToAsync(1, 150, Easing.CubicOut);
        _tile.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { ShellFocus.FocusPressed(this); Press(); };
        _tile.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, Press);

        Content = _tile;
        // Solid tiles mix their color from the theme, so they follow it by hand.
        Loaded += (_, _) => ShellTheme.ThemeChanged += OnThemeChanged;
        Unloaded += (_, _) => ShellTheme.ThemeChanged -= OnThemeChanged;
        Update();
    }

    private void OnThemeChanged(object? sender, EventArgs e) => Update();

    // Picks the tile in code, as a tap or Enter / Space would.
    public void Press()
    {
        if (!IsEnabled) return;
        Clicked?.Invoke(this, EventArgs.Empty);
        this.FindParentOfType<EdgeMenu>()?.Pick(this);
    }

    internal void Apply(bool isActive, EdgeMenuVariant variant)
    {
        _active = isActive;
        _variant = variant;
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
        _icon.Name = Icon;
        _label.Text = Text ?? string.Empty;
        _label.IsVisible = !string.IsNullOrEmpty(Text);
        SemanticProperties.SetDescription(this, Text);

        var hovered = _hovered && IsEnabled;
        if (_variant == EdgeMenuVariant.Solid)
        {
            // Tiles a shade lighter than the dock; brighter when hovered or active.
            var shade = _active ? 0.2f : hovered ? 0.14f : 0.08f;
            _tile.ClearValue(VisualElement.BackgroundColorProperty);
            _tile.BackgroundColor = ShellTheme.Get(ShellToken.PrimaryForeground).WithAlpha(shade);
            _icon.Token = ShellToken.PrimaryForeground;
            _label.Token(Label.TextColorProperty, ShellToken.PrimaryForeground);
            _label.Opacity = _active || hovered ? 1 : 0.75;
        }
        else
        {
            if (_active || hovered) _tile.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
            else
            {
                _tile.ClearValue(VisualElement.BackgroundColorProperty);
                _tile.BackgroundColor = Colors.Transparent;
            }
            _icon.Token = _active || hovered ? ShellToken.AccentForeground : ShellToken.PopoverForeground;
            _label.Token(Label.TextColorProperty, _active || hovered ? ShellToken.AccentForeground : ShellToken.MutedForeground);
            _label.Opacity = 1;
        }
        _label.FontAttributes = _active ? FontAttributes.Bold : FontAttributes.None;
    }
}

public enum EdgeMenuSide { Right, Left }

public enum EdgeMenuVariant { Default, Solid }
