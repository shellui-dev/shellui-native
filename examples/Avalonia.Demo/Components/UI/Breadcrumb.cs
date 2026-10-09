using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AvaloniaDemo.Components.UI;

// Navigation trail — items separated by chevrons; the last item hides its separator.
//   <ui:Breadcrumb>
//       <ui:BreadcrumbItem Text="Home" />
//       <ui:BreadcrumbItem Text="Settings" IsCurrent="True" />
//   </ui:Breadcrumb>
public class Breadcrumb : StackPanel
{
    public Breadcrumb()
    {
        Orientation = Orientation.Horizontal;
        Children.CollectionChanged += (_, _) => RefreshItemSeparators();
    }

    private void RefreshItemSeparators()
    {
        var items = Children.OfType<BreadcrumbItem>().ToList();
        for (var i = 0; i < items.Count; i++)
            items[i].SetSeparatorVisible(i < items.Count - 1);
    }
}
