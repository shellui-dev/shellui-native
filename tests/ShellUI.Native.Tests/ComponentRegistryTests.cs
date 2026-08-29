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
        // 'button' exists and supports MAUI, but no Avalonia template has been added yet.
        // The registry must return null (not throw) so callers can distinguish the failure mode.
        Assert.NotNull(ComponentRegistry.GetComponentContent("button", NativePlatform.MAUI));
        Assert.Null(ComponentRegistry.GetComponentContent("button", NativePlatform.Avalonia));
    }

    [Fact]
    public void SupportsPlatform_reports_MAUI_for_all_components_and_no_others_yet()
    {
        foreach (var name in ComponentRegistry.Components.Keys)
        {
            Assert.True(ComponentRegistry.SupportsPlatform(name, NativePlatform.MAUI),
                $"'{name}' must support MAUI (baseline platform).");

            // Until the Phase 2 branch (feat/avalonia-implementation) lands, no component
            // reports Avalonia/WinUI/WPF support. This test will need updating when
            // Avalonia content is added to templates — that's the intended trigger.
            Assert.False(ComponentRegistry.SupportsPlatform(name, NativePlatform.Avalonia));
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
    public void GetSupportedPlatforms_returns_MAUI_only_for_registered_components()
    {
        var supported = ComponentRegistry.GetSupportedPlatforms("button");
        Assert.Contains(NativePlatform.MAUI, supported);
        Assert.Single(supported); // Until Avalonia lands
    }

    [Fact]
    public void GetSupportedPlatforms_returns_empty_for_unknown_component()
    {
        var supported = ComponentRegistry.GetSupportedPlatforms("nonexistent-xyz");
        Assert.Empty(supported);
    }
}
