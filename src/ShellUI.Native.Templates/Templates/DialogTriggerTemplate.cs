using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DialogTriggerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "dialog-trigger",
        DisplayName = "Dialog Trigger",
        Description = "Triggers dialog open on tap - place inside Dialog",
        Category = ComponentCategory.Overlay,
        FilePath = "DialogTrigger.cs",
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "overlay", "dialog", "trigger" }
    };

    public static string Content => @"namespace YourProjectNamespace.Components.UI;

// Tap to open dialog. Usage: <DialogTrigger><Button Text=""Open"" /></DialogTrigger>
public partial class DialogTrigger : ContentView
{
    public DialogTrigger()
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;
        GestureRecognizers.Add(tap);
        // Ensure taps pass through to child if Content is interactive
        InputTransparent = false;
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        var dialog = this.FindParentOfType<Dialog>();
        dialog?.SetOpen(true);
    }
}
";
}
