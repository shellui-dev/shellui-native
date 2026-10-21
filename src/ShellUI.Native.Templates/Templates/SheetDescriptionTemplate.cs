using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class SheetDescriptionTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "sheet-description",
        DisplayName = "Sheet Description",
        Description = "Muted description under a sheet's title",
        Category = ComponentCategory.Overlay,
        FilePath = "SheetDescription.cs",
        Dependencies = new List<string> { "shell" },
        Tags = new List<string> { "overlay", "sheet", "description" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// text-sm text-muted-foreground
public partial class SheetDescription : Label
{
    public SheetDescription()
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
public class SheetDescription : TextBlock
{
    public SheetDescription()
    {
        FontSize = 14;
        TextWrapping = TextWrapping.Wrap;
        this.Token(ForegroundProperty, ShellToken.MutedForeground);
    }
}
"
    };
}
