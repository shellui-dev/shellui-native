namespace MAUI.Demo.Components.UI;

// Tap to open dialog. Usage: <DialogTrigger><Button Text="Open" /></DialogTrigger>
public partial class DialogTrigger : ContentView
{
    public DialogTrigger()
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;
        GestureRecognizers.Add(tap);
        // Ensure taps pass through to child if Content is interactive
        InputTransparent = false;
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        var dialog = this.FindParentOfType<Dialog>();
        dialog?.SetOpen(true);
    }
}
