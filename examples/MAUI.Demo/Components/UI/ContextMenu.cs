using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Context menu — right-click (long-press on touch) the trigger to open a menu at the pointer.
// Clicking outside, picking an item or pressing Escape closes it.
//   <ui:ContextMenu>
//       <ui:ContextMenuTrigger> ...any view... </ui:ContextMenuTrigger>
//       <ui:ContextMenuContent>
//           <ui:ContextMenuItem Text="Copy" Icon="Copy" Shortcut="Ctrl+C" Clicked="OnCopy" />
//           <ui:ContextMenuSeparator />
//           <ui:ContextMenuItem Text="Delete" Icon="Trash2" IsDestructive="True" Clicked="OnDelete" />
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
