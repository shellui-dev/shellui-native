namespace MAUI.Demo.Components.UI;

public partial class SheetTrigger : ContentView
{
    public SheetTrigger()
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => this.FindParentOfType<Sheet>()?.SetOpen(true);
        GestureRecognizers.Add(tap);
    }
}
