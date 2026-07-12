using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Modal content overlay - backdrop + centered box. Usage: <DialogContent><DialogHeader />...</DialogContent>
[ContentProperty(nameof(Children))]
public partial class DialogContent : ContentView
{
    private readonly Grid _root;
    private readonly Border _backdrop;
    private readonly Border _modalBox;
    private readonly VerticalStackLayout _body;

    public new IList<IView> Children => _body.Children;

    public DialogContent()
    {
        _backdrop = new Border
        {
            BackgroundColor = Color.FromArgb("#80000000"),
            StrokeThickness = 0,
            ZIndex = 0
        };
        var tapBackdrop = new TapGestureRecognizer();
        tapBackdrop.Tapped += (s, e) => this.FindParentOfType<Dialog>()?.SetOpen(false);
        _backdrop.GestureRecognizers.Add(tapBackdrop);

        _body = new VerticalStackLayout { Spacing = 16 };
        _modalBox = new Border
        {
            Content = _body,
            BackgroundColor = Color.FromArgb("#FFFFFF"),
            Stroke = Color.FromArgb("#E5E7EB"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Padding = new Thickness(24),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            MaximumWidthRequest = 400,
            ZIndex = 1
        };

        _root = new Grid
        {
            Children = { _backdrop, _modalBox },
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill
        };
        Content = _root;
    }

}
