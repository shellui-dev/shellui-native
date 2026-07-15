using ShellUI.Native.Templates;

namespace ShellUI.Native.Tests;

// Regression: on 2026-07-04 all 18 templates using RoundRectangle were missing
// `using Microsoft.Maui.Controls.Shapes;` in their Content string, producing non-
// compiling code on install. This test locks the invariant so it can't come back.
public class TemplateContentTests
{
    public static IEnumerable<object[]> AllRegisteredComponents()
        => ComponentRegistry.Components.Keys.Select(name => new object[] { name });

    [Theory]
    [MemberData(nameof(AllRegisteredComponents))]
    public void Every_registered_component_has_content(string name)
    {
        var content = ComponentRegistry.GetComponentContent(name);
        Assert.False(string.IsNullOrWhiteSpace(content),
            $"Component '{name}' is registered in the metadata dictionary but " +
            $"GetComponentContent returned null/empty.");
    }

    [Theory]
    [MemberData(nameof(AllRegisteredComponents))]
    public void Template_that_uses_RoundRectangle_imports_Shapes(string name)
    {
        var content = ComponentRegistry.GetComponentContent(name);
        Assert.NotNull(content);

        if (!content!.Contains("RoundRectangle"))
            return; // template doesn't touch RoundRectangle, invariant doesn't apply

        Assert.Contains("using Microsoft.Maui.Controls.Shapes;", content);
    }

    [Theory]
    [MemberData(nameof(AllRegisteredComponents))]
    public void Template_content_has_namespace_placeholder(string name)
    {
        var content = ComponentRegistry.GetComponentContent(name);
        Assert.NotNull(content);
        Assert.Contains("YourProjectNamespace", content!);
    }
}
