using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DialogTitleTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "dialog-title",
        DisplayName = "Dialog Title",
        Description = "Title text for dialog",
        Category = ComponentCategory.Overlay,
        FilePath = "DialogTitle.cs",
        Dependencies = new List<string>(),
        Tags = new List<string> { "overlay", "dialog", "title" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

public partial class DialogTitle : Label
{
    public DialogTitle()
    {
        FontSize = 18;
        FontAttributes = FontAttributes.Bold;
    }
}
"
    };
}
