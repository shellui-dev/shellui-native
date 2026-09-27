namespace MAUI.Demo.Components.UI;

// Section body — pb-4 text-sm; expands/collapses its height with the item.
[ContentProperty(nameof(Content))]
public partial class AccordionContent : ContentView
{
    public AccordionContent()
    {
        IsVisible = false;
        Opacity = 0;
        IsClippedToBounds = true;
        Padding = new Thickness(0, 0, 0, 16);
    }

    internal void Apply(bool open, bool animate)
    {
        if (animate)
        {
            _ = this.AnimateExpandAsync(open);
            return;
        }
        this.AbortAnimation("ShellExpand");
        IsVisible = open;
        Opacity = open ? 1 : 0;
        HeightRequest = -1;
    }
}
