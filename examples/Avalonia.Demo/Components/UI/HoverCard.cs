using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace AvaloniaDemo.Components.UI;

// Hover card — rich content that floats under its trigger while the pointer is over either.
// Touch, pen and the keyboard (Enter / Space on the trigger) toggle it instead.
//   <ui:HoverCard>
//       <ui:HoverCardTrigger><ui:Button Text="@nextjs" Variant="Link" /></ui:HoverCardTrigger>
//       <ui:HoverCardContent>...</ui:HoverCardContent>
//   </ui:HoverCard>
public class HoverCard : ShellPopoverHost
{
    // Milliseconds before the card opens / closes after the pointer enters / leaves.
    public static readonly StyledProperty<int> OpenDelayProperty =
        AvaloniaProperty.Register<HoverCard, int>(nameof(OpenDelay), 300);

    public static readonly StyledProperty<int> CloseDelayProperty =
        AvaloniaProperty.Register<HoverCard, int>(nameof(CloseDelay), 200);

    private int _version;

    public HoverCard()
    {
        // Light dismiss lays a window-wide layer under the card, which takes the pointer off the
        // trigger and closes a card opened by hover; leaving both closes it instead.
        Popup.IsLightDismissEnabled = false;
        // When a tap did open it, a click elsewhere closes it and still reaches what was clicked.
        Popup.OverlayDismissEventPassThrough = true;
        IsOpenChanged += (_, open) =>
        {
            if (!open) Popup.IsLightDismissEnabled = false;
        };
    }

    public int OpenDelay
    {
        get => GetValue(OpenDelayProperty);
        set => SetValue(OpenDelayProperty, value);
    }

    public int CloseDelay
    {
        get => GetValue(CloseDelayProperty);
        set => SetValue(CloseDelayProperty, value);
    }

    protected override bool IsContent(Control child) => child is HoverCardContent;

    // Touch, pen or keyboard: with no hover to leave, a tap outside closes it.
    internal void ToggleFromTap()
    {
        Popup.IsLightDismissEnabled = !IsOpen;
        Toggle();
    }

    // Called by the trigger and the content as the pointer enters / leaves them.
    internal async void SetHovered(bool hovered)
    {
        var version = ++_version;
        await Task.Delay(hovered ? OpenDelay : CloseDelay);
        if (version == _version) IsOpen = hovered;
    }
}
