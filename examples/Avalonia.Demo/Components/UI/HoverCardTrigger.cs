using Avalonia.Input;
using Avalonia.Interactivity;

namespace AvaloniaDemo.Components.UI;

// The control that opens the enclosing HoverCard: on hover with a mouse; touch, pen and the
// keyboard toggle it. Wrap a link, an avatar, any control.
public class HoverCardTrigger : ShellTriggerView
{
    private bool _mouse;

    public HoverCardTrigger()
    {
        // Tunnel, so a child that handles the press (Button) doesn't hide the pointer type.
        AddHandler(PointerPressedEvent, (_, e) => _mouse = e.Pointer.Type == PointerType.Mouse, RoutingStrategies.Tunnel);
        AddHandler(KeyDownEvent, (_, _) => _mouse = false, RoutingStrategies.Tunnel);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (e.Pointer.Type == PointerType.Mouse) this.FindParentOfType<HoverCard>()?.SetHovered(true);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (e.Pointer.Type == PointerType.Mouse) this.FindParentOfType<HoverCard>()?.SetHovered(false);
    }

    // A mouse click acts on the wrapped control only; hover already shows the card.
    protected override void OnActivated()
    {
        if (!_mouse) this.FindParentOfType<HoverCard>()?.Toggle();
    }
}
