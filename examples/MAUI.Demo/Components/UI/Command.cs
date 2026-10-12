using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Command menu — a search field over a list of grouped items (rounded-md border bg-popover).
// Typing filters the list; Enter picks the highlighted item.
//   <ui:Command Placeholder="Type a command or search..." ItemSelected="OnCommand">
//       <ui:CommandGroup Heading="Suggestions">
//           <ui:CommandItem Text="Calendar" Icon="Calendar" Value="calendar" />
//           <ui:CommandItem Text="Settings" Icon="Settings" Shortcut="Ctrl+," Keywords="preferences options" />
//       </ui:CommandGroup>
//       <ui:CommandSeparator />
//       <ui:CommandGroup Heading="Account"> ... </ui:CommandGroup>
//   </ui:Command>
// CommandPalette shows the same list in a dialog over the page.
[ContentProperty(nameof(Groups))]
public partial class Command : ContentView
{
    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(Command), "Type a command or search...",
            propertyChanged: (b, o, n) => ((Command)b)._entry.Placeholder = n as string ?? string.Empty);

    // Shown when nothing matches the search.
    public static readonly BindableProperty EmptyTextProperty =
        BindableProperty.Create(nameof(EmptyText), typeof(string), typeof(Command), "No results found.",
            propertyChanged: (b, o, n) => ((Command)b)._empty.Text = n as string ?? string.Empty);

    // The list scrolls past this height.
    public static readonly BindableProperty MaxListHeightProperty =
        BindableProperty.Create(nameof(MaxListHeight), typeof(double), typeof(Command), 300d,
            propertyChanged: (b, o, n) => ((Command)b)._scroll.MaximumHeightRequest = (double)n);

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public double MaxListHeight
    {
        get => (double)GetValue(MaxListHeightProperty);
        set => SetValue(MaxListHeightProperty, value);
    }

    // CommandGroup, CommandItem and CommandSeparator children.
    public IList<IView> Groups => _list.Children;

    // The search text.
    public string Query => _entry.Text ?? string.Empty;

    public event EventHandler<CommandItem>? ItemSelected;

    private readonly Border _border;
    private readonly Entry _entry;
    private readonly Label _empty;
    private readonly VerticalStackLayout _list;
    private readonly ScrollView _scroll;
    private CommandItem? _active;

    public Command()
    {
        _entry = new Entry
        {
            FontSize = 14,
            Placeholder = Placeholder,
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
            ClearButtonVisibility = ClearButtonVisibility.Never
        };
        _entry.Token(Entry.TextColorProperty, ShellToken.Foreground);
        _entry.Token(Entry.PlaceholderColorProperty, ShellToken.MutedForeground);
        ShellPlatform.StripNativeChrome(_entry);
        ShellFocus.Track(_entry);
        _entry.TextChanged += (_, _) => Filter();
        _entry.Completed += (_, _) => { if (_active != null) Select(_active); };

        var search = new Grid
        {
            HeightRequest = 44,
            Padding = new Thickness(12, 0),
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }
        };
        search.Add(new Icon { Name = IconName.Search, Size = 16, Token = ShellToken.MutedForeground, VerticalOptions = LayoutOptions.Center }, 0, 0);
        search.Add(_entry, 1, 0);
        var line = new BoxView { HeightRequest = 1, BackgroundColor = Colors.Transparent };
        line.Token(BoxView.ColorProperty, ShellToken.Border);

        _empty = new Label { Text = EmptyText, FontSize = 14, HorizontalTextAlignment = TextAlignment.Center, Padding = new Thickness(0, 24), IsVisible = false };
        _empty.Token(Label.TextColorProperty, ShellToken.MutedForeground);
        _list = new VerticalStackLayout { Spacing = 0 };
        _list.ChildAdded += (_, _) => Filter();
        _scroll = new ScrollView
        {
            Content = new VerticalStackLayout { Padding = new Thickness(4), Children = { _empty, _list } },
            MaximumHeightRequest = MaxListHeight,
            VerticalScrollBarVisibility = ScrollBarVisibility.Default
        };

        _border = new Border
        {
            Content = new VerticalStackLayout { Spacing = 0, Children = { search, line, _scroll } },
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd }
        };
        _border.Token(VisualElement.BackgroundColorProperty, ShellToken.Popover);
        _border.Token(Border.StrokeProperty, ShellToken.Border);
        Content = _border;
        Loaded += (_, _) => Filter();
    }

    // The Command an item or group sits in.
    internal static Command? Of(Element? element)
    {
        for (var parent = element?.Parent; parent != null; parent = parent.Parent)
            if (parent is Command command) return command;
        return null;
    }

    // Used by CommandPalette, which draws the border itself.
    internal void RemoveBorder()
    {
        _border.StrokeThickness = 0;
        _border.ClearValue(VisualElement.BackgroundColorProperty);
        _border.BackgroundColor = Colors.Transparent;
    }

    public new void Focus() => _entry.Focus();

    // Clears the search.
    public void Reset() => _entry.Text = string.Empty;

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
        if (!item.IsEnabled) return;
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
[ContentProperty(nameof(Items))]
public partial class CommandGroup : ContentView
{
    public static readonly BindableProperty HeadingProperty =
        BindableProperty.Create(nameof(Heading), typeof(string), typeof(CommandGroup), string.Empty,
            propertyChanged: (b, o, n) => ((CommandGroup)b).Update());

