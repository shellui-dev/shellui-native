namespace MAUI.Demo.Components.UI;

// text-sm text-muted-foreground
public partial class SheetDescription : Label
{
    public SheetDescription()
    {
        FontSize = 14;
        this.Token(TextColorProperty, ShellToken.MutedForeground);
    }
}
