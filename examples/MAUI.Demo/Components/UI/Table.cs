using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Data table — text-sm rows inside a rounded-md border: a header row in the muted foreground,
// rows divided by border-b that tint on hover, optional caption.
//   <ui:Table Columns="2*,*,*,100">
//       <ui:TableHeader>
//           <ui:TableHead Text="Invoice" /> <ui:TableHead Text="Status" /> ...
//       </ui:TableHeader>
//       <ui:TableRow>
//           <ui:TableCell Text="INV001" /> <ui:TableCell><ui:Badge Text="Paid" /></ui:TableCell> ...
//       </ui:TableRow>
//   </ui:Table>
// Columns takes Grid column widths. Use star and fixed widths: every row is its own grid, so an
// Auto column would size per row. For long lists, use TableRow (with its own Columns) as the item
// template of a CollectionView under a TableHeader.
[ContentProperty(nameof(Rows))]
public partial class Table : ContentView
{
    public static readonly BindableProperty ColumnsProperty =
        BindableProperty.Create(nameof(Columns), typeof(string), typeof(Table), string.Empty,
            propertyChanged: (b, o, n) => ((Table)b).Refresh());

    public static readonly BindableProperty CaptionProperty =
        BindableProperty.Create(nameof(Caption), typeof(string), typeof(Table), string.Empty,
            propertyChanged: (b, o, n) => ((Table)b).UpdateChrome());

    public static readonly BindableProperty BorderedProperty =
        BindableProperty.Create(nameof(Bordered), typeof(bool), typeof(Table), true,
            propertyChanged: (b, o, n) => ((Table)b).UpdateChrome());

    // Narrower than this, the table scrolls horizontally instead of squeezing its columns.
    public static readonly BindableProperty MinimumContentWidthProperty =
        BindableProperty.Create(nameof(MinimumContentWidth), typeof(double), typeof(Table), 0d,
            propertyChanged: (b, o, n) => ((Table)b).UpdateScrolling());

    public string Columns
    {
        get => (string)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public string Caption
    {
        get => (string)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public bool Bordered
    {
        get => (bool)GetValue(BorderedProperty);
        set => SetValue(BorderedProperty, value);
    }

    public double MinimumContentWidth
    {
        get => (double)GetValue(MinimumContentWidthProperty);
        set => SetValue(MinimumContentWidthProperty, value);
    }

    public IList<IView> Rows => _rows.Children;

    private readonly VerticalStackLayout _rows;
    private readonly ScrollView _scroll;
    private readonly Border _frame;
    private readonly Label _caption;

    public Table()
    {
        _rows = new VerticalStackLayout { Spacing = 0 };
        _rows.ChildAdded += (_, _) => Refresh();
        _rows.ChildRemoved += (_, _) => Refresh();
        _scroll = new ScrollView { Orientation = ScrollOrientation.Horizontal };

        _frame = new Border
        {
            Content = _rows,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        _frame.Token(Border.StrokeProperty, ShellToken.Border);
        _frame.SizeChanged += (_, _) => FitScrollContent();

        _caption = new Label { FontSize = 14, HorizontalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 12, 0, 0) };
        _caption.Token(Label.TextColorProperty, ShellToken.MutedForeground);

        Content = new VerticalStackLayout { Spacing = 0, Children = { _frame, _caption } };
        Loaded += (_, _) => Refresh();
        UpdateChrome();
    }

    // Parses "2*,*,100" into column widths. Empty means equal star columns.
    internal static IReadOnlyList<GridLength> ParseColumns(string? columns)
    {
        if (string.IsNullOrWhiteSpace(columns)) return Array.Empty<GridLength>();
        return columns.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParseWidth)
            .ToList();

        static GridLength ParseWidth(string part)
        {
            var invariant = System.Globalization.CultureInfo.InvariantCulture;
            if (part.Equals("auto", StringComparison.OrdinalIgnoreCase)) return GridLength.Auto;
            if (part.EndsWith('*'))
            {
                var factor = part[..^1];
                return new GridLength(factor.Length > 0 && double.TryParse(factor, System.Globalization.NumberStyles.Float, invariant, out var stars) ? stars : 1, GridUnitType.Star);
            }
            return double.TryParse(part, System.Globalization.NumberStyles.Float, invariant, out var width) ? new GridLength(width) : GridLength.Star;
        }
    }

    // The table hands rows their columns and tells the last one to drop its divider.
    private void Refresh()
    {
        var widths = ParseColumns(Columns);
        var rows = _rows.Children.OfType<TableRow>().ToList();
        for (var i = 0; i < rows.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(rows[i].Columns)) rows[i].Apply(widths);
            rows[i].ShowDivider = i < rows.Count - 1;
        }
    }

    private void UpdateChrome()
    {
        _frame.StrokeThickness = Bordered ? 1 : 0;
        _caption.Text = Caption ?? string.Empty;
        _caption.IsVisible = !string.IsNullOrEmpty(Caption);
    }

    private void UpdateScrolling()
    {
        var scrolls = MinimumContentWidth > 0;
        if (scrolls && _frame.Content != _scroll)
        {
            _frame.Content = null;
            _scroll.Content = _rows;
            _frame.Content = _scroll;
        }
        else if (!scrolls && _frame.Content != _rows)
        {
            _scroll.Content = null;
            _frame.Content = _rows;
            _rows.WidthRequest = -1;
        }
        FitScrollContent();
    }

    // Inside a horizontal ScrollView rows measure at their minimum width; give them a real one.
    private void FitScrollContent()
    {
        if (MinimumContentWidth <= 0 || _frame.Width <= 0) return;
        _rows.WidthRequest = Math.Max(_frame.Width - 2 * _frame.StrokeThickness, MinimumContentWidth);
    }
}

