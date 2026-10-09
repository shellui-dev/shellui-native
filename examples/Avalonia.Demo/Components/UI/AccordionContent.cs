using Avalonia;
using Avalonia.Controls;

namespace AvaloniaDemo.Components.UI;

// Section body — pb-4 text-sm; expands/collapses its height with the item.
public class AccordionContent : Border
{
    public AccordionContent()
    {
        IsVisible = false;
        Opacity = 0;
        Padding = new Thickness(0, 0, 0, 16);
    }

    internal void Apply(bool open, bool animate)
    {
        if (animate) _ = this.AnimateExpandAsync(open);
        else this.SetExpanded(open);
    }
}
