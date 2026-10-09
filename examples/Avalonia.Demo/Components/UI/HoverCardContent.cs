using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Hover card panel — w-64 rounded-md border bg-popover p-4 shadow-md. Stays open while hovered.
// XAML children go to Items.
public class HoverCardContent : Border
{
    private readonly StackPanel _stack = new() { Spacing = 8 };

    [Content]
    public Controls Items => _stack.Children;

    public HoverCardContent()
    {
        Child = _stack;
        Width = 256;
        Padding = new Thickness(16);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        BoxShadow = ShellPopups.PanelShadow();
        this.Token(BackgroundProperty, ShellToken.Popover).Token(BorderBrushProperty, ShellToken.Border);
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
}
