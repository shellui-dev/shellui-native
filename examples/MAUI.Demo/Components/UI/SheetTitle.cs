namespace MAUI.Demo.Components.UI;

// text-lg font-semibold text-foreground
public partial class SheetTitle : Label
{
    public SheetTitle()
    {
        FontSize = 18;
        FontAttributes = FontAttributes.Bold;
        this.Token(TextColorProperty, ShellToken.Foreground);
    }
}
