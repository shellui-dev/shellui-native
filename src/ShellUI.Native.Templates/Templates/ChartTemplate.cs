using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class ChartTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "chart",
        DisplayName = "Chart",
        Description = "Shared core of the charts: series, axes, legend, tooltip and animation",
        Category = ComponentCategory.DataDisplay,
        FilePath = "Chart.cs",
        Dependencies = new List<string> { "shell" },
        Tags = new List<string> { "chart", "graph", "core" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using System.Collections.ObjectModel;
using System.Globalization;

namespace YourProjectNamespace.Components.UI;

// The shared core of the charts (bar-chart, line-chart, area-chart, pie-chart, radar-chart,
// radial-chart): the series model, the axes, legend and tooltip, the grow-in animation, and the
// canvas they draw on. Charts are drawn with MAUI's GraphicsView, so they need no package, and
// they take their colors from the theme (ShellToken.Chart1 to Chart5).
// To make a chart of your own, derive from ChartBase and draw in DrawChart.

public enum ChartAlign { Start, Center, End }

// The drawing calls a chart needs. Angles are in degrees, clockwise, 0 at three o'clock.
public interface IChartCanvas
{
    void Line(Point from, Point to, Color color, double width, bool dashed = false);
    void Polyline(IReadOnlyList<Point> points, Color color, double width, bool closed = false);
    void Polygon(IReadOnlyList<Point> points, Color color);
    void Text(string text, Point at, double size, Color color, ChartAlign horizontal = ChartAlign.Start, ChartAlign vertical = ChartAlign.Start, bool bold = false);
    Size Measure(string text, double size, bool bold = false);
}

// A list of numbers that XAML can write as ""186, 305, 237"".
[System.ComponentModel.TypeConverter(typeof(ChartValuesConverter))]
public partial class ChartValues : List<double>
{
    public ChartValues()
    {
    }

    public ChartValues(IEnumerable<double> values) : base(values)
    {
    }

    public static implicit operator ChartValues(double[] values) => new(values);
}

public partial class ChartValuesConverter : System.ComponentModel.TypeConverter
{
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value is string text
            ? new ChartValues(text.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(part => double.Parse(part, CultureInfo.InvariantCulture)))
            : base.ConvertFrom(context, culture, value);
}

// A list of category names that XAML can write as ""Jan, Feb, Mar"".
[System.ComponentModel.TypeConverter(typeof(ChartLabelsConverter))]
public partial class ChartLabels : List<string>
{
    public ChartLabels()
    {
    }

    public ChartLabels(IEnumerable<string> labels) : base(labels)
    {
    }

    public static implicit operator ChartLabels(string[] labels) => new(labels);
}

public partial class ChartLabelsConverter : System.ComponentModel.TypeConverter
{
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value is string text
            ? new ChartLabels(text.Split(',').Select(part => part.Trim()))
            : base.ConvertFrom(context, culture, value);
}

// One named set of values. The chart redraws when a property is set; after changing the numbers
// inside Values, call the chart's Refresh().
public partial class ChartSeries
{
    private string _name = string.Empty;
    private ChartValues _values = new();
    private Color? _color;

    public string Name
    {
        get => _name;
        set
        {
            _name = value ?? string.Empty;
            Changed?.Invoke();
        }
    }

    public ChartValues Values
    {
        get => _values;
        set
        {
            _values = value ?? new ChartValues();
            Changed?.Invoke();
        }
    }

    // Replaces the theme's chart color for this series.
    public Color? Color
    {
        get => _color;
        set
        {
            _color = value;
            Changed?.Invoke();
        }
    }

    internal Action? Changed { get; set; }
}

// Geometry shared by the charts. Curves and arcs come back as points, so a canvas only has to
// draw straight segments.
public static class ChartMath
{
    public static Point Polar(Point center, double radius, double degrees)
    {
        var radians = degrees * Math.PI / 180;
        return new Point(center.X + radius * Math.Cos(radians), center.Y + radius * Math.Sin(radians));
    }

    public static List<Point> Arc(Point center, double radius, double startDegrees, double sweepDegrees)
    {
        var steps = Math.Max(1, (int)Math.Ceiling(Math.Abs(sweepDegrees) / 3));
        var points = new List<Point>(steps + 1);
        for (var i = 0; i <= steps; i++) points.Add(Polar(center, radius, startDegrees + sweepDegrees * i / steps));
        return points;
    }

    // A pie slice, or a ring segment when inner is above zero.
    public static List<Point> Sector(Point center, double inner, double outer, double startDegrees, double sweepDegrees)
    {
        var points = Arc(center, outer, startDegrees, sweepDegrees);
        if (inner <= 0) points.Add(center);
        else points.AddRange(Arc(center, inner, startDegrees + sweepDegrees, -sweepDegrees));
        return points;
    }

    public static List<Point> Circle(Point center, double radius) => Arc(center, radius, 0, 360);

    // A rectangle with its own radius on each corner, clockwise from the top left.
    public static List<Point> RoundRect(Rect rect, double topLeft, double topRight, double bottomRight, double bottomLeft)
    {
        var limit = Math.Min(rect.Width, rect.Height) / 2;
        var points = new List<Point>();
        Corner(points, rect.Left, rect.Top, 1, 1, Math.Min(topLeft, limit), 180);
        Corner(points, rect.Right, rect.Top, -1, 1, Math.Min(topRight, limit), 270);
        Corner(points, rect.Right, rect.Bottom, -1, -1, Math.Min(bottomRight, limit), 0);
        Corner(points, rect.Left, rect.Bottom, 1, -1, Math.Min(bottomLeft, limit), 90);
        return points;
    }

    private static void Corner(List<Point> points, double x, double y, int inX, int inY, double radius, double start)
    {
        if (radius < 0.5)
        {
            points.Add(new Point(x, y));
            return;
        }
        var center = new Point(x + inX * radius, y + inY * radius);
        for (var i = 0; i <= 6; i++) points.Add(Polar(center, radius, start + 15 * i));
    }

    // Rounds a range out to steps of 1, 2 or 5 times a power of ten.
    public static (double Min, double Max, double Step) NiceScale(double min, double max, int ticks)
    {
        if (max <= min) max = min + 1;
        var rough = (max - min) / Math.Max(1, ticks);
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(rough)));
        var fraction = rough / magnitude;
        var step = (fraction <= 1 ? 1 : fraction <= 2 ? 2 : fraction <= 5 ? 5 : 10) * magnitude;
        return (Math.Floor(min / step) * step, Math.Ceiling(max / step) * step, step);
    }

    // A smooth curve through the points that never overshoots them (monotone cubic).
    public static List<Point> Smooth(IReadOnlyList<Point> points)
    {
        var count = points.Count;
        if (count < 3) return points.ToList();
        var slope = new double[count - 1];
        var tangent = new double[count];
        for (var i = 0; i < count - 1; i++)
        {
            var dx = points[i + 1].X - points[i].X;
            slope[i] = dx == 0 ? 0 : (points[i + 1].Y - points[i].Y) / dx;
        }
        tangent[0] = slope[0];
        tangent[count - 1] = slope[count - 2];
        for (var i = 1; i < count - 1; i++) tangent[i] = slope[i - 1] * slope[i] <= 0 ? 0 : (slope[i - 1] + slope[i]) / 2;
        for (var i = 0; i < count - 1; i++)
        {
            if (slope[i] == 0)
            {
                tangent[i] = 0;
                tangent[i + 1] = 0;
                continue;
            }
            var a = tangent[i] / slope[i];
            var b = tangent[i + 1] / slope[i];
            var length = a * a + b * b;
            if (length <= 9) continue;
            var scale = 3 / Math.Sqrt(length);
            tangent[i] = scale * a * slope[i];
            tangent[i + 1] = scale * b * slope[i];
        }

        var curve = new List<Point> { points[0] };
        for (var i = 0; i < count - 1; i++)
        {
            var width = points[i + 1].X - points[i].X;
            for (var step = 1; step <= 12; step++)
            {
                var t = step / 12.0;
                var t2 = t * t;
                var t3 = t2 * t;
                var y = (2 * t3 - 3 * t2 + 1) * points[i].Y + (t3 - 2 * t2 + t) * width * tangent[i]
                    + (-2 * t3 + 3 * t2) * points[i + 1].Y + (t3 - t2) * width * tangent[i + 1];
                curve.Add(new Point(points[i].X + width * t, y));
            }
        }
        return curve;
    }
}

