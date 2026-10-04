using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DrawerContentTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "drawer-content",
        DisplayName = "Drawer Content",
        Description = "Drawer panel content - slides in from side",
        Category = ComponentCategory.Overlay,
        FilePath = "DrawerContent.cs",
        Dependencies = new List<string> { "shell", "element-extensions" },
        Tags = new List<string> { "overlay", "drawer", "content" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Drawer panel — rounded-t-[10px] bg-background with a grab handle (h-2 w-[100px] bg-muted),
// sized to its content (max 80% of the page), sliding in from its edge over a dimmed backdrop.
[ContentProperty(nameof(Children))]
public partial class DrawerContent : ContentView, IShellOverlayContent
{
    private readonly BoxView _backdrop;
    private readonly Border _panel;
    private readonly BoxView _handle;
    private readonly VerticalStackLayout _body;
    private readonly Grid _root;

    public new IList<IView> Children => _body.Children;

    public DrawerContent()
    {
        _backdrop = new BoxView { BackgroundColor = Colors.Transparent };
        _backdrop.Token(BoxView.ColorProperty, ShellToken.Overlay);
        var tapBackdrop = new TapGestureRecognizer();
        tapBackdrop.Tapped += (_, _) => this.FindParentOfType<Drawer>()?.SetOpen(false);
        _backdrop.GestureRecognizers.Add(tapBackdrop);

        _handle = new BoxView
        {
            BackgroundColor = Colors.Transparent,
            HeightRequest = 6,
            WidthRequest = 100,
            CornerRadius = 3,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 12, 0, 0)
        };
        _handle.Token(BoxView.ColorProperty, ShellToken.Muted);

        _body = new VerticalStackLayout { Spacing = 16, Padding = new Thickness(16, 16, 16, 24) };

        var scroll = new ScrollView { Content = _body };
        var stack = new VerticalStackLayout { Spacing = 0, Children = { _handle, scroll } };
        _panel = new Border
        {
            Content = stack,
            StrokeThickness = 1,
            Shadow = new Shadow { Brush = new SolidColorBrush(Colors.Black), Offset = new Point(0, -4), Radius = 16, Opacity = 0.15f }
        };
        _panel.Token(VisualElement.BackgroundColorProperty, ShellToken.Background);
        _panel.Token(Border.StrokeProperty, ShellToken.Border);
        _panel.GestureRecognizers.Add(new TapGestureRecognizer());

        _root = new Grid { Children = { _backdrop, _panel } };
        ShellPortal.EdgeToEdge(this, _root, _panel, stack, scroll, _body);
        _root.SizeChanged += (_, _) => ApplySide();
        Content = _root;
    }

    private DrawerSide Side => this.FindParentOfType<Drawer>()?.Side ?? DrawerSide.Bottom;

    private void ApplySide()
    {
        var side = Side;
        var horizontal = side is DrawerSide.Top or DrawerSide.Bottom;
        _panel.VerticalOptions = side switch
        {
            DrawerSide.Top => LayoutOptions.Start,
            DrawerSide.Bottom => LayoutOptions.End,
            _ => LayoutOptions.Fill
        };
        _panel.HorizontalOptions = side switch
        {
            DrawerSide.Left => LayoutOptions.Start,
            DrawerSide.Right => LayoutOptions.End,
            _ => LayoutOptions.Fill
        };
        _panel.WidthRequest = horizontal ? -1 : Math.Min(320, _root.Width * 0.85);
        _panel.MaximumHeightRequest = horizontal && _root.Height > 0 ? _root.Height * 0.8 : double.PositiveInfinity;
        // The panel runs under the system bars; its content doesn't.
        var safe = ShellPortal.GetSafeInsets(_root);
        _body.Padding = new Thickness(
            16 + (side == DrawerSide.Left ? safe.Left : 0),
            16 + (side is DrawerSide.Top or DrawerSide.Left or DrawerSide.Right ? safe.Top : 0),
            16 + (side == DrawerSide.Right ? safe.Right : 0),
            24 + (side != DrawerSide.Top ? safe.Bottom : 0));
        _handle.IsVisible = side == DrawerSide.Bottom;
        const float r = 10;
        _panel.StrokeShape = new RoundRectangle
        {
            CornerRadius = side switch
            {
                DrawerSide.Bottom => new CornerRadius(r, r, 0, 0),
                DrawerSide.Top => new CornerRadius(0, 0, r, r),
                DrawerSide.Left => new CornerRadius(0, r, 0, r),
                _ => new CornerRadius(r, 0, r, 0)
            }
        };
    }

    private (double X, double Y) Offscreen() => Side switch
    {
        DrawerSide.Bottom => (0, _panel.Height + 24),
        DrawerSide.Top => (0, -_panel.Height - 24),
        DrawerSide.Left => (-_panel.Width - 24, 0),
        _ => (_panel.Width + 24, 0)
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
                _panel.TranslateToAsync(0, 0, 300, Easing.CubicOut));
        }
        else
        {
            var (x, y) = Offscreen();
            await Task.WhenAll(
                _backdrop.FadeToAsync(0, 200, Easing.CubicIn),
                _panel.TranslateToAsync(x, y, 220, Easing.CubicIn));
        }
    }
}
"
    };
}
