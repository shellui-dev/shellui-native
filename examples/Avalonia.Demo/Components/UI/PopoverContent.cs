using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Popover panel — w-72 rounded-md border bg-popover p-4 shadow-md. XAML children go to Items.
public class PopoverContent : Border
{
    private readonly StackPanel _stack = new() { Spacing = 8 };

    [Content]
    public Controls Items => _stack.Children;

    public PopoverContent()
    {
        Child = _stack;
        Padding = new Thickness(16);
        Width = 288;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        BoxShadow = ShellPopups.PanelShadow();
        this.Token(BackgroundProperty, ShellToken.Popover).Token(BorderBrushProperty, ShellToken.Border);
    }
}
