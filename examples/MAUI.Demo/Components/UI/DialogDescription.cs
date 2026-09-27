namespace MAUI.Demo.Components.UI;

// text-sm text-muted-foreground
public partial class DialogDescription : Label
{
    public DialogDescription()
    {
        FontSize = 14;
        this.Token(TextColorProperty, ShellToken.MutedForeground);
    }
}
