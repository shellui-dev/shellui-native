using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

[ContentProperty(nameof(Children))]
public partial class SheetContent : ContentView
{
    private readonly Grid _root;
    private readonly Border _backdrop;
    private readonly Border _panel;
    private readonly VerticalStackLayout _body;

    public new IList<IView> Children => _body.Children;

    public SheetContent()
    {
        _backdrop = new Border
        {
            BackgroundColor = Color.FromArgb("#80000000"),
            StrokeThickness = 0,
            ZIndex = 0
        };
        var tapBackdrop = new TapGestureRecognizer();
        tapBackdrop.Tapped += (s, e) => this.FindParentOfType<Sheet>()?.SetOpen(false);
        _backdrop.GestureRecognizers.Add(tapBackdrop);

        _body = new VerticalStackLayout { Spacing = 16, Padding = new Thickness(24) };
        _panel = new Border
        {
            Content = _body,
            BackgroundColor = Color.FromArgb("#FFFFFF"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            ZIndex = 1
        };

        _root = new Grid { Children = { _backdrop, _panel } };
        Content = _root;
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        var sheet = this.FindParentOfType<Sheet>();
        if (sheet != null)
        {
            _panel.VerticalOptions = sheet.Side switch
            {
                SheetSide.Top => LayoutOptions.Start,
                SheetSide.Bottom => LayoutOptions.End,
                _ => LayoutOptions.Fill
            };
            _panel.HorizontalOptions = sheet.Side switch
            {
                SheetSide.Left => LayoutOptions.Start,
                SheetSide.Right => LayoutOptions.End,
                _ => LayoutOptions.Fill
            };
            _panel.WidthRequest = sheet.Side is SheetSide.Left or SheetSide.Right ? 300 : -1;
            _panel.HeightRequest = sheet.Side is SheetSide.Top or SheetSide.Bottom ? 400 : -1;
        }
    }
}