// Body row — border-b, hover:bg-muted/50, bg-muted when selected. Its children are the cells,
// placed in columns in order.
public partial class TableRow : Grid
{
    // Set only for rows used outside a Table (e.g. a CollectionView item template).
    public static readonly BindableProperty ColumnsProperty =
        BindableProperty.Create(nameof(Columns), typeof(string), typeof(TableRow), string.Empty,
            propertyChanged: (b, o, n) => ((TableRow)b).Apply(Table.ParseColumns((string)n)));

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(TableRow), false,
            propertyChanged: (b, o, n) => ((TableRow)b).UpdateFill());

    public string Columns
    {
        get => (string)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public event EventHandler? Tapped;

    private readonly BoxView _fill;
    private readonly BoxView _divider;
    private IReadOnlyList<GridLength> _widths = Array.Empty<GridLength>();
    private bool _hovered;

    internal bool ShowDivider
    {
        get => _divider.IsVisible;
        set => _divider.IsVisible = value;
    }

    protected virtual bool Interactive => true;

    public TableRow()
    {
        _fill = new BoxView { BackgroundColor = Colors.Transparent, Opacity = 0, InputTransparent = true, ZIndex = -1 };
        _fill.Token(BoxView.ColorProperty, ShellToken.Muted);
        _divider = new BoxView { BackgroundColor = Colors.Transparent, HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
        _divider.Token(BoxView.ColorProperty, ShellToken.Border);
        Children.Add(_fill);
        Children.Add(_divider);
        // A layout with no background only takes pointer input over its children.
        BackgroundColor = Colors.Transparent;

        if (Interactive)
        {
            var pointer = new PointerGestureRecognizer();
            pointer.PointerEntered += (_, _) => { _hovered = true; UpdateFill(); };
            pointer.PointerExited += (_, _) => { _hovered = false; UpdateFill(); };
            GestureRecognizers.Add(pointer);
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => Tapped?.Invoke(this, EventArgs.Empty);
            GestureRecognizers.Add(tap);
        }
    }

    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);
        // _divider is still null while the constructor adds the row's own chrome.
        if (_divider != null && child != _fill && child != _divider) Apply(_widths);
    }

    internal void Apply(IReadOnlyList<GridLength> widths)
    {
        _widths = widths;
        var cells = Children.Where(c => c != _fill && c != _divider).ToList();
        var count = Math.Max(1, Math.Max(cells.Count, widths.Count));

        ColumnDefinitions.Clear();
        for (var i = 0; i < count; i++)
            ColumnDefinitions.Add(new ColumnDefinition(i < widths.Count ? widths[i] : GridLength.Star));
        for (var i = 0; i < cells.Count; i++)
            Grid.SetColumn((BindableObject)cells[i], i);
        Grid.SetColumnSpan(_fill, count);
        Grid.SetColumnSpan(_divider, count);
    }

    private void UpdateFill() => _fill.Opacity = IsSelected ? 1 : _hovered ? 0.5 : 0;
}

// Header row: holds TableHead cells, no hover.
public partial class TableHeader : TableRow
{
    protected override bool Interactive => false;
}

// Header cell — h-11 px-4 font-medium text-muted-foreground.
public partial class TableHead : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(TableHead), string.Empty,
            propertyChanged: (b, o, n) => ((TableHead)b)._label.Text = n as string ?? string.Empty);

    public static readonly BindableProperty HorizontalTextAlignmentProperty =
        BindableProperty.Create(nameof(HorizontalTextAlignment), typeof(TextAlignment), typeof(TableHead), TextAlignment.Start,
            propertyChanged: (b, o, n) => ((TableHead)b)._label.HorizontalTextAlignment = (TextAlignment)n);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public TextAlignment HorizontalTextAlignment
    {
        get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
        set => SetValue(HorizontalTextAlignmentProperty, value);
    }

    private readonly Label _label;

    public TableHead()
    {
        _label = new Label
        {
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        };
        _label.Token(Label.TextColorProperty, ShellToken.MutedForeground);
        HeightRequest = 44;
        Padding = new Thickness(16, 0);
        Content = _label;
    }
}

// Body cell — p-4 text-sm. Set Text, or put any view inside.
public partial class TableCell : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(TableCell), string.Empty,
            propertyChanged: (b, o, n) => ((TableCell)b)._label.Text = n as string ?? string.Empty);

    public static readonly BindableProperty HorizontalTextAlignmentProperty =
        BindableProperty.Create(nameof(HorizontalTextAlignment), typeof(TextAlignment), typeof(TableCell), TextAlignment.Start,
            propertyChanged: (b, o, n) => ((TableCell)b)._label.HorizontalTextAlignment = (TextAlignment)n);

    public static readonly BindableProperty IsBoldProperty =
        BindableProperty.Create(nameof(IsBold), typeof(bool), typeof(TableCell), false,
            propertyChanged: (b, o, n) => ((TableCell)b)._label.FontAttributes = (bool)n ? FontAttributes.Bold : FontAttributes.None);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public TextAlignment HorizontalTextAlignment
    {
        get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
        set => SetValue(HorizontalTextAlignmentProperty, value);
    }

    public bool IsBold
    {
        get => (bool)GetValue(IsBoldProperty);
        set => SetValue(IsBoldProperty, value);
    }

    private readonly Label _label;

    public TableCell()
    {
        _label = new Label
        {
            FontSize = 14,
            VerticalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        };
        _label.Token(Label.TextColorProperty, ShellToken.Foreground);
        Padding = new Thickness(16, 12);
        Content = _label;
    }
}
