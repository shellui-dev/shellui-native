using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class MenubarTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "menubar",
        DisplayName = "Menubar",
        Description = "Row of menus that open under their titles",
        Category = ComponentCategory.Navigation,
        FilePath = "Menubar.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "menubar", "menu", "navigation" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Menu bar — h-10 rounded-md border bg-background p-1: a row of menus, each opening a panel of
// items under its title. Clicking outside, picking an item or pressing Escape closes it.
//   <ui:Menubar>
//       <ui:MenubarMenu Title=""File"">
//           <ui:MenubarItem Text=""New tab"" Shortcut=""Ctrl+T"" Clicked=""OnNewTab"" />
//           <ui:MenubarSeparator />
//           <ui:MenubarItem Text=""Print"" Icon=""FileText"" Clicked=""OnPrint"" />
//       </ui:MenubarMenu>
//       <ui:MenubarMenu Title=""Edit""> ... </ui:MenubarMenu>
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

// Menu bar — h-10 rounded-md border bg-background p-1: a row of menus, each opening a panel of
// items under its title. Clicking outside, picking an item or pressing Escape closes it. The
// titles are tab stops: Enter, Space or Down opens a menu, and Left / Right move between them.
//   <ui:Menubar>
//       <ui:MenubarMenu Title=""File"">
//           <ui:MenubarItem Text=""New tab"" Shortcut=""Ctrl+T"" Clicked=""OnNewTab"" />
//           <ui:MenubarSeparator />
//           <ui:MenubarItem Text=""Print"" Icon=""FileText"" Clicked=""OnPrint"" />
//       </ui:MenubarMenu>
//       <ui:MenubarMenu Title=""Edit""> ... </ui:MenubarMenu>
//   </ui:Menubar>
public class Menubar : Border
{
    private readonly StackPanel _row = new() { Orientation = Orientation.Horizontal, Spacing = 4, VerticalAlignment = VerticalAlignment.Center };

    // The MenubarMenu children.
    [Content]
    public Controls Menus => _row.Children;

    public Menubar()
    {
        Child = _row;
        Height = 40;
        Padding = new Thickness(3);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        HorizontalAlignment = HorizontalAlignment.Left;
        this.Token(BackgroundProperty, ShellToken.Background).Token(BorderBrushProperty, ShellToken.Border);
        _row.Children.CollectionChanged += (_, _) =>
        {
            foreach (var menu in _row.Children.OfType<MenubarMenu>()) menu.Bar = this;
        };
    }

    // Moves keyboard focus to the menu before or after `from`, wrapping around.
    internal void FocusNeighbour(MenubarMenu from, int direction)
    {
        var menus = _row.Children.OfType<MenubarMenu>().Where(m => m.IsEffectivelyEnabled).ToList();
        var index = menus.IndexOf(from);
        if (index < 0 || menus.Count < 2) return;
        menus[(index + direction + menus.Count) % menus.Count].Focus(NavigationMethod.Directional);
    }
}

