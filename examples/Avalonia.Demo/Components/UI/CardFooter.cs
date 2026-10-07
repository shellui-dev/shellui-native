using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AvaloniaDemo.Components.UI;

// Card footer — flex items-center p-6 pt-0; actions right-aligned.
public class CardFooter : StackPanel
{
    public CardFooter()
    {
        Orientation = Orientation.Horizontal;
        Spacing = 8;
        HorizontalAlignment = HorizontalAlignment.Right;
        Margin = new Thickness(24, 0, 24, 24);
    }
}
