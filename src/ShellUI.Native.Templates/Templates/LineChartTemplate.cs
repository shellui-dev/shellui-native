using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class LineChartTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "line-chart",
        DisplayName = "Line Chart",
        Description = "Smooth or straight lines over categories",
        Category = ComponentCategory.DataDisplay,
        FilePath = "LineChart.cs",
        Dependencies = new List<string> { "shell", "chart" },
        Tags = new List<string> { "chart", "line", "graph", "trend" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using System.Globalization;

namespace YourProjectNamespace.Components.UI;

// Line chart — one line per series from edge to edge of the plot, smooth by default. The
// hovered category gets a hairline, a dot on each line and a tooltip.
//   <ui:LineChart Labels=""Jan, Feb, Mar, Apr, May, Jun"" HeightRequest=""260"">
//       <ui:ChartSeries Name=""Desktop"" Values=""186, 305, 237, 73, 209, 214"" />
//       <ui:ChartSeries Name=""Mobile"" Values=""80, 200, 120, 190, 130, 140"" />
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
",
        [NativePlatform.Avalonia] = @"using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Media;

namespace YourProjectNamespace.Components.UI;

// Line chart — one line per series from edge to edge of the plot, smooth by default. The
// hovered category gets a hairline, a dot on each line and a tooltip.
//   <ui:LineChart Labels=""Jan, Feb, Mar, Apr, May, Jun"" Height=""260"">
//       <ui:ChartSeries Name=""Desktop"" Values=""186, 305, 237, 73, 209, 214"" />
//       <ui:ChartSeries Name=""Mobile"" Values=""80, 200, 120, 190, 130, 140"" />
//   </ui:LineChart>
public class LineChart : CartesianChart
{
    // Curves the line through its points; false joins them with straight segments.
    public static readonly StyledProperty<bool> IsSmoothProperty =
        AvaloniaProperty.Register<LineChart, bool>(nameof(IsSmooth), true);

    // A dot on every point.
    public static readonly StyledProperty<bool> ShowDotsProperty =
        AvaloniaProperty.Register<LineChart, bool>(nameof(ShowDots));

    public static readonly StyledProperty<double> StrokeWidthProperty =
        AvaloniaProperty.Register<LineChart, double>(nameof(StrokeWidth), 2d);

    static LineChart()
    {
        AffectsRender<LineChart>(IsSmoothProperty, ShowDotsProperty, StrokeWidthProperty);
    }

    public bool IsSmooth
    {
        get => GetValue(IsSmoothProperty);
        set => SetValue(IsSmoothProperty, value);
    }

    public bool ShowDots
    {
        get => GetValue(ShowDotsProperty);
        set => SetValue(ShowDotsProperty, value);
    }

    public double StrokeWidth
    {
        get => GetValue(StrokeWidthProperty);
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
"
    };
}
