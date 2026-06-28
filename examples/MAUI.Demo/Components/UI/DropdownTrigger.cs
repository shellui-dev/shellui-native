namespace MAUI.Demo.Components.UI;

public partial class DropdownTrigger : ContentView
{
    public DropdownTrigger()
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => this.FindParentOfType<Dropdown>()?.ToggleAsync();
        GestureRecognizers.Add(tap);
    }
}
