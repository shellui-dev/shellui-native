namespace MAUI.Demo.Components.UI;

// Actions row — flex justify-end gap-2.
[ContentProperty(nameof(Children))]
public partial class DialogFooter : ContentView
{
    private readonly HorizontalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public DialogFooter()
    {
        _stack = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 8, 0, 0)
        };
        Content = _stack;
    }
}
