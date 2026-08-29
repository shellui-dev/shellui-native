using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class AccordionTriggerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "accordion-trigger",
        DisplayName = "Accordion Trigger",
        Description = "Tap-to-toggle handle for the enclosing AccordionItem",
        Category = ComponentCategory.Layout,
        FilePath = "AccordionTrigger.cs",
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "layout", "accordion", "trigger" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

public partial class AccordionTrigger : ContentView
{
    public AccordionTrigger()
    {
        MinimumHeightRequest = 40;
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => this.FindParentOfType<AccordionItem>()?.Toggle();
        GestureRecognizers.Add(tap);
    }
}
"
    };
}