// Where a bar, line or area chart's plot sits and how values and categories map onto it.
public sealed class ChartFrame
{
    public ChartFrame(Rect plot, double min, double max, int count, bool bands, bool horizontal)
    {
        Plot = plot;
        Min = min;
        Max = max;
        Count = count;
        Bands = bands;
        Horizontal = horizontal;
    }

    public Rect Plot { get; }
    public double Min { get; }
    public double Max { get; }
    public int Count { get; }
    // Bars get a band each; lines and areas run from edge to edge.
    public bool Bands { get; }
    public bool Horizontal { get; }

    private double Start => Horizontal ? Plot.Top : Plot.Left;
    private double Length => Horizontal ? Plot.Height : Plot.Width;

    // The room one category takes along the category axis.
    public double Band => Length / Math.Max(1, Bands ? Count : Count - 1);

    // Where a value sits along the value axis.
    public double Value(double value)
    {
        var t = Max > Min ? (value - Min) / (Max - Min) : 0;
        return Horizontal ? Plot.Left + t * Plot.Width : Plot.Bottom - t * Plot.Height;
    }

    // The middle of a category along the category axis.
    public double Category(int index)
    {
        if (Bands) return Start + (index + 0.5) * Band;
        return Count <= 1 ? Start + Length / 2 : Start + index * Band;
    }

    // The category under a point, or -1 outside the plot.
    public int IndexAt(Point point)
    {
        if (Count == 0 || point.X < Plot.Left - 12 || point.X > Plot.Right + 12 || point.Y < Plot.Top - 12 || point.Y > Plot.Bottom + 12) return -1;
        var along = (Horizontal ? point.Y : point.X) - Start;
        var index = Bands ? (int)Math.Floor(along / Band) : (int)Math.Round(along / Band);
        return Math.Clamp(index, 0, Count - 1);
    }
}

internal static class ChartColor
{
    public static Color Fade(Color color, double opacity) => color.WithAlpha((float)(color.Alpha * opacity));
}

// IChartCanvas on a MAUI ICanvas.
internal sealed class ChartCanvas : IChartCanvas
{
    private readonly ICanvas _canvas;

    public ChartCanvas(ICanvas canvas) => _canvas = canvas;

    public void Line(Point from, Point to, Color color, double width, bool dashed = false)
    {
        _canvas.StrokeColor = color;
        _canvas.StrokeSize = (float)width;
        _canvas.StrokeLineCap = LineCap.Butt;
        _canvas.StrokeDashPattern = dashed ? new float[] { 3, 3 } : null;
        _canvas.DrawLine((float)from.X, (float)from.Y, (float)to.X, (float)to.Y);
    }

    public void Polyline(IReadOnlyList<Point> points, Color color, double width, bool closed = false)
    {
        if (points.Count < 2) return;
        _canvas.StrokeColor = color;
        _canvas.StrokeSize = (float)width;
        _canvas.StrokeLineCap = LineCap.Round;
        _canvas.StrokeLineJoin = LineJoin.Round;
        _canvas.StrokeDashPattern = null;
        _canvas.DrawPath(Path(points, closed));
    }

    public void Polygon(IReadOnlyList<Point> points, Color color)
    {
        if (points.Count < 3) return;
        _canvas.FillColor = color;
        _canvas.FillPath(Path(points, closed: true));
    }

    public void Text(string text, Point at, double size, Color color, ChartAlign horizontal = ChartAlign.Start, ChartAlign vertical = ChartAlign.Start, bool bold = false)
    {
        if (string.IsNullOrEmpty(text)) return;
        var measured = Measure(text, size, bold);
        // The platform aligns the text inside a roomy box around the anchor, so a measurement
        // that is a little off never clips or shifts it.
        var width = measured.Width + 24;
        var height = measured.Height * 2;
        var x = horizontal == ChartAlign.Start ? at.X : horizontal == ChartAlign.Center ? at.X - width / 2 : at.X - width;
        var middle = vertical == ChartAlign.Start ? at.Y + measured.Height / 2 : vertical == ChartAlign.Center ? at.Y : at.Y - measured.Height / 2;
        _canvas.Font = bold ? Microsoft.Maui.Graphics.Font.DefaultBold : Microsoft.Maui.Graphics.Font.Default;
        _canvas.FontSize = (float)size;
        _canvas.FontColor = color;
        _canvas.DrawString(text, (float)x, (float)(middle - height / 2), (float)width, (float)height,
            horizontal == ChartAlign.Start ? HorizontalAlignment.Left : horizontal == ChartAlign.Center ? HorizontalAlignment.Center : HorizontalAlignment.Right,
            VerticalAlignment.Center, TextFlow.OverflowBounds);
    }

    public Size Measure(string text, double size, bool bold = false)
    {
        if (string.IsNullOrEmpty(text)) return Size.Zero;
        var measured = _canvas.GetStringSize(text, bold ? Microsoft.Maui.Graphics.Font.DefaultBold : Microsoft.Maui.Graphics.Font.Default, (float)size);
        // Platforms disagree about the height of a line, so it comes from the font size.
        return new Size(Math.Ceiling(measured.Width) + 1, Math.Ceiling(size * 1.35));
    }

    private static PathF Path(IReadOnlyList<Point> points, bool closed)
    {
        var path = new PathF();
        path.MoveTo((float)points[0].X, (float)points[0].Y);
        for (var i = 1; i < points.Count; i++) path.LineTo((float)points[i].X, (float)points[i].Y);
        if (closed) path.Close();
        return path;
    }
}

// What every chart shares: its series and labels, the legend, the tooltip under the pointer and
// the grow-in animation. Give a chart a height.
[ContentProperty(nameof(Series))]
public abstract partial class ChartBase : ContentView, IDrawable
{
    // The category names along the axis, or the slice names of a pie.
    public static readonly BindableProperty LabelsProperty =
        BindableProperty.Create(nameof(Labels), typeof(ChartLabels), typeof(ChartBase), null,
            propertyChanged: (b, o, n) => ((ChartBase)b).Refresh());

    public static readonly BindableProperty ShowLegendProperty =
        BindableProperty.Create(nameof(ShowLegend), typeof(bool), typeof(ChartBase), true,
            propertyChanged: (b, o, n) => ((ChartBase)b).Refresh());

    // Shows the values under the pointer (or the finger).
    public static readonly BindableProperty ShowTooltipProperty =
        BindableProperty.Create(nameof(ShowTooltip), typeof(bool), typeof(ChartBase), true,
            propertyChanged: (b, o, n) => ((ChartBase)b).Refresh());

    // Grows the chart in when it first appears.
    public static readonly BindableProperty IsAnimatedProperty =
        BindableProperty.Create(nameof(IsAnimated), typeof(bool), typeof(ChartBase), true,
            propertyChanged: (b, o, n) => ((ChartBase)b).Refresh());

    // A .NET number format for axis and tooltip values.
    public static readonly BindableProperty ValueFormatProperty =
        BindableProperty.Create(nameof(ValueFormat), typeof(string), typeof(ChartBase), ""#,0.##"",
            propertyChanged: (b, o, n) => ((ChartBase)b).Refresh());

    public static readonly BindableProperty EmptyTextProperty =
        BindableProperty.Create(nameof(EmptyText), typeof(string), typeof(ChartBase), ""No data"",
            propertyChanged: (b, o, n) => ((ChartBase)b).Refresh());

