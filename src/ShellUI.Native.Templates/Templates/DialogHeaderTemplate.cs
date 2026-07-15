using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DialogHeaderTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "dialog-header",
        DisplayName = "Dialog Header",
        Description = "Header section for dialog content",
        Category = ComponentCategory.Overlay,
        FilePath = "DialogHeader.cs",
        Dependencies = new List<string>(),
        Tags = new List<string> { "overlay", "dialog", "header" }
    };

    public static string Content => @"namespace YourProjectNamespace.Components.UI;

[ContentProperty(nameof(Children))]
public partial class DialogHeader : ContentView
{
    private readonly VerticalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public DialogHeader()
    {
        _stack = new VerticalStackLayout { Spacing = 4 };
        Content = _stack;
    }
}
";
}