    public string Heading
    {
        get => (string)GetValue(HeadingProperty);
        set => SetValue(HeadingProperty, value);
    }

    public IList<IView> Items => _items.Children;

    private readonly Label _heading;
    private readonly VerticalStackLayout _items;

    public CommandGroup()
    {
        _heading = new Label { FontSize = 12, FontAttributes = FontAttributes.Bold, Padding = new Thickness(8, 6), IsVisible = false };
        _heading.Token(Label.TextColorProperty, ShellToken.MutedForeground);
        _items = new VerticalStackLayout { Spacing = 0 };
        Content = new VerticalStackLayout { Spacing = 0, Children = { _heading, _items } };
    }

    private void Update()
    {
        _heading.Text = Heading ?? string.Empty;
        _heading.IsVisible = !string.IsNullOrEmpty(Heading);
    }
}

// One row of a Command — rounded-sm px-2 h-9 text-sm; the highlighted row is bg-accent.
public partial class CommandItem : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(CommandItem), string.Empty,
            propertyChanged: (b, o, n) => ((CommandItem)b).Update());

    // Identifies the item to your code.
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(CommandItem), null);

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(CommandItem), IconName.None,
            propertyChanged: (b, o, n) => ((CommandItem)b).Update());

    // Keyboard hint shown at the end of the row (display only).
    public static readonly BindableProperty ShortcutProperty =
        BindableProperty.Create(nameof(Shortcut), typeof(string), typeof(CommandItem), string.Empty,
            propertyChanged: (b, o, n) => ((CommandItem)b).Update());

    // Extra words the search matches besides Text.
    public static readonly BindableProperty KeywordsProperty =
        BindableProperty.Create(nameof(Keywords), typeof(string), typeof(CommandItem), string.Empty);

    public string Text
    {
        get => (string)GetValue(TextProperty);
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

    public string Shortcut
    {
        get => (string)GetValue(ShortcutProperty);
        set => SetValue(ShortcutProperty, value);
    }

    public string Keywords
    {
        get => (string)GetValue(KeywordsProperty);
        set => SetValue(KeywordsProperty, value);
    }

    public event EventHandler? Clicked;

    private readonly Border _row;
    private readonly Icon _icon;
    private readonly Label _label;
    private readonly Label _shortcut;

    public CommandItem()
    {
        _icon = new Icon { Size = 16, IsVisible = false, Token = ShellToken.PopoverForeground, VerticalOptions = LayoutOptions.Center };
        _label = new Label { FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.TailTruncation };
        _label.Token(Label.TextColorProperty, ShellToken.PopoverForeground);
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
            HeightRequest = 36,
            Padding = new Thickness(8, 0),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusSm },
            BackgroundColor = Colors.Transparent
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { if (IsEnabled) Command.Of(this)?.SetActive(this); };
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

    internal bool Matches(string word) =>
        (Text ?? string.Empty).Contains(word, StringComparison.CurrentCultureIgnoreCase) ||
        (Keywords ?? string.Empty).Contains(word, StringComparison.CurrentCultureIgnoreCase);

    internal void Apply(bool active)
    {
        if (active) _row.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        else
        {
            _row.ClearValue(VisualElement.BackgroundColorProperty);
            _row.BackgroundColor = Colors.Transparent;
        }
    }

    internal void RaiseClicked() => Clicked?.Invoke(this, EventArgs.Empty);

    private void Press()
    {
        if (!IsEnabled) return;
        if (Command.Of(this) is { } command) command.Select(this);
        else RaiseClicked();
    }

    private void Update()
    {
        _icon.Name = Icon;
        _icon.IsVisible = Icon != IconName.None;
        _label.Text = Text ?? string.Empty;
        _shortcut.Text = Shortcut ?? string.Empty;
        _shortcut.IsVisible = !string.IsNullOrEmpty(Shortcut);
        SemanticProperties.SetDescription(this, Text ?? string.Empty);
    }
}

