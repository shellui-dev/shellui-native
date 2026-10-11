using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class ContextMenuTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "context-menu",
        DisplayName = "Context Menu",
        Description = "Right-click / long-press menu that opens at the pointer",
        Category = ComponentCategory.Overlay,
        FilePath = "ContextMenu.cs",
        Dependencies = new List<string> { "shell", "icon", "element-extensions" },
        Tags = new List<string> { "context", "menu", "right-click", "long-press" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Context menu — right-click (long-press on touch) the trigger to open a menu at the pointer.
// Clicking outside, picking an item or pressing Escape closes it.
//   <ui:ContextMenu>
//       <ui:ContextMenuTrigger> ...any view... </ui:ContextMenuTrigger>
//       <ui:ContextMenuContent>
//           <ui:ContextMenuItem Text=""Copy"" Icon=""Copy"" Shortcut=""Ctrl+C"" Clicked=""OnCopy"" />
//           <ui:ContextMenuSeparator />
//           <ui:ContextMenuItem Text=""Delete"" Icon=""Trash2"" IsDestructive=""True"" Clicked=""OnDelete"" />
//       </ui:ContextMenuContent>
//   </ui:ContextMenu>
public partial class ContextMenu : ShellPopoverHost
{
    private Point? _point;

    public ContextMenu()
    {
        // The trigger usually is a surface (a card, a row): let it fill its container.
        HorizontalOptions = LayoutOptions.Fill;
    }

    protected override bool IsContent(Element child) => child is ContextMenuContent;
    protected override Point? AnchorPoint => _point;
    protected override double Offset => _point is null ? 4 : 0;

    // Opens the menu at a point relative to this view, or below the trigger when null.
    public void OpenAt(Point? point)
    {
        if (IsOpen) IsOpen = false;
        _point = point;
        IsOpen = true;
    }
}

// The surface that opens the enclosing ContextMenu on right-click / long-press.
public partial class ContextMenuTrigger : ContentView
{
#if ANDROID || IOS || MACCATALYST
    private object? _hooked;
#endif

    public ContextMenuTrigger()
    {
        var secondary = new TapGestureRecognizer { Buttons = ButtonsMask.Secondary };
        secondary.Tapped += (_, e) =>
        {
            var menu = this.FindParentOfType<ContextMenu>();
            menu?.OpenAt(e.GetPosition(menu));
        };
        GestureRecognizers.Add(secondary);
        HandlerChanged += (_, _) => HookLongPress();
    }

    private void Open(Point? point) => this.FindParentOfType<ContextMenu>()?.OpenAt(point);

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName != nameof(Content)) return;
        if (Content is View content) content.HandlerChanged += (_, _) => HookLongPress();
        HookLongPress();
    }

    private void HookLongPress()
    {
#if ANDROID
        // Listen on the content, not on this view: this view's gesture handling takes the touch
        // stream before Android's own long-press detection would see it.
        var native = (Content as View)?.Handler?.PlatformView as Android.Views.View
                     ?? Handler?.PlatformView as Android.Views.View;
        if (native is null || ReferenceEquals(native, _hooked)) return;
        _hooked = native;
        native.LongClickable = true;
        native.LongClick += (_, e) => { e.Handled = true; Open(null); };
#elif IOS || MACCATALYST
        if (Handler?.PlatformView is not UIKit.UIView native || ReferenceEquals(native, _hooked)) return;
        _hooked = native;
        native.AddGestureRecognizer(new UIKit.UILongPressGestureRecognizer(gesture =>
        {
            if (gesture.State == UIKit.UIGestureRecognizerState.Began) Open(null);
        }));
#endif
    }
}

// Menu panel — min-w-[13rem] rounded-md border bg-popover p-1 shadow-md.
[ContentProperty(nameof(Children))]
public partial class ContextMenuContent : ContentView
{
    private readonly VerticalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public ContextMenuContent()
    {
        _stack = new VerticalStackLayout { Spacing = 0 };
        var panel = new Border
        {
            Content = _stack,
            Padding = new Thickness(4),
            MinimumWidthRequest = 208,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            Shadow = ShellPopups.PanelShadow()
        };
        panel.Token(VisualElement.BackgroundColorProperty, ShellToken.Popover);
        panel.Token(Border.StrokeProperty, ShellToken.Border);
        // Taps on the panel padding must not fall through to the click-outside catcher.
        panel.GestureRecognizers.Add(new TapGestureRecognizer());
        Content = panel;
    }
}

