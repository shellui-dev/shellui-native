namespace MAUI.Demo.Components.UI;

// Opens the enclosing Dialog. Usage: <ui:DialogTrigger><ui:Button Text="Open" /></ui:DialogTrigger>
public partial class DialogTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Dialog>()?.SetOpen(true);
}
