using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DonutChartTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "donut-chart",
        DisplayName = "Donut Chart",
        Description = "A pie chart with a hole for a value and a label",
        Category = ComponentCategory.DataDisplay,
        FilePath = "DonutChart.cs",
        Dependencies = new List<string> { "pie-chart" },
        Tags = new List<string> { "chart", "donut", "pie", "share" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// A pie with a hole — InnerRadius starts at 0.6, with CenterValue and CenterLabel in the middle.
//   <ui:DonutChart Labels=""Chrome, Safari, Firefox"" CenterValue=""662"" CenterLabel=""Visitors"" HeightRequest=""260"">
//       <ui:ChartSeries Values=""275, 200, 187"" />
//   </ui:DonutChart>
public partial class DonutChart : PieChart
{
    public DonutChart()
    {
        InnerRadius = 0.6;
    }
}
",
        [NativePlatform.Avalonia] = @"namespace YourProjectNamespace.Components.UI;

// A pie with a hole — InnerRadius starts at 0.6, with CenterValue and CenterLabel in the middle.
//   <ui:DonutChart Labels=""Chrome, Safari, Firefox"" CenterValue=""662"" CenterLabel=""Visitors"" Height=""260"">
//       <ui:ChartSeries Values=""275, 200, 187"" />
//   </ui:DonutChart>
public class DonutChart : PieChart
{
    public DonutChart()
    {
        InnerRadius = 0.6;
    }
}
"
    };
}
