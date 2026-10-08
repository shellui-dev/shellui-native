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
        Dependencies = new List<string> { "shell", "element-extensions" },
        Tags = new List<string> { "overlay", "sheet", "trigger" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Opens the enclosing Sheet. Usage: <ui:SheetTrigger><ui:Button Text=""Open"" /></ui:SheetTrigger>
public partial class SheetTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Sheet>()?.SetOpen(true);
}
",
        [NativePlatform.Avalonia] = @"namespace YourProjectNamespace.Components.UI;

// Opens the enclosing Sheet. Usage: <ui:SheetTrigger><ui:Button Text=""Open"" /></ui:SheetTrigger>
public class SheetTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Sheet>()?.SetOpen(true);
}
"
    };
}
