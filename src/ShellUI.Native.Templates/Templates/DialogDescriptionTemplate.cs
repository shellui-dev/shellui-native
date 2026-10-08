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
        Dependencies = new List<string> { "shell" },
        Tags = new List<string> { "overlay", "dialog", "description" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// text-sm text-muted-foreground
public partial class DialogDescription : Label
{
    public DialogDescription()
    {
        FontSize = 14;
        this.Token(TextColorProperty, ShellToken.MutedForeground);
    }
}
",
        [NativePlatform.Avalonia] = @"using Avalonia.Controls;
using Avalonia.Media;

namespace YourProjectNamespace.Components.UI;

// text-sm text-muted-foreground
public class DialogDescription : TextBlock
{
    public DialogDescription()
    {
        FontSize = 14;
        TextWrapping = TextWrapping.Wrap;
        this.Token(ForegroundProperty, ShellToken.MutedForeground);
    }
}
"
    };
}
