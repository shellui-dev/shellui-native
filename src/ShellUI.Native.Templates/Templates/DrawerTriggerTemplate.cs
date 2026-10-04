using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DrawerTriggerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "drawer-trigger",
        DisplayName = "Drawer Trigger",
        Description = "Opens drawer on tap",
        Category = ComponentCategory.Overlay,
        FilePath = "DrawerTrigger.cs",
        Dependencies = new List<string> { "shell", "element-extensions" },
        Tags = new List<string> { "overlay", "drawer", "trigger" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Opens the enclosing Drawer. Usage: <ui:DrawerTrigger><ui:Button Text=""Open"" /></ui:DrawerTrigger>
public partial class DrawerTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Drawer>()?.SetOpen(true);
}
"
    };
}