    // What the chart shows, for screen readers.
    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(ChartBase), """",
            propertyChanged: (b, o, n) => ((ChartBase)b).Refresh());

    public ChartLabels? Labels
    {
        get => (ChartLabels?)GetValue(LabelsProperty);
        set => SetValue(LabelsProperty, value);
    }

    public bool ShowLegend
    {
        get => (bool)GetValue(ShowLegendProperty);
        set => SetValue(ShowLegendProperty, value);
    }

    public bool ShowTooltip
    {
        get => (bool)GetValue(ShowTooltipProperty);
        set => SetValue(ShowTooltipProperty, value);
    }

    public bool IsAnimated
    {
        get => (bool)GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }

    public string ValueFormat
    {
        get => (string)GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }

    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public IList<ChartSeries> Series => _series;

    private const string GrowAnimation = ""ChartGrow"";

    private readonly ObservableCollection<ChartSeries> _series = new();
    private readonly GraphicsView _view;
    private Point _pointer;
    private int _hover = -1;
    private double _progress = 1;

    protected ChartBase()
    {
        _view = new GraphicsView { Drawable = this, BackgroundColor = Colors.Transparent };
        _view.MoveHoverInteraction += (_, e) => Hover(First(e.Touches));
        _view.EndHoverInteraction += (_, _) => Hover(null);
        // A touch has no hover: a tap or a drag shows the tooltip instead.
        _view.StartInteraction += (_, e) => Hover(First(e.Touches));
        _view.DragInteraction += (_, e) => Hover(First(e.Touches));
        _series.CollectionChanged += (_, _) =>
        {
            foreach (var series in _series) series.Changed = Refresh;
            Refresh();
        };
        Content = _view;
        MinimumHeightRequest = 160;
        Loaded += (_, _) =>
        {
            ShellTheme.ThemeChanged += OnThemeChanged;
            PlayAnimation();
        };
        Unloaded += (_, _) => ShellTheme.ThemeChanged -= OnThemeChanged;
    }

    // Redraws the chart. Call it after changing the numbers inside a series' Values.
    public void Refresh() => _view?.Invalidate();

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == DescriptionProperty.PropertyName)
            SemanticProperties.SetDescription(this, Description ?? string.Empty);
    }

    // Grows the chart in again.
    public void PlayAnimation()
    {
        this.AbortAnimation(GrowAnimation);
        if (!IsAnimated)
        {
            _progress = 1;
            Refresh();
            return;
        }
        new Animation(value =>
        {
            _progress = value;
            Refresh();
        }, 0, 1, Easing.CubicOut).Commit(this, GrowAnimation, 16, 600);
    }

    void IDrawable.Draw(ICanvas canvas, RectF dirtyRect) =>
        Paint(new ChartCanvas(canvas), new Size(dirtyRect.Width, dirtyRect.Height));

    private static Point? First(PointF[] touches) => touches.Length > 0 ? new Point(touches[0].X, touches[0].Y) : null;

    // The theme's chart colors in order; past the fifth they repeat, fainter.
    protected static Color Palette(int index)
    {
        var color = ShellTheme.Get(ShellToken.Chart1 + index % 5);
        return index < 5 ? color : ChartColor.Fade(color, 0.6);
    }

    protected Color SeriesColor(int index) => Series[index].Color ?? Palette(index);

    protected int CategoryCount => Math.Max(Labels?.Count ?? 0, Series.Count == 0 ? 0 : Series.Max(s => s.Values.Count));

    protected string LabelAt(int index) =>
        Labels != null && index < Labels.Count ? Labels[index] : (index + 1).ToString(CultureInfo.CurrentCulture);

    protected static double ValueAt(ChartSeries series, int index) => index < series.Values.Count ? series.Values[index] : 0;

    protected string Format(double value) =>
        value.ToString(string.IsNullOrEmpty(ValueFormat) ? ""#,0.##"" : ValueFormat, CultureInfo.CurrentCulture);

    // 0 to 1 while the chart animates in, then 1.
    protected double Progress => IsAnimated ? _progress : 1;

    // The category, slice or ring under the pointer, or -1.
    protected int HoveredIndex => ShowTooltip ? _hover : -1;

    protected virtual bool HasData => Series.Any(s => s.Values.Count > 0);

    // Draws the chart itself into `area`; the legend and tooltip are added around it.
    protected abstract void DrawChart(IChartCanvas canvas, Rect area);

    protected virtual int HitTest(Point point) => -1;

    protected virtual List<(Color Color, string Name)> LegendEntries() =>
        Series.Select((s, i) => (SeriesColor(i), s.Name)).Where(entry => !string.IsNullOrEmpty(entry.Name)).ToList();

    protected virtual (string Title, List<(Color Color, string Name, string Value)> Rows) Tooltip(int index) =>
        (LabelAt(index), Series.Select((s, i) => (SeriesColor(i), s.Name, Format(ValueAt(s, index)))).ToList());

    private void OnThemeChanged(object? sender, EventArgs e) => Refresh();

    private void Hover(Point? point)
    {
        var index = point is { } at ? HitTest(at) : -1;
        if (index < 0 && _hover < 0) return;
        _pointer = point ?? _pointer;
        _hover = index;
        Refresh();
    }

    private void Paint(IChartCanvas canvas, Size size)
    {
        if (size.Width < 8 || size.Height < 8) return;
        if (!HasData)
        {
            canvas.Text(EmptyText ?? string.Empty, new Point(size.Width / 2, size.Height / 2), 14,
                ShellTheme.Get(ShellToken.MutedForeground), ChartAlign.Center, ChartAlign.Center);
            return;
        }

        var legend = ShowLegend ? LegendEntries() : new List<(Color Color, string Name)>();
        var legendHeight = legend.Count > 0 ? DrawLegend(canvas, legend, size, draw: false) : 0;
        DrawChart(canvas, new Rect(0, 0, size.Width, size.Height - legendHeight));
        if (legend.Count > 0) DrawLegend(canvas, legend, size, draw: true);
        if (HoveredIndex >= 0) DrawTooltip(canvas, size);
    }

    // justify-center gap-4 text-xs: a 8px rounded square and the name per entry, wrapped into
    // as many centered rows as it takes. Returns the height the legend needs.
    private static double DrawLegend(IChartCanvas canvas, List<(Color Color, string Name)> entries, Size size, bool draw)
    {
        const double fontSize = 12, mark = 8, markGap = 6, gap = 16, rowHeight = 20, top = 12;
        var widths = entries.Select(e => mark + markGap + canvas.Measure(e.Name, fontSize).Width).ToList();
        var rows = new List<List<int>> { new() };
        var used = 0.0;
        for (var i = 0; i < entries.Count; i++)
        {
            var next = used == 0 ? widths[i] : used + gap + widths[i];
            if (used > 0 && next > size.Width)
            {
                rows.Add(new List<int>());
                next = widths[i];
            }
            rows[^1].Add(i);
            used = next;
        }

        var height = top + rows.Count * rowHeight;
        if (!draw) return height;
        var text = ShellTheme.Get(ShellToken.Foreground);
        var y = size.Height - rows.Count * rowHeight;
        foreach (var row in rows)
        {
            var x = (size.Width - row.Sum(i => widths[i]) - gap * (row.Count - 1)) / 2;
            foreach (var i in row)
            {
                var middle = y + rowHeight / 2;
                canvas.Polygon(ChartMath.RoundRect(new Rect(x, middle - mark / 2, mark, mark), 2, 2, 2, 2), entries[i].Color);
                canvas.Text(entries[i].Name, new Point(x + mark + markGap, middle), fontSize, text, ChartAlign.Start, ChartAlign.Center);
                x += widths[i] + gap;
            }
            y += rowHeight;
        }
        return height;
    }

    // rounded-lg border bg-background px-2.5 py-1.5 text-xs: a title, then a colored square,
    // the name and the value per row. It follows the pointer and stays inside the chart.
    private void DrawTooltip(IChartCanvas canvas, Size size)
    {
        var (title, rows) = Tooltip(_hover);
        if (rows.Count == 0) return;
        const double fontSize = 12, pad = 10, rowHeight = 18, mark = 10;
        var hasTitle = !string.IsNullOrEmpty(title);
        var names = rows.Max(r => canvas.Measure(r.Name, fontSize).Width);
        var values = rows.Max(r => canvas.Measure(r.Value, fontSize, bold: true).Width);
        var width = Math.Max(128, pad * 2 + mark + 6 + names + 16 + values);
        if (hasTitle) width = Math.Max(width, pad * 2 + canvas.Measure(title, fontSize, bold: true).Width);
        var height = 16 + rows.Count * rowHeight + (hasTitle ? rowHeight + 2 : 0);

        var x = _pointer.X + 14;
        if (x + width > size.Width) x = _pointer.X - 14 - width;
        x = Math.Clamp(x, 0, Math.Max(0, size.Width - width));
        var y = _pointer.Y + 14;
        if (y + height > size.Height) y = _pointer.Y - 14 - height;
        y = Math.Clamp(y, 0, Math.Max(0, size.Height - height));

        canvas.Polygon(ChartMath.RoundRect(new Rect(x, y, width, height), 8, 8, 8, 8), ShellTheme.Get(ShellToken.Border));
        canvas.Polygon(ChartMath.RoundRect(new Rect(x + 1, y + 1, width - 2, height - 2), 7, 7, 7, 7), ShellTheme.Get(ShellToken.Background));
        var foreground = ShellTheme.Get(ShellToken.Foreground);
        var muted = ShellTheme.Get(ShellToken.MutedForeground);
        var top = y + 8;
        if (hasTitle)
        {
            canvas.Text(title, new Point(x + pad, top + rowHeight / 2), fontSize, foreground, ChartAlign.Start, ChartAlign.Center, bold: true);
            top += rowHeight + 2;
        }
        foreach (var row in rows)
        {
            var middle = top + rowHeight / 2;
            canvas.Polygon(ChartMath.RoundRect(new Rect(x + pad, middle - mark / 2, mark, mark), 2, 2, 2, 2), row.Color);
            canvas.Text(row.Name, new Point(x + pad + mark + 6, middle), fontSize, muted, ChartAlign.Start, ChartAlign.Center);
            canvas.Text(row.Value, new Point(x + width - pad, middle), fontSize, foreground, ChartAlign.End, ChartAlign.Center, bold: true);
            top += rowHeight;
        }
    }

    // A large value over a small label, for the middle of a donut or radial chart.
    protected static void DrawCenterText(IChartCanvas canvas, Point center, string? value, string? label)
    {
        var hasValue = !string.IsNullOrEmpty(value);
        var hasLabel = !string.IsNullOrEmpty(label);
        if (hasValue)
            canvas.Text(value!, new Point(center.X, center.Y - (hasLabel ? 8 : 0)), 24, ShellTheme.Get(ShellToken.Foreground), ChartAlign.Center, ChartAlign.Center, bold: true);
        if (hasLabel)
            canvas.Text(label!, new Point(center.X, center.Y + (hasValue ? 14 : 0)), 12, ShellTheme.Get(ShellToken.MutedForeground), ChartAlign.Center, ChartAlign.Center);
    }
}

// What bar, line and area charts share: a value axis, a category axis and a grid.
public abstract partial class CartesianChart : ChartBase
{
    public static readonly BindableProperty ShowGridProperty =
        BindableProperty.Create(nameof(ShowGrid), typeof(bool), typeof(CartesianChart), true,
            propertyChanged: (b, o, n) => ((CartesianChart)b).Refresh());

    // The labels under the plot.
    public static readonly BindableProperty ShowXAxisProperty =
        BindableProperty.Create(nameof(ShowXAxis), typeof(bool), typeof(CartesianChart), true,
            propertyChanged: (b, o, n) => ((CartesianChart)b).Refresh());

    // The labels beside the plot.
    public static readonly BindableProperty ShowYAxisProperty =
        BindableProperty.Create(nameof(ShowYAxis), typeof(bool), typeof(CartesianChart), true,
            propertyChanged: (b, o, n) => ((CartesianChart)b).Refresh());

    // The ends of the value axis; by default they fit the data and include zero.
    public static readonly BindableProperty MinimumProperty =
        BindableProperty.Create(nameof(Minimum), typeof(double), typeof(CartesianChart), double.NaN,
            propertyChanged: (b, o, n) => ((CartesianChart)b).Refresh());

    public static readonly BindableProperty MaximumProperty =
        BindableProperty.Create(nameof(Maximum), typeof(double), typeof(CartesianChart), double.NaN,
            propertyChanged: (b, o, n) => ((CartesianChart)b).Refresh());

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public bool ShowXAxis
    {
        get => (bool)GetValue(ShowXAxisProperty);
        set => SetValue(ShowXAxisProperty, value);
    }

    public bool ShowYAxis
    {
        get => (bool)GetValue(ShowYAxisProperty);
        set => SetValue(ShowYAxisProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    // The frame of the last draw, for hit testing.
    protected ChartFrame? LastFrame { get; private set; }

    protected override int HitTest(Point point) => LastFrame?.IndexAt(point) ?? -1;

    // Draws the grid and the axis labels into `area` and returns how values and categories map
    // onto the plot. The value axis always includes zero unless Minimum or Maximum say otherwise.
    protected ChartFrame DrawAxes(IChartCanvas canvas, Rect area, double dataMin, double dataMax, bool bands, bool horizontal = false)
    {
        const double fontSize = 12;
        var muted = ShellTheme.Get(ShellToken.MutedForeground);
        var line = ShellTheme.Get(ShellToken.Border);
        var count = CategoryCount;
        var (low, high, step) = ChartMath.NiceScale(
            double.IsNaN(Minimum) ? Math.Min(0, dataMin) : Minimum,
            double.IsNaN(Maximum) ? Math.Max(0, dataMax) : Maximum, 5);
        if (!double.IsNaN(Minimum)) low = Minimum;
        if (!double.IsNaN(Maximum)) high = Maximum;
        var ticks = new List<double>();
        for (var tick = Math.Ceiling(low / step) * step; tick <= high + step / 1000; tick += step)
            ticks.Add(Math.Abs(tick) < step / 1000 ? 0 : tick);

        var showValues = horizontal ? ShowXAxis : ShowYAxis;
        var showCategories = horizontal ? ShowYAxis : ShowXAxis;
        var labels = Enumerable.Range(0, count).Select(LabelAt).ToList();
        double left = 0;
        if (horizontal && showCategories) left = labels.Max(label => canvas.Measure(label, fontSize).Width) + 8;
        if (!horizontal && showValues) left = ticks.Max(tick => canvas.Measure(Format(tick), fontSize).Width) + 8;
        // An edge-to-edge axis needs room for half of its first and last label.
        if (!bands && !horizontal) left = Math.Max(left, 12);
        var right = horizontal ? 16 : bands ? 0 : 12;
        var bottom = (horizontal ? showValues : showCategories) ? 24 : 0;
        var plot = new Rect(area.Left + left, area.Top + 8, Math.Max(1, area.Width - left - right), Math.Max(1, area.Height - 8 - bottom));
        var frame = new ChartFrame(plot, low, high, count, bands, horizontal);

        foreach (var tick in ticks)
        {
            var at = frame.Value(tick);
            // Half a pixel off the grid keeps a 1px line sharp.
            var sharp = Math.Round(at) + 0.5;
            if (ShowGrid)
            {
                if (horizontal) canvas.Line(new Point(sharp, plot.Top), new Point(sharp, plot.Bottom), line, 1);
                else canvas.Line(new Point(plot.Left, sharp), new Point(plot.Right, sharp), line, 1);
            }
            if (!showValues) continue;
            if (horizontal) canvas.Text(Format(tick), new Point(at, plot.Bottom + 8), fontSize, muted, ChartAlign.Center);
            else canvas.Text(Format(tick), new Point(plot.Left - 8, at), fontSize, muted, ChartAlign.End, ChartAlign.Center);
        }

        if (showCategories && horizontal)
        {
            for (var i = 0; i < count; i++)
                canvas.Text(labels[i], new Point(plot.Left - 8, frame.Category(i)), fontSize, muted, ChartAlign.End, ChartAlign.Center);
        }
        else if (showCategories)
        {
            // Labels that would run into each other are thinned out.
            var widest = labels.Max(label => canvas.Measure(label, fontSize).Width);
            var every = Math.Max(1, (int)Math.Ceiling((widest + 8) / Math.Max(1, frame.Band)));
            for (var i = 0; i < count; i += every)
                canvas.Text(labels[i], new Point(frame.Category(i), plot.Bottom + 8), fontSize, muted, ChartAlign.Center);
        }

        LastFrame = frame;
        return frame;
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Metadata;
using Avalonia.Threading;

namespace YourProjectNamespace.Components.UI;

// The shared core of the charts (bar-chart, line-chart, area-chart, pie-chart, radar-chart,
// radial-chart): the series model, the axes, legend and tooltip, the grow-in animation, and the
// canvas they draw on. Charts are drawn in Render, so they need no package, and they take their
// colors from the theme (ShellToken.Chart1 to Chart5).
// To make a chart of your own, derive from ChartBase and draw in DrawChart.

public enum ChartAlign { Start, Center, End }

// The drawing calls a chart needs. Angles are in degrees, clockwise, 0 at three o'clock.
public interface IChartCanvas
{
    void Line(Point from, Point to, Color color, double width, bool dashed = false);
    void Polyline(IReadOnlyList<Point> points, Color color, double width, bool closed = false);
    void Polygon(IReadOnlyList<Point> points, Color color);
    void Text(string text, Point at, double size, Color color, ChartAlign horizontal = ChartAlign.Start, ChartAlign vertical = ChartAlign.Start, bool bold = false);
    Size Measure(string text, double size, bool bold = false);
}

// A list of numbers that XAML can write as ""186, 305, 237"".
[System.ComponentModel.TypeConverter(typeof(ChartValuesConverter))]
public class ChartValues : List<double>
{
    public ChartValues()
    {
    }

    public ChartValues(IEnumerable<double> values) : base(values)
    {
    }

    public static implicit operator ChartValues(double[] values) => new(values);
}

public class ChartValuesConverter : System.ComponentModel.TypeConverter
{
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value is string text
            ? new ChartValues(text.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(part => double.Parse(part, CultureInfo.InvariantCulture)))
            : base.ConvertFrom(context, culture, value);
}

// A list of category names that XAML can write as ""Jan, Feb, Mar"".
[System.ComponentModel.TypeConverter(typeof(ChartLabelsConverter))]
public class ChartLabels : List<string>
{
    public ChartLabels()
    {
    }

    public ChartLabels(IEnumerable<string> labels) : base(labels)
    {
    }

    public static implicit operator ChartLabels(string[] labels) => new(labels);
}

public class ChartLabelsConverter : System.ComponentModel.TypeConverter
{
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value is string text
            ? new ChartLabels(text.Split(',').Select(part => part.Trim()))
            : base.ConvertFrom(context, culture, value);
}

// One named set of values. The chart redraws when a property is set; after changing the numbers
// inside Values, call the chart's Refresh().
public class ChartSeries
{
    private string _name = string.Empty;
    private ChartValues _values = new();
    private Color? _color;

    public string Name
    {
        get => _name;
        set
        {
            _name = value ?? string.Empty;
            Changed?.Invoke();
        }
    }

    public ChartValues Values
    {
        get => _values;
        set
        {
            _values = value ?? new ChartValues();
            Changed?.Invoke();
        }
    }

    // Replaces the theme's chart color for this series.
    public Color? Color
    {
        get => _color;
        set
        {
            _color = value;
            Changed?.Invoke();
        }
    }

    internal Action? Changed { get; set; }
}

// Geometry shared by the charts. Curves and arcs come back as points, so a canvas only has to
// draw straight segments.
public static class ChartMath
{
    public static Point Polar(Point center, double radius, double degrees)
    {
        var radians = degrees * Math.PI / 180;
        return new Point(center.X + radius * Math.Cos(radians), center.Y + radius * Math.Sin(radians));
    }

    public static List<Point> Arc(Point center, double radius, double startDegrees, double sweepDegrees)
    {
        var steps = Math.Max(1, (int)Math.Ceiling(Math.Abs(sweepDegrees) / 3));
        var points = new List<Point>(steps + 1);
        for (var i = 0; i <= steps; i++) points.Add(Polar(center, radius, startDegrees + sweepDegrees * i / steps));
        return points;
    }

    // A pie slice, or a ring segment when inner is above zero.
    public static List<Point> Sector(Point center, double inner, double outer, double startDegrees, double sweepDegrees)
    {
        var points = Arc(center, outer, startDegrees, sweepDegrees);
        if (inner <= 0) points.Add(center);
        else points.AddRange(Arc(center, inner, startDegrees + sweepDegrees, -sweepDegrees));
        return points;
    }

    public static List<Point> Circle(Point center, double radius) => Arc(center, radius, 0, 360);

    // A rectangle with its own radius on each corner, clockwise from the top left.
    public static List<Point> RoundRect(Rect rect, double topLeft, double topRight, double bottomRight, double bottomLeft)
    {
        var limit = Math.Min(rect.Width, rect.Height) / 2;
        var points = new List<Point>();
        Corner(points, rect.Left, rect.Top, 1, 1, Math.Min(topLeft, limit), 180);
        Corner(points, rect.Right, rect.Top, -1, 1, Math.Min(topRight, limit), 270);
        Corner(points, rect.Right, rect.Bottom, -1, -1, Math.Min(bottomRight, limit), 0);
        Corner(points, rect.Left, rect.Bottom, 1, -1, Math.Min(bottomLeft, limit), 90);
        return points;
    }

    private static void Corner(List<Point> points, double x, double y, int inX, int inY, double radius, double start)
    {
        if (radius < 0.5)
        {
            points.Add(new Point(x, y));
            return;
        }
        var center = new Point(x + inX * radius, y + inY * radius);
        for (var i = 0; i <= 6; i++) points.Add(Polar(center, radius, start + 15 * i));
    }

    // Rounds a range out to steps of 1, 2 or 5 times a power of ten.
    public static (double Min, double Max, double Step) NiceScale(double min, double max, int ticks)
    {
        if (max <= min) max = min + 1;
        var rough = (max - min) / Math.Max(1, ticks);
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(rough)));
        var fraction = rough / magnitude;
        var step = (fraction <= 1 ? 1 : fraction <= 2 ? 2 : fraction <= 5 ? 5 : 10) * magnitude;
        return (Math.Floor(min / step) * step, Math.Ceiling(max / step) * step, step);
    }

    // A smooth curve through the points that never overshoots them (monotone cubic).
    public static List<Point> Smooth(IReadOnlyList<Point> points)
    {
        var count = points.Count;
        if (count < 3) return points.ToList();
        var slope = new double[count - 1];
        var tangent = new double[count];
        for (var i = 0; i < count - 1; i++)
        {
            var dx = points[i + 1].X - points[i].X;
            slope[i] = dx == 0 ? 0 : (points[i + 1].Y - points[i].Y) / dx;
        }
        tangent[0] = slope[0];
        tangent[count - 1] = slope[count - 2];
        for (var i = 1; i < count - 1; i++) tangent[i] = slope[i - 1] * slope[i] <= 0 ? 0 : (slope[i - 1] + slope[i]) / 2;
        for (var i = 0; i < count - 1; i++)
        {
            if (slope[i] == 0)
            {
                tangent[i] = 0;
                tangent[i + 1] = 0;
                continue;
            }
            var a = tangent[i] / slope[i];
            var b = tangent[i + 1] / slope[i];
            var length = a * a + b * b;
            if (length <= 9) continue;
            var scale = 3 / Math.Sqrt(length);
            tangent[i] = scale * a * slope[i];
            tangent[i + 1] = scale * b * slope[i];
        }

        var curve = new List<Point> { points[0] };
        for (var i = 0; i < count - 1; i++)
        {
            var width = points[i + 1].X - points[i].X;
            for (var step = 1; step <= 12; step++)
            {
                var t = step / 12.0;
                var t2 = t * t;
                var t3 = t2 * t;
                var y = (2 * t3 - 3 * t2 + 1) * points[i].Y + (t3 - 2 * t2 + t) * width * tangent[i]
                    + (-2 * t3 + 3 * t2) * points[i + 1].Y + (t3 - t2) * width * tangent[i + 1];
                curve.Add(new Point(points[i].X + width * t, y));
            }
        }
        return curve;
    }
}

// Where a bar, line or area chart's plot sits and how values and categories map onto it.
public sealed class ChartFrame
{
    public ChartFrame(Rect plot, double min, double max, int count, bool bands, bool horizontal)
    {
        Plot = plot;
        Min = min;
        Max = max;
        Count = count;
        Bands = bands;
        Horizontal = horizontal;
    }

    public Rect Plot { get; }
    public double Min { get; }
    public double Max { get; }
    public int Count { get; }
    // Bars get a band each; lines and areas run from edge to edge.
    public bool Bands { get; }
    public bool Horizontal { get; }

    private double Start => Horizontal ? Plot.Top : Plot.Left;
    private double Length => Horizontal ? Plot.Height : Plot.Width;

    // The room one category takes along the category axis.
    public double Band => Length / Math.Max(1, Bands ? Count : Count - 1);

    // Where a value sits along the value axis.
    public double Value(double value)
    {
        var t = Max > Min ? (value - Min) / (Max - Min) : 0;
        return Horizontal ? Plot.Left + t * Plot.Width : Plot.Bottom - t * Plot.Height;
    }

    // The middle of a category along the category axis.
    public double Category(int index)
    {
        if (Bands) return Start + (index + 0.5) * Band;
        return Count <= 1 ? Start + Length / 2 : Start + index * Band;
    }

    // The category under a point, or -1 outside the plot.
    public int IndexAt(Point point)
    {
        if (Count == 0 || point.X < Plot.Left - 12 || point.X > Plot.Right + 12 || point.Y < Plot.Top - 12 || point.Y > Plot.Bottom + 12) return -1;
        var along = (Horizontal ? point.Y : point.X) - Start;
        var index = Bands ? (int)Math.Floor(along / Band) : (int)Math.Round(along / Band);
        return Math.Clamp(index, 0, Count - 1);
    }
}

internal static class ChartColor
{
    public static Color Fade(Color color, double opacity) => new((byte)Math.Round(color.A * opacity), color.R, color.G, color.B);
}

// IChartCanvas on an Avalonia DrawingContext.
internal sealed class ChartCanvas : IChartCanvas
{
    private readonly DrawingContext _context;
    private readonly FontFamily _font;

    public ChartCanvas(DrawingContext context, FontFamily font)
    {
        _context = context;
        _font = font;
    }

    public void Line(Point from, Point to, Color color, double width, bool dashed = false) =>
        _context.DrawLine(new Pen(new ImmutableSolidColorBrush(color), width) { DashStyle = dashed ? new DashStyle(new double[] { 3, 3 }, 0) : null }, from, to);

    public void Polyline(IReadOnlyList<Point> points, Color color, double width, bool closed = false)
    {
        if (points.Count < 2) return;
        var pen = new Pen(new ImmutableSolidColorBrush(color), width) { LineJoin = PenLineJoin.Round, LineCap = PenLineCap.Round };
        _context.DrawGeometry(null, pen, Path(points, filled: false, closed));
    }

    public void Polygon(IReadOnlyList<Point> points, Color color)
    {
        if (points.Count < 3) return;
        _context.DrawGeometry(new ImmutableSolidColorBrush(color), null, Path(points, filled: true, closed: true));
    }

    public void Text(string text, Point at, double size, Color color, ChartAlign horizontal = ChartAlign.Start, ChartAlign vertical = ChartAlign.Start, bool bold = false)
    {
        if (string.IsNullOrEmpty(text)) return;
        var formatted = Format(text, size, bold, new ImmutableSolidColorBrush(color));
        var x = horizontal == ChartAlign.Start ? at.X : horizontal == ChartAlign.Center ? at.X - formatted.Width / 2 : at.X - formatted.Width;
        var y = vertical == ChartAlign.Start ? at.Y : vertical == ChartAlign.Center ? at.Y - formatted.Height / 2 : at.Y - formatted.Height;
        _context.DrawText(formatted, new Point(x, y));
    }

    public Size Measure(string text, double size, bool bold = false)
    {
        if (string.IsNullOrEmpty(text)) return default;
        var formatted = Format(text, size, bold, null);
        return new Size(Math.Ceiling(formatted.Width), Math.Ceiling(formatted.Height));
    }

    private FormattedText Format(string text, double size, bool bold, IBrush? brush) =>
        new(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            new Typeface(_font, FontStyle.Normal, bold ? FontWeight.Medium : FontWeight.Normal), size, brush);

    private static StreamGeometry Path(IReadOnlyList<Point> points, bool filled, bool closed)
    {
        var geometry = new StreamGeometry();
        using var path = geometry.Open();
        path.BeginFigure(points[0], filled);
        for (var i = 1; i < points.Count; i++) path.LineTo(points[i]);
        path.EndFigure(closed);
        return geometry;
    }
}

// What every chart shares: its series and labels, the legend, the tooltip under the pointer and
// the grow-in animation. Give a chart a height.
public abstract class ChartBase : Control
{
    // The category names along the axis, or the slice names of a pie.
    public static readonly StyledProperty<ChartLabels?> LabelsProperty =
        AvaloniaProperty.Register<ChartBase, ChartLabels?>(nameof(Labels));

    public static readonly StyledProperty<bool> ShowLegendProperty =
        AvaloniaProperty.Register<ChartBase, bool>(nameof(ShowLegend), true);

    // Shows the values under the pointer (or the finger).
    public static readonly StyledProperty<bool> ShowTooltipProperty =
        AvaloniaProperty.Register<ChartBase, bool>(nameof(ShowTooltip), true);

    // Grows the chart in when it first appears.
    public static readonly StyledProperty<bool> IsAnimatedProperty =
        AvaloniaProperty.Register<ChartBase, bool>(nameof(IsAnimated), true);

    // A .NET number format for axis and tooltip values.
    public static readonly StyledProperty<string?> ValueFormatProperty =
        AvaloniaProperty.Register<ChartBase, string?>(nameof(ValueFormat), ""#,0.##"");

    public static readonly StyledProperty<string?> EmptyTextProperty =
        AvaloniaProperty.Register<ChartBase, string?>(nameof(EmptyText), ""No data"");

    // What the chart shows, for screen readers.
    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<ChartBase, string?>(nameof(Description), """");

