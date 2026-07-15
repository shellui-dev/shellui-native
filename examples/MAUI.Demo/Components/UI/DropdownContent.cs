namespace MAUI.Demo.Components.UI;

[ContentProperty(nameof(Children))]
public partial class DropdownContent : ContentView
{
    private readonly VerticalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public DropdownContent()
    {
        _stack = new VerticalStackLayout { Spacing = 0, MinimumWidthRequest = 180 };
        Content = _stack;
    }
}
