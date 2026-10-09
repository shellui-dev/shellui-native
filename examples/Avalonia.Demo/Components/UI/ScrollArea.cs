using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AvaloniaDemo.Components.UI;

// Scrollable region with Fluent's thin scrollbar, which widens on hover and hides when idle.
// For the shadcn look, wrap it in a 1px border with 6px corners:
//   <Border BorderThickness="1" CornerRadius="6"><ui:ScrollArea Height="200">...</ui:ScrollArea></Border>
public class ScrollArea : ScrollViewer
{
    // Keeps ScrollViewer's theme; a subclass would otherwise have none.
    protected override Type StyleKeyOverride => typeof(ScrollViewer);

    public ScrollArea()
    {
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
    }
}
