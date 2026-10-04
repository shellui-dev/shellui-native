namespace MAUI.Demo.Components.UI;

// Hover card — rich content that floats next to its trigger while the pointer is over either.
// On touch devices (no hover) tapping the trigger toggles it.
//   <ui:HoverCard>
//       <ui:HoverCardTrigger><ui:Button Text="@nextjs" Variant="Link" /></ui:HoverCardTrigger>
//       <ui:HoverCardContent>...</ui:HoverCardContent>
//   </ui:HoverCard>
public partial class HoverCard : ShellPopoverHost
{
    // Milliseconds before the card opens / closes after the pointer enters / leaves.
    public static readonly BindableProperty OpenDelayProperty =
        BindableProperty.Create(nameof(OpenDelay), typeof(int), typeof(HoverCard), 300);

    public static readonly BindableProperty CloseDelayProperty =
        BindableProperty.Create(nameof(CloseDelay), typeof(int), typeof(HoverCard), 200);

    public int OpenDelay
    {
        get => (int)GetValue(OpenDelayProperty);
        set => SetValue(OpenDelayProperty, value);
    }

    public int CloseDelay
    {
        get => (int)GetValue(CloseDelayProperty);
        set => SetValue(CloseDelayProperty, value);
    }

    private int _version;

    // Touch devices have no hover: there the trigger toggles the card on tap, and tapping
    // outside closes it. With a pointer the card follows hover and must not block the page.
    internal static bool IsTouch => DeviceInfo.Idiom != DeviceIdiom.Desktop;

    protected override bool Modal => IsTouch;

    protected override bool IsContent(Element child) => child is HoverCardContent;

    // Called by the trigger and the content as the pointer enters / leaves them.
    internal async void SetHovered(bool hovered)
    {
        var version = ++_version;
        await Task.Delay(hovered ? OpenDelay : CloseDelay);
        if (version == _version) IsOpen = hovered;
    }
}
