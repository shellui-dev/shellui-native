using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// ScrollArea - thin ScrollView subclass. Exists so consumers get a consistent named
// component and future scrollbar-styling / fade-edge work has a hook. Deliberately
// minimal at v1 — no visual overhead beyond MAUI's native ScrollView.
public static class ScrollAreaTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "scroll-area",
        DisplayName = "Scroll Area",
        Description = "ScrollView with consistent defaults. Thin wrapper at v1 - hook for future scrollbar styling",
        Category = ComponentCategory.Layout,
        FilePath = "ScrollArea.cs",
        Dependencies = new List<string>(),
        Variants = new List<string> { "vertical", "horizontal", "both" },
        Tags = new List<string> { "layout", "scroll", "container" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Scrollable region with the platform's scrollbar. For the shadcn look, wrap it in a Border
// with a 1px stroke and 6px corners: <Border ...><ui:ScrollArea HeightRequest=""200"">...</ui:ScrollArea></Border>
public partial class ScrollArea : ScrollView
{
    public ScrollArea()
    {
        Orientation = ScrollOrientation.Vertical;
        VerticalScrollBarVisibility = ScrollBarVisibility.Default;
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace YourProjectNamespace.Components.UI;

// Scrollable region with Fluent's thin scrollbar, which widens on hover and hides when idle.
// For the shadcn look, wrap it in a 1px border with 6px corners:
//   <Border BorderThickness=""1"" CornerRadius=""6""><ui:ScrollArea Height=""200"">...</ui:ScrollArea></Border>
public class ScrollArea : ScrollViewer
{
    // Keeps ScrollViewer's theme; a subclass would otherwise have none.
    protected override Type StyleKeyOverride => typeof(ScrollViewer);

    public ScrollArea()
    {
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
    }
}
"
    };
}
