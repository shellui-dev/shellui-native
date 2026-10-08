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

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Title + description stack — flex flex-col space-y-1.5.
[ContentProperty(nameof(Children))]
public partial class DialogHeader : ContentView
{
    private readonly VerticalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public DialogHeader()
    {
        _stack = new VerticalStackLayout { Spacing = 6, Margin = new Thickness(0, 0, 24, 0) };
        Content = _stack;
    }
}
",
        [NativePlatform.Avalonia] = @"using Avalonia;
using Avalonia.Controls;

namespace YourProjectNamespace.Components.UI;

// Title + description stack — flex flex-col space-y-1.5. Leaves room for the close button.
public class DialogHeader : StackPanel
{
    public DialogHeader()
    {
        Spacing = 6;
        Margin = new Thickness(0, 0, 24, 0);
    }
}
"
    };
}
