using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Command menu — a search field over a list of grouped items (rounded-md border bg-popover).
// Typing filters the list; Up / Down move the highlight and Enter picks it.
//   <ui:Command Placeholder="Type a command or search..." ItemSelected="OnCommand">
//       <ui:CommandGroup Heading="Suggestions">
//           <ui:CommandItem Text="Calendar" Icon="Calendar" Value="calendar" />
//           <ui:CommandItem Text="Settings" Icon="Settings" Shortcut="Ctrl+," Keywords="preferences options" />
//       </ui:CommandGroup>
//       <ui:CommandSeparator />
//       <ui:CommandGroup Heading="Account"> ... </ui:CommandGroup>
//   </ui:Command>
// CommandPalette shows the same list in a dialog over the window.
public class Command : Border
{
    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<Command, string?>(nameof(Placeholder), "Type a command or search...");

    // Shown when nothing matches the search.
    public static readonly StyledProperty<string?> EmptyTextProperty =
        AvaloniaProperty.Register<Command, string?>(nameof(EmptyText), "No results found.");

    // The list scrolls past this height.
    public static readonly StyledProperty<double> MaxListHeightProperty =
        AvaloniaProperty.Register<Command, double>(nameof(MaxListHeight), 300);

    private readonly TextBox _textBox;
    private readonly TextBlock _empty;
    private readonly StackPanel _list = new();
    private readonly ScrollViewer _scroll;
    private CommandItem? _active;

