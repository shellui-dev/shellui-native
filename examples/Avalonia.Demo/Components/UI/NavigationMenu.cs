using System;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Navigation menu — a row of top-level links for a site or app header. An item with links
// inside shows a chevron and opens a panel of them (title and description); an item without
// is a plain link. Items are tab stops: Enter, Space or Down opens one, Left / Right move
// between them, and Up / Down move through an open panel.
//   <ui:NavigationMenu>
//       <ui:NavigationMenuItem Title="Getting started">
//           <ui:NavigationMenuLink Title="Introduction" Description="What ShellUI Native is." Clicked="OnLink" />
//           <ui:NavigationMenuLink Title="Installation" Description="Add it to your app." Clicked="OnLink" />
//       </ui:NavigationMenuItem>
//       <ui:NavigationMenuItem Title="Docs" Clicked="OnDocs" />
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
        Child = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*"), ColumnSpacing = 8, Children = { _icon, text } };
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
