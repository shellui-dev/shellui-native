namespace AvaloniaDemo.Components.UI;

// Opens the enclosing Dialog. Usage: <ui:DialogTrigger><ui:Button Text="Open" /></ui:DialogTrigger>
public class DialogTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Dialog>()?.SetOpen(true);
}