// One menu of a Menubar: the title in the bar (rounded-sm px-3 text-sm font-medium) and the
// panel that opens under it (min-w-[12rem] rounded-md border bg-popover p-1 shadow-md).
public class MenubarMenu : Border, IShellFocusable
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<MenubarMenu, string?>(nameof(Title));

    private readonly TextBlock _label;
    private readonly Border _trigger;
    private readonly StackPanel _stack = new();
    private readonly ShellAnchoredPopup _popup;

    static MenubarMenu()
    {
        TitleProperty.Changed.AddClassHandler<MenubarMenu>((m, _) => m.Update());
        IsEnabledProperty.Changed.AddClassHandler<MenubarMenu>((m, _) => m.Opacity = m.IsEnabled ? 1.0 : 0.5);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    // The MenubarItem and MenubarSeparator children.
    [Content]
    public Controls Items => _stack.Children;

    public bool IsOpen => _popup.IsOpen;

    public event EventHandler? Opened;
    public event EventHandler? Closed;

    internal Menubar? Bar { get; set; }

    public MenubarMenu()
    {
        _label = new TextBlock { FontSize = 14, FontWeight = FontWeight.Medium, VerticalAlignment = VerticalAlignment.Center };
        _label.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        _trigger = new Border
        {
            Child = _label,
            Height = 32,
            Padding = new Thickness(12, 0),
            CornerRadius = new CornerRadius(ShellTheme.RadiusSm),
            // Transparent, not null, so the whole title takes the pointer.
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand)
        };

        var panel = new Border
        {
            Child = _stack,
            Padding = new Thickness(4),
            MinWidth = 192,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            BoxShadow = ShellPopups.PanelShadow()
        };
        panel.Token(BackgroundProperty, ShellToken.Popover).Token(BorderBrushProperty, ShellToken.Border);

        _popup = new ShellAnchoredPopup(_trigger);
        _popup.Popup.Child = panel;
        _popup.Closed += (_, _) =>
        {
            Update();
            Closed?.Invoke(this, EventArgs.Empty);
        };

        Child = new Panel { Children = { _trigger, _popup.Popup } };
        VerticalAlignment = VerticalAlignment.Center;
        ShellFocus.Ring(this, _trigger);
        Update();
    }

    public void SetOpen(bool open)
    {
        if (open == _popup.IsOpen) return;
        if (!open)
        {
            _popup.Close();
            return;
        }
        foreach (var item in _stack.Children.OfType<MenubarItem>()) item.Menu = this;
        _popup.Open();
        Update();
        Opened?.Invoke(this, EventArgs.Empty);
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
        if (e.InitialPressMouseButton != MouseButton.Left || !IsEffectivelyEnabled) return;
        SetOpen(!IsOpen);
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!ReferenceEquals(e.Source, this)) return;
        switch (e.Key)
        {
            case Key.Enter or Key.Space or Key.Down:
                SetOpen(true);
                // Into the panel, so Tab and Enter work on the items.
                _stack.Children.OfType<MenubarItem>().FirstOrDefault(i => i.IsEffectivelyEnabled)?.Focus(NavigationMethod.Directional);
                break;
            case Key.Left:
                Bar?.FocusNeighbour(this, -1);
                break;
            case Key.Right:
                Bar?.FocusNeighbour(this, 1);
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    private void Update()
    {
        _label.Text = Title ?? string.Empty;
        AutomationProperties.SetName(this, Title ?? string.Empty);
        if (IsOpen || (IsPointerOver && IsEffectivelyEnabled)) _trigger.Token(BackgroundProperty, ShellToken.Accent);
        else
        {
            _trigger.ClearToken(BackgroundProperty);
            _trigger.Background = Brushes.Transparent;
        }
    }
}

// Menu row — rounded-sm px-2 py-1.5 text-sm, hover:bg-accent, optional icon and shortcut hint.
// A tab stop that activates with Enter or Space; Up and Down move between the rows.
public class MenubarItem : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<MenubarItem, string?>(nameof(Text));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<MenubarItem, IconName>(nameof(Icon));

    // Keyboard hint shown at the end of the row (display only).
    public static readonly StyledProperty<string?> ShortcutProperty =
        AvaloniaProperty.Register<MenubarItem, string?>(nameof(Shortcut));

    public static readonly StyledProperty<bool> IsDestructiveProperty =
        AvaloniaProperty.Register<MenubarItem, bool>(nameof(IsDestructive));

    private readonly Icon _icon;
    private readonly TextBlock _label;
    private readonly TextBlock _shortcut;

    static MenubarItem()
    {
        TextProperty.Changed.AddClassHandler<MenubarItem>((i, _) => i.Update());
        IconProperty.Changed.AddClassHandler<MenubarItem>((i, _) => i.Update());
        ShortcutProperty.Changed.AddClassHandler<MenubarItem>((i, _) => i.Update());
        IsDestructiveProperty.Changed.AddClassHandler<MenubarItem>((i, _) => i.Update());
        IsEnabledProperty.Changed.AddClassHandler<MenubarItem>((i, _) => i.Opacity = i.IsEnabled ? 1.0 : 0.5);
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

    internal MenubarMenu? Menu { get; set; }

    public MenubarItem()
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
        switch (e.Key)
        {
            case Key.Enter or Key.Space:
                Press();
                break;
            case Key.Up or Key.Down:
                if (Parent is not Panel rows) return;
                var items = rows.Children.OfType<MenubarItem>().Where(i => i.IsEffectivelyEnabled).ToList();
                var index = items.IndexOf(this);
                if (index < 0) return;
                items[(index + (e.Key == Key.Down ? 1 : -1) + items.Count) % items.Count].Focus(NavigationMethod.Directional);
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    // Picks the item in code, as a click or Enter/Space would.
    public void Press()
    {
        if (!IsEffectivelyEnabled) return;
        Menu?.Close();
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
public class MenubarSeparator : Border
{
    public MenubarSeparator()
    {
        Height = 1;
        Margin = new Thickness(-4, 4);
        this.Token(BackgroundProperty, ShellToken.Border);
    }
}
"
    };
}
