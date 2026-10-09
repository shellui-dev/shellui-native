namespace AvaloniaDemo.Components.UI;

// Toggles the enclosing Collapsible. Wrap a Button or any control (a row, an icon, a label).
public class CollapsibleTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Collapsible>()?.Toggle();
}