    static Command()
    {
        PlaceholderProperty.Changed.AddClassHandler<Command>((c, e) => c._textBox.PlaceholderText = (string?)e.NewValue);
        EmptyTextProperty.Changed.AddClassHandler<Command>((c, e) => c._empty.Text = (string?)e.NewValue);
        MaxListHeightProperty.Changed.AddClassHandler<Command>((c, e) => c._scroll.MaxHeight = (double)e.NewValue!);
    }

    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string? EmptyText
    {
        get => GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public double MaxListHeight
    {
        get => GetValue(MaxListHeightProperty);
        set => SetValue(MaxListHeightProperty, value);
    }

    // CommandGroup, CommandItem and CommandSeparator children.
    [Content]
    public Controls Groups => _list.Children;

    // The search text.
    public string Query => _textBox.Text ?? string.Empty;

    public event EventHandler<CommandItem>? ItemSelected;

    public Command()
    {
        _textBox = new TextBox
        {
            FontSize = 14,
            PlaceholderText = Placeholder,
            VerticalAlignment = VerticalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        ShellPlatform.StripNativeChrome(_textBox);
        _textBox.TextChanged += (_, _) => Filter();
        // Tunnel: the text box would use Up / Down itself.
        _textBox.AddHandler(KeyDownEvent, OnSearchKeyDown, RoutingStrategies.Tunnel);

        Grid.SetColumn(_textBox, 1);
        var search = new Grid
        {
            Height = 44,
            Margin = new Thickness(12, 0),
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            ColumnSpacing = 8,
            Children = { new Icon { Kind = IconName.Search, Size = 16, Token = ShellToken.MutedForeground, VerticalAlignment = VerticalAlignment.Center }, _textBox }
        };
        var line = new Border { Height = 1 };
        line.Token(BackgroundProperty, ShellToken.Border);

        _empty = new TextBlock { Text = EmptyText, FontSize = 14, TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 24), IsVisible = false };
        _empty.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        _scroll = new ScrollViewer
        {
            Content = new StackPanel { Margin = new Thickness(4), Children = { _empty, _list } },
            MaxHeight = MaxListHeight,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        Child = new StackPanel { Children = { search, line, _scroll } };
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        ClipToBounds = true;
        this.Token(BackgroundProperty, ShellToken.Popover).Token(BorderBrushProperty, ShellToken.Border);
        _list.Children.CollectionChanged += (_, _) => Filter();
    }

    // The Command an item or group sits in.
    internal static Command? Of(StyledElement? element)
    {
        for (var parent = element?.Parent; parent != null; parent = parent.Parent)
            if (parent is Command command) return command;
        return null;
    }

    // Used by CommandPalette, which draws the border itself.
    internal void RemoveBorder()
    {
        BorderThickness = default;
        CornerRadius = default;
        this.ClearToken(BackgroundProperty);
        Background = null;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Filter();
    }

    public new bool Focus(NavigationMethod method = NavigationMethod.Unspecified, KeyModifiers keyModifiers = KeyModifiers.None)
        => _textBox.Focus(method, keyModifiers);

    // Clears the search.
    public void Reset() => _textBox.Text = string.Empty;

    // Every item, in order, whether it sits in a group or directly in the list.
    public IEnumerable<CommandItem> Items()
    {
        foreach (var child in _list.Children)
        {
            if (child is CommandItem item) yield return item;
            else if (child is CommandGroup group)
                foreach (var grouped in group.Items.OfType<CommandItem>()) yield return grouped;
        }
    }

    internal void Select(CommandItem item)
    {
        if (!item.IsEffectivelyEnabled) return;
        ItemSelected?.Invoke(this, item);
        item.RaiseClicked();
    }

    internal void SetActive(CommandItem? item)
    {
        if (ReferenceEquals(_active, item)) return;
        _active?.Apply(false);
        _active = item;
        _active?.Apply(true);
    }

    private void OnSearchKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down or Key.Up:
                var items = Items().Where(i => i.IsVisible && i.IsEnabled).ToList();
                if (items.Count == 0) break;
                var index = _active is null ? -1 : items.IndexOf(_active);
                var next = e.Key == Key.Down ? (index + 1) % items.Count : (index <= 0 ? items.Count : index) - 1;
                SetActive(items[next]);
                items[next].BringIntoView();
                break;
            case Key.Enter:
                if (_active != null) Select(_active);
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    private void Filter()
    {
        var words = Query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var any = false;
        foreach (var item in Items())
        {
            item.IsVisible = words.All(item.Matches);
            any |= item.IsVisible;
        }
        foreach (var child in _list.Children)
        {
            if (child is CommandGroup group) group.IsVisible = group.Items.OfType<CommandItem>().Any(i => i.IsVisible);
            // Dividers only make sense between groups, so they go while the list is filtered.
            else if (child is CommandSeparator separator) separator.IsVisible = words.Length == 0;
        }
        _empty.IsVisible = !any;
        SetActive(Items().FirstOrDefault(i => i.IsVisible && i.IsEnabled));
    }
}

// A labelled section of a Command — the heading is px-2 py-1.5 text-xs font-medium muted.
public class CommandGroup : Border
{
    public static readonly StyledProperty<string?> HeadingProperty =
        AvaloniaProperty.Register<CommandGroup, string?>(nameof(Heading));

    private readonly TextBlock _heading;
    private readonly StackPanel _items = new();

    static CommandGroup()
    {
        HeadingProperty.Changed.AddClassHandler<CommandGroup>((g, _) => g.Update());
    }

    public string? Heading
    {
        get => GetValue(HeadingProperty);
        set => SetValue(HeadingProperty, value);
    }

    [Content]
    public Controls Items => _items.Children;

    public CommandGroup()
    {
        _heading = new TextBlock { FontSize = 12, FontWeight = FontWeight.Medium, Margin = new Thickness(8, 6), IsVisible = false };
        _heading.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        Child = new StackPanel { Children = { _heading, _items } };
    }

    private void Update()
    {
        _heading.Text = Heading ?? string.Empty;
        _heading.IsVisible = !string.IsNullOrEmpty(Heading);
    }
}

// One row of a Command — rounded-sm px-2 h-9 text-sm; the highlighted row is bg-accent.
public class CommandItem : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<CommandItem, string?>(nameof(Text));

    // Identifies the item to your code.
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<CommandItem, string?>(nameof(Value));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<CommandItem, IconName>(nameof(Icon));

    // Keyboard hint shown at the end of the row (display only).
    public static readonly StyledProperty<string?> ShortcutProperty =
        AvaloniaProperty.Register<CommandItem, string?>(nameof(Shortcut));

    // Extra words the search matches besides Text.
    public static readonly StyledProperty<string?> KeywordsProperty =
        AvaloniaProperty.Register<CommandItem, string?>(nameof(Keywords));

    private readonly Icon _icon;
    private readonly TextBlock _label;
    private readonly TextBlock _shortcut;

    static CommandItem()
    {
        TextProperty.Changed.AddClassHandler<CommandItem>((i, _) => i.Update());
        IconProperty.Changed.AddClassHandler<CommandItem>((i, _) => i.Update());
        ShortcutProperty.Changed.AddClassHandler<CommandItem>((i, _) => i.Update());
        IsEnabledProperty.Changed.AddClassHandler<CommandItem>((i, _) => i.Opacity = i.IsEnabled ? 1.0 : 0.5);
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

    public string? Shortcut
    {
        get => GetValue(ShortcutProperty);
        set => SetValue(ShortcutProperty, value);
    }

    public string? Keywords
    {
        get => GetValue(KeywordsProperty);
        set => SetValue(KeywordsProperty, value);
    }

    public event EventHandler? Clicked;

    public CommandItem()
    {
        _icon = new Icon { Size = 16, IsVisible = false, Token = ShellToken.PopoverForeground, VerticalAlignment = VerticalAlignment.Center };
        _label = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        _label.Token(TextBlock.ForegroundProperty, ShellToken.PopoverForeground);
        _shortcut = new TextBlock { FontSize = 12, VerticalAlignment = VerticalAlignment.Center, IsVisible = false };
        _shortcut.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"), ColumnSpacing = 8 };
        Grid.SetColumn(_label, 1);
        Grid.SetColumn(_shortcut, 2);
        grid.Children.Add(_icon);
        grid.Children.Add(_label);
        grid.Children.Add(_shortcut);

        Child = grid;
        Height = 36;
        Padding = new Thickness(8, 0);
        CornerRadius = new CornerRadius(ShellTheme.RadiusSm);
        // Transparent, not null, so the whole row takes the pointer.
        Background = Brushes.Transparent;
        Cursor = new Cursor(StandardCursorType.Hand);
        Update();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (IsEffectivelyEnabled) Command.Of(this)?.SetActive(this);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left) return;
        Press();
        e.Handled = true;
    }

    // Picks the item in code, as a click or Enter would.
    public void Press()
    {
        if (!IsEffectivelyEnabled) return;
        if (Command.Of(this) is { } command) command.Select(this);
        else RaiseClicked();
    }

    internal bool Matches(string word) =>
        (Text ?? string.Empty).Contains(word, StringComparison.CurrentCultureIgnoreCase) ||
        (Keywords ?? string.Empty).Contains(word, StringComparison.CurrentCultureIgnoreCase);

    internal void Apply(bool active)
    {
        if (active) this.Token(BackgroundProperty, ShellToken.Accent);
        else
        {
            this.ClearToken(BackgroundProperty);
            Background = Brushes.Transparent;
        }
    }

    internal void RaiseClicked() => Clicked?.Invoke(this, EventArgs.Empty);

    private void Update()
    {
        _icon.Kind = Icon;
        _icon.IsVisible = Icon != IconName.None;
        _label.Text = Text ?? string.Empty;
        _shortcut.Text = Shortcut ?? string.Empty;
        _shortcut.IsVisible = !string.IsNullOrEmpty(Shortcut);
        AutomationProperties.SetName(this, Text ?? string.Empty);
    }
}

// Divider between groups — -mx-1 my-1 h-px bg-border.
public class CommandSeparator : Border
{
    public CommandSeparator()
    {
        Height = 1;
        Margin = new Thickness(-4, 4);
        this.Token(BackgroundProperty, ShellToken.Border);
    }
}

// Command palette — a Command in a dialog near the top of the window (max-w-xl rounded-xl border
// bg-popover shadow), over a dimmed backdrop. Hotkey (Ctrl+K) toggles it; picking an item,
// clicking the backdrop or pressing Escape closes it.
//   <ui:CommandPalette x:Name="Palette" ItemSelected="OnCommand">
//       <ui:CommandGroup Heading="Suggestions"> ... </ui:CommandGroup>
//   </ui:CommandPalette>
// Open it from code with Palette.SetOpen(true).
public class CommandPalette : ShellOverlayHost
{
    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<CommandPalette, string?>(nameof(Placeholder), "Type a command or search...");

