using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class AccordionContentTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "accordion-content",
        DisplayName = "Accordion Content",
        Description = "Body shown when the enclosing AccordionItem is open",
        Category = ComponentCategory.Layout,
        FilePath = "AccordionContent.cs",
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "layout", "accordion", "content" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Section body — pb-4 text-sm; expands/collapses its height with the item.
[ContentProperty(nameof(Content))]
public partial class AccordionContent : ContentView
{
    public AccordionContent()
    {
        IsVisible = false;
        Opacity = 0;
        IsClippedToBounds = true;
        Padding = new Thickness(0, 0, 0, 16);
    }

    internal void Apply(bool open, bool animate)
    {
        if (animate)
        {
            _ = this.AnimateExpandAsync(open);
            return;
        }
        this.AbortAnimation(""ShellExpand"");
        IsVisible = open;
        Opacity = open ? 1 : 0;
        HeightRequest = -1;
    }
}
"
    };
}
