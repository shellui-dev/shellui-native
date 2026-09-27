namespace MAUI.Demo.Components.UI;

// Content shown while the enclosing Collapsible is open; expands/collapses its height.
[ContentProperty(nameof(Content))]
public partial class CollapsibleContent : ContentView
{
    public CollapsibleContent()
    {
        IsVisible = false;
        Opacity = 0;
        IsClippedToBounds = true;
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
