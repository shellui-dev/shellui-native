using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class TableTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "table",
        DisplayName = "Table",
        Description = "Data table with header, rows, hover and optional caption",
        Category = ComponentCategory.DataDisplay,
        FilePath = "Table.cs",
        Dependencies = new List<string> { "shell" },
        Tags = new List<string> { "table", "data", "grid", "rows" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Data table — text-sm rows inside a rounded-md border: a header row in the muted foreground,
// rows divided by border-b that tint on hover, optional caption.
//   <ui:Table Columns=""2*,*,*,100"">
//       <ui:TableHeader>
//           <ui:TableHead Text=""Invoice"" /> <ui:TableHead Text=""Status"" /> ...
//       </ui:TableHeader>
//       <ui:TableRow>
//           <ui:TableCell Text=""INV001"" /> <ui:TableCell><ui:Badge Text=""Paid"" /></ui:TableCell> ...
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

    // Parses ""2*,*,100"" into column widths. Empty means equal star columns.
    internal static IReadOnlyList<GridLength> ParseColumns(string? columns)
    {
        if (string.IsNullOrWhiteSpace(columns)) return Array.Empty<GridLength>();
        return columns.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParseWidth)
            .ToList();

        static GridLength ParseWidth(string part)
        {
            var invariant = System.Globalization.CultureInfo.InvariantCulture;
            if (part.Equals(""auto"", StringComparison.OrdinalIgnoreCase)) return GridLength.Auto;
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
",
        [NativePlatform.Avalonia] = @"using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace YourProjectNamespace.Components.UI;

// Data table — text-sm rows inside a rounded-md border: a header row in the muted foreground,
// rows divided by border-b that tint on hover, optional caption.
//   <ui:Table Columns=""2*,*,*,100"">
//       <ui:TableHeader>
//           <ui:TableHead Text=""Invoice"" /> <ui:TableHead Text=""Status"" /> ...
//       </ui:TableHeader>
//       <ui:TableRow Tapped=""OnRowTapped"">
//           <ui:TableCell Text=""INV001"" /> <ui:TableCell><ui:Badge Text=""Paid"" /></ui:TableCell> ...
//       </ui:TableRow>
//   </ui:Table>
// Columns takes Grid column widths. Use star and fixed widths: every row is its own grid, so an
// Auto column would size per row. For long lists, use TableRow (with its own Columns) as the item
// template of an ItemsControl under a TableHeader.
public class Table : Border
{
    public static readonly StyledProperty<string?> ColumnsProperty =
        AvaloniaProperty.Register<Table, string?>(nameof(Columns));

    public static readonly StyledProperty<string?> CaptionProperty =
        AvaloniaProperty.Register<Table, string?>(nameof(Caption));

    public static readonly StyledProperty<bool> BorderedProperty =
        AvaloniaProperty.Register<Table, bool>(nameof(Bordered), true);

    // Narrower than this, the table scrolls sideways instead of squeezing its columns.
    public static readonly StyledProperty<double> MinimumContentWidthProperty =
        AvaloniaProperty.Register<Table, double>(nameof(MinimumContentWidth));

    private readonly StackPanel _rows = new();
    private readonly ScrollViewer _scroll;
    private readonly Border _frame;
    private readonly TextBlock _caption;

    static Table()
    {
        ColumnsProperty.Changed.AddClassHandler<Table>((t, _) => t.Refresh());
        CaptionProperty.Changed.AddClassHandler<Table>((t, _) => t.UpdateChrome());
        BorderedProperty.Changed.AddClassHandler<Table>((t, _) => t.UpdateChrome());
        MinimumContentWidthProperty.Changed.AddClassHandler<Table>((t, _) => t.UpdateScrolling());
    }

    public string? Columns
    {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public string? Caption
    {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public bool Bordered
    {
        get => GetValue(BorderedProperty);
        set => SetValue(BorderedProperty, value);
    }

    public double MinimumContentWidth
    {
        get => GetValue(MinimumContentWidthProperty);
        set => SetValue(MinimumContentWidthProperty, value);
    }

    [Content]
    public Controls Rows => _rows.Children;

    public Table()
    {
        _rows.Children.CollectionChanged += (_, _) => Refresh();
        _scroll = new ScrollViewer
        {
            Content = _rows,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        _scroll.SizeChanged += (_, _) => FitScrollContent();

        _frame = new Border
        {
            Child = _scroll,
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            // Row tints stay inside the rounded corners.
            ClipToBounds = true
        };
        _frame.Token(BorderBrushProperty, ShellToken.Border);

        _caption = new TextBlock { FontSize = 14, TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 12, 0, 0) };
        _caption.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);

        Child = new StackPanel { Children = { _frame, _caption } };
        UpdateChrome();
    }

    // Parses ""2*,*,100"" into column widths. Empty means equal star columns.
    internal static IReadOnlyList<GridLength> ParseColumns(string? columns)
    {
        if (string.IsNullOrWhiteSpace(columns)) return Array.Empty<GridLength>();
        return columns.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParseWidth)
            .ToList();

        static GridLength ParseWidth(string part)
        {
            if (part.Equals(""auto"", StringComparison.OrdinalIgnoreCase)) return GridLength.Auto;
            if (part.EndsWith('*'))
            {
                var factor = part[..^1];
                return new GridLength(factor.Length > 0 && double.TryParse(factor, NumberStyles.Float, CultureInfo.InvariantCulture, out var stars) ? stars : 1, GridUnitType.Star);
            }
            return double.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out var width) ? new GridLength(width) : GridLength.Star;
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
        _frame.BorderThickness = new Thickness(Bordered ? 1 : 0);
        _caption.Text = Caption ?? string.Empty;
        _caption.IsVisible = !string.IsNullOrEmpty(Caption);
    }

    private void UpdateScrolling()
    {
        _scroll.HorizontalScrollBarVisibility = MinimumContentWidth > 0 ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
        FitScrollContent();
    }

    // A sideways ScrollViewer measures rows at their smallest width; give them a real one.
    private void FitScrollContent()
    {
        _rows.Width = MinimumContentWidth > 0 && _scroll.Bounds.Width > 0
            ? Math.Max(_scroll.Bounds.Width, MinimumContentWidth)
            : double.NaN;
    }
}

// Body row — border-b, hover:bg-muted/50, bg-muted when selected. Its children are the cells,
// placed in columns in order. Handle clicks and taps with Avalonia's own Tapped event.
public class TableRow : Grid
{
    // Set only for rows used outside a Table (e.g. an ItemsControl item template).
    public static readonly StyledProperty<string?> ColumnsProperty =
        AvaloniaProperty.Register<TableRow, string?>(nameof(Columns));

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<TableRow, bool>(nameof(IsSelected));

    private readonly Border _fill = new() { Opacity = 0, IsHitTestVisible = false, ZIndex = -1 };
    private readonly Border _divider = new() { Height = 1, VerticalAlignment = VerticalAlignment.Bottom, IsHitTestVisible = false };
    private IReadOnlyList<GridLength> _widths = Array.Empty<GridLength>();

    static TableRow()
    {
        ColumnsProperty.Changed.AddClassHandler<TableRow>((r, _) => r.Apply(Table.ParseColumns(r.Columns)));
        IsSelectedProperty.Changed.AddClassHandler<TableRow>((r, _) => r.UpdateFill());
    }

    public string? Columns
    {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    internal bool ShowDivider
    {
        get => _divider.IsVisible;
        set => _divider.IsVisible = value;
    }

    protected virtual bool Interactive => true;

    public TableRow()
    {
        _fill.Token(Border.BackgroundProperty, ShellToken.Muted);
        _divider.Token(Border.BackgroundProperty, ShellToken.Border);
        Children.Add(_fill);
        Children.Add(_divider);
        // A panel with no background only takes pointer input over its children.
        Background = Brushes.Transparent;
        Children.CollectionChanged += (_, _) => Apply(_widths);
        if (Interactive) Cursor = new Cursor(StandardCursorType.Hand);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        UpdateFill();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        UpdateFill();
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
            SetColumn(cells[i], i);
        SetColumnSpan(_fill, count);
        SetColumnSpan(_divider, count);
    }

    private void UpdateFill() => _fill.Opacity = IsSelected ? 1 : Interactive && IsPointerOver ? 0.5 : 0;
}

// Header row: holds TableHead cells, no hover.
public class TableHeader : TableRow
{
    protected override bool Interactive => false;
}

// Header cell — h-11 px-4 font-medium text-muted-foreground.
public class TableHead : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<TableHead, string?>(nameof(Text));

    public static readonly StyledProperty<TextAlignment> HorizontalTextAlignmentProperty =
        AvaloniaProperty.Register<TableHead, TextAlignment>(nameof(HorizontalTextAlignment), TextAlignment.Start);

    private readonly TextBlock _label;

    static TableHead()
    {
        TextProperty.Changed.AddClassHandler<TableHead>((h, _) => h._label.Text = h.Text ?? string.Empty);
        HorizontalTextAlignmentProperty.Changed.AddClassHandler<TableHead>((h, _) => h._label.TextAlignment = h.HorizontalTextAlignment);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public TextAlignment HorizontalTextAlignment
    {
        get => GetValue(HorizontalTextAlignmentProperty);
        set => SetValue(HorizontalTextAlignmentProperty, value);
    }

    public TableHead()
    {
        _label = new TextBlock { FontSize = 14, FontWeight = FontWeight.Medium, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        _label.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        Height = 44;
        Padding = new Thickness(16, 0);
        Child = _label;
    }
}

// Body cell — p-4 text-sm. Set Text, or put any control inside.
public class TableCell : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<TableCell, string?>(nameof(Text));

    public static readonly StyledProperty<TextAlignment> HorizontalTextAlignmentProperty =
        AvaloniaProperty.Register<TableCell, TextAlignment>(nameof(HorizontalTextAlignment), TextAlignment.Start);

    public static readonly StyledProperty<bool> IsBoldProperty =
        AvaloniaProperty.Register<TableCell, bool>(nameof(IsBold));

    private readonly TextBlock _label;

    static TableCell()
    {
        TextProperty.Changed.AddClassHandler<TableCell>((c, _) => c._label.Text = c.Text ?? string.Empty);
        HorizontalTextAlignmentProperty.Changed.AddClassHandler<TableCell>((c, _) => c._label.TextAlignment = c.HorizontalTextAlignment);
        IsBoldProperty.Changed.AddClassHandler<TableCell>((c, _) => c._label.FontWeight = c.IsBold ? FontWeight.Medium : FontWeight.Normal);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public TextAlignment HorizontalTextAlignment
    {
        get => GetValue(HorizontalTextAlignmentProperty);
        set => SetValue(HorizontalTextAlignmentProperty, value);
    }

    public bool IsBold
    {
        get => GetValue(IsBoldProperty);
        set => SetValue(IsBoldProperty, value);
    }

    public TableCell()
    {
        _label = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        _label.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        Padding = new Thickness(16, 12);
        Child = _label;
    }
}
"
    };
}
