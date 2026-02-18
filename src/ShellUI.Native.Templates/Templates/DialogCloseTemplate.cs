using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DialogCloseTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "dialog-close",
        DisplayName = "Dialog Close",
        Description = "Closes dialog on tap - use as button or icon",
        Category = ComponentCategory.Overlay,
        FilePath = "DialogClose.cs",
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "overlay", "dialog", "close" }
    };

    public static string Content => @"namespace YourProjectNamespace.Components.UI;

public partial class DialogClose : ContentView
{
    public DialogClose()
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => FindParentOfType<Dialog>()?.SetOpen(false);
        GestureRecognizers.Add(tap);
    }
}
";
}
