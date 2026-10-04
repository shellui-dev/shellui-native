using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// CollapsibleContent - inline panel that appears/disappears with the parent Collapsible's
// Open state. Fades over 150ms rather than animating height: cross-platform height animation
// in MAUI is unreliable (needs measured child height), fade + visibility toggle is not.
public static class CollapsibleContentTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "collapsible-content",
        DisplayName = "Collapsible Content",
        Description = "Body of a Collapsible - shown when the parent Collapsible is Open. Place inside Collapsible",
        Category = ComponentCategory.Layout,
        FilePath = "CollapsibleContent.cs",
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "layout", "collapsible", "content" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Content shown while the enclosing Collapsible is open; expands/collapses its height.
[ContentProperty(nameof(Content))]
public partial class CollapsibleContent : ContentView
{
    public CollapsibleContent()
    {
        IsVisible = false;
        Opacity = 0;
        IsClippedToBounds = true;
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
