namespace MAUI.Demo.Components.UI;

public partial class DrawerTrigger : ContentView
{
    public DrawerTrigger()
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => this.FindParentOfType<Drawer>()?.SetOpen(true);
        GestureRecognizers.Add(tap);
    }
}
