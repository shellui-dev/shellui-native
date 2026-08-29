namespace MAUI.Demo.Components.UI;

// Usage: <ScrollArea><VerticalStackLayout>...</VerticalStackLayout></ScrollArea>
public partial class ScrollArea : ScrollView
{
    public ScrollArea()
    {
        Orientation = ScrollOrientation.Vertical;
    }
}
