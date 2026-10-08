namespace AvaloniaDemo.Components.UI;

// Toggles the enclosing Popover. Wrap a Button or any control.
public class PopoverTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Popover>()?.Toggle();
}
