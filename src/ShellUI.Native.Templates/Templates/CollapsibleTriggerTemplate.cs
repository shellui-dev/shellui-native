using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// CollapsibleTrigger - tap toggles the parent Collapsible's Open state.
// Row-level control: honors the 40px form baseline via MinimumHeightRequest.
public static class CollapsibleTriggerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "collapsible-trigger",
        DisplayName = "Collapsible Trigger",
        Description = "Tap-to-toggle handle for a Collapsible - place inside Collapsible",
        Category = ComponentCategory.Layout,
        FilePath = "CollapsibleTrigger.cs",
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "layout", "collapsible", "trigger" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Tap to toggle. Wrap your visible handle - a Label, an icon, a whole row.
// MinimumHeightRequest = 40 keeps hit target aligned with Input / Select / Button
// (Form Sizing Contract).
public partial class CollapsibleTrigger : ContentView
{
    public CollapsibleTrigger()
    {
        MinimumHeightRequest = 40;
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;
        GestureRecognizers.Add(tap);
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        this.FindParentOfType<Collapsible>()?.Toggle();
    }
}
"
    };
}
