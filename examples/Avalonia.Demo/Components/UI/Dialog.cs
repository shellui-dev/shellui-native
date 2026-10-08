namespace AvaloniaDemo.Components.UI;

// Modal dialog. Declare it anywhere; the content covers the window while open:
//   <ui:Dialog x:Name="ConfirmDialog">
//       <ui:DialogTrigger><ui:Button Text="Open" /></ui:DialogTrigger>   (optional)
//       <ui:DialogContent>...</ui:DialogContent>
//   </ui:Dialog>
// Open from code with ConfirmDialog.SetOpen(true).
public class Dialog : ShellOverlayHost
{
}
