using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class SheetTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "sheet",
        DisplayName = "Sheet",
        Description = "Bottom/top sheet overlay - use with SheetTrigger and SheetContent",
        Category = ComponentCategory.Overlay,
        FilePath = "Sheet.cs",
        Dependencies = new List<string> { "shell", "sheet-trigger", "sheet-content" },
        Variants = new List<string> { "bottom", "top", "left", "right" },
        Tags = new List<string> { "overlay", "sheet", "panel" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Side sheet (defaults to right). Place it where it can fill the page:
//   <ui:Sheet x:Name=""Settings""><ui:SheetContent>...</ui:SheetContent></ui:Sheet>
public partial class Sheet : ShellOverlayHost
{
    public static readonly BindableProperty SideProperty =
        BindableProperty.Create(nameof(Side), typeof(SheetSide), typeof(Sheet), SheetSide.Right);

    public SheetSide Side
    {
        get => (SheetSide)GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    protected override bool IsTrigger(Element child) => child is SheetTrigger;
}

public enum SheetSide { Left, Right, Top, Bottom }
"
    };
}
