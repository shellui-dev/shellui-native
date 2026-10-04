namespace MAUI.Demo.Components.UI;

// Title + description stack — flex flex-col space-y-1.5.
[ContentProperty(nameof(Children))]
public partial class DialogHeader : ContentView
{
    private readonly VerticalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public DialogHeader()
    {
        _stack = new VerticalStackLayout { Spacing = 6, Margin = new Thickness(0, 0, 24, 0) };
        Content = _stack;
    }
}
