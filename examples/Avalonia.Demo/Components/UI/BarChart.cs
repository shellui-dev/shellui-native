using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Bar chart — one bar per series for each category, side by side or stacked, upright or
// lying down. Bars are rounded at their outer end, and the hovered category gets a muted band
// and a tooltip.
//   <ui:BarChart Labels="Jan, Feb, Mar, Apr, May, Jun" Height="260">
//       <ui:ChartSeries Name="Desktop" Values="186, 305, 237, 73, 209, 214" />
//       <ui:ChartSeries Name="Mobile" Values="80, 200, 120, 190, 130, 140" />
//   </ui:BarChart>
public class BarChart : CartesianChart
{
    // Stacks the series on top of each other instead of side by side.
    public static readonly StyledProperty<bool> IsStackedProperty =
        AvaloniaProperty.Register<BarChart, bool>(nameof(IsStacked));

    // Lays the bars down: categories run down the side, values along the bottom.
    public static readonly StyledProperty<bool> IsHorizontalProperty =
        AvaloniaProperty.Register<BarChart, bool>(nameof(IsHorizontal));

    public static readonly StyledProperty<double> CornerRadiusProperty =
        AvaloniaProperty.Register<BarChart, double>(nameof(CornerRadius), 4d);

    // The thickest a bar gets, however much room there is.
    public static readonly StyledProperty<double> MaxBarSizeProperty =
        AvaloniaProperty.Register<BarChart, double>(nameof(MaxBarSize), 56d);

    static BarChart()
    {
        AffectsRender<BarChart>(IsStackedProperty, IsHorizontalProperty, CornerRadiusProperty, MaxBarSizeProperty);
    }

    public bool IsStacked
    {
        get => GetValue(IsStackedProperty);
        set => SetValue(IsStackedProperty, value);
    }

    public bool IsHorizontal
    {
        get => GetValue(IsHorizontalProperty);
        set => SetValue(IsHorizontalProperty, value);
    }

    public double CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public double MaxBarSize
    {
        get => GetValue(MaxBarSizeProperty);
        set => SetValue(MaxBarSizeProperty, value);
    }

    protected override void DrawChart(IChartCanvas canvas, Rect area)
    {
        var count = CategoryCount;
        var series = Series;
        double min = 0, max = 0;
        for (var i = 0; i < count; i++)
        {
            double positive = 0, negative = 0;
            foreach (var entry in series)
            {
                var value = ValueAt(entry, i);
                if (!IsStacked)
                {
                    positive = Math.Max(positive, value);
                    negative = Math.Min(negative, value);
                }
                else if (value >= 0) positive += value;
                else negative += value;
            }
            max = Math.Max(max, positive);
            min = Math.Min(min, negative);
        }

        var frame = DrawAxes(canvas, area, min, max, bands: true, horizontal: IsHorizontal);
        var plot = frame.Plot;

        // The cursor: a muted band behind the hovered category.
        if (HoveredIndex >= 0)
        {
            var middle = frame.Category(HoveredIndex);
            var band = IsHorizontal
                ? new Rect(plot.Left, middle - frame.Band / 2, plot.Width, frame.Band)
                : new Rect(middle - frame.Band / 2, plot.Top, frame.Band, plot.Height);
            canvas.Polygon(ChartMath.RoundRect(band, 4, 4, 4, 4), ChartColor.Fade(ShellTheme.Get(ShellToken.Muted), 0.6));
        }

        const double gap = 4;
        var slots = IsStacked ? 1 : Math.Max(1, series.Count);
        var thickness = Math.Max(1, Math.Min(MaxBarSize, (frame.Band * 0.8 - gap * (slots - 1)) / slots));
        var group = thickness * slots + gap * (slots - 1);
        for (var i = 0; i < count; i++)
        {
            var start = frame.Category(i) - group / 2;
            double positive = 0, negative = 0;
            for (var s = 0; s < series.Count; s++)
            {
                var full = ValueAt(series[s], i);
                var value = full * Progress;
                if (full == 0) continue;
                var from = !IsStacked ? 0 : full >= 0 ? positive : negative;
                var to = from + value;
                if (IsStacked && full >= 0) positive = to;
                else if (IsStacked) negative = to;

                // Only the outer end of a bar, or of a whole stack, is rounded.
                var outer = !IsStacked || !series.Skip(s + 1).Any(next => ValueAt(next, i) != 0 && ValueAt(next, i) >= 0 == full >= 0);
                var radius = outer ? CornerRadius : 0;
                var offset = start + (IsStacked ? 0 : s * (thickness + gap));
                var a = frame.Value(from);
                var b = frame.Value(to);
                if (IsHorizontal)
                {
                    var rect = new Rect(Math.Min(a, b), offset, Math.Abs(b - a), thickness);
                    canvas.Polygon(full >= 0 ? ChartMath.RoundRect(rect, 0, radius, radius, 0) : ChartMath.RoundRect(rect, radius, 0, 0, radius), SeriesColor(s));
                }
                else
                {
                    var rect = new Rect(offset, Math.Min(a, b), thickness, Math.Abs(b - a));
                    canvas.Polygon(full >= 0 ? ChartMath.RoundRect(rect, radius, radius, 0, 0) : ChartMath.RoundRect(rect, 0, 0, radius, radius), SeriesColor(s));
                }
            }
        }
    }
}
