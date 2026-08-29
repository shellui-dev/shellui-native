using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// TabsList - horizontal container for TabsTrigger buttons. Thin wrapper; exists so the
// XAML reads naturally and future variants (vertical tabs, pill style) have a hook.
public static class TabsListTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "tabs-list",
        DisplayName = "Tabs List",
        Description = "Horizontal row of TabsTriggers - place inside Tabs",
        Category = ComponentCategory.Navigation,
        FilePath = "TabsList.cs",
        Dependencies = new List<string>(),
        Tags = new List<string> { "navigation", "tabs", "list" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

public partial class TabsList : HorizontalStackLayout
{
    public TabsList()
    {
        Spacing = 0;
    }
}
"
    };
}
