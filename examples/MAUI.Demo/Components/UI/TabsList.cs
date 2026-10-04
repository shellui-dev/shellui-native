using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

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
