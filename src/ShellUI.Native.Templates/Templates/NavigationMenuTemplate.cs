using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class NavigationMenuTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "navigation-menu",
        DisplayName = "Navigation Menu",
        Description = "Row of top-level links that open panels of links",
        Category = ComponentCategory.Navigation,
        FilePath = "NavigationMenu.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "navigation", "menu", "header", "links" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Navigation menu — a row of top-level links for a site or app header. An item with links
// inside shows a chevron and opens a panel of them (title and description); an item without
// is a plain link.
//   <ui:NavigationMenu>
//       <ui:NavigationMenuItem Title=""Getting started"">
//           <ui:NavigationMenuLink Title=""Introduction"" Description=""What ShellUI Native is."" Clicked=""OnLink"" />
//           <ui:NavigationMenuLink Title=""Installation"" Description=""Add it to your app."" Clicked=""OnLink"" />
//       </ui:NavigationMenuItem>
//       <ui:NavigationMenuItem Title=""Docs"" Clicked=""OnDocs"" />
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
",
        [NativePlatform.Avalonia] = @"using System;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace YourProjectNamespace.Components.UI;

// Navigation menu — a row of top-level links for a site or app header. An item with links
// inside shows a chevron and opens a panel of them (title and description); an item without
// is a plain link. Items are tab stops: Enter, Space or Down opens one, Left / Right move
// between them, and Up / Down move through an open panel.
//   <ui:NavigationMenu>
//       <ui:NavigationMenuItem Title=""Getting started"">
//           <ui:NavigationMenuLink Title=""Introduction"" Description=""What ShellUI Native is."" Clicked=""OnLink"" />
//           <ui:NavigationMenuLink Title=""Installation"" Description=""Add it to your app."" Clicked=""OnLink"" />
//       </ui:NavigationMenuItem>
//       <ui:NavigationMenuItem Title=""Docs"" Clicked=""OnDocs"" />
//   </ui:NavigationMenu>
public class NavigationMenu : Border
{
    private readonly StackPanel _row = new() { Orientation = Orientation.Horizontal, Spacing = 4 };

    // The NavigationMenuItem children.
    [Content]
    public Controls Items => _row.Children;

    public NavigationMenu()
    {
        Child = _row;
        HorizontalAlignment = HorizontalAlignment.Left;
        _row.Children.CollectionChanged += (_, _) =>
        {
            foreach (var item in _row.Children.OfType<NavigationMenuItem>()) item.Menu = this;
        };
    }

    // Moves keyboard focus to the item before or after `from`, wrapping around.
    internal void FocusNeighbour(NavigationMenuItem from, int direction)
    {
        var items = _row.Children.OfType<NavigationMenuItem>().Where(i => i.IsEffectivelyEnabled).ToList();
        var index = items.IndexOf(from);
        if (index < 0 || items.Count < 2) return;
        items[(index + direction + items.Count) % items.Count].Focus(NavigationMethod.Directional);
    }
}