// Menu row — rounded-sm px-2 py-1.5 text-sm, hover:bg-accent, optional icon and shortcut hint.
public partial class ContextMenuItem : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(ContextMenuItem), string.Empty,
            propertyChanged: (b, o, n) => ((ContextMenuItem)b).Update());

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(ContextMenuItem), IconName.None,
            propertyChanged: (b, o, n) => ((ContextMenuItem)b).Update());

    // Keyboard hint shown at the end of the row (display only).
    public static readonly BindableProperty ShortcutProperty =
        BindableProperty.Create(nameof(Shortcut), typeof(string), typeof(ContextMenuItem), string.Empty,
            propertyChanged: (b, o, n) => ((ContextMenuItem)b).Update());

    public static readonly BindableProperty IsDestructiveProperty =
        BindableProperty.Create(nameof(IsDestructive), typeof(bool), typeof(ContextMenuItem), false,
            propertyChanged: (b, o, n) => ((ContextMenuItem)b).Update());

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IconName Icon
    {
        get => (IconName)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Shortcut
    {
        get => (string)GetValue(ShortcutProperty);
        set => SetValue(ShortcutProperty, value);
    }

    public bool IsDestructive
    {
        get => (bool)GetValue(IsDestructiveProperty);
        set => SetValue(IsDestructiveProperty, value);
    }

    public event EventHandler? Clicked;

    private readonly Border _row;
    private readonly Icon _icon;
    private readonly Label _label;
    private readonly Label _shortcut;

    public ContextMenuItem()
    {
        _icon = new Icon { Size = 16, IsVisible = false, VerticalOptions = LayoutOptions.Center };
        _label = new Label { FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center };
        _shortcut = new Label { FontSize = 12, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center, IsVisible = false };
        _shortcut.Token(Label.TextColorProperty, ShellToken.MutedForeground);

        var grid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)
            }
        };
        grid.Add(_icon, 0, 0);
        grid.Add(_label, 1, 0);
        grid.Add(_shortcut, 2, 0);

        _row = new Border
        {
            Content = grid,
            HeightRequest = 32,
            Padding = new Thickness(8, 0),
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
        this.FindParentOfType<ContextMenu>()?.Close();
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        var color = IsDestructive ? ShellToken.Destructive : ShellToken.PopoverForeground;
        _icon.Name = Icon;
        _icon.IsVisible = Icon != IconName.None;
        _icon.Token = color;
        _label.Text = Text ?? string.Empty;
        _label.Token(Label.TextColorProperty, color);
        _shortcut.Text = Shortcut ?? string.Empty;
        _shortcut.IsVisible = !string.IsNullOrEmpty(Shortcut);
    }
}

// Divider between groups of items — -mx-1 my-1 h-px bg-border.
public partial class ContextMenuSeparator : ContentView
{
    public ContextMenuSeparator()
    {
        var line = new BoxView { HeightRequest = 1, BackgroundColor = Colors.Transparent };
        line.Token(BoxView.ColorProperty, ShellToken.Border);
        Margin = new Thickness(-4, 4);
        Content = line;
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace YourProjectNamespace.Components.UI;

// Context menu — right-click the trigger (long-press on touch, or the Menu key / Shift+F10) to
// open a menu at the pointer. Clicking outside, picking an item or pressing Escape closes it.
//   <ui:ContextMenu>
//       <ui:ContextMenuTrigger> ...any control... </ui:ContextMenuTrigger>
//       <ui:ContextMenuContent>
//           <ui:ContextMenuItem Text=""Copy"" Icon=""Copy"" Shortcut=""Ctrl+C"" Clicked=""OnCopy"" />
//           <ui:ContextMenuSeparator />
//           <ui:ContextMenuItem Text=""Delete"" Icon=""Trash2"" IsDestructive=""True"" Clicked=""OnDelete"" />
//       </ui:ContextMenuContent>
//   </ui:ContextMenu>
public class ContextMenu : ShellPopoverHost
{
    private bool _atPoint;

    public ContextMenu()
    {
        // The trigger usually is a surface (a card, a row): let it fill its container.
        HorizontalAlignment = HorizontalAlignment.Stretch;
        // Clicks on the trigger reach it while the menu is open, so a second right-click moves the menu.
        Popup.OverlayInputPassThroughElement = this;
    }

    protected override bool IsContent(Control child) => child is ContextMenuContent;
    protected override double Offset => _atPoint ? 0 : 4;

    // Opens the menu at a point relative to this control, or below the trigger when null.
    public void OpenAt(Point? point)
    {
        _atPoint = point.HasValue;
        Popup.PlacementRect = point is { } at ? new Rect(at, new Size(1, 1)) : null;
        if (IsOpen) Popup.VerticalOffset = Offset;
        else IsOpen = true;
    }
}

// The surface that opens the enclosing ContextMenu.
public class ContextMenuTrigger : Border
{
    public ContextMenuTrigger()
    {
        // Transparent, not null, so empty areas of the surface take the right-click too.
        Background = Brushes.Transparent;
        AddHandler(ContextRequestedEvent, OnContextRequested);
    }

    private void OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (this.FindParentOfType<ContextMenu>() is not { } menu) return;
        menu.OpenAt(e.TryGetPosition(menu, out var point) ? point : null);
        e.Handled = true;
    }

    // A long-press on touch opens the menu under the trigger.
    protected override void OnHolding(HoldingRoutedEventArgs e)
    {
        base.OnHolding(e);
        if (e.HoldingState != HoldingState.Started || this.FindParentOfType<ContextMenu>() is not { IsOpen: false } menu) return;
        menu.OpenAt(null);
        e.Handled = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            this.FindParentOfType<ContextMenu>()?.Close();
    }
}

// Menu panel — min-w-[13rem] rounded-md border bg-popover p-1 shadow-md. XAML children go to Items.
public class ContextMenuContent : Border
{
    private readonly StackPanel _stack = new();

