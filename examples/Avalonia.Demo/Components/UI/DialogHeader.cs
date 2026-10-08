using Avalonia;
using Avalonia.Controls;

namespace AvaloniaDemo.Components.UI;

// Title + description stack — flex flex-col space-y-1.5. Leaves room for the close button.
public class DialogHeader : StackPanel
{
    public DialogHeader()
    {
        Spacing = 6;
        Margin = new Thickness(0, 0, 24, 0);
    }
}