// A top-level entry — h-10 rounded-md px-4 text-sm font-medium, hover:bg-accent. With
// NavigationMenuLink children it opens a w-56 panel (rounded-md border bg-popover p-1 shadow-lg).
public class NavigationMenuItem : Border, IShellFocusable
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<NavigationMenuItem, string?>(nameof(Title));

    private readonly TextBlock _label;
    private readonly Icon _chevron;
    private readonly RotateTransform _turn = new();
    private readonly Border _trigger;
    private readonly StackPanel _stack = new();
    private readonly ShellAnchoredPopup _popup;

    static NavigationMenuItem()
    {
        TitleProperty.Changed.AddClassHandler<NavigationMenuItem>((i, _) => i.Update());
        IsEnabledProperty.Changed.AddClassHandler<NavigationMenuItem>((i, _) => i.Opacity = i.IsEnabled ? 1.0 : 0.5);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // The NavigationMenuLink children.
    [Content]
    public Controls Links => _stack.Children;

    public bool IsOpen => _popup.IsOpen;

    // Raised when an item without links is pressed.
    public event EventHandler? Clicked;

    internal NavigationMenu? Menu { get; set; }

    public NavigationMenuItem()
    {
        _label = new TextBlock { FontSize = 14, FontWeight = FontWeight.Medium, VerticalAlignment = VerticalAlignment.Center };
        _label.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        _chevron = new Icon
        {
            Kind = IconName.ChevronDown,
            Size = 12,
            Token = ShellToken.Foreground,
            VerticalAlignment = VerticalAlignment.Center,
            RenderTransform = _turn,
            RenderTransformOrigin = RelativePoint.Center,
            IsVisible = false
        };
        _trigger = new Border
        {
            Child = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, VerticalAlignment = VerticalAlignment.Center, Children = { _label, _chevron } },
            Height = 40,
            Padding = new Thickness(16, 0),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            // Transparent, not null, so the whole item takes the pointer.
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand)
        };

        var panel = new Border
        {
            Child = _stack,
            Padding = new Thickness(4),
            Width = 224,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            BoxShadow = ShellPopups.PanelShadow()
        };
        panel.Token(BackgroundProperty, ShellToken.Popover).Token(BorderBrushProperty, ShellToken.Border);

        _popup = new ShellAnchoredPopup(_trigger);
        _popup.Popup.Child = panel;
        _popup.Closed += (_, _) => Update();
        _stack.Children.CollectionChanged += (_, _) => Update();

        Child = new Panel { Children = { _trigger, _popup.Popup } };
        ShellFocus.Ring(this, _trigger);
        Update();
    }

    private bool HasLinks => _stack.Children.Count > 0;

    // Opens the panel, or raises Clicked when there are no links, as a click or Enter would.
    public void Press()
    {
        if (!IsEffectivelyEnabled) return;
        if (HasLinks) SetOpen(!IsOpen);
        else Clicked?.Invoke(this, EventArgs.Empty);
    }

    public void SetOpen(bool open)
    {
        if (open == _popup.IsOpen || (open && !HasLinks)) return;
        if (!open)
        {
            _popup.Close();
            return;
        }
        foreach (var link in _stack.Children.OfType<NavigationMenuLink>()) link.Owner = this;
        _popup.Open();
        Update();
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
        if (e.InitialPressMouseButton != MouseButton.Left) return;
        Press();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!ReferenceEquals(e.Source, this)) return;
        switch (e.Key)
        {
            case Key.Enter or Key.Space:
                Press();
                FocusFirstLink();
                break;
            case Key.Down:
                SetOpen(true);
                FocusFirstLink();
                break;
            case Key.Left:
                Menu?.FocusNeighbour(this, -1);
                break;
            case Key.Right:
                Menu?.FocusNeighbour(this, 1);
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    // Into the panel, so the arrow keys and Enter work on the links.
    private void FocusFirstLink()
    {
        if (IsOpen) _stack.Children.OfType<NavigationMenuLink>().FirstOrDefault(l => l.IsEffectivelyEnabled)?.Focus(NavigationMethod.Directional);
    }

    private void Update()
    {
        _label.Text = Title ?? string.Empty;
        _chevron.IsVisible = HasLinks;
        _turn.Angle = IsOpen ? 180 : 0;
        AutomationProperties.SetName(this, Title ?? string.Empty);
        if (IsOpen || (IsPointerOver && IsEffectivelyEnabled)) _trigger.Token(BackgroundProperty, ShellToken.Accent);
        else
        {
            _trigger.ClearToken(BackgroundProperty);
            _trigger.Background = Brushes.Transparent;
        }
    }
}

// A link in an item's panel — rounded-sm p-2, hover:bg-accent: a text-sm font-medium title over
// an optional muted description, with an optional icon. A tab stop that activates with Enter or
// Space; Up and Down move between the links.
public class NavigationMenuLink : Border
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<NavigationMenuLink, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<NavigationMenuLink, string?>(nameof(Description));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<NavigationMenuLink, IconName>(nameof(Icon));

    private readonly Icon _icon;
    private readonly TextBlock _title;
    private readonly TextBlock _description;

    static NavigationMenuLink()
    {
        TitleProperty.Changed.AddClassHandler<NavigationMenuLink>((l, _) => l.Update());
        DescriptionProperty.Changed.AddClassHandler<NavigationMenuLink>((l, _) => l.Update());
        IconProperty.Changed.AddClassHandler<NavigationMenuLink>((l, _) => l.Update());
        IsEnabledProperty.Changed.AddClassHandler<NavigationMenuLink>((l, _) => l.Opacity = l.IsEnabled ? 1.0 : 0.5);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public IconName Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public event EventHandler? Clicked;

    internal NavigationMenuItem? Owner { get; set; }

    public NavigationMenuLink()
    {
        _icon = new Icon { Size = 16, IsVisible = false, Token = ShellToken.PopoverForeground, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 2, 0, 0) };
        _title = new TextBlock { FontSize = 14, FontWeight = FontWeight.Medium };
        _title.Token(TextBlock.ForegroundProperty, ShellToken.PopoverForeground);
        _description = new TextBlock { FontSize = 13, IsVisible = false, TextWrapping = TextWrapping.Wrap };
        _description.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);

        var text = new StackPanel { Spacing = 2, Children = { _title, _description } };
        Grid.SetColumn(text, 1);
        Child = new Grid { ColumnDefinitions = new ColumnDefinitions(""Auto,*""), ColumnSpacing = 8, Children = { _icon, text } };
        Padding = new Thickness(8);
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
                var links = rows.Children.OfType<NavigationMenuLink>().Where(l => l.IsEffectivelyEnabled).ToList();
                var index = links.IndexOf(this);
                if (index < 0) return;
                links[(index + (e.Key == Key.Down ? 1 : -1) + links.Count) % links.Count].Focus(NavigationMethod.Directional);
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    // Picks the link in code, as a click or Enter/Space would.
    public void Press()
    {
        if (!IsEffectivelyEnabled) return;
        Owner?.Close();
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        _icon.Kind = Icon;
        _icon.IsVisible = Icon != IconName.None;
        _title.Text = Title ?? string.Empty;
        _description.Text = Description ?? string.Empty;
        _description.IsVisible = !string.IsNullOrEmpty(Description);
        AutomationProperties.SetName(this, Title ?? string.Empty);
    }
}
"
    };
}