    private DispatcherTimer? _timer;
    private Point _pointer;
    private int _hover = -1;
    private double _progress = 1;

    static ChartBase()
    {
        AffectsRender<ChartBase>(LabelsProperty, ShowLegendProperty, ShowTooltipProperty, IsAnimatedProperty, ValueFormatProperty, EmptyTextProperty, DescriptionProperty);
        DescriptionProperty.Changed.AddClassHandler<ChartBase>((chart, e) => AutomationProperties.SetName(chart, (string?)e.NewValue ?? string.Empty));
    }

    public ChartLabels? Labels
    {
        get => GetValue(LabelsProperty);
        set => SetValue(LabelsProperty, value);
    }

    public bool ShowLegend
    {
        get => GetValue(ShowLegendProperty);
        set => SetValue(ShowLegendProperty, value);
    }

    public bool ShowTooltip
    {
        get => GetValue(ShowTooltipProperty);
        set => SetValue(ShowTooltipProperty, value);
    }

    public bool IsAnimated
    {
        get => GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }

    public string? ValueFormat
    {
        get => GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }

    public string? EmptyText
    {
        get => GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    [Content]
    public AvaloniaList<ChartSeries> Series { get; } = new();

    protected ChartBase()
    {
        MinHeight = 160;
        Series.CollectionChanged += (_, _) =>
        {
            foreach (var series in Series) series.Changed = Refresh;
            Refresh();
        };
    }

    // Redraws the chart. Call it after changing the numbers inside a series' Values.
    public void Refresh() => InvalidateVisual();

    // Grows the chart in again.
    public void PlayAnimation()
    {
        _timer?.Stop();
        _timer = null;
        if (!IsAnimated)
        {
            _progress = 1;
            Refresh();
            return;
        }
        var started = DateTime.UtcNow;
        _progress = 0;
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, (_, _) =>
        {
            var t = Math.Min(1, (DateTime.UtcNow - started).TotalMilliseconds / 600);
            // Ease out, like MAUI's CubicOut.
            _progress = 1 - Math.Pow(1 - t, 3);
            Refresh();
            if (t < 1) return;
            _timer?.Stop();
            _timer = null;
        });
        _timer.Start();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        // A control only takes the pointer where it has drawn something.
        context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));
        Paint(new ChartCanvas(context, TextElement.GetFontFamily(this)), Bounds.Size);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ShellTheme.ThemeChanged += OnThemeChanged;
        PlayAnimation();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ShellTheme.ThemeChanged -= OnThemeChanged;
        _timer?.Stop();
        _timer = null;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        Hover(e.GetPosition(this));
    }

    // A touch has no hover: a press shows the tooltip instead.
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Hover(e.GetPosition(this));
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        Hover(null);
    }

    // The theme's chart colors in order; past the fifth they repeat, fainter.
    protected static Color Palette(int index)
    {
        var color = ShellTheme.Get(ShellToken.Chart1 + index % 5);
        return index < 5 ? color : ChartColor.Fade(color, 0.6);
    }

    protected Color SeriesColor(int index) => Series[index].Color ?? Palette(index);

    protected int CategoryCount => Math.Max(Labels?.Count ?? 0, Series.Count == 0 ? 0 : Series.Max(s => s.Values.Count));

    protected string LabelAt(int index) =>
        Labels != null && index < Labels.Count ? Labels[index] : (index + 1).ToString(CultureInfo.CurrentCulture);

    protected static double ValueAt(ChartSeries series, int index) => index < series.Values.Count ? series.Values[index] : 0;

    protected string Format(double value) =>
        value.ToString(string.IsNullOrEmpty(ValueFormat) ? ""#,0.##"" : ValueFormat, CultureInfo.CurrentCulture);

    // 0 to 1 while the chart animates in, then 1.
    protected double Progress => IsAnimated ? _progress : 1;

    // The category, slice or ring under the pointer, or -1.
    protected int HoveredIndex => ShowTooltip ? _hover : -1;

    protected virtual bool HasData => Series.Any(s => s.Values.Count > 0);

    // Draws the chart itself into `area`; the legend and tooltip are added around it.
    protected abstract void DrawChart(IChartCanvas canvas, Rect area);

    protected virtual int HitTest(Point point) => -1;

    protected virtual List<(Color Color, string Name)> LegendEntries() =>
        Series.Select((s, i) => (SeriesColor(i), s.Name)).Where(entry => !string.IsNullOrEmpty(entry.Name)).ToList();

    protected virtual (string Title, List<(Color Color, string Name, string Value)> Rows) Tooltip(int index) =>
        (LabelAt(index), Series.Select((s, i) => (SeriesColor(i), s.Name, Format(ValueAt(s, index)))).ToList());

    private void OnThemeChanged(object? sender, EventArgs e) => Refresh();

    private void Hover(Point? point)
    {
        var index = point is { } at ? HitTest(at) : -1;
        if (index < 0 && _hover < 0) return;
        _pointer = point ?? _pointer;
        _hover = index;
        Refresh();
    }

    private void Paint(IChartCanvas canvas, Size size)
    {
        if (size.Width < 8 || size.Height < 8) return;
        if (!HasData)
        {
            canvas.Text(EmptyText ?? string.Empty, new Point(size.Width / 2, size.Height / 2), 14,
                ShellTheme.Get(ShellToken.MutedForeground), ChartAlign.Center, ChartAlign.Center);
            return;
        }

        var legend = ShowLegend ? LegendEntries() : new List<(Color Color, string Name)>();
        var legendHeight = legend.Count > 0 ? DrawLegend(canvas, legend, size, draw: false) : 0;
        DrawChart(canvas, new Rect(0, 0, size.Width, size.Height - legendHeight));
        if (legend.Count > 0) DrawLegend(canvas, legend, size, draw: true);
        if (HoveredIndex >= 0) DrawTooltip(canvas, size);
    }

    // justify-center gap-4 text-xs: a 8px rounded square and the name per entry, wrapped into
    // as many centered rows as it takes. Returns the height the legend needs.
    private static double DrawLegend(IChartCanvas canvas, List<(Color Color, string Name)> entries, Size size, bool draw)
    {
        const double fontSize = 12, mark = 8, markGap = 6, gap = 16, rowHeight = 20, top = 12;
        var widths = entries.Select(e => mark + markGap + canvas.Measure(e.Name, fontSize).Width).ToList();
        var rows = new List<List<int>> { new() };
        var used = 0.0;
        for (var i = 0; i < entries.Count; i++)
        {
            var next = used == 0 ? widths[i] : used + gap + widths[i];
            if (used > 0 && next > size.Width)
            {
                rows.Add(new List<int>());
                next = widths[i];
            }
            rows[^1].Add(i);
            used = next;
        }

        var height = top + rows.Count * rowHeight;
        if (!draw) return height;
        var text = ShellTheme.Get(ShellToken.Foreground);
        var y = size.Height - rows.Count * rowHeight;
        foreach (var row in rows)
        {
            var x = (size.Width - row.Sum(i => widths[i]) - gap * (row.Count - 1)) / 2;
            foreach (var i in row)
            {
                var middle = y + rowHeight / 2;
                canvas.Polygon(ChartMath.RoundRect(new Rect(x, middle - mark / 2, mark, mark), 2, 2, 2, 2), entries[i].Color);
                canvas.Text(entries[i].Name, new Point(x + mark + markGap, middle), fontSize, text, ChartAlign.Start, ChartAlign.Center);
                x += widths[i] + gap;
            }
            y += rowHeight;
        }
        return height;
    }

    // rounded-lg border bg-background px-2.5 py-1.5 text-xs: a title, then a colored square,
    // the name and the value per row. It follows the pointer and stays inside the chart.
    private void DrawTooltip(IChartCanvas canvas, Size size)
    {
        var (title, rows) = Tooltip(_hover);
        if (rows.Count == 0) return;
        const double fontSize = 12, pad = 10, rowHeight = 18, mark = 10;
        var hasTitle = !string.IsNullOrEmpty(title);
        var names = rows.Max(r => canvas.Measure(r.Name, fontSize).Width);
        var values = rows.Max(r => canvas.Measure(r.Value, fontSize, bold: true).Width);
        var width = Math.Max(128, pad * 2 + mark + 6 + names + 16 + values);
        if (hasTitle) width = Math.Max(width, pad * 2 + canvas.Measure(title, fontSize, bold: true).Width);
        var height = 16 + rows.Count * rowHeight + (hasTitle ? rowHeight + 2 : 0);

        var x = _pointer.X + 14;
        if (x + width > size.Width) x = _pointer.X - 14 - width;
        x = Math.Clamp(x, 0, Math.Max(0, size.Width - width));
        var y = _pointer.Y + 14;
        if (y + height > size.Height) y = _pointer.Y - 14 - height;
        y = Math.Clamp(y, 0, Math.Max(0, size.Height - height));

        canvas.Polygon(ChartMath.RoundRect(new Rect(x, y, width, height), 8, 8, 8, 8), ShellTheme.Get(ShellToken.Border));
        canvas.Polygon(ChartMath.RoundRect(new Rect(x + 1, y + 1, width - 2, height - 2), 7, 7, 7, 7), ShellTheme.Get(ShellToken.Background));
        var foreground = ShellTheme.Get(ShellToken.Foreground);
        var muted = ShellTheme.Get(ShellToken.MutedForeground);
        var top = y + 8;
        if (hasTitle)
        {
            canvas.Text(title, new Point(x + pad, top + rowHeight / 2), fontSize, foreground, ChartAlign.Start, ChartAlign.Center, bold: true);
            top += rowHeight + 2;
        }
        foreach (var row in rows)
        {
            var middle = top + rowHeight / 2;
            canvas.Polygon(ChartMath.RoundRect(new Rect(x + pad, middle - mark / 2, mark, mark), 2, 2, 2, 2), row.Color);
            canvas.Text(row.Name, new Point(x + pad + mark + 6, middle), fontSize, muted, ChartAlign.Start, ChartAlign.Center);
            canvas.Text(row.Value, new Point(x + width - pad, middle), fontSize, foreground, ChartAlign.End, ChartAlign.Center, bold: true);
            top += rowHeight;
        }
    }

    // A large value over a small label, for the middle of a donut or radial chart.
    protected static void DrawCenterText(IChartCanvas canvas, Point center, string? value, string? label)
    {
        var hasValue = !string.IsNullOrEmpty(value);
        var hasLabel = !string.IsNullOrEmpty(label);
        if (hasValue)
            canvas.Text(value!, new Point(center.X, center.Y - (hasLabel ? 8 : 0)), 24, ShellTheme.Get(ShellToken.Foreground), ChartAlign.Center, ChartAlign.Center, bold: true);
        if (hasLabel)
            canvas.Text(label!, new Point(center.X, center.Y + (hasValue ? 14 : 0)), 12, ShellTheme.Get(ShellToken.MutedForeground), ChartAlign.Center, ChartAlign.Center);
    }
}