// Divider between groups — -mx-1 my-1 h-px bg-border.
public partial class CommandSeparator : ContentView
{
    public CommandSeparator()
    {
        var line = new BoxView { HeightRequest = 1, BackgroundColor = Colors.Transparent };
        line.Token(BoxView.ColorProperty, ShellToken.Border);
        Margin = new Thickness(-4, 4);
        Content = line;
    }
}

// Command palette — a Command in a dialog near the top of the page (max-w-xl rounded-xl border
// bg-popover shadow), over a dimmed backdrop. Picking an item, tapping the backdrop or pressing
// Escape closes it. Declare it where a Dialog would go and open it from code:
//   <ui:CommandPalette x:Name="Palette" ItemSelected="OnCommand">
//       <ui:CommandGroup Heading="Suggestions"> ... </ui:CommandGroup>
//   </ui:CommandPalette>
//   Palette.SetOpen(true);
[ContentProperty(nameof(Groups))]
public partial class CommandPalette : ShellOverlayHost
{
    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(CommandPalette), "Type a command or search...",
            propertyChanged: (b, o, n) => ((CommandPalette)b)._command.Placeholder = n as string ?? string.Empty);

    public static readonly BindableProperty EmptyTextProperty =
        BindableProperty.Create(nameof(EmptyText), typeof(string), typeof(CommandPalette), "No results found.",
            propertyChanged: (b, o, n) => ((CommandPalette)b)._command.EmptyText = n as string ?? string.Empty);

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public IList<IView> Groups => _command.Groups;

    // The Command inside the dialog.
    public Command Command => _command;

    public event EventHandler<CommandItem>? ItemSelected;

    private readonly Command _command;

    public CommandPalette()
    {
        _command = new Command { MaxListHeight = 360 };
        _command.RemoveBorder();
        _command.ItemSelected += (_, item) =>
        {
            SetOpen(false);
            ItemSelected?.Invoke(this, item);
        };
        Children.Add(new CommandPaletteContent(this, _command));
        OpenChanged += async (_, open) =>
        {
            if (!open) return;
            _command.Reset();
            // The field can take focus once the dialog is on screen.
            await Task.Delay(120);
            if (Open) _command.Focus();
        };
    }

    protected override bool IsTrigger(Element child) => false;
}

// The backdrop and box of a CommandPalette.
internal sealed partial class CommandPaletteContent : ContentView, IShellOverlayContent
{
    private readonly BoxView _backdrop;
    private readonly Border _box;

    public CommandPaletteContent(CommandPalette palette, Command command)
    {
        _backdrop = new BoxView { BackgroundColor = Colors.Transparent };
        _backdrop.Token(BoxView.ColorProperty, ShellToken.Overlay);
        var tapBackdrop = new TapGestureRecognizer();
        tapBackdrop.Tapped += (_, _) => palette.SetOpen(false);
        _backdrop.GestureRecognizers.Add(tapBackdrop);

        _box = new Border
        {
            Content = command,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.Center,
            Shadow = new Shadow { Brush = new SolidColorBrush(Colors.Black), Offset = new Point(0, 16), Radius = 32, Opacity = 0.25f }
        };
        _box.Token(VisualElement.BackgroundColorProperty, ShellToken.Popover);
        _box.Token(Border.StrokeProperty, ShellToken.Border);
        // Swallow taps on the box so they don't fall through to the backdrop.
        _box.GestureRecognizers.Add(new TapGestureRecognizer());

        var root = new Grid { Children = { _backdrop, _box } };
        ShellPortal.EdgeToEdge(this, root);
        root.SizeChanged += (_, _) =>
        {
            _box.WidthRequest = Math.Max(0, Math.Min(576, root.Width - 32));
            // top-[15vh]
            _box.Margin = new Thickness(0, Math.Round(root.Height * 0.15), 0, 0);
        };
        Content = root;
    }

    public async Task AnimateAsync(bool open)
    {
        _box.AbortAnimation("FadeTo");
        if (open)
        {
            _backdrop.Opacity = 0;
            _box.Opacity = 0;
            _box.Scale = 0.95;
            await Task.WhenAll(
                _backdrop.FadeToAsync(1, 150, Easing.CubicOut),
                _box.FadeToAsync(1, 150, Easing.CubicOut),
                _box.ScaleToAsync(1, 150, Easing.CubicOut));
        }
        else
        {
            await Task.WhenAll(
                _backdrop.FadeToAsync(0, 120, Easing.CubicIn),
                _box.FadeToAsync(0, 120, Easing.CubicIn),
                _box.ScaleToAsync(0.95, 120, Easing.CubicIn));
        }
    }
}
