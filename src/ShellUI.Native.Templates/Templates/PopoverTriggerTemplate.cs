using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class PopoverTriggerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "popover-trigger",
        DisplayName = "Popover Trigger",
        Description = "Opens popover on tap",
        Category = ComponentCategory.Overlay,
        FilePath = "PopoverTrigger.cs",
        Dependencies = new List<string> { "shell", "element-extensions" },
        Tags = new List<string> { "overlay", "popover", "trigger" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Toggles the enclosing Popover. Wrap a Button or any view.
public partial class PopoverTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Popover>()?.Toggle();
}
",
        [NativePlatform.Avalonia] = @"namespace YourProjectNamespace.Components.UI;

// Toggles the enclosing Popover. Wrap a Button or any control.
public class PopoverTrigger : ShellTriggerView
{
    protected override void OnActivated() => this.FindParentOfType<Popover>()?.Toggle();
}
"
    };
}
