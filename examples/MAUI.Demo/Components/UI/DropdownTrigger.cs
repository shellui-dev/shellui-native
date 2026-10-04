namespace MAUI.Demo.Components.UI;

// Toggles the enclosing Dropdown. Wrap a Button or any view.
public partial class DropdownTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Dropdown>()?.Toggle();
}
