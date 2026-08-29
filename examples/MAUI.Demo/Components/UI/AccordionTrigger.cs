namespace MAUI.Demo.Components.UI;

public partial class AccordionTrigger : ContentView
{
    public AccordionTrigger()
    {
        MinimumHeightRequest = 40;
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => this.FindParentOfType<AccordionItem>()?.Toggle();
        GestureRecognizers.Add(tap);
    }
}
