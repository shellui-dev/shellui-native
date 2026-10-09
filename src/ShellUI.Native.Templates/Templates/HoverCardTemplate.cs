using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class HoverCardTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "hover-card",
        DisplayName = "Hover Card",
        Description = "Rich content that floats next to its trigger on hover",
        Category = ComponentCategory.Overlay,
        FilePath = "HoverCard.cs",
        Dependencies = new List<string> { "shell", "hover-card-trigger", "hover-card-content" },
        Tags = new List<string> { "hover", "card", "preview", "popover" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Hover card — rich content that floats next to its trigger while the pointer is over either.
// On touch devices (no hover) tapping the trigger toggles it.
//   <ui:HoverCard>
//       <ui:HoverCardTrigger><ui:Button Text=""@nextjs"" Variant=""Link"" /></ui:HoverCardTrigger>
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
",
        [NativePlatform.Avalonia] = @"using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace YourProjectNamespace.Components.UI;

// Hover card — rich content that floats under its trigger while the pointer is over either.
// Touch, pen and the keyboard (Enter / Space on the trigger) toggle it instead.
//   <ui:HoverCard>
//       <ui:HoverCardTrigger><ui:Button Text=""@nextjs"" Variant=""Link"" /></ui:HoverCardTrigger>
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
        // A click elsewhere closes the card and still reaches what was clicked.
        Popup.OverlayDismissEventPassThrough = true;
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

    // Called by the trigger and the content as the pointer enters / leaves them.
    internal async void SetHovered(bool hovered)
    {
        var version = ++_version;
        await Task.Delay(hovered ? OpenDelay : CloseDelay);
        if (version == _version) IsOpen = hovered;
    }
}
"
    };
}
