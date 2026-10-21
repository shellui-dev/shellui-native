using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class SheetHeaderTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "sheet-header",
        DisplayName = "Sheet Header",
        Description = "Title and description at the top of a sheet",
        Category = ComponentCategory.Overlay,
        FilePath = "SheetHeader.cs",
        Dependencies = new List<string>(),
        Tags = new List<string> { "overlay", "sheet", "header" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Title + description at the top of a sheet — flex flex-col space-y-2.
//   <ui:SheetContent>
//       <ui:SheetHeader>
//           <ui:SheetTitle Text=""Edit profile"" />
//           <ui:SheetDescription Text=""Make changes to your profile here."" />
//       </ui:SheetHeader>
//       ...
//       <ui:SheetFooter><ui:Button Text=""Save changes"" /></ui:SheetFooter>
//   </ui:SheetContent>
[ContentProperty(nameof(Children))]
public partial class SheetHeader : ContentView
{
    private readonly VerticalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public SheetHeader()
    {
        _stack = new VerticalStackLayout { Spacing = 8 };
        Content = _stack;
    }
}
",
        [NativePlatform.Avalonia] = @"using Avalonia.Controls;

namespace YourProjectNamespace.Components.UI;

// Title + description at the top of a sheet — flex flex-col space-y-2.
//   <ui:SheetContent>
//       <ui:SheetHeader>
//           <ui:SheetTitle Text=""Edit profile"" />
//           <ui:SheetDescription Text=""Make changes to your profile here."" />
//       </ui:SheetHeader>
//       ...
//       <ui:SheetFooter><ui:Button Text=""Save changes"" /></ui:SheetFooter>
//   </ui:SheetContent>
public class SheetHeader : StackPanel
{
    public SheetHeader()
    {
        Spacing = 8;
    }
}
"
    };
}
