namespace MAUI.Demo.Components.UI;

[ContentProperty(nameof(Children))]
public partial class PopoverContent : ContentView
{
    private readonly VerticalStackLayout _stack;

    public IList<IView> Children => _stack.Children;

    public PopoverContent()
    {
        _stack = new VerticalStackLayout { Spacing = 12 };
        Content = _stack;
    }
}
