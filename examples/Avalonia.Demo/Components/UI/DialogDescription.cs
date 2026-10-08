using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// text-sm text-muted-foreground
public class DialogDescription : TextBlock
{
    public DialogDescription()
    {
        FontSize = 14;
        TextWrapping = TextWrapping.Wrap;
        this.Token(ForegroundProperty, ShellToken.MutedForeground);
    }
}
