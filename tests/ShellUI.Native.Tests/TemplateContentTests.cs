using ShellUI.Native.Core.Models;
using ShellUI.Native.Templates;

namespace ShellUI.Native.Tests;

// Regression: on 2026-07-04 all 18 templates using RoundRectangle were missing
// `using Microsoft.Maui.Controls.Shapes;` in their Content string, producing non-
// compiling code on install. This test locks the invariant so it can't come back.
//
// Post Template System v2 (2026-07-18): content is keyed by (name, NativePlatform).
// The MAUI baseline invariants are unchanged — every component must have MAUI content
// today. Avalonia is added per template; its invariants are checked for templates that have it.
public class TemplateContentTests
{
    public static IEnumerable<object[]> AllRegisteredComponents()
        => ComponentRegistry.Components.Keys.Select(name => new object[] { name });

    public static IEnumerable<object[]> AvaloniaComponents()
        => ComponentRegistry.Components.Keys
            .Where(name => ComponentRegistry.SupportsPlatform(name, NativePlatform.Avalonia))
            .Select(name => new object[] { name });

    // Ported so far: the Phase 2 foundation, P0 and P1. Grows as components are ported.
    [Theory]
    [InlineData("shell")]
    [InlineData("icon")]
    [InlineData("theme-toggle")]
    [InlineData("button")]
    [InlineData("button-variants")]
    [InlineData("input")]
    [InlineData("label")]
    [InlineData("checkbox")]
    [InlineData("switch")]
    [InlineData("card")]
    [InlineData("card-header")]
    [InlineData("card-content")]
    [InlineData("card-footer")]
    [InlineData("separator")]
    [InlineData("badge")]
    [InlineData("progress")]
    [InlineData("alert")]
    [InlineData("element-extensions")]
    [InlineData("dialog")]
    [InlineData("dialog-trigger")]
    [InlineData("dialog-content")]
    [InlineData("dialog-header")]
    [InlineData("dialog-footer")]
    [InlineData("dialog-title")]
    [InlineData("dialog-description")]
    [InlineData("dialog-close")]
    [InlineData("drawer")]
    [InlineData("drawer-trigger")]
    [InlineData("drawer-content")]
    [InlineData("sheet")]
    [InlineData("sheet-trigger")]
    [InlineData("sheet-content")]
    [InlineData("dropdown")]
    [InlineData("dropdown-trigger")]
    [InlineData("dropdown-content")]
    [InlineData("dropdown-item")]
    [InlineData("popover")]
    [InlineData("popover-trigger")]
    [InlineData("popover-content")]
    public void Ported_components_have_Avalonia_content(string name)
    {
        Assert.False(string.IsNullOrWhiteSpace(ComponentRegistry.GetComponentContent(name, NativePlatform.Avalonia)));
    }

    [Theory]
    [MemberData(nameof(AvaloniaComponents))]
    public void Avalonia_template_is_Avalonia_code_with_the_namespace_placeholder(string name)
    {
        var content = ComponentRegistry.GetComponentContent(name, NativePlatform.Avalonia)!;
        Assert.Contains("namespace YourProjectNamespace.Components.UI", content); // .Variants for button-variants
        // Small parts (Dialog, triggers) need no Avalonia using, so check for MAUI types instead.
        Assert.DoesNotContain("Microsoft.Maui", content);
        Assert.DoesNotContain("BindableProperty", content);
        Assert.DoesNotContain("ContentView", content);
        Assert.DoesNotContain("AvaloniaDemo", content);
    }

    // An Avalonia install must never pull in a dependency that only has MAUI code.
    [Theory]
    [MemberData(nameof(AvaloniaComponents))]
    public void Avalonia_template_dependencies_have_Avalonia_content(string name)
    {
        foreach (var dependency in ComponentRegistry.GetMetadata(name)!.Dependencies)
            Assert.True(ComponentRegistry.SupportsPlatform(dependency, NativePlatform.Avalonia),
                $"'{name}' has Avalonia content but its dependency '{dependency}' does not.");
    }

    [Theory]
    [MemberData(nameof(AvaloniaComponents))]
    public void Avalonia_components_use_theme_tokens_not_hardcoded_colors(string name)
    {
        if (name == "shell") return; // defines the palettes

        var content = ComponentRegistry.GetComponentContent(name, NativePlatform.Avalonia)!;
        Assert.DoesNotContain("Color.Parse", content);
        Assert.DoesNotContain("Color.FromRgb", content);
        Assert.DoesNotContain("Color.FromArgb", content);
    }

    [Theory]
    [MemberData(nameof(AllRegisteredComponents))]
    public void Every_registered_component_has_MAUI_content(string name)
    {
        var content = ComponentRegistry.GetComponentContent(name, NativePlatform.MAUI);
        Assert.False(string.IsNullOrWhiteSpace(content),
            $"Component '{name}' is registered but GetComponentContent(name, MAUI) returned " +
            $"null/empty. Every component must ship a MAUI template until Phase 2.");
    }

    [Theory]
    [MemberData(nameof(AllRegisteredComponents))]
    public void MAUI_template_that_uses_RoundRectangle_imports_Shapes(string name)
    {
        var content = ComponentRegistry.GetComponentContent(name, NativePlatform.MAUI);
        Assert.NotNull(content);

        if (!content!.Contains("RoundRectangle"))
            return; // template doesn't touch RoundRectangle, invariant doesn't apply

        Assert.Contains("using Microsoft.Maui.Controls.Shapes;", content);
    }

    [Theory]
    [MemberData(nameof(AllRegisteredComponents))]
    public void MAUI_template_content_has_namespace_placeholder(string name)
    {
        var content = ComponentRegistry.GetComponentContent(name, NativePlatform.MAUI);
        Assert.NotNull(content);
        Assert.Contains("YourProjectNamespace", content!);
    }

    // Colors come from the theme tokens published by the `shell` template (ShellTheme), so
    // light/dark mode and user overrides reach every component. A hardcoded hex in a component
    // is exactly the bug that left dialogs white-on-white in dark mode.
    [Theory]
    [MemberData(nameof(AllRegisteredComponents))]
    public void MAUI_components_use_theme_tokens_not_hardcoded_colors(string name)
    {
        if (name == "shell") return; // defines the palettes

        var content = ComponentRegistry.GetComponentContent(name, NativePlatform.MAUI);
        Assert.NotNull(content);
        Assert.DoesNotContain("Color.FromArgb", content!);
    }

    [Theory]
    [MemberData(nameof(AllRegisteredComponents))]
    public void Every_component_reports_MAUI_as_a_supported_platform(string name)
    {
        Assert.True(
            ComponentRegistry.SupportsPlatform(name, NativePlatform.MAUI),
            $"Component '{name}' does not report MAUI as a supported platform. " +
            $"GetSupportedPlatforms returned: [{string.Join(", ", ComponentRegistry.GetSupportedPlatforms(name))}]");
    }
}
