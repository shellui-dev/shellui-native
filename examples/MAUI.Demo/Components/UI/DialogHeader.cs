namespace MAUI.Demo.Components.UI;

[ContentProperty(nameof(Children))]
public partial class DialogHeader : ContentView
{
    private readonly VerticalStackLayout _stack;

    public IList<IView> Children => _stack.Children;

    public DialogHeader()
    {
        _stack = new VerticalStackLayout { Spacing = 4 };
        Content = _stack;
    }
}
