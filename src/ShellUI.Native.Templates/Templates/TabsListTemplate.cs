using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// TabsList - horizontal container for TabsTrigger buttons. Thin wrapper; exists so the
// XAML reads naturally and future variants (vertical tabs, pill style) have a hook.
public static class TabsListTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "tabs-list",
        DisplayName = "Tabs List",
        Description = "Horizontal row of TabsTriggers - place inside Tabs",
        Category = ComponentCategory.Navigation,
        FilePath = "TabsList.cs",
        Dependencies = new List<string> { "shell" },
        Tags = new List<string> { "navigation", "tabs", "list" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Tab strip — inline-flex h-10 rounded-lg bg-muted p-[3px].
[ContentProperty(nameof(Children))]
public partial class TabsList : ContentView
{
    private readonly HorizontalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public TabsList()
    {
        _stack = new HorizontalStackLayout { Spacing = 0 };
        var border = new Border
        {
            Content = _stack,
            HeightRequest = 40,
            Padding = new Thickness(4),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusLg }
        };
        border.Token(VisualElement.BackgroundColorProperty, ShellToken.Muted);
        Content = border;
        HorizontalOptions = LayoutOptions.Start;
    }
}
",
        [NativePlatform.Avalonia] = @"using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace YourProjectNamespace.Components.UI;

// Tab strip — inline-flex h-10 rounded-lg bg-muted p-1.
public class TabsList : Border
{
    private readonly StackPanel _stack = new() { Orientation = Orientation.Horizontal };

    [Content]
    public Controls Items => _stack.Children;

    public TabsList()
    {
        Child = _stack;
        Height = 40;
        Padding = new Thickness(4);
        CornerRadius = new CornerRadius(ShellTheme.RadiusLg);
        HorizontalAlignment = HorizontalAlignment.Left;
        this.Token(BackgroundProperty, ShellToken.Muted);
    }
}
"
    };
}
