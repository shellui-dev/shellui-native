using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class SheetFooterTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "sheet-footer",
        DisplayName = "Sheet Footer",
        Description = "Row of actions at the end of a sheet",
        Category = ComponentCategory.Overlay,
        FilePath = "SheetFooter.cs",
        Dependencies = new List<string>(),
        Tags = new List<string> { "overlay", "sheet", "footer" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Actions row at the end of a sheet — flex justify-end gap-2.
[ContentProperty(nameof(Children))]
public partial class SheetFooter : ContentView
{
    private readonly HorizontalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public SheetFooter()
    {
        _stack = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 8, 0, 0)
        };
        Content = _stack;
    }
}
",
        [NativePlatform.Avalonia] = @"using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace YourProjectNamespace.Components.UI;

// Actions row at the end of a sheet — flex justify-end gap-2.
public class SheetFooter : StackPanel
{
    public SheetFooter()
    {
        Orientation = Orientation.Horizontal;
        Spacing = 8;
        HorizontalAlignment = HorizontalAlignment.Right;
        Margin = new Thickness(0, 8, 0, 0);
    }
}
"
    };
}
