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
",
        [NativePlatform.Avalonia] = @"using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace YourProjectNamespace.Components.UI;

// Menu panel — min-w-[8rem] rounded-md border bg-popover p-1 shadow-md. XAML children go to Items.
public class DropdownContent : Border
{
    private readonly StackPanel _stack = new();

    [Content]
    public Controls Items => _stack.Children;

    public DropdownContent()
    {
        Child = _stack;
        Padding = new Thickness(4);
        MinWidth = 180;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        BoxShadow = ShellPopups.PanelShadow();
        this.Token(BackgroundProperty, ShellToken.Popover).Token(BorderBrushProperty, ShellToken.Border);
    }
}
"
    };
}
