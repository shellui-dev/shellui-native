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
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "overlay", "dropdown", "trigger" }
    };

    public static string Content => @"namespace YourProjectNamespace.Components.UI;

public partial class DropdownTrigger : ContentView
{
    public DropdownTrigger()
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => FindParentOfType<Dropdown>()?.ToggleAsync();
        GestureRecognizers.Add(tap);
    }
}
";
}
