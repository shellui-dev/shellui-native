namespace MAUI.Demo.Components.UI;

// Toggles the enclosing Popover. Wrap a Button or any view.
public partial class PopoverTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Popover>()?.Toggle();
}