    [Content]
    public Controls Items => _stack.Children;

    public ContextMenuContent()
    {
        Child = _stack;
        Padding = new Thickness(4);
        MinWidth = 208;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        BoxShadow = ShellPopups.PanelShadow();
        this.Token(BackgroundProperty, ShellToken.Popover).Token(BorderBrushProperty, ShellToken.Border);
    }
}

// Menu row — rounded-sm px-2 py-1.5 text-sm, hover:bg-accent, optional icon and shortcut hint.
public class ContextMenuItem : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<ContextMenuItem, string?>(nameof(Text));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<ContextMenuItem, IconName>(nameof(Icon));

    // Keyboard hint shown at the end of the row (display only).
    public static readonly StyledProperty<string?> ShortcutProperty =
        AvaloniaProperty.Register<ContextMenuItem, string?>(nameof(Shortcut));

    public static readonly StyledProperty<bool> IsDestructiveProperty =
        AvaloniaProperty.Register<ContextMenuItem, bool>(nameof(IsDestructive));

    private readonly Icon _icon;
    private readonly TextBlock _label;
    private readonly TextBlock _shortcut;

    static ContextMenuItem()
    {
        TextProperty.Changed.AddClassHandler<ContextMenuItem>((i, _) => i.Update());
        IconProperty.Changed.AddClassHandler<ContextMenuItem>((i, _) => i.Update());
        ShortcutProperty.Changed.AddClassHandler<ContextMenuItem>((i, _) => i.Update());
        IsDestructiveProperty.Changed.AddClassHandler<ContextMenuItem>((i, _) => i.Update());
        IsEnabledProperty.Changed.AddClassHandler<ContextMenuItem>((i, _) => i.Opacity = i.IsEnabled ? 1.0 : 0.5);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IconName Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? Shortcut
    {
        get => GetValue(ShortcutProperty);
        set => SetValue(ShortcutProperty, value);
    }

    public bool IsDestructive
    {
        get => GetValue(IsDestructiveProperty);
        set => SetValue(IsDestructiveProperty, value);
    }

    public event EventHandler? Clicked;

    public ContextMenuItem()
    {
        _icon = new Icon { Size = 16, IsVisible = false, VerticalAlignment = VerticalAlignment.Center };
        _label = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
        _shortcut = new TextBlock { FontSize = 12, VerticalAlignment = VerticalAlignment.Center, IsVisible = false };
        _shortcut.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions(""Auto,*,Auto""), ColumnSpacing = 8 };
        Grid.SetColumn(_label, 1);
        Grid.SetColumn(_shortcut, 2);
        grid.Children.Add(_icon);
        grid.Children.Add(_label);
        grid.Children.Add(_shortcut);

        Child = grid;
        Height = 32;
        Padding = new Thickness(8, 0);
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
        if (e.Key is not (Key.Enter or Key.Space)) return;
        Press();
        e.Handled = true;
    }

    // Picks the item in code, as a click or Enter/Space would.
    public void Press()
    {
        if (!IsEffectivelyEnabled) return;
        this.FindParentOfType<ContextMenu>()?.Close();
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        var color = IsDestructive ? ShellToken.Destructive : ShellToken.PopoverForeground;
        _icon.Kind = Icon;
        _icon.IsVisible = Icon != IconName.None;
        _icon.Token = color;
        _label.Text = Text ?? string.Empty;
        _label.Token(TextBlock.ForegroundProperty, color);
        _shortcut.Text = Shortcut ?? string.Empty;
        _shortcut.IsVisible = !string.IsNullOrEmpty(Shortcut);
    }
}

// Divider between groups of items — -mx-1 my-1 h-px bg-border.
public class ContextMenuSeparator : Border
{
    public ContextMenuSeparator()
    {
        Height = 1;
        Margin = new Thickness(-4, 4);
        this.Token(BackgroundProperty, ShellToken.Border);
    }
}
"
    };
}
