namespace MAUI.Demo.Components.UI;

public partial class DialogClose : ContentView
{
    public DialogClose()
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => this.FindParentOfType<Dialog>()?.SetOpen(false);
        GestureRecognizers.Add(tap);
    }
}
