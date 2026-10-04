using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DialogCloseTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "dialog-close",
        DisplayName = "Dialog Close",
        Description = "Closes dialog on tap - use as button or icon",
        Category = ComponentCategory.Overlay,
        FilePath = "DialogClose.cs",
        Dependencies = new List<string> { "shell", "element-extensions" },
        Tags = new List<string> { "overlay", "dialog", "close" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Closes the enclosing Dialog. Wrap a Button: <ui:DialogClose><ui:Button Text=""Cancel"" /></ui:DialogClose>
public partial class DialogClose : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Dialog>()?.SetOpen(false);
}
"
    };
}
