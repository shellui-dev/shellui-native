using ShellUI.Native.Core.Models;
using ShellUI.Native.Templates;

namespace ShellUI.Native.Tests;

public class ComponentRegistryTests
{
    [Fact]
    public void Every_metadata_name_matches_its_registry_key()
    {
        foreach (var (key, metadata) in ComponentRegistry.Components)
        {
            Assert.Equal(key, metadata.Name);
        }
    }

    [Fact]
    public void Every_declared_dependency_is_itself_registered()
    {
        var registeredNames = ComponentRegistry.Components.Keys.ToHashSet();

        foreach (var (name, metadata) in ComponentRegistry.Components)
        {
            foreach (var dep in metadata.Dependencies)
            {
                Assert.True(registeredNames.Contains(dep),
                    $"Component '{name}' declares dependency '{dep}' but it is not " +
                    $"registered in ComponentRegistry.Components. This will cause the " +
                    $"CLI to fail at install time.");
            }
        }
    }

    // `shell` is the base layer every component builds on; a cycle (e.g. shell <-> element-extensions)
    // would make install order ambiguous.
    [Fact]
    public void Dependency_graph_has_no_cycles()
    {
        var components = ComponentRegistry.Components;
        var state = new Dictionary<string, int>(); // 1 = visiting, 2 = done

        void Visit(string name, List<string> path)
        {
            if (state.TryGetValue(name, out var s))
            {
                Assert.True(s == 2, $"Dependency cycle: {string.Join(" -> ", path)} -> {name}");
                return;
            }
            state[name] = 1;
            path.Add(name);
            foreach (var dep in components[name].Dependencies)
                Visit(dep, path);
            path.RemoveAt(path.Count - 1);
            state[name] = 2;
        }

        foreach (var name in components.Keys)
            Visit(name, new List<string>());
    }

    [Fact]
    public void Exists_is_case_insensitive()
    {
        Assert.True(ComponentRegistry.Exists("button"));
        Assert.True(ComponentRegistry.Exists("BUTTON"));
        Assert.True(ComponentRegistry.Exists("Button"));
        Assert.False(ComponentRegistry.Exists("nonexistent-component-xyz"));
    }

    [Fact]
    public void GetMetadata_returns_null_for_unknown_components()
    {
        Assert.Null(ComponentRegistry.GetMetadata("nonexistent-component-xyz"));
    }

    [Fact]
    public void GetComponentContent_returns_null_for_unknown_component()
    {
        Assert.Null(ComponentRegistry.GetComponentContent("nonexistent-component-xyz", NativePlatform.MAUI));
    }

    [Fact]
    public void GetComponentContent_returns_null_for_known_component_but_unsupported_platform()
    {
        // 'button' exists, but no component has a WinUI template. The registry must return null
        // (not throw) so callers can distinguish the failure mode.
        Assert.NotNull(ComponentRegistry.GetComponentContent("button", NativePlatform.MAUI));
        Assert.Null(ComponentRegistry.GetComponentContent("button", NativePlatform.WinUI));
    }

    [Fact]
    public void SupportsPlatform_reports_MAUI_for_all_components_and_no_WinUI_or_WPF()
    {
        foreach (var name in ComponentRegistry.Components.Keys)
        {
            Assert.True(ComponentRegistry.SupportsPlatform(name, NativePlatform.MAUI),
                $"'{name}' must support MAUI (baseline platform).");

            // Avalonia is being added component by component (see TemplateContentTests);
            // WinUI and WPF have no templates.
            Assert.False(ComponentRegistry.SupportsPlatform(name, NativePlatform.WinUI));
            Assert.False(ComponentRegistry.SupportsPlatform(name, NativePlatform.WPF));
        }
    }

    [Fact]
    public void SupportsPlatform_returns_false_for_unknown_component()
    {
        Assert.False(ComponentRegistry.SupportsPlatform("nonexistent-xyz", NativePlatform.MAUI));
    }

    [Fact]
    public void GetSupportedPlatforms_lists_the_platforms_with_a_template()
    {
        var supported = ComponentRegistry.GetSupportedPlatforms("button");
        Assert.Equal(new[] { NativePlatform.MAUI, NativePlatform.Avalonia }.ToHashSet(), supported);
    }

    [Fact]
    public void GetSupportedPlatforms_returns_empty_for_unknown_component()
    {
        var supported = ComponentRegistry.GetSupportedPlatforms("nonexistent-xyz");
        Assert.Empty(supported);
    }

