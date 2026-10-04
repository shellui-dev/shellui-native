namespace MAUI.Demo.Components.UI;

// text-lg font-semibold leading-none
public partial class DialogTitle : Label
{
    public DialogTitle()
    {
        FontSize = 18;
        FontAttributes = FontAttributes.Bold;
        this.Token(TextColorProperty, ShellToken.Foreground);
    }
}
