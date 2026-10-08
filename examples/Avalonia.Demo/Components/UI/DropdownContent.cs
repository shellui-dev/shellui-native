using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Menu panel — min-w-[8rem] rounded-md border bg-popover p-1 shadow-md. XAML children go to Items.
public class DropdownContent : Border
{
    private readonly StackPanel _stack = new();

    [Content]
    public Controls Items => _stack.Children;

    public DropdownContent()
    {
        Child = _stack;
        Padding = new Thickness(4);
        MinWidth = 180;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        BoxShadow = ShellPopups.PanelShadow();
        this.Token(BackgroundProperty, ShellToken.Popover).Token(BorderBrushProperty, ShellToken.Border);
    }
}
