using System;
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

namespace AvaloniaDemo.Components.UI;

// Data table — text-sm rows inside a rounded-md border: a header row in the muted foreground,
// rows divided by border-b that tint on hover, optional caption.
//   <ui:Table Columns="2*,*,*,100">
//       <ui:TableHeader>
//           <ui:TableHead Text="Invoice" /> <ui:TableHead Text="Status" /> ...
//       </ui:TableHeader>
//       <ui:TableRow Tapped="OnRowTapped">
//           <ui:TableCell Text="INV001" /> <ui:TableCell><ui:Badge Text="Paid" /></ui:TableCell> ...
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

    // Parses "2*,*,100" into column widths. Empty means equal star columns.
    internal static IReadOnlyList<GridLength> ParseColumns(string? columns)
    {
        if (string.IsNullOrWhiteSpace(columns)) return Array.Empty<GridLength>();
        return columns.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParseWidth)
            .ToList();

        static GridLength ParseWidth(string part)
        {
            if (part.Equals("auto", StringComparison.OrdinalIgnoreCase)) return GridLength.Auto;
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
