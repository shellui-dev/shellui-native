using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class RadialChartTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "radial-chart",
        DisplayName = "Radial Chart",
        Description = "Concentric progress rings",
        Category = ComponentCategory.DataDisplay,
        FilePath = "RadialChart.cs",
        Dependencies = new List<string> { "shell", "chart" },
        Tags = new List<string> { "chart", "radial", "ring", "progress" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using System.Globalization;

namespace YourProjectNamespace.Components.UI;

// Radial chart — the first series' values as concentric rings on a muted track, each filling
// clockwise from twelve o'clock towards Maximum. Rings are named by Labels and colored from
// the theme's chart colors in order, the first outermost.
//   <ui:RadialChart Labels=""Move, Exercise, Stand"" Maximum=""100"" CenterValue=""72%"" CenterLabel=""Today"" HeightRequest=""260"">
//       <ui:ChartSeries Values=""72, 48, 90"" />
//   </ui:RadialChart>
public partial class RadialChart : ChartBase
{
    // The value of a full ring; by default the largest value.
    public static readonly BindableProperty MaximumProperty =
        BindableProperty.Create(nameof(Maximum), typeof(double), typeof(RadialChart), double.NaN,
            propertyChanged: (b, o, n) => ((RadialChart)b).Refresh());

    // The hole in the middle, as a share of the radius.
    public static readonly BindableProperty InnerRadiusProperty =
        BindableProperty.Create(nameof(InnerRadius), typeof(double), typeof(RadialChart), 0.35,
            propertyChanged: (b, o, n) => ((RadialChart)b).Refresh());

    // Rounds the ends of each ring.
    public static readonly BindableProperty IsRoundedProperty =
        BindableProperty.Create(nameof(IsRounded), typeof(bool), typeof(RadialChart), true,
            propertyChanged: (b, o, n) => ((RadialChart)b).Refresh());

    // Large text in the middle.
    public static readonly BindableProperty CenterValueProperty =
        BindableProperty.Create(nameof(CenterValue), typeof(string), typeof(RadialChart), """",
            propertyChanged: (b, o, n) => ((RadialChart)b).Refresh());

    // Small muted text under CenterValue.
    public static readonly BindableProperty CenterLabelProperty =
        BindableProperty.Create(nameof(CenterLabel), typeof(string), typeof(RadialChart), """",
            propertyChanged: (b, o, n) => ((RadialChart)b).Refresh());

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double InnerRadius
    {
        get => (double)GetValue(InnerRadiusProperty);
        set => SetValue(InnerRadiusProperty, value);
    }

    public bool IsRounded
    {
        get => (bool)GetValue(IsRoundedProperty);
        set => SetValue(IsRoundedProperty, value);
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

    private Point _center;
    private double _outer;
    private double _hole;
    private double _band;

    protected override bool HasData => Series.Count > 0 && Series[0].Values.Count > 0;

    protected override void DrawChart(IChartCanvas canvas, Rect area)
    {
        var values = Series[0].Values;
        _center = new Point(area.Left + area.Width / 2, area.Top + area.Height / 2);
        _outer = Math.Max(8, Math.Min(area.Width, area.Height) / 2 - 8);
        _hole = _outer * Math.Clamp(InnerRadius, 0.05, 0.9);
        _band = (_outer - _hole) / values.Count;
        var thickness = _band - Math.Min(4, _band * 0.25);
        var max = double.IsNaN(Maximum) ? values.Max() : Maximum;
        if (max <= 0) max = 1;
        var track = ShellTheme.Get(ShellToken.Muted);

        // One ring per value, the first outermost, each filling clockwise from twelve o'clock.
        for (var i = 0; i < values.Count; i++)
        {
            var outer = _outer - i * _band;
            var inner = outer - thickness;
            canvas.Polygon(ChartMath.Sector(_center, inner, outer, 0, 360), track);
            var sweep = Math.Clamp(values[i] / max, 0, 1) * 360 * Progress;
            if (sweep <= 0) continue;
            var grow = HoveredIndex == i ? 2 : 0;
            var color = Palette(i);
            canvas.Polygon(ChartMath.Sector(_center, inner - grow, outer + grow, -90, sweep), color);
            if (!IsRounded || sweep >= 359) continue;
            var middle = (inner + outer) / 2;
            canvas.Polygon(ChartMath.Circle(ChartMath.Polar(_center, middle, -90), thickness / 2 + grow), color);
            canvas.Polygon(ChartMath.Circle(ChartMath.Polar(_center, middle, -90 + sweep), thickness / 2 + grow), color);
        }
        DrawCenterText(canvas, _center, CenterValue, CenterLabel);
    }

    protected override int HitTest(Point point)
    {
        var dx = point.X - _center.X;
        var dy = point.Y - _center.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        if (_band <= 0 || distance < _hole || distance > _outer) return -1;
        return Math.Clamp((int)Math.Floor((_outer - distance) / _band), 0, Series[0].Values.Count - 1);
    }

    protected override List<(Color Color, string Name)> LegendEntries() =>
        Series[0].Values.Select((_, i) => (Palette(i), LabelAt(i))).ToList();

    protected override (string Title, List<(Color Color, string Name, string Value)> Rows) Tooltip(int index) =>
        (string.Empty, new List<(Color Color, string Name, string Value)> { (Palette(index), LabelAt(index), Format(Series[0].Values[index])) });
}
",
        [NativePlatform.Avalonia] = @"using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Media;

namespace YourProjectNamespace.Components.UI;

// Radial chart — the first series' values as concentric rings on a muted track, each filling
// clockwise from twelve o'clock towards Maximum. Rings are named by Labels and colored from
// the theme's chart colors in order, the first outermost.
//   <ui:RadialChart Labels=""Move, Exercise, Stand"" Maximum=""100"" CenterValue=""72%"" CenterLabel=""Today"" Height=""260"">
//       <ui:ChartSeries Values=""72, 48, 90"" />
//   </ui:RadialChart>
public class RadialChart : ChartBase
{
    // The value of a full ring; by default the largest value.
    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<RadialChart, double>(nameof(Maximum), double.NaN);

    // The hole in the middle, as a share of the radius.
    public static readonly StyledProperty<double> InnerRadiusProperty =
        AvaloniaProperty.Register<RadialChart, double>(nameof(InnerRadius), 0.35);

    // Rounds the ends of each ring.
    public static readonly StyledProperty<bool> IsRoundedProperty =
        AvaloniaProperty.Register<RadialChart, bool>(nameof(IsRounded), true);

    // Large text in the middle.
    public static readonly StyledProperty<string?> CenterValueProperty =
        AvaloniaProperty.Register<RadialChart, string?>(nameof(CenterValue), """");

    // Small muted text under CenterValue.
    public static readonly StyledProperty<string?> CenterLabelProperty =
        AvaloniaProperty.Register<RadialChart, string?>(nameof(CenterLabel), """");

    static RadialChart()
    {
        AffectsRender<RadialChart>(MaximumProperty, InnerRadiusProperty, IsRoundedProperty, CenterValueProperty, CenterLabelProperty);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double InnerRadius
    {
        get => GetValue(InnerRadiusProperty);
        set => SetValue(InnerRadiusProperty, value);
    }

    public bool IsRounded
    {
        get => GetValue(IsRoundedProperty);
        set => SetValue(IsRoundedProperty, value);
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

    private Point _center;
    private double _outer;
    private double _hole;
    private double _band;

    protected override bool HasData => Series.Count > 0 && Series[0].Values.Count > 0;

    protected override void DrawChart(IChartCanvas canvas, Rect area)
    {
        var values = Series[0].Values;
        _center = new Point(area.Left + area.Width / 2, area.Top + area.Height / 2);
        _outer = Math.Max(8, Math.Min(area.Width, area.Height) / 2 - 8);
        _hole = _outer * Math.Clamp(InnerRadius, 0.05, 0.9);
        _band = (_outer - _hole) / values.Count;
        var thickness = _band - Math.Min(4, _band * 0.25);
        var max = double.IsNaN(Maximum) ? values.Max() : Maximum;
        if (max <= 0) max = 1;
        var track = ShellTheme.Get(ShellToken.Muted);

        // One ring per value, the first outermost, each filling clockwise from twelve o'clock.
        for (var i = 0; i < values.Count; i++)
        {
            var outer = _outer - i * _band;
            var inner = outer - thickness;
            canvas.Polygon(ChartMath.Sector(_center, inner, outer, 0, 360), track);
            var sweep = Math.Clamp(values[i] / max, 0, 1) * 360 * Progress;
            if (sweep <= 0) continue;
            var grow = HoveredIndex == i ? 2 : 0;
            var color = Palette(i);
            canvas.Polygon(ChartMath.Sector(_center, inner - grow, outer + grow, -90, sweep), color);
            if (!IsRounded || sweep >= 359) continue;
            var middle = (inner + outer) / 2;
            canvas.Polygon(ChartMath.Circle(ChartMath.Polar(_center, middle, -90), thickness / 2 + grow), color);
            canvas.Polygon(ChartMath.Circle(ChartMath.Polar(_center, middle, -90 + sweep), thickness / 2 + grow), color);
        }
        DrawCenterText(canvas, _center, CenterValue, CenterLabel);
    }

    protected override int HitTest(Point point)
    {
        var dx = point.X - _center.X;
        var dy = point.Y - _center.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        if (_band <= 0 || distance < _hole || distance > _outer) return -1;
        return Math.Clamp((int)Math.Floor((_outer - distance) / _band), 0, Series[0].Values.Count - 1);
    }

    protected override List<(Color Color, string Name)> LegendEntries() =>
        Series[0].Values.Select((_, i) => (Palette(i), LabelAt(i))).ToList();

    protected override (string Title, List<(Color Color, string Name, string Value)> Rows) Tooltip(int index) =>
        (string.Empty, new List<(Color Color, string Name, string Value)> { (Palette(index), LabelAt(index), Format(Series[0].Values[index])) });
}
"
    };
}
