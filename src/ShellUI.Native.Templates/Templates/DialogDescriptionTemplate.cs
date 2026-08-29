using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DialogDescriptionTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "dialog-description",
        DisplayName = "Dialog Description",
        Description = "Description text for dialog",
        Category = ComponentCategory.Overlay,
        FilePath = "DialogDescription.cs",
        Dependencies = new List<string>(),
        Tags = new List<string> { "overlay", "dialog", "description" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

public partial class DialogDescription : Label
{
    public DialogDescription()
    {
        FontSize = 14;
        TextColor = Color.FromArgb(""#6B7280"");
    }
}
"
    };
}
