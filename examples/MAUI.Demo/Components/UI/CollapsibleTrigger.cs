namespace MAUI.Demo.Components.UI;

// Toggles the enclosing Collapsible. Wrap a Button or any view (a row, an icon, a label).
public partial class CollapsibleTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Collapsible>()?.Toggle();
}
