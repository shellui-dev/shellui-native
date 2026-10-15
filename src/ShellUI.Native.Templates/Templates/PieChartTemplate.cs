using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class PieChartTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "pie-chart",
        DisplayName = "Pie Chart",
        Description = "Slices of a whole, clockwise from twelve o'clock",
        Category = ComponentCategory.DataDisplay,
        FilePath = "PieChart.cs",
        Dependencies = new List<string> { "shell", "chart" },
        Tags = new List<string> { "chart", "pie", "share" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using System.Globalization;

namespace YourProjectNamespace.Components.UI;

// Pie chart — the first series' values as slices, clockwise from twelve o'clock, named by
// Labels and colored from the theme's chart colors in order. The hovered slice stands out and
// shows its value and share. For a pie with a hole, see DonutChart (donut-chart).
//   <ui:PieChart Labels=""Chrome, Safari, Firefox, Edge, Other"" HeightRequest=""260"">
//       <ui:ChartSeries Values=""275, 200, 187, 173, 90"" />
//   </ui:PieChart>
public partial class PieChart : ChartBase
{
    // The hole, as a share of the radius: 0 is a pie, 0.6 a donut.
    public static readonly BindableProperty InnerRadiusProperty =
        BindableProperty.Create(nameof(InnerRadius), typeof(double), typeof(PieChart), 0d,
            propertyChanged: (b, o, n) => ((PieChart)b).Refresh());

    // Degrees of space between slices.
    public static readonly BindableProperty GapProperty =
        BindableProperty.Create(nameof(Gap), typeof(double), typeof(PieChart), 1d,
            propertyChanged: (b, o, n) => ((PieChart)b).Refresh());

    // Writes each slice's value outside it.
    public static readonly BindableProperty ShowLabelsProperty =
        BindableProperty.Create(nameof(ShowLabels), typeof(bool), typeof(PieChart), false,
            propertyChanged: (b, o, n) => ((PieChart)b).Refresh());

    // Large text in the hole of a donut.
    public static readonly BindableProperty CenterValueProperty =
        BindableProperty.Create(nameof(CenterValue), typeof(string), typeof(PieChart), """",
            propertyChanged: (b, o, n) => ((PieChart)b).Refresh());

    // Small muted text under CenterValue.
    public static readonly BindableProperty CenterLabelProperty =
        BindableProperty.Create(nameof(CenterLabel), typeof(string), typeof(PieChart), """",
            propertyChanged: (b, o, n) => ((PieChart)b).Refresh());

    public double InnerRadius
    {
        get => (double)GetValue(InnerRadiusProperty);
        set => SetValue(InnerRadiusProperty, value);
    }

    public double Gap
    {
        get => (double)GetValue(GapProperty);
        set => SetValue(GapProperty, value);
    }

    public bool ShowLabels
    {
        get => (bool)GetValue(ShowLabelsProperty);
        set => SetValue(ShowLabelsProperty, value);
    }

    public string CenterValue
    {
        get => (string)GetValue(CenterValueProperty);
        set => SetValue(CenterValueProperty, value);
    }

    public string CenterLabel
    {
        get => (string)GetValue(CenterLabelProperty);
        set => SetValue(CenterLabelProperty, value);
    }

    // The slices of the last draw, for hit testing.
    private readonly List<(double Start, double Sweep)> _slices = new();
    private Point _center;
    private double _inner;
    private double _outer;

    private List<double> Slices() =>
        Series.Count == 0 ? new List<double>() : Series[0].Values.Select(value => Math.Max(0, value)).ToList();

    protected override bool HasData => Slices().Any(value => value > 0);

    protected override void DrawChart(IChartCanvas canvas, Rect area)
    {
        var values = Slices();
        var total = values.Sum();
        _slices.Clear();
        _center = new Point(area.Left + area.Width / 2, area.Top + area.Height / 2);
        _outer = Math.Max(4, Math.Min(area.Width, area.Height) / 2 - (ShowLabels ? 30 : 10));
        _inner = _outer * Math.Clamp(InnerRadius, 0, 0.95);
        var gap = values.Count(value => value > 0) > 1 ? Gap : 0;
        var start = -90.0;
        for (var i = 0; i < values.Count; i++)
        {
            var sweep = values[i] / total * 360 * Progress;
            _slices.Add((start, sweep));
            if (sweep > gap)
            {
                // The hovered slice stands out a little.
                var grow = HoveredIndex == i ? 6 : 0;
                canvas.Polygon(ChartMath.Sector(_center, _inner, _outer + grow, start + gap / 2, sweep - gap), Palette(i));
                if (ShowLabels && Progress >= 1)
                    canvas.Text(Format(values[i]), ChartMath.Polar(_center, _outer + 16, start + sweep / 2), 12,
                        ShellTheme.Get(ShellToken.Foreground), ChartAlign.Center, ChartAlign.Center);
            }
            start += sweep;
        }
        if (_inner > 0) DrawCenterText(canvas, _center, CenterValue, CenterLabel);
    }

    protected override int HitTest(Point point)
    {
        var dx = point.X - _center.X;
        var dy = point.Y - _center.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        if (distance < _inner || distance > _outer + 6) return -1;
        // Clockwise from twelve o'clock, like the slices.
        var angle = (Math.Atan2(dy, dx) * 180 / Math.PI + 90 + 360) % 360;
        for (var i = 0; i < _slices.Count; i++)
        {
            var from = _slices[i].Start + 90;
            if (angle >= from && angle < from + _slices[i].Sweep) return i;
        }
        return -1;
    }

    protected override List<(Color Color, string Name)> LegendEntries() =>
        Slices().Select((_, i) => (Palette(i), LabelAt(i))).ToList();

    protected override (string Title, List<(Color Color, string Name, string Value)> Rows) Tooltip(int index)
    {
        var values = Slices();
        var share = values[index] / values.Sum();
        return (string.Empty, new List<(Color Color, string Name, string Value)>
        {
            (Palette(index), LabelAt(index), $""{Format(values[index])} ({share.ToString(""P0"", CultureInfo.CurrentCulture)})"")
        });
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

// Pie chart — the first series' values as slices, clockwise from twelve o'clock, named by
// Labels and colored from the theme's chart colors in order. The hovered slice stands out and
// shows its value and share. For a pie with a hole, see DonutChart (donut-chart).
//   <ui:PieChart Labels=""Chrome, Safari, Firefox, Edge, Other"" Height=""260"">
//       <ui:ChartSeries Values=""275, 200, 187, 173, 90"" />
//   </ui:PieChart>
public class PieChart : ChartBase
{
    // The hole, as a share of the radius: 0 is a pie, 0.6 a donut.
    public static readonly StyledProperty<double> InnerRadiusProperty =
        AvaloniaProperty.Register<PieChart, double>(nameof(InnerRadius));

    // Degrees of space between slices.
    public static readonly StyledProperty<double> GapProperty =
        AvaloniaProperty.Register<PieChart, double>(nameof(Gap), 1d);

    // Writes each slice's value outside it.
    public static readonly StyledProperty<bool> ShowLabelsProperty =
        AvaloniaProperty.Register<PieChart, bool>(nameof(ShowLabels));

    // Large text in the hole of a donut.
    public static readonly StyledProperty<string?> CenterValueProperty =
        AvaloniaProperty.Register<PieChart, string?>(nameof(CenterValue), """");

    // Small muted text under CenterValue.
    public static readonly StyledProperty<string?> CenterLabelProperty =
        AvaloniaProperty.Register<PieChart, string?>(nameof(CenterLabel), """");

    static PieChart()
    {
        AffectsRender<PieChart>(InnerRadiusProperty, GapProperty, ShowLabelsProperty, CenterValueProperty, CenterLabelProperty);
    }

    public double InnerRadius
    {
        get => GetValue(InnerRadiusProperty);
        set => SetValue(InnerRadiusProperty, value);
    }

    public double Gap
    {
        get => GetValue(GapProperty);
        set => SetValue(GapProperty, value);
    }

    public bool ShowLabels
    {
        get => GetValue(ShowLabelsProperty);
        set => SetValue(ShowLabelsProperty, value);
    }

    public string? CenterValue
    {
        get => GetValue(CenterValueProperty);
        set => SetValue(CenterValueProperty, value);
    }

    public string? CenterLabel
    {
        get => GetValue(CenterLabelProperty);
        set => SetValue(CenterLabelProperty, value);
    }

    // The slices of the last draw, for hit testing.
    private readonly List<(double Start, double Sweep)> _slices = new();
    private Point _center;
    private double _inner;
    private double _outer;

    private List<double> Slices() =>
        Series.Count == 0 ? new List<double>() : Series[0].Values.Select(value => Math.Max(0, value)).ToList();

    protected override bool HasData => Slices().Any(value => value > 0);

    protected override void DrawChart(IChartCanvas canvas, Rect area)
    {
        var values = Slices();
        var total = values.Sum();
        _slices.Clear();
        _center = new Point(area.Left + area.Width / 2, area.Top + area.Height / 2);
        _outer = Math.Max(4, Math.Min(area.Width, area.Height) / 2 - (ShowLabels ? 30 : 10));
        _inner = _outer * Math.Clamp(InnerRadius, 0, 0.95);
        var gap = values.Count(value => value > 0) > 1 ? Gap : 0;
        var start = -90.0;
        for (var i = 0; i < values.Count; i++)
        {
            var sweep = values[i] / total * 360 * Progress;
            _slices.Add((start, sweep));
            if (sweep > gap)
            {
                // The hovered slice stands out a little.
                var grow = HoveredIndex == i ? 6 : 0;
                canvas.Polygon(ChartMath.Sector(_center, _inner, _outer + grow, start + gap / 2, sweep - gap), Palette(i));
                if (ShowLabels && Progress >= 1)
                    canvas.Text(Format(values[i]), ChartMath.Polar(_center, _outer + 16, start + sweep / 2), 12,
                        ShellTheme.Get(ShellToken.Foreground), ChartAlign.Center, ChartAlign.Center);
            }
            start += sweep;
        }
        if (_inner > 0) DrawCenterText(canvas, _center, CenterValue, CenterLabel);
    }

    protected override int HitTest(Point point)
    {
        var dx = point.X - _center.X;
        var dy = point.Y - _center.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        if (distance < _inner || distance > _outer + 6) return -1;
        // Clockwise from twelve o'clock, like the slices.
        var angle = (Math.Atan2(dy, dx) * 180 / Math.PI + 90 + 360) % 360;
        for (var i = 0; i < _slices.Count; i++)
        {
            var from = _slices[i].Start + 90;
            if (angle >= from && angle < from + _slices[i].Sweep) return i;
        }
        return -1;
    }

    protected override List<(Color Color, string Name)> LegendEntries() =>
        Slices().Select((_, i) => (Palette(i), LabelAt(i))).ToList();

    protected override (string Title, List<(Color Color, string Name, string Value)> Rows) Tooltip(int index)
    {
        var values = Slices();
        var share = values[index] / values.Sum();
        return (string.Empty, new List<(Color Color, string Name, string Value)>
        {
            (Palette(index), LabelAt(index), $""{Format(values[index])} ({share.ToString(""P0"", CultureInfo.CurrentCulture)})"")
        });
    }
}
"
    };
}
