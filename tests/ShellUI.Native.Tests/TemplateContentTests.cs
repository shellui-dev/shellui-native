using ShellUI.Native.Core.Models;
using ShellUI.Native.Templates;

namespace ShellUI.Native.Tests;

// Regression: on 2026-07-04 all 18 templates using RoundRectangle were missing
// `using Microsoft.Maui.Controls.Shapes;` in their Content string, producing non-
// compiling code on install. This test locks the invariant so it can't come back.
//
// Post Template System v2 (2026-07-18): content is keyed by (name, NativePlatform).
// The MAUI baseline invariants are unchanged — every component must have MAUI content
// today. Avalonia support is opt-in per-template and NOT asserted here.
public class TemplateContentTests
{
    public static IEnumerable<object[]> AllRegisteredComponents()
        => ComponentRegistry.Components.Keys.Select(name => new object[] { name });

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
