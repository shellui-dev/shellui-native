using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// CardContent component template
public static class CardContentTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "card-content",
        DisplayName = "Card Content",
        Description = "Main content section for Card component",
        Category = ComponentCategory.Layout,
        FilePath = "CardContent.cs",
        Dependencies = new List<string>(),
        Variants = new List<string>(),
        Tags = new List<string> { "layout", "card", "content" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Card body — p-6 pt-0.
public partial class CardContent : ContentView
{
    public CardContent()
    {
        Padding = new Thickness(24, 0, 24, 24);
    }
}
",
        [NativePlatform.Avalonia] = @"using Avalonia;
using Avalonia.Controls;

namespace YourProjectNamespace.Components.UI;

// Card body — p-6 pt-0.
public class CardContent : Border
{
    public CardContent() => Padding = new Thickness(24, 0, 24, 24);
}
"
    };
}
