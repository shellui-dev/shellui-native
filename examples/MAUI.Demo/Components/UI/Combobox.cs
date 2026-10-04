using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Combobox — a Select with a search field: h-10 trigger, and a floating panel with a filter
// input above the option list (check on the selected option, "no results" when nothing matches).
// Usage: <ui:Combobox Placeholder="Select framework..." Value="{Binding Framework}" /> then set
// ItemsSource in code or by binding.
public partial class Combobox : ContentView, IShellPopup
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(Combobox), string.Empty, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((Combobox)b).OnValueChanged());

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IList<string>), typeof(Combobox), null,
            propertyChanged: (b, o, n) => ((Combobox)b).RebuildItems());

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(Combobox), "Select...",
            propertyChanged: (b, o, n) => ((Combobox)b).UpdateTrigger());

    public static readonly BindableProperty SearchPlaceholderProperty =
        BindableProperty.Create(nameof(SearchPlaceholder), typeof(string), typeof(Combobox), "Search...",
            propertyChanged: (b, o, n) => ((Combobox)b)._search.Placeholder = (string)n);

    public static readonly BindableProperty EmptyTextProperty =
        BindableProperty.Create(nameof(EmptyText), typeof(string), typeof(Combobox), "No results found.",
            propertyChanged: (b, o, n) => ((Combobox)b)._empty.Text = (string)n);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IList<string>? ItemsSource
    {
        get => (IList<string>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
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

    public event EventHandler<string>? ValueChanged;

    private const double ItemHeight = 32;
    private const double MaxListHeight = 240;

    private readonly Border _trigger;
    private readonly Label _value;
    private readonly Border _panel;
    private readonly Entry _search;
    private readonly VerticalStackLayout _list;
    private readonly Grid _listArea;
    private readonly Label _empty;
    private readonly List<(string Text, Border Row, Icon Check)> _rows = new();
    private ShellPopupHandle? _handle;

    public Combobox()
    {
        _value = new Label { FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.TailTruncation };
        var triggerRow = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        triggerRow.Add(_value, 0, 0);
        triggerRow.Add(new Icon { Name = IconName.ChevronsUpDown, Size = 16, Token = ShellToken.MutedForeground }, 1, 0);

        _trigger = new Border
        {
            Content = triggerRow,
            HeightRequest = 40,
            Padding = new Thickness(12, 0),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        _trigger.Token(Border.StrokeProperty, ShellToken.Input);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (!IsEnabled) return; ShellFocus.FocusPressed(this); SetOpen(!IsOpen); };
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
        // Enter picks the first match.
        _search.Completed += (_, _) =>
        {
            var first = _rows.FirstOrDefault(r => r.Row.IsVisible);
            if (first.Row != null) Pick(first.Text);
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
    }

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

    private void Pick(string text)
    {
        Value = text;
        SetOpen(false);
    }

    private void OnValueChanged()
    {
        UpdateTrigger();
        UpdateChecks();
        ValueChanged?.Invoke(this, Value ?? string.Empty);
    }

    private void UpdateTrigger()
    {
        var hasValue = !string.IsNullOrEmpty(Value);
        _value.Text = hasValue ? Value : Placeholder;
        _value.Token(Label.TextColorProperty, hasValue ? ShellToken.Foreground : ShellToken.MutedForeground);
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
        UpdateTrigger();
        UpdateChecks();
        Filter();
    }

    private View CreateItem(string text)
    {
        var label = new Label { Text = text, FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.TailTruncation };
        label.Token(Label.TextColorProperty, ShellToken.PopoverForeground);
        var check = new Icon { Name = IconName.Check, Size = 16, IsVisible = false };
        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        row.Add(label, 0, 0);
        row.Add(check, 1, 0);

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
        tap.Tapped += (_, _) => Pick(text);
        item.GestureRecognizers.Add(tap);

        _rows.Add((text, item, check));
        return item;
    }

    private void UpdateChecks()
    {
        foreach (var (text, _, check) in _rows)
            check.IsVisible = text == Value;
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
