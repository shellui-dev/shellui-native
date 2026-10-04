using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

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
