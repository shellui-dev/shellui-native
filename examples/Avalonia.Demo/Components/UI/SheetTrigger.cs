namespace AvaloniaDemo.Components.UI;

// Opens the enclosing Sheet. Usage: <ui:SheetTrigger><ui:Button Text="Open" /></ui:SheetTrigger>
public class SheetTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Sheet>()?.SetOpen(true);
}
