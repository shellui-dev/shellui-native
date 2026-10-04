using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// CollapsibleTrigger - tap toggles the parent Collapsible's Open state.
// Row-level control: honors the 40px form baseline via MinimumHeightRequest.
public static class CollapsibleTriggerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "collapsible-trigger",
        DisplayName = "Collapsible Trigger",
        Description = "Tap-to-toggle handle for a Collapsible - place inside Collapsible",
        Category = ComponentCategory.Layout,
        FilePath = "CollapsibleTrigger.cs",
        Dependencies = new List<string> { "shell", "element-extensions" },
        Tags = new List<string> { "layout", "collapsible", "trigger" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Toggles the enclosing Collapsible. Wrap a Button or any view (a row, an icon, a label).
public partial class CollapsibleTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Collapsible>()?.Toggle();
}
"
    };
}
