namespace MAUI.Demo.Components.UI;

// Closes the enclosing Dialog. Wrap a Button: <ui:DialogClose><ui:Button Text="Cancel" /></ui:DialogClose>
public partial class DialogClose : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Dialog>()?.SetOpen(false);
}
