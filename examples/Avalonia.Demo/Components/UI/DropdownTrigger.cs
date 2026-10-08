namespace AvaloniaDemo.Components.UI;

// Toggles the enclosing Dropdown. Wrap a Button or any control.
public class DropdownTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Dropdown>()?.Toggle();
}