    public static readonly StyledProperty<string?> EmptyTextProperty =
        AvaloniaProperty.Register<CommandPalette, string?>(nameof(EmptyText), "No results found.");

    // The key gesture that opens and closes the palette anywhere in the window; null for none.
    public static readonly StyledProperty<KeyGesture?> HotkeyProperty =
        AvaloniaProperty.Register<CommandPalette, KeyGesture?>(nameof(Hotkey), new KeyGesture(Key.K, KeyModifiers.Control));

    private readonly Command _command = new() { MaxListHeight = 360 };
    private TopLevel? _window;

    static CommandPalette()
    {
        PlaceholderProperty.Changed.AddClassHandler<CommandPalette>((p, e) => p._command.Placeholder = (string?)e.NewValue);
        EmptyTextProperty.Changed.AddClassHandler<CommandPalette>((p, e) => p._command.EmptyText = (string?)e.NewValue);
    }

    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string? EmptyText
    {
        get => GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public KeyGesture? Hotkey
    {
        get => GetValue(HotkeyProperty);
        set => SetValue(HotkeyProperty, value);
    }

    // The Command inside the dialog.
    public Command Command => _command;

    public event EventHandler<CommandItem>? ItemSelected;

    public CommandPalette()
    {
        _command.RemoveBorder();
        _command.ItemSelected += (_, item) =>
        {
            SetOpen(false);
            ItemSelected?.Invoke(this, item);
        };
        Items.Add(new CommandPaletteContent(this, _command));
        // The groups declared inside the palette belong to its Command.
        Items.CollectionChanged += (_, _) =>
        {
            foreach (var child in Items.Where(c => c is not IShellOverlayContent && !_command.Groups.Contains(c)).ToList())
                _command.Groups.Add(child);
        };
        OpenChanged += (_, open) =>
        {
            if (open) _command.Reset();
        };
    }

    protected override bool ClaimsChild(Control child) => child is not IShellOverlayContent;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _window = TopLevel.GetTopLevel(this);
        _window?.AddHandler(KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _window?.RemoveHandler(KeyDownEvent, OnWindowKeyDown);
        _window = null;
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (Hotkey is not { } hotkey || !hotkey.Matches(e) || !IsEffectivelyEnabled) return;
        SetOpen(!Open);
        e.Handled = true;
    }
}

// The backdrop and box of a CommandPalette.
internal sealed class CommandPaletteContent : Border, IShellOverlayContent
{
    private readonly Border _backdrop;
    private readonly Border _box;

