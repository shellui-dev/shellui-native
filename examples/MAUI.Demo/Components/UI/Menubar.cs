using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Menu bar — h-10 rounded-md border bg-background p-1: a row of menus, each opening a panel of
// items under its title. Clicking outside, picking an item or pressing Escape closes it.
//   <ui:Menubar>
//       <ui:MenubarMenu Title="File">
//           <ui:MenubarItem Text="New tab" Shortcut="Ctrl+T" Clicked="OnNewTab" />
//           <ui:MenubarSeparator />
//           <ui:MenubarItem Text="Print" Icon="FileText" Clicked="OnPrint" />
//       </ui:MenubarMenu>
//       <ui:MenubarMenu Title="Edit"> ... </ui:MenubarMenu>
//   </ui:Menubar>
[ContentProperty(nameof(Menus))]
public partial class Menubar : ContentView
{
    public IList<IView> Menus => _row.Children;

    private readonly HorizontalStackLayout _row;

    public Menubar()
    {
        _row = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };
        var bar = new Border
        {
            Content = _row,
            HeightRequest = 40,
            Padding = new Thickness(4),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd }
        };
        bar.Token(VisualElement.BackgroundColorProperty, ShellToken.Background);
        bar.Token(Border.StrokeProperty, ShellToken.Border);
        Content = bar;
        HorizontalOptions = LayoutOptions.Start;
    }
}

// One menu of a Menubar: the title in the bar (rounded-sm px-3 text-sm font-medium) and the
// panel that opens under it (min-w-[12rem] rounded-md border bg-popover p-1 shadow-md).
[ContentProperty(nameof(Items))]
public partial class MenubarMenu : ContentView, IShellPopup
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(MenubarMenu), string.Empty,
            propertyChanged: (b, o, n) => ((MenubarMenu)b).Update());

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IList<IView> Items => _stack.Children;

    public bool IsOpen { get; private set; }

    public event EventHandler? Opened;
    public event EventHandler? Closed;

    private readonly Border _trigger;
    private readonly Label _label;
    private readonly VerticalStackLayout _stack;
    private readonly Border _panel;
    private ShellPopupHandle? _handle;
    private bool _hovered;

    public MenubarMenu()
    {
        _label = new Label { FontSize = 14, FontAttributes = FontAttributes.Bold, VerticalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center, LineBreakMode = LineBreakMode.NoWrap };
        _label.Token(Label.TextColorProperty, ShellToken.Foreground);
        _trigger = new Border
        {
            Content = _label,
            HeightRequest = 30,
            Padding = new Thickness(12, 0),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusSm },
            BackgroundColor = Colors.Transparent
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { _hovered = true; Update(); };
        pointer.PointerExited += (_, _) => { _hovered = false; Update(); };
        _trigger.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (!IsEnabled) return; ShellFocus.FocusPressed(this); SetOpen(!IsOpen); };
        _trigger.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, () => { if (IsEnabled) SetOpen(!IsOpen); });

        _stack = new VerticalStackLayout { Spacing = 0 };
        _panel = new Border
        {
            Content = _stack,
            Padding = new Thickness(4),
            MinimumWidthRequest = 192,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            Shadow = ShellPopups.PanelShadow()
        };
        _panel.Token(VisualElement.BackgroundColorProperty, ShellToken.Popover);
        _panel.Token(Border.StrokeProperty, ShellToken.Border);
        // Taps on the panel padding must not fall through to the click-outside catcher.
        _panel.GestureRecognizers.Add(new TapGestureRecognizer());

        Content = _trigger;
        VerticalOptions = LayoutOptions.Center;
        Update();
    }

    public void SetOpen(bool open)
    {
        if (IsOpen == open) return;
        IsOpen = open;
        Update();
        if (open)
        {
            // The panel floats outside this view, so items can't find their menu by walking up.
            foreach (var item in _stack.Children.OfType<MenubarItem>()) item.Menu = this;
            ShellPopups.Opened(this);
            _handle = ShellPortal.ShowPopup(_trigger, _panel, new ShellPopupOptions { Owner = this, OnDismiss = Close });
            Opened?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            ShellPopups.Closed(this);
            var handle = _handle;
            _handle = null;
            if (handle != null) _ = handle.CloseAsync();
            Closed?.Invoke(this, EventArgs.Empty);
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

// Menu row — rounded-sm px-2 py-1.5 text-sm, hover:bg-accent, optional icon and shortcut hint.
public partial class MenubarItem : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(MenubarItem), string.Empty,
            propertyChanged: (b, o, n) => ((MenubarItem)b).Update());

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(MenubarItem), IconName.None,
            propertyChanged: (b, o, n) => ((MenubarItem)b).Update());

    // Keyboard hint shown at the end of the row (display only).
    public static readonly BindableProperty ShortcutProperty =
        BindableProperty.Create(nameof(Shortcut), typeof(string), typeof(MenubarItem), string.Empty,
            propertyChanged: (b, o, n) => ((MenubarItem)b).Update());

    public static readonly BindableProperty IsDestructiveProperty =
        BindableProperty.Create(nameof(IsDestructive), typeof(bool), typeof(MenubarItem), false,
            propertyChanged: (b, o, n) => ((MenubarItem)b).Update());

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

    internal MenubarMenu? Menu { get; set; }

    private readonly Border _row;
    private readonly Icon _icon;
    private readonly Label _label;
    private readonly Label _shortcut;

    public MenubarItem()
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
        Menu?.Close();
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
public partial class MenubarSeparator : ContentView
{
    public MenubarSeparator()
    {
        var line = new BoxView { HeightRequest = 1, BackgroundColor = Colors.Transparent };
        line.Token(BoxView.ColorProperty, ShellToken.Border);
        Margin = new Thickness(-4, 4);
        Content = line;
    }
}
