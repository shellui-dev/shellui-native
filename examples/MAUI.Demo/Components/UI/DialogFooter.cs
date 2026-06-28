namespace MAUI.Demo.Components.UI;

[ContentProperty(nameof(Children))]
public partial class DialogFooter : ContentView
{
    private readonly HorizontalStackLayout _stack;

    public IList<IView> Children => _stack.Children;

    public DialogFooter()
    {
        _stack = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.End
        };
        Content = _stack;
    }
}
