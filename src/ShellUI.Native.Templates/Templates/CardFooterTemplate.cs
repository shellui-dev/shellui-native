using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// CardFooter component template
public static class CardFooterTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "card-footer",
        DisplayName = "Card Footer",
        Description = "Footer section for Card component, typically used for actions",
        Category = ComponentCategory.Layout,
        FilePath = "CardFooter.cs",
        Dependencies = new List<string>(),
        Variants = new List<string>(),
        Tags = new List<string> { "layout", "card", "footer", "actions" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Card footer — flex items-center p-6 pt-0; actions right-aligned.
[ContentProperty(nameof(FooterContent))]
public partial class CardFooter : ContentView
{
    private readonly HorizontalStackLayout _content;

    public IList<IView> FooterContent => _content.Children;

    public CardFooter()
    {
        _content = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.End
        };
        Padding = new Thickness(24, 0, 24, 24);
        Content = _content;
    }
}
",
        [NativePlatform.Avalonia] = @"using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace YourProjectNamespace.Components.UI;

// Card footer — flex items-center p-6 pt-0; actions right-aligned.
public class CardFooter : StackPanel
{
    public CardFooter()
    {
        Orientation = Orientation.Horizontal;
        Spacing = 8;
        HorizontalAlignment = HorizontalAlignment.Right;
        Margin = new Thickness(24, 0, 24, 24);
    }
}
"
    };
}
