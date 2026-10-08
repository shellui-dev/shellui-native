namespace AvaloniaDemo.Components.UI;

// Opens the enclosing Drawer. Usage: <ui:DrawerTrigger><ui:Button Text="Open" /></ui:DrawerTrigger>
public class DrawerTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Drawer>()?.SetOpen(true);
}
