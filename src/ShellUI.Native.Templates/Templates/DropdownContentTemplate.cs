using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DropdownContentTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "dropdown-content",
        DisplayName = "Dropdown Content",
        Description = "Dropdown menu panel - contains DropdownItems",
        Category = ComponentCategory.Overlay,
        FilePath = "DropdownContent.cs",
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "overlay", "dropdown", "content" }
    };

    public static string Content => @"namespace YourProjectNamespace.Components.UI;

[ContentProperty(nameof(Children))]
public partial class DropdownContent : ContentView
{
    private readonly VerticalStackLayout _stack;

    public IList<IView> Children => _stack.Children;

    public DropdownContent()
    {
        _stack = new VerticalStackLayout { Spacing = 0, MinimumWidthRequest = 180 };
        Content = _stack;
    }
}
";
}
