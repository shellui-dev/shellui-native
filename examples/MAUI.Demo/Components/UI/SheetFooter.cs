namespace MAUI.Demo.Components.UI;

// Actions row at the end of a sheet — flex justify-end gap-2.
[ContentProperty(nameof(Children))]
public partial class SheetFooter : ContentView
{
    private readonly HorizontalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public SheetFooter()
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
