using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class HoverCardContentTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "hover-card-content",
        DisplayName = "Hover Card Content",
        Description = "Floating panel of a hover card",
        Category = ComponentCategory.Overlay,
        FilePath = "HoverCardContent.cs",
        Dependencies = new List<string> { "shell", "element-extensions" },
        Tags = new List<string> { "hover", "card", "content" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Hover card panel — w-64 rounded-md border bg-popover p-4 shadow-md. Stays open while hovered.
[ContentProperty(nameof(Children))]
public partial class HoverCardContent : ContentView
{
    private readonly VerticalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public HoverCardContent()
    {
        _stack = new VerticalStackLayout { Spacing = 8 };
        var panel = new Border
        {
            Content = _stack,
            Padding = new Thickness(16),
            WidthRequest = 256,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            Shadow = ShellPopups.PanelShadow()
        };
        panel.Token(VisualElement.BackgroundColorProperty, ShellToken.Popover);
        panel.Token(Border.StrokeProperty, ShellToken.Border);
        Content = panel;

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => this.FindParentOfType<HoverCard>()?.SetHovered(true);
        pointer.PointerExited += (_, _) => this.FindParentOfType<HoverCard>()?.SetHovered(false);
        GestureRecognizers.Add(pointer);
    }
}
"
    };
}
