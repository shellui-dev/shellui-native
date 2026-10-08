using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class PopoverContentTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "popover-content",
        DisplayName = "Popover Content",
        Description = "Popover panel content",
        Category = ComponentCategory.Overlay,
        FilePath = "PopoverContent.cs",
        Dependencies = new List<string> { "shell" },
        Tags = new List<string> { "overlay", "popover", "content" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Popover panel — w-72 rounded-md border bg-popover p-4 shadow-md.
[ContentProperty(nameof(Children))]
public partial class PopoverContent : ContentView
{
    private readonly VerticalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public PopoverContent()
    {
        _stack = new VerticalStackLayout { Spacing = 8 };
        var panel = new Border
        {
            Content = _stack,
            Padding = new Thickness(16),
            WidthRequest = 288,
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

// Popover panel — w-72 rounded-md border bg-popover p-4 shadow-md. XAML children go to Items.
public class PopoverContent : Border
{
    private readonly StackPanel _stack = new() { Spacing = 8 };

    [Content]
    public Controls Items => _stack.Children;

    public PopoverContent()
    {
        Child = _stack;
        Padding = new Thickness(16);
        Width = 288;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        BoxShadow = ShellPopups.PanelShadow();
        this.Token(BackgroundProperty, ShellToken.Popover).Token(BorderBrushProperty, ShellToken.Border);
    }
}
"
    };
}
