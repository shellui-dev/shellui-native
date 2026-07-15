namespace MAUI.Demo.Components.UI;

public partial class PopoverTrigger : ContentView
{
    public PopoverTrigger()
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => this.FindParentOfType<Popover>()?.ToggleAsync();
        GestureRecognizers.Add(tap);
    }
}
