using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class SheetTitleTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "sheet-title",
        DisplayName = "Sheet Title",
        Description = "Title text of a sheet",
        Category = ComponentCategory.Overlay,
        FilePath = "SheetTitle.cs",
        Dependencies = new List<string> { "shell" },
        Tags = new List<string> { "overlay", "sheet", "title" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// text-lg font-semibold text-foreground
public partial class SheetTitle : Label
{
    public SheetTitle()
    {
        FontSize = 18;
        FontAttributes = FontAttributes.Bold;
        this.Token(TextColorProperty, ShellToken.Foreground);
    }
}
",
        [NativePlatform.Avalonia] = @"using Avalonia.Controls;
using Avalonia.Media;

namespace YourProjectNamespace.Components.UI;

// text-lg font-semibold text-foreground
public class SheetTitle : TextBlock
{
    public SheetTitle()
    {
        FontSize = 18;
        FontWeight = FontWeight.SemiBold;
        TextWrapping = TextWrapping.Wrap;
        this.Token(ForegroundProperty, ShellToken.Foreground);
    }
}
"
    };
}
