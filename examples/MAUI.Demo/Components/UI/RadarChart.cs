using System.Globalization;

namespace MAUI.Demo.Components.UI;

// Radar chart — one spoke per category around a ringed grid, and a filled shape per series.
// It needs at least three categories. The hovered spoke shows every series' value there.
//   <ui:RadarChart Labels="Speed, Power, Range, Comfort, Price, Safety" HeightRequest="280">
//       <ui:ChartSeries Name="Model A" Values="80, 65, 90, 70, 55, 85" />
//       <ui:ChartSeries Name="Model B" Values="60, 85, 70, 80, 75, 65" />
//   </ui:RadarChart>
public partial class RadarChart : ChartBase
{
    // The value at the outer ring; by default it fits the data.
    public static readonly BindableProperty MaximumProperty =
        BindableProperty.Create(nameof(Maximum), typeof(double), typeof(RadarChart), double.NaN,
            propertyChanged: (b, o, n) => ((RadarChart)b).Refresh());

    public static readonly BindableProperty FillOpacityProperty =
        BindableProperty.Create(nameof(FillOpacity), typeof(double), typeof(RadarChart), 0.5,
            propertyChanged: (b, o, n) => ((RadarChart)b).Refresh());

    // A dot on every point.
    public static readonly BindableProperty ShowDotsProperty =
        BindableProperty.Create(nameof(ShowDots), typeof(bool), typeof(RadarChart), false,
            propertyChanged: (b, o, n) => ((RadarChart)b).Refresh());

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double FillOpacity
    {
        get => (double)GetValue(FillOpacityProperty);
        set => SetValue(FillOpacityProperty, value);
    }

    public bool ShowDots
    {
        get => (bool)GetValue(ShowDotsProperty);
        set => SetValue(ShowDotsProperty, value);
    }

    private Point _center;
    private double _radius;

    // A radar needs at least three axes.
    protected override bool HasData => base.HasData && CategoryCount >= 3;

    protected override void DrawChart(IChartCanvas canvas, Rect area)
    {
        var count = CategoryCount;
        var line = ShellTheme.Get(ShellToken.Border);
        var muted = ShellTheme.Get(ShellToken.MutedForeground);
        _center = new Point(area.Left + area.Width / 2, area.Top + area.Height / 2);
        _radius = Math.Max(4, Math.Min(area.Width, area.Height) / 2 - 32);
        var max = double.IsNaN(Maximum)
            ? ChartMath.NiceScale(0, Series.SelectMany(s => s.Values).DefaultIfEmpty(1).Max(), 4).Max
            : Maximum;
        if (max <= 0) max = 1;
        double Angle(int i) => -90 + 360.0 * i / count;

        // Four rings and a spoke per axis.
        for (var level = 1; level <= 4; level++)
            canvas.Polyline(Enumerable.Range(0, count).Select(i => ChartMath.Polar(_center, _radius * level / 4, Angle(i))).ToList(), line, 1, closed: true);
        for (var i = 0; i < count; i++)
        {
            canvas.Line(_center, ChartMath.Polar(_center, _radius, Angle(i)), line, 1);
            var cos = Math.Cos(Angle(i) * Math.PI / 180);
            var sin = Math.Sin(Angle(i) * Math.PI / 180);
            // Each label hangs off its spoke, away from the middle.
            canvas.Text(LabelAt(i), ChartMath.Polar(_center, _radius + 10, Angle(i)), 12, muted,
                cos > 0.3 ? ChartAlign.Start : cos < -0.3 ? ChartAlign.End : ChartAlign.Center,
                sin > 0.3 ? ChartAlign.Start : sin < -0.3 ? ChartAlign.End : ChartAlign.Center);
        }

        var hovered = HoveredIndex;
        for (var s = 0; s < Series.Count; s++)
        {
            var color = SeriesColor(s);
            var points = Enumerable.Range(0, count)
                .Select(i => ChartMath.Polar(_center, _radius * Math.Clamp(ValueAt(Series[s], i) / max, 0, 1) * Progress, Angle(i)))
                .ToList();
            canvas.Polygon(points, ChartColor.Fade(color, FillOpacity));
            canvas.Polyline(points, color, 2, closed: true);
            if (ShowDots)
                foreach (var point in points) canvas.Polygon(ChartMath.Circle(point, 3.5), color);
            if (hovered < 0) continue;
            canvas.Polygon(ChartMath.Circle(points[hovered], 6), ShellTheme.Get(ShellToken.Background));
            canvas.Polygon(ChartMath.Circle(points[hovered], 4.5), color);
        }
    }

    protected override int HitTest(Point point)
    {
        var count = CategoryCount;
        var dx = point.X - _center.X;
        var dy = point.Y - _center.Y;
        if (count == 0 || Math.Sqrt(dx * dx + dy * dy) > _radius + 24) return -1;
        // The nearest spoke, clockwise from twelve o'clock.
        var angle = (Math.Atan2(dy, dx) * 180 / Math.PI + 90 + 360) % 360;
        return (int)Math.Round(angle / (360.0 / count)) % count;
    }
}
