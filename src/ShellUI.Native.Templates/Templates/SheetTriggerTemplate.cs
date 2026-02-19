using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class SheetTriggerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "sheet-trigger",
        DisplayName = "Sheet Trigger",
        Description = "Opens sheet on tap",
        Category = ComponentCategory.Overlay,
        FilePath = "SheetTrigger.cs",
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "overlay", "sheet", "trigger" }
    };

    public static string Content => @"namespace YourProjectNamespace.Components.UI;

public partial class SheetTrigger : ContentView
{
    public SheetTrigger()
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => FindParentOfType<Sheet>()?.SetOpen(true);
        GestureRecognizers.Add(tap);
    }
}
";
}
