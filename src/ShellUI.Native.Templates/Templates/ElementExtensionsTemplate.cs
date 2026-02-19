using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Shared extension for overlay components - FindParentOfType for CascadingParameter-style composition
public static class ElementExtensionsTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "element-extensions",
        DisplayName = "Element Extensions",
        Description = "Shared extension methods for compositional components (FindParentOfType)",
        Category = ComponentCategory.Utility,
        FilePath = "ElementExtensions.cs",
        Dependencies = new List<string>(),
        IsAvailable = false, // Hidden from list; installed as dependency of overlay components
        Tags = new List<string> { "utility", "extensions", "composition" }
    };

    public static string Content => @"namespace YourProjectNamespace.Components.UI;

/// <summary>Extensions for compositional components (Dialog, Drawer, etc.) - find parent in visual tree.</summary>
public static class ElementExtensions
{
    public static T? FindParentOfType<T>(this Element element) where T : Element
    {
        var p = element.Parent;
        while (p != null)
        {
            if (p is T t) return t;
            p = p.Parent;
        }
        return null;
    }
}
";
}
