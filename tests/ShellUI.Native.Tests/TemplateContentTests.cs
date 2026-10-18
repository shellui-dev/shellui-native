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

    // Every component registered so far. A new MAUI component is added here once it is ported.
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
    [InlineData("textarea")]
    [InlineData("select")]
    [InlineData("slider")]
    [InlineData("radio-group")]
    [InlineData("radio-group-item")]
    [InlineData("calendar")]
    [InlineData("date-picker")]
    [InlineData("time-picker")]
    [InlineData("collapsible")]
    [InlineData("collapsible-trigger")]
    [InlineData("collapsible-content")]
    [InlineData("accordion")]
    [InlineData("accordion-item")]
    [InlineData("accordion-trigger")]
    [InlineData("accordion-content")]
    [InlineData("tabs")]
    [InlineData("tabs-list")]
    [InlineData("tabs-trigger")]
    [InlineData("tabs-content")]
    [InlineData("breadcrumb")]
    [InlineData("breadcrumb-item")]
    [InlineData("skeleton")]
    [InlineData("scroll-area")]
    [InlineData("spinner")]
    [InlineData("tooltip")]
    [InlineData("toast")]
    [InlineData("alert-dialog")]
    [InlineData("hover-card")]
    [InlineData("hover-card-trigger")]
    [InlineData("hover-card-content")]
    [InlineData("avatar")]
    [InlineData("toggle")]
    [InlineData("callout")]
    [InlineData("table")]
    [InlineData("empty-state")]
    [InlineData("pagination")]
    [InlineData("context-menu")]
    [InlineData("carousel")]
    [InlineData("stepper")]
    [InlineData("toggle-group")]
    [InlineData("number-input")]
    [InlineData("tag-input")]
    [InlineData("kbd")]
    [InlineData("stat-card")]
    [InlineData("timeline")]
    [InlineData("multi-select")]
    [InlineData("tree-view")]
    [InlineData("copy-button")]
    [InlineData("link-card")]
    [InlineData("aspect-ratio")]
    [InlineData("wrap-layout")]
    [InlineData("combobox")]
    [InlineData("input-otp")]
    [InlineData("resizable")]
    [InlineData("navbar")]
    [InlineData("sidebar")]
    [InlineData("date-range-picker")]
    [InlineData("file-upload")]
    [InlineData("button-group")]
    [InlineData("input-group")]
    [InlineData("menubar")]
    [InlineData("command")]
    [InlineData("navigation-menu")]
    [InlineData("data-table")]
    [InlineData("chat")]
    [InlineData("form")]
    [InlineData("image-viewer")]
    [InlineData("prev-next-nav")]
    [InlineData("chart")]
    [InlineData("bar-chart")]
    [InlineData("line-chart")]
    [InlineData("area-chart")]
    [InlineData("pie-chart")]
    [InlineData("donut-chart")]
    [InlineData("radar-chart")]
    [InlineData("radial-chart")]
    public void Ported_components_have_Avalonia_content(string name)
    {
        Assert.False(string.IsNullOrWhiteSpace(ComponentRegistry.GetComponentContent(name, NativePlatform.Avalonia)));
    }

    [Theory]
    [MemberData(nameof(AvaloniaComponents))]
    public void Avalonia_template_is_Avalonia_code_with_the_namespace_placeholder(string name)
    {
        var content = ComponentRegistry.GetComponentContent(name, NativePlatform.Avalonia)!;
        // Blocks live next to the components and use them; .Variants for button-variants.
        Assert.Contains(ComponentRegistry.GetMetadata(name)!.Category == ComponentCategory.Block
            ? "namespace YourProjectNamespace.Components.Blocks"
            : "namespace YourProjectNamespace.Components.UI", content);
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

    // ShellLabel passes text alignment and wrapping through (issue #27). Avalonia's ShellLabel is a
    // TextBlock, so TextAlignment and TextWrapping are already its own properties.
    [Fact]
    public void MAUI_label_exposes_text_alignment_and_line_break_mode()
    {
        var content = ComponentRegistry.GetComponentContent("label", NativePlatform.MAUI)!;
        Assert.Contains("HorizontalTextAlignmentProperty", content);
        Assert.Contains("LineBreakModeProperty", content);
        Assert.Contains("class ShellLabel : TextBlock", ComponentRegistry.GetComponentContent("label", NativePlatform.Avalonia)!);
    }

    // Sheets can be wider than shadcn's 384px (issue #26).
    [Theory]
    [InlineData(NativePlatform.MAUI)]
    [InlineData(NativePlatform.Avalonia)]
    public void Sheet_content_has_a_max_panel_width(NativePlatform platform)
    {
        var content = ComponentRegistry.GetComponentContent("sheet-content", platform)!;
        Assert.Contains("MaxPanelWidthProperty", content);
        Assert.Contains("Math.Min(MaxPanelWidth", content);
    }

    // A MAUI Blazor Hybrid app has a <App>.Components.Layout namespace (MainLayout.razor), and a
    // bare `Layout` inside <App>.Components.* resolves to it (CS0118), so MAUI code names the type in full.
    [Theory]
    [MemberData(nameof(AllRegisteredComponents))]
    public void MAUI_templates_name_the_Layout_type_in_full(string name)
    {
        var content = ComponentRegistry.GetComponentContent(name, NativePlatform.MAUI)!;
        Assert.DoesNotMatch(@"(\b(is|as|case|new)\s+|typeof\(|[<(,]\s*)Layout\b(?![.\w])", content);
    }

    // Adding or removing a Shadow on Android re-wraps the native view, which takes focus from the field.
    [Theory]
    [InlineData("input")]
    [InlineData("textarea")]
    [InlineData("input-group")]
    public void MAUI_text_fields_keep_their_focus_shadow(string name)
    {
        var content = ComponentRegistry.GetComponentContent(name, NativePlatform.MAUI)!;
        Assert.DoesNotContain("ClearValue(VisualElement.ShadowProperty)", content);
    }

    // An app with an <App>.Application project (clean architecture) has that namespace, and a bare
    // `Application` inside <App>.App.Components.* resolves to it (CS0234), so templates name the type in full.
    [Theory]
    [MemberData(nameof(AllRegisteredComponents))]
    public void Templates_name_the_Application_type_in_full(string name)
    {
        foreach (var platform in new[] { NativePlatform.MAUI, NativePlatform.Avalonia })
        {
            if (ComponentRegistry.GetComponentContent(name, platform) is { } content)
                Assert.DoesNotMatch(@"(?<![\w.])Application(\.|\?|\s+\w+\s*[=;),])", content);
        }
    }
}
