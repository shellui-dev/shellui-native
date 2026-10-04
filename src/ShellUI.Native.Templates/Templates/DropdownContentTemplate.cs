using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DropdownContentTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "dropdown-content",
        DisplayName = "Dropdown Content",
        Description = "Dropdown menu panel - contains DropdownItems",
        Category = ComponentCategory.Overlay,
        FilePath = "DropdownContent.cs",
        Dependencies = new List<string> { "shell" },
        Tags = new List<string> { "overlay", "dropdown", "content" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Menu panel — min-w-[8rem] rounded-md border bg-popover p-1 shadow-md.
[ContentProperty(nameof(Children))]
public partial class DropdownContent : ContentView
{
    private readonly VerticalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public DropdownContent()
    {
        _stack = new VerticalStackLayout { Spacing = 0 };
        var panel = new Border
        {
            Content = _stack,
            Padding = new Thickness(4),
            MinimumWidthRequest = 180,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            Shadow = ShellPopups.PanelShadow()
        };
        panel.Token(VisualElement.BackgroundColorProperty, ShellToken.Popover);
        panel.Token(Border.StrokeProperty, ShellToken.Border);
        Content = panel;
    }
}
"
    };
}
