using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DialogTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "dialog",
        DisplayName = "Dialog",
        Description = "Modal dialog container - use with DialogTrigger and DialogContent",
        Category = ComponentCategory.Overlay,
        FilePath = "Dialog.cs",
        Dependencies = new List<string> { "shell", "dialog-trigger", "dialog-content", "dialog-header", "dialog-footer", "dialog-title", "dialog-description", "dialog-close" },
        Tags = new List<string> { "overlay", "modal", "dialog" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Modal dialog. Place it where it can fill the page (e.g. last child of the page's root Grid):
//   <ui:Dialog x:Name=""ConfirmDialog"">
//       <ui:DialogTrigger><ui:Button Text=""Open"" /></ui:DialogTrigger>   (optional)
//       <ui:DialogContent>...</ui:DialogContent>
//   </ui:Dialog>
// Open from code with ConfirmDialog.SetOpen(true).
public partial class Dialog : ShellOverlayHost
{
    protected override bool IsTrigger(Element child) => child is DialogTrigger;
}
"
    };
}