    [Theory]
    [InlineData("dialog-trigger", "dialog")]
    [InlineData("dialog-close", "dialog")]
    [InlineData("drawer-content", "drawer")]
    [InlineData("sheet-trigger", "sheet")]
    [InlineData("sheet-header", "sheet")]
    [InlineData("sheet-title", "sheet")]
    [InlineData("sheet-description", "sheet")]
    [InlineData("sheet-footer", "sheet")]
    [InlineData("dropdown-item", "dropdown")]
    [InlineData("popover-trigger", "popover")]
    [InlineData("hover-card-trigger", "hover-card")]
    [InlineData("radio-group-item", "radio-group")]
    [InlineData("collapsible-trigger", "collapsible")]
    [InlineData("accordion-item", "accordion")]
    [InlineData("tabs-list", "tabs")]
    [InlineData("card-header", "card")]
    public void GetFamily_maps_a_part_to_its_parent(string part, string family)
    {
        Assert.Equal(family, ComponentRegistry.GetFamily(part));
    }

    // A loading stat card draws skeletons, so `add stat-card` installs skeleton too.
    [Fact]
    public void Stat_card_depends_on_skeleton()
    {
        Assert.Contains("skeleton", ComponentRegistry.Components["stat-card"].Dependencies);
    }

    // InputVariant (Password) is declared with Input and shared by InputGroup.
    [Fact]
    public void Input_group_depends_on_input()
    {
        Assert.Contains("input", ComponentRegistry.Components["input-group"].Dependencies);
    }

    [Fact]
    public void Sheet_parts_have_both_platforms()
    {
        foreach (var part in new[] { "sheet-header", "sheet-title", "sheet-description", "sheet-footer" })
            Assert.Equal(new[] { NativePlatform.MAUI, NativePlatform.Avalonia }, ComponentRegistry.GetSupportedPlatforms(part).OrderBy(p => p));
    }

    [Theory]
    [InlineData("dialog")]
    [InlineData("alert-dialog")]
    [InlineData("toggle-group")]
    [InlineData("tag-input")]
    [InlineData("date-picker")]
    [InlineData("hover-card")]
    [InlineData("nonexistent-xyz")]
    public void GetFamily_is_null_for_standalone_components(string name)
    {
        Assert.Null(ComponentRegistry.GetFamily(name));
    }

    // The installed tool has no Directory.Build.props, so the version must reach it through the assembly.
    [Fact]
    public void Component_version_is_the_package_version_from_Directory_Build_props()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Directory.Build.props")))
            dir = dir.Parent;
        Assert.NotNull(dir);

        var props = File.ReadAllText(Path.Combine(dir!.FullName, "Directory.Build.props"));
        var version = System.Text.RegularExpressions.Regex.Match(props, "<ShellUINativeVersion>([^<]+)<").Groups[1].Value;
        var suffix = System.Text.RegularExpressions.Regex.Match(props, "<ShellUINativeVersionSuffix>([^<]*)<").Groups[1].Value;
        var expected = suffix.Length == 0 ? version : $"{version}-{suffix}";

        Assert.All(ComponentRegistry.Components.Values, m => Assert.Equal(expected, m.Version));
    }

    // The charts are added by the same names as in ShellUI (Blazor). Its chart-series,
    // multi-series-chart, chart-variants and chart-styles are covered by the chart core here.
    [Theory]
    [InlineData("chart")]
    [InlineData("bar-chart")]
    [InlineData("line-chart")]
    [InlineData("area-chart")]
    [InlineData("pie-chart")]
    [InlineData("donut-chart")]
    [InlineData("radar-chart")]
    [InlineData("radial-chart")]
    public void Charts_use_ShellUI_names(string name)
    {
        Assert.True(ComponentRegistry.Exists(name), name);
    }

    [Fact]
    public void Donut_chart_builds_on_the_pie_chart()
    {
        Assert.Contains("pie-chart", ComponentRegistry.GetMetadata("donut-chart")!.Dependencies);
        Assert.Contains("class DonutChart : PieChart", ComponentRegistry.GetComponentContent("donut-chart", NativePlatform.Avalonia)!);
    }

    [Theory]
    [InlineData("buton", "button")]
    [InlineData("chart-bar", "bar-chart")]
    [InlineData("chart-04", "chart")]
    [InlineData("datepicker", "date-picker")]
    public void Suggest_finds_close_names(string name, string expected)
    {
        Assert.Contains(expected, ComponentRegistry.Suggest(name));
    }

    [Fact]
    public void Suggest_returns_at_most_three_and_nothing_for_unrelated_names()
    {
        Assert.True(ComponentRegistry.Suggest("chart").Count <= 3);
        Assert.Empty(ComponentRegistry.Suggest("zzqqxx"));
    }
}
