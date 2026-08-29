namespace MAUI.Demo.Components.UI;

// Tap to toggle. Wrap your visible handle - a Label, an icon, a whole row.
// MinimumHeightRequest = 40 keeps hit target aligned with Input / Select / Button
// (Form Sizing Contract).
public partial class CollapsibleTrigger : ContentView
{
    public CollapsibleTrigger()
    {
        MinimumHeightRequest = 40;
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;
        GestureRecognizers.Add(tap);
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        this.FindParentOfType<Collapsible>()?.Toggle();
    }
}
