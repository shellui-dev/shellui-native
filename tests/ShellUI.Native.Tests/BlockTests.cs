using ShellUI.Native.CLI.Services;
using ShellUI.Native.Core.Models;
using ShellUI.Native.Templates;

namespace ShellUI.Native.Tests;

// Blocks are numbered presets built from components (bottom-nav-01, like ShellUI's dashboard-01).
// They install next to the components, in Components/Blocks, as <App>.Components.Blocks, and use
// the components from wherever the project keeps them.
public class BlockTests
{
    public static IEnumerable<object[]> Blocks() =>
        ComponentRegistry.Components.Values
            .Where(m => m.Category == ComponentCategory.Block)
            .Select(m => new object[] { m.Name });

    [Fact]
    public void The_bottom_nav_and_edge_menu_blocks_are_registered()
    {
        var names = Blocks().Select(b => (string)b[0]).ToList();
        Assert.Equal(new[] { "bottom-nav-01", "bottom-nav-02", "bottom-nav-03", "bottom-nav-04", "bottom-nav-05", "edge-menu-01" },
            names.OrderBy(n => n, StringComparer.Ordinal));
    }

    [Theory]
    [MemberData(nameof(Blocks))]
    public void A_block_installs_to_Components_Blocks_on_both_platforms(string name)
    {
        var meta = ComponentRegistry.GetMetadata(name)!;
        Assert.StartsWith("../Blocks/", meta.FilePath);
        Assert.Contains(name.StartsWith("edge-menu") ? "edge-menu" : "bottom-nav", meta.Dependencies);
        foreach (var platform in new[] { NativePlatform.MAUI, NativePlatform.Avalonia })
        {
            var content = ComponentRegistry.GetComponentContent(name, platform);
            Assert.NotNull(content);
            Assert.Contains("namespace YourProjectNamespace.Components.Blocks;", content);
            Assert.Contains("using YourProjectNamespace.Components.UI;", content);
        }
    }

    [Theory]
    [InlineData("Components/UI", "MyApp.Components.UI")]
    [InlineData("Components/Native", "MyApp.Components.Native")]
    public void A_block_uses_the_components_from_the_projects_components_folder(string componentsPath, string components)
    {
        var content = ComponentRegistry.GetComponentContent("bottom-nav-01", NativePlatform.MAUI)!;
        var code = TemplateText.Apply(content, "MyApp", componentsPath);

        Assert.Contains("namespace MyApp.Components.Blocks;", code);
        Assert.Contains($"using {components};", code);
        Assert.Equal(Path.GetFullPath(Path.Combine("app", "Components", "Blocks", "BottomNav01.cs")),
            Path.GetFullPath(Path.Combine("app", componentsPath, ComponentRegistry.GetMetadata("bottom-nav-01")!.FilePath)));
    }
}
