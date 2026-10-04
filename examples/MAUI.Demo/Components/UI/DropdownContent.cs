using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

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
