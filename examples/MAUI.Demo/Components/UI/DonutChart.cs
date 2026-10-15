namespace MAUI.Demo.Components.UI;

// A pie with a hole — InnerRadius starts at 0.6, with CenterValue and CenterLabel in the middle.
//   <ui:DonutChart Labels="Chrome, Safari, Firefox" CenterValue="662" CenterLabel="Visitors" HeightRequest="260">
//       <ui:ChartSeries Values="275, 200, 187" />
//   </ui:DonutChart>
public partial class DonutChart : PieChart
{
    public DonutChart()
    {
        InnerRadius = 0.6;
    }
}
