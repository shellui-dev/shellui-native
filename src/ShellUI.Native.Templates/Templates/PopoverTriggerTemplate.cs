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
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "overlay", "popover", "trigger" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

public partial class PopoverTrigger : ContentView
{
    public PopoverTrigger()
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => FindParentOfType<Popover>()?.ToggleAsync();
        GestureRecognizers.Add(tap);
    }
}
"
    };
}
