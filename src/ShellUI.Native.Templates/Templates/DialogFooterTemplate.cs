using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DialogFooterTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "dialog-footer",
        DisplayName = "Dialog Footer",
        Description = "Footer section for dialog content (e.g. action buttons)",
        Category = ComponentCategory.Overlay,
        FilePath = "DialogFooter.cs",
        Dependencies = new List<string>(),
        Tags = new List<string> { "overlay", "dialog", "footer" }
    };

    public static string Content => @"namespace YourProjectNamespace.Components.UI;

[ContentProperty(nameof(Children))]
public partial class DialogFooter : ContentView
{
    private readonly HorizontalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public DialogFooter()
    {
        _stack = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.End
        };
        Content = _stack;
    }
}
";
}
