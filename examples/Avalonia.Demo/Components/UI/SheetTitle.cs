using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// text-lg font-semibold text-foreground
public class SheetTitle : TextBlock
{
    public SheetTitle()
    {
        FontSize = 18;
        FontWeight = FontWeight.SemiBold;
        TextWrapping = TextWrapping.Wrap;
        this.Token(ForegroundProperty, ShellToken.Foreground);
    }
}
