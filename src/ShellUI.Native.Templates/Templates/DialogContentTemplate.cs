using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DialogContentTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "dialog-content",
        DisplayName = "Dialog Content",
        Description = "Modal content - shows when dialog is open. Place inside Dialog",
        Category = ComponentCategory.Overlay,
        FilePath = "DialogContent.cs",
        Dependencies = new List<string> { "shell", "icon", "element-extensions" },
        Tags = new List<string> { "overlay", "dialog", "content" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Dialog panel — backdrop + centered box: max-w-lg rounded-lg border bg-background p-6
// shadow-lg, close button top-right; fades and zooms in from 95%.
[ContentProperty(nameof(Children))]
public partial class DialogContent : ContentView, IShellOverlayContent
{
    public static readonly BindableProperty ShowCloseButtonProperty =
        BindableProperty.Create(nameof(ShowCloseButton), typeof(bool), typeof(DialogContent), true,
            propertyChanged: (b, o, n) => ((DialogContent)b)._close.IsVisible = (bool)n);

    public bool ShowCloseButton
    {
        get => (bool)GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    private readonly BoxView _backdrop;
    private readonly Border _box;
    private readonly Border _close;
    private readonly VerticalStackLayout _body;

    public new IList<IView> Children => _body.Children;

    public DialogContent()
    {
        _backdrop = new BoxView();
        _backdrop.Token(BoxView.ColorProperty, ShellToken.Overlay);
        var tapBackdrop = new TapGestureRecognizer();
        tapBackdrop.Tapped += (_, _) => this.FindParentOfType<Dialog>()?.SetOpen(false);
        _backdrop.GestureRecognizers.Add(tapBackdrop);

        _body = new VerticalStackLayout { Spacing = 16 };

        _close = new Border
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
        SemanticProperties.SetDescription(_close, ""Close"");
        var closePointer = new PointerGestureRecognizer();
        closePointer.PointerEntered += (_, _) => _close.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        closePointer.PointerExited += (_, _) => { _close.ClearValue(VisualElement.BackgroundColorProperty); _close.BackgroundColor = Colors.Transparent; };
        _close.GestureRecognizers.Add(closePointer);
        var closeTap = new TapGestureRecognizer();
        closeTap.Tapped += (_, _) => this.FindParentOfType<Dialog>()?.SetOpen(false);
        _close.GestureRecognizers.Add(closeTap);

        _box = new Border
        {
            Content = new Grid { Children = { _body, _close } },
            Padding = new Thickness(24),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusLg },
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Shadow = new Shadow { Brush = new SolidColorBrush(Colors.Black), Offset = new Point(0, 10), Radius = 24, Opacity = 0.2f }
        };
        _box.Token(VisualElement.BackgroundColorProperty, ShellToken.Background);
        _box.Token(Border.StrokeProperty, ShellToken.Border);
        // Swallow taps on the box so they don't fall through to the backdrop.
        _box.GestureRecognizers.Add(new TapGestureRecognizer());

        var root = new Grid { Children = { _backdrop, _box } };
        root.SizeChanged += (_, _) => _box.WidthRequest = Math.Max(0, Math.Min(512, root.Width - 32));
        Content = root;
    }

    public async Task AnimateAsync(bool open)
    {
        _box.AbortAnimation(""FadeTo"");
        if (open)
        {
            _backdrop.Opacity = 0;
            _box.Opacity = 0;
            _box.Scale = 0.95;
            await Task.WhenAll(
                _backdrop.FadeToAsync(1, 150, Easing.CubicOut),
                _box.FadeToAsync(1, 150, Easing.CubicOut),
                _box.ScaleToAsync(1, 150, Easing.CubicOut));
        }
        else
        {
            await Task.WhenAll(
                _backdrop.FadeToAsync(0, 120, Easing.CubicIn),
                _box.FadeToAsync(0, 120, Easing.CubicIn),
                _box.ScaleToAsync(0.95, 120, Easing.CubicIn));
        }
    }
}
"
    };
}
