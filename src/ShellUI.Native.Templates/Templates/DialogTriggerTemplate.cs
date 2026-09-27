using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DialogTriggerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "dialog-trigger",
        DisplayName = "Dialog Trigger",
        Description = "Triggers dialog open on tap - place inside Dialog",
        Category = ComponentCategory.Overlay,
        FilePath = "DialogTrigger.cs",
        Dependencies = new List<string> { "shell", "element-extensions" },
        Tags = new List<string> { "overlay", "dialog", "trigger" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Opens the enclosing Dialog. Usage: <ui:DialogTrigger><ui:Button Text=""Open"" /></ui:DialogTrigger>
public partial class DialogTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Dialog>()?.SetOpen(true);
}
"
    };
}