// What bar, line and area charts share: a value axis, a category axis and a grid.
public abstract class CartesianChart : ChartBase
{
    public static readonly StyledProperty<bool> ShowGridProperty =
        AvaloniaProperty.Register<CartesianChart, bool>(nameof(ShowGrid), true);

    // The labels under the plot.
    public static readonly StyledProperty<bool> ShowXAxisProperty =
        AvaloniaProperty.Register<CartesianChart, bool>(nameof(ShowXAxis), true);

    // The labels beside the plot.
    public static readonly StyledProperty<bool> ShowYAxisProperty =
        AvaloniaProperty.Register<CartesianChart, bool>(nameof(ShowYAxis), true);

    // The ends of the value axis; by default they fit the data and include zero.
    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<CartesianChart, double>(nameof(Minimum), double.NaN);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<CartesianChart, double>(nameof(Maximum), double.NaN);

    static CartesianChart()
    {
        AffectsRender<CartesianChart>(ShowGridProperty, ShowXAxisProperty, ShowYAxisProperty, MinimumProperty, MaximumProperty);
    }

    public bool ShowGrid
    {
        get => GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public bool ShowXAxis
    {
        get => GetValue(ShowXAxisProperty);
        set => SetValue(ShowXAxisProperty, value);
    }

    public bool ShowYAxis
    {
        get => GetValue(ShowYAxisProperty);
        set => SetValue(ShowYAxisProperty, value);
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    // The frame of the last draw, for hit testing.
    protected ChartFrame? LastFrame { get; private set; }

    protected override int HitTest(Point point) => LastFrame?.IndexAt(point) ?? -1;

    // Draws the grid and the axis labels into `area` and returns how values and categories map
    // onto the plot. The value axis always includes zero unless Minimum or Maximum say otherwise.
    protected ChartFrame DrawAxes(IChartCanvas canvas, Rect area, double dataMin, double dataMax, bool bands, bool horizontal = false)
    {
        const double fontSize = 12;
        var muted = ShellTheme.Get(ShellToken.MutedForeground);
        var line = ShellTheme.Get(ShellToken.Border);
        var count = CategoryCount;
        var (low, high, step) = ChartMath.NiceScale(
            double.IsNaN(Minimum) ? Math.Min(0, dataMin) : Minimum,
            double.IsNaN(Maximum) ? Math.Max(0, dataMax) : Maximum, 5);
        if (!double.IsNaN(Minimum)) low = Minimum;
        if (!double.IsNaN(Maximum)) high = Maximum;
        var ticks = new List<double>();
        for (var tick = Math.Ceiling(low / step) * step; tick <= high + step / 1000; tick += step)
            ticks.Add(Math.Abs(tick) < step / 1000 ? 0 : tick);

        var showValues = horizontal ? ShowXAxis : ShowYAxis;
        var showCategories = horizontal ? ShowYAxis : ShowXAxis;
        var labels = Enumerable.Range(0, count).Select(LabelAt).ToList();
        double left = 0;
        if (horizontal && showCategories) left = labels.Max(label => canvas.Measure(label, fontSize).Width) + 8;
        if (!horizontal && showValues) left = ticks.Max(tick => canvas.Measure(Format(tick), fontSize).Width) + 8;
        // An edge-to-edge axis needs room for half of its first and last label.
        if (!bands && !horizontal) left = Math.Max(left, 12);
        var right = horizontal ? 16 : bands ? 0 : 12;
        var bottom = (horizontal ? showValues : showCategories) ? 24 : 0;
        var plot = new Rect(area.Left + left, area.Top + 8, Math.Max(1, area.Width - left - right), Math.Max(1, area.Height - 8 - bottom));
        var frame = new ChartFrame(plot, low, high, count, bands, horizontal);

        foreach (var tick in ticks)
        {
            var at = frame.Value(tick);
            // Half a pixel off the grid keeps a 1px line sharp.
            var sharp = Math.Round(at) + 0.5;
            if (ShowGrid)
            {
                if (horizontal) canvas.Line(new Point(sharp, plot.Top), new Point(sharp, plot.Bottom), line, 1);
                else canvas.Line(new Point(plot.Left, sharp), new Point(plot.Right, sharp), line, 1);
            }
            if (!showValues) continue;
            if (horizontal) canvas.Text(Format(tick), new Point(at, plot.Bottom + 8), fontSize, muted, ChartAlign.Center);
            else canvas.Text(Format(tick), new Point(plot.Left - 8, at), fontSize, muted, ChartAlign.End, ChartAlign.Center);
        }

        if (showCategories && horizontal)
        {
            for (var i = 0; i < count; i++)
                canvas.Text(labels[i], new Point(plot.Left - 8, frame.Category(i)), fontSize, muted, ChartAlign.End, ChartAlign.Center);
        }
        else if (showCategories)
        {
            // Labels that would run into each other are thinned out.
            var widest = labels.Max(label => canvas.Measure(label, fontSize).Width);
            var every = Math.Max(1, (int)Math.Ceiling((widest + 8) / Math.Max(1, frame.Band)));
            for (var i = 0; i < count; i += every)
                canvas.Text(labels[i], new Point(frame.Category(i), plot.Bottom + 8), fontSize, muted, ChartAlign.Center);
        }

        LastFrame = frame;
        return frame;
    }
}
"
    };
}
