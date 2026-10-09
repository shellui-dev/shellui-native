using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class SheetContentTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "sheet-content",
        DisplayName = "Sheet Content",
        Description = "Sheet panel content",
        Category = ComponentCategory.Overlay,
        FilePath = "SheetContent.cs",
        Dependencies = new List<string> { "shell", "icon", "element-extensions" },
        Tags = new List<string> { "overlay", "sheet", "content" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

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
        SemanticProperties.SetDescription(close, ""Close"");
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
",
        [NativePlatform.Avalonia] = @"using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace YourProjectNamespace.Components.UI;

// Sheet panel — full-height edge panel, w-3/4 sm:max-w-sm, bg-background p-6 shadow-lg,
// close button top-right, slides in from its side over a dimmed backdrop. XAML children go to Items.
public class SheetContent : Border, IShellOverlayContent
{
    private readonly Border _backdrop;
    private readonly Border _panel;
    private readonly StackPanel _body = new() { Spacing = 16, Margin = new Thickness(0, 0, 24, 0) };

    [Content]
    public Controls Items => _body.Children;

    public SheetContent()
    {
        _backdrop = new Border();
        _backdrop.Token(BackgroundProperty, ShellToken.Overlay);
        _backdrop.PointerPressed += (_, e) =>
        {
            this.FindParentOfType<Sheet>()?.SetOpen(false);
            e.Handled = true;
        };

        var close = new Border
        {
            Child = new Icon { Kind = IconName.X, Size = 16, Token = ShellToken.MutedForeground },
            Width = 24,
            Height = 24,
            CornerRadius = new CornerRadius(ShellTheme.RadiusSm),
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, -8, -8, 0),
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        AutomationProperties.SetName(close, ""Close"");
        close.PointerEntered += (_, _) => close.Token(BackgroundProperty, ShellToken.Accent);
        close.PointerExited += (_, _) => { close.ClearToken(BackgroundProperty); close.Background = Brushes.Transparent; };
        close.PointerReleased += (_, e) =>
        {
            this.FindParentOfType<Sheet>()?.SetOpen(false);
            e.Handled = true;
        };
        // A tab stop inside the focus trap, as in shadcn; Enter or Space closes.
        ShellFocus.Ring(close, close);
        close.KeyDown += (_, e) =>
        {
            if (e.Key is not (Key.Enter or Key.Space)) return;
            this.FindParentOfType<Sheet>()?.SetOpen(false);
            e.Handled = true;
        };

        _panel = new Border
        {
            Child = new Panel { Children = { new ScrollViewer { Content = _body }, close } },
            Padding = new Thickness(24),
            BorderThickness = new Thickness(1),
            BoxShadow = new BoxShadows(new BoxShadow { Blur = 24, Color = ShellTheme.Shadow(0.2) })
        };
        _panel.Token(BackgroundProperty, ShellToken.Background).Token(BorderBrushProperty, ShellToken.Border);
        _panel.PointerPressed += (_, e) => e.Handled = true;

        Child = new Panel { Children = { _backdrop, _panel } };
        SizeChanged += (_, _) => ApplySide();
    }

    private SheetSide Side => this.FindParentOfType<Sheet>()?.Side ?? SheetSide.Right;

    private void ApplySide()
    {
        var side = Side;
        var vertical = side is SheetSide.Left or SheetSide.Right;
        _panel.VerticalAlignment = side switch
        {
            SheetSide.Top => VerticalAlignment.Top,
            SheetSide.Bottom => VerticalAlignment.Bottom,
            _ => VerticalAlignment.Stretch
        };
        _panel.HorizontalAlignment = side switch
        {
            SheetSide.Left => HorizontalAlignment.Left,
            SheetSide.Right => HorizontalAlignment.Right,
            _ => HorizontalAlignment.Stretch
        };
        _panel.Width = vertical ? Math.Min(384, Bounds.Width * 0.75) : double.NaN;
    }

    // translate() that puts the panel just past its edge; invariant, so it parses in any culture.
    private string Offscreen()
    {
        var (x, y) = Side switch
        {
            SheetSide.Left => (-_panel.Bounds.Width - 32, 0),
            SheetSide.Top => (0, -_panel.Bounds.Height - 32),
            SheetSide.Bottom => (0, _panel.Bounds.Height + 32),
            _ => (_panel.Bounds.Width + 32, 0d)
        };
        return FormattableString.Invariant($""translate({x}px, {y}px)"");
    }

    public async Task AnimateAsync(bool open)
    {
        if (open)
        {
            ShellMotion.Set(_backdrop, 0, ""scale(1)"");
            ShellMotion.Set(_panel, 0, ""scale(1)"");
            ApplySide();
            await ShellMotion.WaitForLayoutAsync(_panel);
            ShellMotion.Set(_panel, 1, Offscreen());
            await Task.WhenAll(
                ShellMotion.To(_backdrop, 1, ""scale(1)"", 200, new CubicEaseOut()),
                ShellMotion.To(_panel, 1, ""translate(0px, 0px)"", 350, new CubicEaseOut()));
        }
        else
        {
            await Task.WhenAll(
                ShellMotion.To(_backdrop, 0, ""scale(1)"", 200, new CubicEaseIn()),
                ShellMotion.To(_panel, 1, Offscreen(), 250, new CubicEaseIn()));
        }
    }
}
"
    };
}
