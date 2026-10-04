namespace MAUI.Demo.Components.UI;

// Modal dialog. Place it where it can fill the page (e.g. last child of the page's root Grid):
//   <ui:Dialog x:Name="ConfirmDialog">
//       <ui:DialogTrigger><ui:Button Text="Open" /></ui:DialogTrigger>   (optional)
//       <ui:DialogContent>...</ui:DialogContent>
//   </ui:Dialog>
// Open from code with ConfirmDialog.SetOpen(true).
public partial class Dialog : ShellOverlayHost
{
    protected override bool IsTrigger(Element child) => child is DialogTrigger;
}