    public CommandPaletteContent(CommandPalette palette, Command command)
    {
        _backdrop = new Border();
        _backdrop.Token(BackgroundProperty, ShellToken.Overlay);
        _backdrop.PointerPressed += (_, e) =>
        {
            palette.SetOpen(false);
            e.Handled = true;
        };

        _box = new Border
        {
            Child = command,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            ClipToBounds = true,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            BoxShadow = new BoxShadows(new BoxShadow { OffsetY = 16, Blur = 32, Color = ShellTheme.Shadow(0.25) })
        };
        _box.Token(BackgroundProperty, ShellToken.Popover).Token(BorderBrushProperty, ShellToken.Border);
        // Clicks on the box must not reach the backdrop behind it.
        _box.PointerPressed += (_, e) => e.Handled = true;

        Child = new Panel { Children = { _backdrop, _box } };
        SizeChanged += (_, e) =>
        {
            _box.Width = Math.Max(0, Math.Min(576, e.NewSize.Width - 32));
            // top-[15vh]
            _box.Margin = new Thickness(0, Math.Round(e.NewSize.Height * 0.15), 0, 0);
        };
    }

    public async Task AnimateAsync(bool open)
    {
        if (open)
        {
            ShellMotion.Set(_backdrop, 0, "scale(1)");
            ShellMotion.Set(_box, 0, "scale(0.95)");
            await Task.WhenAll(
                ShellMotion.To(_backdrop, 1, "scale(1)", 150, new CubicEaseOut()),
                ShellMotion.To(_box, 1, "scale(1)", 150, new CubicEaseOut()));
        }
        else
        {
            await Task.WhenAll(
                ShellMotion.To(_backdrop, 0, "scale(1)", 120, new CubicEaseIn()),
                ShellMotion.To(_box, 0, "scale(0.95)", 120, new CubicEaseIn()));
        }
    }
}
