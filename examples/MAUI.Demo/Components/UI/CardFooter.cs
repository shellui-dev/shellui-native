namespace MAUI.Demo.Components.UI;

// Card footer — flex items-center p-6 pt-0; actions right-aligned.
[ContentProperty(nameof(FooterContent))]
public partial class CardFooter : ContentView
{
    private readonly HorizontalStackLayout _content;

    public IList<IView> FooterContent => _content.Children;

    public CardFooter()
    {
        _content = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.End
        };
        Padding = new Thickness(24, 0, 24, 24);
        Content = _content;
    }
}
