namespace MAUI.Demo.Components.UI;

// Scrollable region with the platform's scrollbar. For the shadcn look, wrap it in a Border
// with a 1px stroke and 6px corners: <Border ...><ui:ScrollArea HeightRequest="200">...</ui:ScrollArea></Border>
public partial class ScrollArea : ScrollView
{
    public ScrollArea()
    {
        Orientation = ScrollOrientation.Vertical;
        VerticalScrollBarVisibility = ScrollBarVisibility.Default;
    }
}
