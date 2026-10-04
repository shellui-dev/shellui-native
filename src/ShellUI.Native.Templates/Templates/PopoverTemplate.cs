using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class PopoverTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "popover",
        DisplayName = "Popover",
        Description = "Floating popover - use with PopoverTrigger and PopoverContent",
        Category = ComponentCategory.Overlay,
        FilePath = "Popover.cs",
        Dependencies = new List<string> { "shell", "popover-trigger", "popover-content" },
        Tags = new List<string> { "overlay", "popover", "floating" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Popover — a panel that floats in the page layer next to its trigger; clicking outside closes it.
//   <ui:Popover>
//       <ui:PopoverTrigger><ui:Button Text=""Info"" Variant=""Outline"" /></ui:PopoverTrigger>
//       <ui:PopoverContent>...</ui:PopoverContent>
//   </ui:Popover>
public partial class Popover : ShellPopoverHost
{
    protected override bool IsContent(Element child) => child is PopoverContent;

    // Kept for compatibility with earlier versions.
    public void ToggleAsync() => Toggle();
    public void CloseAsync() => Close();
}
"
    };
}
