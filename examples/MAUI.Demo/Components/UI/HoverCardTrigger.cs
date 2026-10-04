namespace MAUI.Demo.Components.UI;

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
