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
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "overlay", "drawer", "trigger" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

public partial class DrawerTrigger : ContentView
{
    public DrawerTrigger()
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => FindParentOfType<Drawer>()?.SetOpen(true);
        GestureRecognizers.Add(tap);
    }
}
"
    };
}
