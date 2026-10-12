using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Navigation menu — a row of top-level links for a site or app header. An item with links
// inside shows a chevron and opens a panel of them (title and description); an item without
// is a plain link.
//   <ui:NavigationMenu>
//       <ui:NavigationMenuItem Title="Getting started">
//           <ui:NavigationMenuLink Title="Introduction" Description="What ShellUI Native is." Clicked="OnLink" />
//           <ui:NavigationMenuLink Title="Installation" Description="Add it to your app." Clicked="OnLink" />
//       </ui:NavigationMenuItem>
//       <ui:NavigationMenuItem Title="Docs" Clicked="OnDocs" />
//   </ui:NavigationMenu>
[ContentProperty(nameof(Items))]
public partial class NavigationMenu : ContentView
{
    public IList<IView> Items => _row.Children;

    private readonly HorizontalStackLayout _row;

    public NavigationMenu()
    {
        _row = new HorizontalStackLayout { Spacing = 4 };
        Content = _row;
        HorizontalOptions = LayoutOptions.Start;
    }
}

// A top-level entry — h-10 rounded-md px-4 text-sm font-medium, hover:bg-accent. With
// NavigationMenuLink children it opens a w-56 panel (rounded-md border bg-popover p-1 shadow-lg).
[ContentProperty(nameof(Links))]
public partial class NavigationMenuItem : ContentView, IShellPopup
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(NavigationMenuItem), string.Empty,
            propertyChanged: (b, o, n) => ((NavigationMenuItem)b).Update());

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IList<IView> Links => _stack.Children;

    public bool IsOpen { get; private set; }

    // Raised when an item without links is pressed.
    public event EventHandler? Clicked;

    private readonly Border _trigger;
    private readonly Label _label;
    private readonly Icon _chevron;
    private readonly VerticalStackLayout _stack;
    private readonly Border _panel;
    private ShellPopupHandle? _handle;
    private bool _hovered;

    public NavigationMenuItem()
    {
        _label = new Label { FontSize = 14, FontAttributes = FontAttributes.Bold, VerticalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center, LineBreakMode = LineBreakMode.NoWrap };
        _label.Token(Label.TextColorProperty, ShellToken.Foreground);
        _chevron = new Icon { Name = IconName.ChevronDown, Size = 12, Token = ShellToken.Foreground, VerticalOptions = LayoutOptions.Center, IsVisible = false };
        _trigger = new Border
        {
            Content = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center, Children = { _label, _chevron } },
            HeightRequest = 40,
            Padding = new Thickness(16, 0),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { _hovered = true; Update(); };
        pointer.PointerExited += (_, _) => { _hovered = false; Update(); };
        _trigger.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { ShellFocus.FocusPressed(this); Press(); };
        _trigger.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, Press);

        _stack = new VerticalStackLayout { Spacing = 0 };
        _stack.ChildAdded += (_, _) => Update();
        _stack.ChildRemoved += (_, _) => Update();
        _panel = new Border
        {
            Content = _stack,
            Padding = new Thickness(4),
            WidthRequest = 224,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            Shadow = ShellPopups.PanelShadow()
        };
        _panel.Token(VisualElement.BackgroundColorProperty, ShellToken.Popover);
        _panel.Token(Border.StrokeProperty, ShellToken.Border);
        // Taps on the panel padding must not fall through to the click-outside catcher.
        _panel.GestureRecognizers.Add(new TapGestureRecognizer());

        Content = _trigger;
        Update();
    }

    private bool HasLinks => _stack.Children.Count > 0;

    private void Press()
    {
        if (!IsEnabled) return;
        if (HasLinks) SetOpen(!IsOpen);
        else Clicked?.Invoke(this, EventArgs.Empty);
    }

    public void SetOpen(bool open)
    {
        if (IsOpen == open || (open && !HasLinks)) return;
        IsOpen = open;
        Update();
        _ = _chevron.RotateToAsync(open ? 180 : 0, 200, Easing.CubicOut);
        if (open)
        {
            // The panel floats outside this view, so links can't find their item by walking up.
            foreach (var link in _stack.Children.OfType<NavigationMenuLink>()) link.Owner = this;
            ShellPopups.Opened(this);
            _handle = ShellPortal.ShowPopup(_trigger, _panel, new ShellPopupOptions { Owner = this, OnDismiss = Close });
        }
        else
        {
            ShellPopups.Closed(this);
            var handle = _handle;
            _handle = null;
            if (handle != null) _ = handle.CloseAsync();
        }
    }

    public void Close() => SetOpen(false);

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName)
            Opacity = IsEnabled ? 1.0 : 0.5;
    }

    private void Update()
    {
        _label.Text = Title ?? string.Empty;
        _chevron.IsVisible = HasLinks;
        SemanticProperties.SetDescription(this, Title ?? string.Empty);
        if (IsOpen || (_hovered && IsEnabled))
            _trigger.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        else
        {
            _trigger.ClearValue(VisualElement.BackgroundColorProperty);
            _trigger.BackgroundColor = Colors.Transparent;
        }
    }
}

// A link in an item's panel — rounded-sm p-2, hover:bg-accent: a text-sm font-medium title over
// an optional muted description, with an optional icon.
public partial class NavigationMenuLink : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(NavigationMenuLink), string.Empty,
            propertyChanged: (b, o, n) => ((NavigationMenuLink)b).Update());

    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(NavigationMenuLink), string.Empty,
            propertyChanged: (b, o, n) => ((NavigationMenuLink)b).Update());

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(NavigationMenuLink), IconName.None,
            propertyChanged: (b, o, n) => ((NavigationMenuLink)b).Update());

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public IconName Icon
    {
        get => (IconName)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public event EventHandler? Clicked;

    internal NavigationMenuItem? Owner { get; set; }

    private readonly Border _row;
    private readonly Icon _icon;
    private readonly Label _title;
    private readonly Label _description;

    public NavigationMenuLink()
    {
        _icon = new Icon { Size = 16, IsVisible = false, Token = ShellToken.PopoverForeground, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(0, 2, 0, 0) };
        _title = new Label { FontSize = 14, FontAttributes = FontAttributes.Bold };
        _title.Token(Label.TextColorProperty, ShellToken.PopoverForeground);
        _description = new Label { FontSize = 13, IsVisible = false, LineBreakMode = LineBreakMode.WordWrap };
        _description.Token(Label.TextColorProperty, ShellToken.MutedForeground);

        var grid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }
        };
        grid.Add(_icon, 0, 0);
        grid.Add(new VerticalStackLayout { Spacing = 2, Children = { _title, _description } }, 1, 0);

        _row = new Border
        {
            Content = grid,
            Padding = new Thickness(8),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusSm },
            BackgroundColor = Colors.Transparent
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { if (IsEnabled) _row.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent); };
        pointer.PointerExited += (_, _) => { _row.ClearValue(VisualElement.BackgroundColorProperty); _row.BackgroundColor = Colors.Transparent; };
        _row.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => Press();
        _row.GestureRecognizers.Add(tap);

        Content = _row;
        Update();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(IsEnabled)) Opacity = IsEnabled ? 1 : 0.5;
    }

    private void Press()
    {
        if (!IsEnabled) return;
        Owner?.Close();
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        _icon.Name = Icon;
        _icon.IsVisible = Icon != IconName.None;
        _title.Text = Title ?? string.Empty;
        _description.Text = Description ?? string.Empty;
        _description.IsVisible = !string.IsNullOrEmpty(Description);
        SemanticProperties.SetDescription(this, Title ?? string.Empty);
    }
}
