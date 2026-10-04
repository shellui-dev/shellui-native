using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Sheet panel — full-height edge panel, w-3/4 sm:max-w-sm, bg-background p-6 shadow-lg,
// close button top-right, slides in from its side over a dimmed backdrop.
[ContentProperty(nameof(Children))]
public partial class SheetContent : ContentView, IShellOverlayContent
{
    private readonly BoxView _backdrop;
    private readonly Border _panel;
    private readonly VerticalStackLayout _body;
    private readonly Grid _root;

    public new IList<IView> Children => _body.Children;

    public SheetContent()
    {
        _backdrop = new BoxView { BackgroundColor = Colors.Transparent };
        _backdrop.Token(BoxView.ColorProperty, ShellToken.Overlay);
        var tapBackdrop = new TapGestureRecognizer();
        tapBackdrop.Tapped += (_, _) => this.FindParentOfType<Sheet>()?.SetOpen(false);
        _backdrop.GestureRecognizers.Add(tapBackdrop);

        _body = new VerticalStackLayout { Spacing = 16, Margin = new Thickness(0, 0, 24, 0) };

        var close = new Border
        {
            Content = new Icon { Name = IconName.X, Size = 16, Token = ShellToken.MutedForeground },
            WidthRequest = 24,
            HeightRequest = 24,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusSm },
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, -8, -8, 0)
        };
        SemanticProperties.SetDescription(close, "Close");
        var closePointer = new PointerGestureRecognizer();
        closePointer.PointerEntered += (_, _) => close.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        closePointer.PointerExited += (_, _) => { close.ClearValue(VisualElement.BackgroundColorProperty); close.BackgroundColor = Colors.Transparent; };
        close.GestureRecognizers.Add(closePointer);
        var closeTap = new TapGestureRecognizer();
        closeTap.Tapped += (_, _) => this.FindParentOfType<Sheet>()?.SetOpen(false);
        close.GestureRecognizers.Add(closeTap);

        var scroll = new ScrollView { Content = _body };
        var inner = new Grid { Children = { scroll, close } };
        _panel = new Border
        {
            Content = inner,
            Padding = new Thickness(24),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            Shadow = new Shadow { Brush = new SolidColorBrush(Colors.Black), Offset = new Point(0, 0), Radius = 24, Opacity = 0.2f }
        };
        _panel.Token(VisualElement.BackgroundColorProperty, ShellToken.Background);
        _panel.Token(Border.StrokeProperty, ShellToken.Border);
        _panel.GestureRecognizers.Add(new TapGestureRecognizer());

        _root = new Grid { Children = { _backdrop, _panel } };
        ShellPortal.EdgeToEdge(this, _root, _panel, inner, scroll, _body);
        _root.SizeChanged += (_, _) => ApplySide();
        Content = _root;
    }

    private SheetSide Side => this.FindParentOfType<Sheet>()?.Side ?? SheetSide.Right;

    private void ApplySide()
    {
        var side = Side;
        var vertical = side is SheetSide.Left or SheetSide.Right;
        _panel.VerticalOptions = side switch
        {
            SheetSide.Top => LayoutOptions.Start,
            SheetSide.Bottom => LayoutOptions.End,
            _ => LayoutOptions.Fill
        };
        _panel.HorizontalOptions = side switch
        {
            SheetSide.Left => LayoutOptions.Start,
            SheetSide.Right => LayoutOptions.End,
            _ => LayoutOptions.Fill
        };
        _panel.WidthRequest = vertical ? Math.Min(384, _root.Width * 0.75) : -1;
        // The panel runs under the system bars; its content doesn't.
        var safe = ShellPortal.GetSafeInsets(_root);
        _panel.Padding = new Thickness(
            24 + (side == SheetSide.Left ? safe.Left : 0),
            24 + (side != SheetSide.Bottom ? safe.Top : 0),
            24 + (side == SheetSide.Right ? safe.Right : 0),
            24 + (side != SheetSide.Top ? safe.Bottom : 0));
    }

    private (double X, double Y) Offscreen() => Side switch
    {
        SheetSide.Left => (-_panel.Width - 32, 0),
        SheetSide.Top => (0, -_panel.Height - 32),
        SheetSide.Bottom => (0, _panel.Height + 32),
        _ => (_panel.Width + 32, 0)
    };

    public async Task AnimateAsync(bool open)
    {
        if (open)
        {
            _backdrop.Opacity = 0;
            _panel.Opacity = 0;
            ApplySide();
            await ShellOverlayHost.WaitForLayoutAsync(_panel);
            var (x, y) = Offscreen();
            _panel.TranslationX = x;
            _panel.TranslationY = y;
            _panel.Opacity = 1;
            await Task.WhenAll(
                _backdrop.FadeToAsync(1, 200, Easing.CubicOut),
                _panel.TranslateToAsync(0, 0, 350, Easing.CubicOut));
        }
        else
        {
            var (x, y) = Offscreen();
            await Task.WhenAll(
                _backdrop.FadeToAsync(0, 200, Easing.CubicIn),
                _panel.TranslateToAsync(x, y, 250, Easing.CubicIn));
        }
    }
}
