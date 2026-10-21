using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AvaloniaDemo.Components.UI;

// Actions row at the end of a sheet — flex justify-end gap-2.
public class SheetFooter : StackPanel
{
    public SheetFooter()
    {
        Orientation = Orientation.Horizontal;
        Spacing = 8;
        HorizontalAlignment = HorizontalAlignment.Right;
        Margin = new Thickness(0, 8, 0, 0);
    }
}
