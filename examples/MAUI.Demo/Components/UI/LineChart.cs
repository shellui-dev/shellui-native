using System.Globalization;

namespace MAUI.Demo.Components.UI;

// Line chart — one line per series from edge to edge of the plot, smooth by default. The
// hovered category gets a hairline, a dot on each line and a tooltip.
//   <ui:LineChart Labels="Jan, Feb, Mar, Apr, May, Jun" HeightRequest="260">
//       <ui:ChartSeries Name="Desktop" Values="186, 305, 237, 73, 209, 214" />
//       <ui:ChartSeries Name="Mobile" Values="80, 200, 120, 190, 130, 140" />
//   </ui:LineChart>
public partial class LineChart : CartesianChart
{
    // Curves the line through its points; false joins them with straight segments.
    public static readonly BindableProperty IsSmoothProperty =
        BindableProperty.Create(nameof(IsSmooth), typeof(bool), typeof(LineChart), true,
            propertyChanged: (b, o, n) => ((LineChart)b).Refresh());

    // A dot on every point.
    public static readonly BindableProperty ShowDotsProperty =
        BindableProperty.Create(nameof(ShowDots), typeof(bool), typeof(LineChart), false,
            propertyChanged: (b, o, n) => ((LineChart)b).Refresh());

    public static readonly BindableProperty StrokeWidthProperty =
        BindableProperty.Create(nameof(StrokeWidth), typeof(double), typeof(LineChart), 2d,
            propertyChanged: (b, o, n) => ((LineChart)b).Refresh());

    public bool IsSmooth
    {
        get => (bool)GetValue(IsSmoothProperty);
        set => SetValue(IsSmoothProperty, value);
    }

    public bool ShowDots
    {
        get => (bool)GetValue(ShowDotsProperty);
        set => SetValue(ShowDotsProperty, value);
    }

    public double StrokeWidth
    {
        get => (double)GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    protected override void DrawChart(IChartCanvas canvas, Rect area)
    {
        var all = Series.SelectMany(s => s.Values).DefaultIfEmpty(0).ToList();
        var frame = DrawAxes(canvas, area, all.Min(), all.Max(), bands: false);
        var plot = frame.Plot;
        var hovered = HoveredIndex;
        if (hovered >= 0)
        {
            // The cursor: a hairline through the hovered category.
            var x = Math.Round(frame.Category(hovered)) + 0.5;
            canvas.Line(new Point(x, plot.Top), new Point(x, plot.Bottom), ShellTheme.Get(ShellToken.Border), 1);
        }

        for (var s = 0; s < Series.Count; s++)
        {
            var values = Series[s].Values;
            if (values.Count == 0) continue;
            var color = SeriesColor(s);
            var points = values.Select((value, i) => new Point(frame.Category(i), frame.Value(value * Progress))).ToList();
            if (points.Count > 1) canvas.Polyline(IsSmooth ? ChartMath.Smooth(points) : points, color, StrokeWidth);
            if (ShowDots || points.Count == 1)
                foreach (var point in points) canvas.Polygon(ChartMath.Circle(point, StrokeWidth + 1.5), color);
            if (hovered < 0 || hovered >= points.Count) continue;
            canvas.Polygon(ChartMath.Circle(points[hovered], StrokeWidth + 4), ShellTheme.Get(ShellToken.Background));
            canvas.Polygon(ChartMath.Circle(points[hovered], StrokeWidth + 2.5), color);
        }
    }
}
