using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DropdownTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "dropdown",
        DisplayName = "Dropdown",
        Description = "Dropdown menu - use with DropdownTrigger, DropdownContent, DropdownItem",
        Category = ComponentCategory.Overlay,
        FilePath = "Dropdown.cs",
        Dependencies = new List<string> { "shell", "dropdown-trigger", "dropdown-content", "dropdown-item" },
        Tags = new List<string> { "overlay", "dropdown", "menu" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Dropdown menu. The content floats in the page layer below the trigger (above it when there
// is no room); clicking outside, picking an item or opening another menu closes it.
//   <ui:Dropdown>
//       <ui:DropdownTrigger><ui:Button Text=""Options"" Variant=""Outline"" /></ui:DropdownTrigger>
//       <ui:DropdownContent>
//           <ui:DropdownItem Text=""Profile"" Icon=""User"" Clicked=""OnProfile"" />
//       </ui:DropdownContent>
//   </ui:Dropdown>
public partial class Dropdown : ShellPopoverHost
{
    protected override bool IsContent(Element child) => child is DropdownContent;

    // Kept for compatibility with earlier versions.
    public void ToggleAsync() => Toggle();
    public void CloseAsync() => Close();
}
",
        [NativePlatform.Avalonia] = @"using Avalonia.Controls;

namespace YourProjectNamespace.Components.UI;

// Dropdown menu. The content floats below the trigger (above it when there is no room);
// clicking outside, picking an item, Escape or opening another menu closes it.
//   <ui:Dropdown>
//       <ui:DropdownTrigger><ui:Button Text=""Options"" Variant=""Outline"" /></ui:DropdownTrigger>
//       <ui:DropdownContent>
//           <ui:DropdownItem Text=""Profile"" Icon=""User"" Clicked=""OnProfile"" />
//       </ui:DropdownContent>
//   </ui:Dropdown>
public class Dropdown : ShellPopoverHost
{
    protected override bool IsContent(Control child) => child is DropdownContent;
}
"
    };
}
