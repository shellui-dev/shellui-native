using Avalonia.Controls;

namespace AvaloniaDemo.Components.UI;

// Content shown while the enclosing Collapsible is open; expands/collapses its height.
public class CollapsibleContent : Border
{
    public CollapsibleContent()
    {
        IsVisible = false;
        Opacity = 0;
    }

    internal void Apply(bool open, bool animate)
    {
        if (animate) _ = this.AnimateExpandAsync(open);
        else this.SetExpanded(open);
    }
}
