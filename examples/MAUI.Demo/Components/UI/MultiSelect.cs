using System.Collections.Specialized;
using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Multi-select — a min-h-10 trigger that shows the picked options as removable chips, and a
// floating panel with a search field and a checked list. Picking toggles an option and leaves
// the panel open; click outside or press Escape to close it.
// Usage: <ui:MultiSelect Placeholder="Select frameworks..." Values="{Binding Frameworks}" /> then
// set ItemsSource in code or by binding.
public partial class MultiSelect : ContentView, IShellPopup
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IList<string>), typeof(MultiSelect), null,
            propertyChanged: (b, o, n) => ((MultiSelect)b).RebuildItems());

    public static readonly BindableProperty ValuesProperty =
        BindableProperty.Create(nameof(Values), typeof(IList<string>), typeof(MultiSelect), null, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((MultiSelect)b).OnValuesReplaced(o, n));

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(MultiSelect), "Select...",
            propertyChanged: (b, o, n) => ((MultiSelect)b)._placeholder.Text = (string)n);

    public static readonly BindableProperty SearchPlaceholderProperty =
        BindableProperty.Create(nameof(SearchPlaceholder), typeof(string), typeof(MultiSelect), "Search...",
            propertyChanged: (b, o, n) => ((MultiSelect)b)._search.Placeholder = (string)n);

    public static readonly BindableProperty EmptyTextProperty =
        BindableProperty.Create(nameof(EmptyText), typeof(string), typeof(MultiSelect), "No results found.",
            propertyChanged: (b, o, n) => ((MultiSelect)b)._empty.Text = (string)n);

    public IList<string>? ItemsSource
    {
        get => (IList<string>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public IList<string>? Values
    {
        get => (IList<string>?)GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string SearchPlaceholder
    {
        get => (string)GetValue(SearchPlaceholderProperty);
        set => SetValue(SearchPlaceholderProperty, value);
    }

    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public bool IsOpen { get; private set; }

    public event EventHandler<IReadOnlyList<string>>? ValuesChanged;

    private const double ItemHeight = 32;
    private const double MaxListHeight = 240;

    private readonly Border _trigger;
    private readonly WrapLayout _chips;
    private readonly Label _placeholder;
    private readonly Border _panel;
    private readonly Entry _search;
    private readonly VerticalStackLayout _list;
    private readonly Grid _listArea;
    private readonly Label _empty;
    private readonly List<(string Text, Border Row, Icon Check)> _rows = new();
    private ShellPopupHandle? _handle;
    private bool _chipTapped;

    public MultiSelect()
    {
        _placeholder = new Label { Text = Placeholder, FontSize = 14, VerticalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(4, 0, 0, 0) };
        _placeholder.Token(Label.TextColorProperty, ShellToken.MutedForeground);
        _chips = new WrapLayout { Spacing = 4, LineSpacing = 4, VerticalOptions = LayoutOptions.Center };

        var triggerRow = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        triggerRow.Add(new Grid { Children = { _placeholder, _chips } }, 0, 0);
        triggerRow.Add(new Icon { Name = IconName.ChevronsUpDown, Size = 16, Token = ShellToken.MutedForeground, VerticalOptions = LayoutOptions.Center }, 1, 0);

        _trigger = new Border
        {
            Content = triggerRow,
            MinimumHeightRequest = 40,
            Padding = new Thickness(8, 6, 12, 6),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        _trigger.Token(Border.StrokeProperty, ShellToken.Input);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            // A tap on a chip's × reaches the trigger too on some platforms; it removed a value,
            // it should not also open or close the panel.
            if (_chipTapped) { _chipTapped = false; return; }
            if (!IsEnabled) return;
            ShellFocus.FocusPressed(this);
            SetOpen(!IsOpen);
        };
        _trigger.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, () => { if (IsEnabled) SetOpen(!IsOpen); });

        _search = new Entry
        {
            Placeholder = SearchPlaceholder,
            FontSize = 14,
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center,
            ClearButtonVisibility = ClearButtonVisibility.Never,
            IsTextPredictionEnabled = false,
            IsSpellCheckEnabled = false
        };
        _search.Token(Entry.TextColorProperty, ShellToken.PopoverForeground);
        _search.Token(Entry.PlaceholderColorProperty, ShellToken.MutedForeground);
        ShellPlatform.StripNativeChrome(_search);
        ShellFocus.Track(_search);
        _search.TextChanged += (_, _) => Filter();
        // Enter toggles the first match.
        _search.Completed += (_, _) =>
        {
            var first = _rows.FirstOrDefault(r => r.Row.IsVisible);
            if (first.Row != null) Toggle(first.Text);
        };

        var searchRow = new Grid
        {
            HeightRequest = 40,
            Padding = new Thickness(12, 0),
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }
        };
        searchRow.Add(new Icon { Name = IconName.Search, Size = 16, Token = ShellToken.MutedForeground }, 0, 0);
        searchRow.Add(_search, 1, 0);

        var divider = new BoxView { HeightRequest = 1, BackgroundColor = Colors.Transparent };
        divider.Token(BoxView.ColorProperty, ShellToken.Border);

        _list = new VerticalStackLayout { Spacing = 0 };
        _empty = new Label
        {
            Text = EmptyText,
            FontSize = 14,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            IsVisible = false
        };
        _empty.Token(Label.TextColorProperty, ShellToken.MutedForeground);
        // Fixed height (set in RebuildItems): the panel keeps its size and position while filtering.
        _listArea = new Grid { Margin = new Thickness(4), Children = { new ScrollView { Content = _list }, _empty } };

        _panel = new Border
        {
            Content = new VerticalStackLayout { Spacing = 0, Children = { searchRow, divider, _listArea } },
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            Shadow = ShellPopups.PanelShadow()
        };
        _panel.Token(VisualElement.BackgroundColorProperty, ShellToken.Popover);
        _panel.Token(Border.StrokeProperty, ShellToken.Border);
        // Taps on the panel's own chrome must not fall through to the click-outside catcher.
        _panel.GestureRecognizers.Add(new TapGestureRecognizer());

        Content = _trigger;
        RebuildItems();
        RebuildChips();
    }

    private IReadOnlyList<string> Current => (IReadOnlyList<string>?)Values?.ToList() ?? Array.Empty<string>();

    public void SetOpen(bool open)
    {
        if (IsOpen == open) return;
        IsOpen = open;
        _trigger.Token(Border.StrokeProperty, open ? ShellToken.Ring : ShellToken.Input);
        if (open)
        {
            _search.Text = string.Empty;
            Filter();
            ShellPopups.Opened(this);
            _handle = ShellPortal.ShowPopup(_trigger, _panel, new ShellPopupOptions
            {
                MatchAnchorWidth = true,
                Owner = this,
                OnDismiss = Close
            });
            // Type-to-filter straight away where there is a hardware keyboard; on touch devices
            // the soft keyboard would cover the list, so the user taps the field to search.
            if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
                Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(150), () => { if (IsOpen) _search.Focus(); });
        }
        else
        {
            ShellPopups.Closed(this);
            if (_search.IsFocused) _search.Unfocus();
            var handle = _handle;
            _handle = null;
            if (handle != null) _ = handle.CloseAsync();
        }
    }

    public void Close() => SetOpen(false);

    // Adds the option when it isn't picked, removes it when it is.
    public void Toggle(string text)
    {
        // A new list each time, so a two-way binding sees the change.
        var next = new List<string>(Current);
        if (!next.Remove(text)) next.Add(text);
        Values = next;
    }

    private void OnValuesReplaced(object? oldValue, object? newValue)
    {
        if (oldValue is INotifyCollectionChanged before) before.CollectionChanged -= OnValuesMutated;
        if (newValue is INotifyCollectionChanged after) after.CollectionChanged += OnValuesMutated;
        OnValuesMutated(this, null);
    }

    private void OnValuesMutated(object? sender, NotifyCollectionChangedEventArgs? e)
    {
        RebuildChips();
        UpdateChecks();
        ValuesChanged?.Invoke(this, Current);
    }

    private void RebuildChips()
    {
        _chips.Children.Clear();
        var current = Current;
        foreach (var value in current)
            _chips.Children.Add(CreateChip(value));
        _placeholder.IsVisible = current.Count == 0;
    }

    private View CreateChip(string value)
    {
        var label = new Label { Text = value, FontSize = 12, FontAttributes = FontAttributes.Bold, VerticalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.TailTruncation };
        label.Token(Label.TextColorProperty, ShellToken.SecondaryForeground);
        var close = new Icon { Name = IconName.X, Size = 12, Token = ShellToken.MutedForeground, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        // 12px icon in a 20px tap target.
        var remove = new Grid { WidthRequest = 20, HeightRequest = 20, BackgroundColor = Colors.Transparent, Children = { close } };
        SemanticProperties.SetDescription(remove, $"Remove {value}");
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => close.Token = ShellToken.Destructive;
        pointer.PointerExited += (_, _) => close.Token = ShellToken.MutedForeground;
        remove.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            if (!IsEnabled) return;
            _chipTapped = true;
            // Cleared on the next turn in case the tap never reaches the trigger.
            Dispatcher.Dispatch(() => _chipTapped = false);
            Toggle(value);
        };
        remove.GestureRecognizers.Add(tap);

        var chip = new Border
        {
            Content = new HorizontalStackLayout { Spacing = 2, Children = { label, remove } },
            Padding = new Thickness(8, 0, 2, 0),
            HeightRequest = 24,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd }
        };
        chip.Token(VisualElement.BackgroundColorProperty, ShellToken.Secondary);
        return chip;
    }

    private void RebuildItems()
    {
        _list.Children.Clear();
        _rows.Clear();
        var items = ItemsSource;
        _listArea.HeightRequest = Math.Min(Math.Max(items?.Count ?? 0, 2) * ItemHeight, MaxListHeight);
        if (items != null)
        {
            foreach (var text in items)
                _list.Children.Add(CreateItem(text));
        }
        UpdateChecks();
        Filter();
    }

    private View CreateItem(string text)
    {
        var check = new Icon { Name = IconName.Check, Size = 16, Opacity = 0 };
        var label = new Label { Text = text, FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.TailTruncation };
        label.Token(Label.TextColorProperty, ShellToken.PopoverForeground);
        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }
        };
        row.Add(check, 0, 0);
        row.Add(label, 1, 0);

        var item = new Border
        {
            Content = row,
            HeightRequest = ItemHeight,
            Padding = new Thickness(8, 0),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusSm },
            BackgroundColor = Colors.Transparent
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => item.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        pointer.PointerExited += (_, _) => { item.ClearValue(VisualElement.BackgroundColorProperty); item.BackgroundColor = Colors.Transparent; };
        item.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => Toggle(text);
        item.GestureRecognizers.Add(tap);

        _rows.Add((text, item, check));
        return item;
    }

    private void UpdateChecks()
    {
        var current = Current;
        // The check keeps its column when hidden, so option labels stay aligned.
        foreach (var (text, _, check) in _rows)
            check.Opacity = current.Contains(text) ? 1 : 0;
    }

    private void Filter()
    {
        var query = _search.Text?.Trim() ?? string.Empty;
        var any = false;
        foreach (var (text, row, _) in _rows)
        {
            row.IsVisible = query.Length == 0 || text.Contains(query, StringComparison.OrdinalIgnoreCase);
            any |= row.IsVisible;
        }
        _empty.IsVisible = !any;
    }
}
