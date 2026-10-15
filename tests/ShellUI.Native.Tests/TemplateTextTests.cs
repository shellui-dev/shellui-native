using ShellUI.Native.CLI.Services;
using ShellUI.Native.Core.Models;
using ShellUI.Native.Templates;

namespace ShellUI.Native.Tests;

// The components namespace follows the components folder, so ShellUI Native can sit next to
// ShellUI (Blazor) in one hybrid project without two classes named <App>.Components.UI.Button (issue #28).
public class TemplateTextTests
{
    [Theory]
    [InlineData("Components/UI", "MyApp.Components.UI")]
    [InlineData("Components/Native", "MyApp.Components.Native")]
    [InlineData("Components\\Native\\", "MyApp.Components.Native")]
    [InlineData("UI", "MyApp.UI")]
    [InlineData("Components/native-ui", "MyApp.Components.native_ui")]
    [InlineData("", "MyApp")]
    public void Components_namespace_follows_the_folder(string folder, string expected)
    {
        Assert.Equal(expected, TemplateText.ComponentsNamespace("MyApp", folder));
    }

    [Fact]
    public void Default_folder_gives_the_same_code_as_before()
    {
        var content = ComponentRegistry.GetComponentContent("button", NativePlatform.MAUI)!;
        Assert.Equal(content.Replace("YourProjectNamespace", "MyApp"),
            TemplateText.Apply(content, "MyApp", TemplateText.DefaultComponentsPath));
    }

    [Fact]
    public void Another_folder_moves_the_namespace_and_its_variants()
    {
        var button = TemplateText.Apply(ComponentRegistry.GetComponentContent("button", NativePlatform.MAUI)!,
            "MyApp", TemplateText.BesideBlazorComponentsPath);
        var variants = TemplateText.Apply(ComponentRegistry.GetComponentContent("button-variants", NativePlatform.MAUI)!,
            "MyApp", TemplateText.BesideBlazorComponentsPath);

        Assert.Contains("namespace MyApp.Components.Native;", button);
        Assert.Contains("namespace MyApp.Components.Native.Variants;", variants);
        Assert.DoesNotContain("Components.UI", button + variants);
        Assert.DoesNotContain("YourProjectNamespace", button + variants);
    }
}
