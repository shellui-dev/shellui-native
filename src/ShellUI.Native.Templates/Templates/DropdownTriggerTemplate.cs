using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DropdownTriggerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "dropdown-trigger",
        DisplayName = "Dropdown Trigger",
        Description = "Opens dropdown on tap",
        Category = ComponentCategory.Overlay,
        FilePath = "DropdownTrigger.cs",
        Dependencies = new List<string> { "shell", "element-extensions" },
        Tags = new List<string> { "overlay", "dropdown", "trigger" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Toggles the enclosing Dropdown. Wrap a Button or any view.
public partial class DropdownTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Dropdown>()?.Toggle();
}
"
    };
}
