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
    public void GetComponentContent_returns_null_for_unknown_components()
    {
        Assert.Null(ComponentRegistry.GetComponentContent("nonexistent-component-xyz"));
    }
}
