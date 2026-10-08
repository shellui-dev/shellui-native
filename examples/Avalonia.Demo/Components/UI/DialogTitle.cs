using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// text-lg font-semibold leading-none
public class DialogTitle : TextBlock
{
    public DialogTitle()
    {
        FontSize = 18;
        FontWeight = FontWeight.SemiBold;
        TextWrapping = TextWrapping.Wrap;
        this.Token(ForegroundProperty, ShellToken.Foreground);
    }
}
