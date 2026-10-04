using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class HoverCardTriggerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "hover-card-trigger",
        DisplayName = "Hover Card Trigger",
        Description = "View that opens a hover card while hovered",
        Category = ComponentCategory.Overlay,
        FilePath = "HoverCardTrigger.cs",
        Dependencies = new List<string> { "shell", "element-extensions" },
        Tags = new List<string> { "hover", "card", "trigger" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// The view that opens the enclosing HoverCard: on hover with a pointer, on tap on touch devices.
// Wrap a link, an avatar, any view.
public partial class HoverCardTrigger : ShellTriggerView
{
    public HoverCardTrigger()
    {
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => this.FindParentOfType<HoverCard>()?.SetHovered(true);
        pointer.PointerExited += (_, _) => this.FindParentOfType<HoverCard>()?.SetHovered(false);
        GestureRecognizers.Add(pointer);
    }

    protected override void OnActivated()
    {
        if (HoverCard.IsTouch) this.FindParentOfType<HoverCard>()?.Toggle();
    }
}
"
    };
}
