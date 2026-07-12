using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class PopoverContentTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "popover-content",
        DisplayName = "Popover Content",
        Description = "Popover panel content",
        Category = ComponentCategory.Overlay,
        FilePath = "PopoverContent.cs",
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "overlay", "popover", "content" }
    };

    public static string Content => @"namespace YourProjectNamespace.Components.UI;

[ContentProperty(nameof(Children))]
public partial class PopoverContent : ContentView
{
    private readonly VerticalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public PopoverContent()
    {
        _stack = new VerticalStackLayout { Spacing = 12 };
        Content = _stack;
    }
}
";
}
