using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class AreaChartTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "area-chart",
        DisplayName = "Area Chart",
        Description = "Filled lines, overlapping or stacked",
        Category = ComponentCategory.DataDisplay,
        FilePath = "AreaChart.cs",
        Dependencies = new List<string> { "shell", "chart" },
        Tags = new List<string> { "chart", "area", "graph", "stacked" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using System.Globalization;

namespace YourProjectNamespace.Components.UI;

// Area chart — a line per series with the space under it filled at 40%. Stacked, each
// series sits on the ones before it.
//   <ui:AreaChart Labels=""Jan, Feb, Mar, Apr, May, Jun"" IsStacked=""True"" HeightRequest=""260"">
//       <ui:ChartSeries Name=""Desktop"" Values=""186, 305, 237, 73, 209, 214"" />
//       <ui:ChartSeries Name=""Mobile"" Values=""80, 200, 120, 190, 130, 140"" />
//   </ui:AreaChart>
public partial class AreaChart : CartesianChart
{
    // Curves the line through its points; false joins them with straight segments.
    public static readonly BindableProperty IsSmoothProperty =
        BindableProperty.Create(nameof(IsSmooth), typeof(bool), typeof(AreaChart), true,
            propertyChanged: (b, o, n) => ((AreaChart)b).Refresh());

    // Stacks the series instead of overlapping them.
    public static readonly BindableProperty IsStackedProperty =
        BindableProperty.Create(nameof(IsStacked), typeof(bool), typeof(AreaChart), false,
            propertyChanged: (b, o, n) => ((AreaChart)b).Refresh());

    public static readonly BindableProperty FillOpacityProperty =
        BindableProperty.Create(nameof(FillOpacity), typeof(double), typeof(AreaChart), 0.4,
            propertyChanged: (b, o, n) => ((AreaChart)b).Refresh());

    public static readonly BindableProperty StrokeWidthProperty =
        BindableProperty.Create(nameof(StrokeWidth), typeof(double), typeof(AreaChart), 2d,
            propertyChanged: (b, o, n) => ((AreaChart)b).Refresh());

    public bool IsSmooth
    {
        get => (bool)GetValue(IsSmoothProperty);
        set => SetValue(IsSmoothProperty, value);
    }

    public bool IsStacked
    {
        get => (bool)GetValue(IsStackedProperty);
        set => SetValue(IsStackedProperty, value);
    }

    public double FillOpacity
    {
        get => (double)GetValue(FillOpacityProperty);
        set => SetValue(FillOpacityProperty, value);
    }

    public double StrokeWidth
    {
        get => (double)GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    protected override void DrawChart(IChartCanvas canvas, Rect area)
    {
        var count = CategoryCount;
        double min = 0, max = 0;
        for (var i = 0; i < count; i++)
        {
            var values = Series.Select(s => ValueAt(s, i)).ToList();
            max = Math.Max(max, IsStacked ? values.Where(v => v > 0).Sum() : values.Max());
            min = Math.Min(min, IsStacked ? values.Where(v => v < 0).Sum() : values.Min());
        }

        var frame = DrawAxes(canvas, area, min, max, bands: false);
        var plot = frame.Plot;
        var hovered = HoveredIndex;
        if (hovered >= 0)
        {
            // The cursor: a hairline through the hovered category.
            var x = Math.Round(frame.Category(hovered)) + 0.5;
            canvas.Line(new Point(x, plot.Top), new Point(x, plot.Bottom), ShellTheme.Get(ShellToken.Border), 1);
        }

        // In a stack each series sits on the running total of the ones before it.
        var stack = new double[count];
        var dots = new List<(Point At, Color Color)>();
        for (var s = 0; s < Series.Count; s++)
        {
            var color = SeriesColor(s);
            var top = new List<Point>();
            var bottom = new List<Point>();
            for (var i = 0; i < count; i++)
            {
                var from = IsStacked ? stack[i] : 0;
                var to = from + ValueAt(Series[s], i) * Progress;
                if (IsStacked) stack[i] = to;
                top.Add(new Point(frame.Category(i), frame.Value(to)));
                bottom.Add(new Point(frame.Category(i), frame.Value(from)));
            }

            var upper = IsSmooth ? ChartMath.Smooth(top) : top;
            var lower = IsSmooth ? ChartMath.Smooth(bottom) : bottom;
            var shape = new List<Point>(upper);
            shape.AddRange(Enumerable.Reverse(lower));
            canvas.Polygon(shape, ChartColor.Fade(color, FillOpacity));
            if (upper.Count > 1) canvas.Polyline(upper, color, StrokeWidth);
            if (hovered >= 0 && hovered < top.Count) dots.Add((top[hovered], color));
        }

        foreach (var dot in dots)
        {
            canvas.Polygon(ChartMath.Circle(dot.At, StrokeWidth + 4), ShellTheme.Get(ShellToken.Background));
            canvas.Polygon(ChartMath.Circle(dot.At, StrokeWidth + 2.5), dot.Color);
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

// Area chart — a line per series with the space under it filled at 40%. Stacked, each
// series sits on the ones before it.
//   <ui:AreaChart Labels=""Jan, Feb, Mar, Apr, May, Jun"" IsStacked=""True"" Height=""260"">
//       <ui:ChartSeries Name=""Desktop"" Values=""186, 305, 237, 73, 209, 214"" />
//       <ui:ChartSeries Name=""Mobile"" Values=""80, 200, 120, 190, 130, 140"" />
//   </ui:AreaChart>
public class AreaChart : CartesianChart
{
    // Curves the line through its points; false joins them with straight segments.
    public static readonly StyledProperty<bool> IsSmoothProperty =
        AvaloniaProperty.Register<AreaChart, bool>(nameof(IsSmooth), true);

    // Stacks the series instead of overlapping them.
    public static readonly StyledProperty<bool> IsStackedProperty =
        AvaloniaProperty.Register<AreaChart, bool>(nameof(IsStacked));

    public static readonly StyledProperty<double> FillOpacityProperty =
        AvaloniaProperty.Register<AreaChart, double>(nameof(FillOpacity), 0.4);

    public static readonly StyledProperty<double> StrokeWidthProperty =
        AvaloniaProperty.Register<AreaChart, double>(nameof(StrokeWidth), 2d);

    static AreaChart()
    {
        AffectsRender<AreaChart>(IsSmoothProperty, IsStackedProperty, FillOpacityProperty, StrokeWidthProperty);
    }

    public bool IsSmooth
    {
        get => GetValue(IsSmoothProperty);
        set => SetValue(IsSmoothProperty, value);
    }

    public bool IsStacked
    {
        get => GetValue(IsStackedProperty);
        set => SetValue(IsStackedProperty, value);
    }

    public double FillOpacity
    {
        get => GetValue(FillOpacityProperty);
        set => SetValue(FillOpacityProperty, value);
    }

    public double StrokeWidth
    {
        get => GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    protected override void DrawChart(IChartCanvas canvas, Rect area)
    {
        var count = CategoryCount;
        double min = 0, max = 0;
        for (var i = 0; i < count; i++)
        {
            var values = Series.Select(s => ValueAt(s, i)).ToList();
            max = Math.Max(max, IsStacked ? values.Where(v => v > 0).Sum() : values.Max());
            min = Math.Min(min, IsStacked ? values.Where(v => v < 0).Sum() : values.Min());
        }

        var frame = DrawAxes(canvas, area, min, max, bands: false);
        var plot = frame.Plot;
        var hovered = HoveredIndex;
        if (hovered >= 0)
        {
            // The cursor: a hairline through the hovered category.
            var x = Math.Round(frame.Category(hovered)) + 0.5;
            canvas.Line(new Point(x, plot.Top), new Point(x, plot.Bottom), ShellTheme.Get(ShellToken.Border), 1);
        }

        // In a stack each series sits on the running total of the ones before it.
        var stack = new double[count];
        var dots = new List<(Point At, Color Color)>();
        for (var s = 0; s < Series.Count; s++)
        {
            var color = SeriesColor(s);
            var top = new List<Point>();
            var bottom = new List<Point>();
            for (var i = 0; i < count; i++)
            {
                var from = IsStacked ? stack[i] : 0;
                var to = from + ValueAt(Series[s], i) * Progress;
                if (IsStacked) stack[i] = to;
                top.Add(new Point(frame.Category(i), frame.Value(to)));
                bottom.Add(new Point(frame.Category(i), frame.Value(from)));
            }

            var upper = IsSmooth ? ChartMath.Smooth(top) : top;
            var lower = IsSmooth ? ChartMath.Smooth(bottom) : bottom;
            var shape = new List<Point>(upper);
            shape.AddRange(Enumerable.Reverse(lower));
            canvas.Polygon(shape, ChartColor.Fade(color, FillOpacity));
            if (upper.Count > 1) canvas.Polyline(upper, color, StrokeWidth);
            if (hovered >= 0 && hovered < top.Count) dots.Add((top[hovered], color));
        }

        foreach (var dot in dots)
        {
            canvas.Polygon(ChartMath.Circle(dot.At, StrokeWidth + 4), ShellTheme.Get(ShellToken.Background));
            canvas.Polygon(ChartMath.Circle(dot.At, StrokeWidth + 2.5), dot.Color);
        }
    }
}
"
    };
}
