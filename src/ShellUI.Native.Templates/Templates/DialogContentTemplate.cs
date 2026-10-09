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
        _backdrop = new BoxView { BackgroundColor = Colors.Transparent };
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
        // The backdrop dims under the system bars; the centered box stays clear of them.
        ShellPortal.EdgeToEdge(this, root);
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

// Dialog panel — backdrop + centered box: max-w-lg rounded-lg border bg-background p-6
// shadow-lg, close button top-right; fades and zooms in from 95%. XAML children go to Items.
public class DialogContent : Border, IShellOverlayContent
{
    public static readonly StyledProperty<bool> ShowCloseButtonProperty =
        AvaloniaProperty.Register<DialogContent, bool>(nameof(ShowCloseButton), true);

    private readonly Border _backdrop;
    private readonly Border _box;
    private readonly Border _close;
    private readonly StackPanel _body = new() { Spacing = 16 };

    static DialogContent()
    {
        ShowCloseButtonProperty.Changed.AddClassHandler<DialogContent>((c, e) => c._close.IsVisible = (bool)e.NewValue!);
    }

    public bool ShowCloseButton
    {
        get => GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    [Content]
    public Controls Items => _body.Children;

    public DialogContent()
    {
        _backdrop = new Border();
        _backdrop.Token(BackgroundProperty, ShellToken.Overlay);
        _backdrop.PointerPressed += (_, e) =>
        {
            this.FindParentOfType<Dialog>()?.SetOpen(false);
            e.Handled = true;
        };

        _close = new Border
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
        AutomationProperties.SetName(_close, ""Close"");
        _close.PointerEntered += (_, _) => _close.Token(BackgroundProperty, ShellToken.Accent);
        _close.PointerExited += (_, _) => { _close.ClearToken(BackgroundProperty); _close.Background = Brushes.Transparent; };
        _close.PointerReleased += (_, e) =>
        {
            this.FindParentOfType<Dialog>()?.SetOpen(false);
            e.Handled = true;
        };
        // A tab stop inside the focus trap, as in shadcn; Enter or Space closes.
        ShellFocus.Ring(_close, _close);
        _close.KeyDown += (_, e) =>
        {
            if (e.Key is not (Key.Enter or Key.Space)) return;
            this.FindParentOfType<Dialog>()?.SetOpen(false);
            e.Handled = true;
        };

        _box = new Border
        {
            Child = new Panel { Children = { _body, _close } },
            Padding = new Thickness(24),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusLg),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            BoxShadow = new BoxShadows(new BoxShadow { OffsetY = 10, Blur = 24, Color = ShellTheme.Shadow(0.2) })
        };
        _box.Token(BackgroundProperty, ShellToken.Background).Token(BorderBrushProperty, ShellToken.Border);
        // Clicks on the box must not reach the backdrop behind it.
        _box.PointerPressed += (_, e) => e.Handled = true;

        Child = new Panel { Children = { _backdrop, _box } };
        SizeChanged += (_, e) => _box.Width = Math.Max(0, Math.Min(512, e.NewSize.Width - 32));
    }

    public async Task AnimateAsync(bool open)
    {
        if (open)
        {
            ShellMotion.Set(_backdrop, 0, ""scale(1)"");
            ShellMotion.Set(_box, 0, ""scale(0.95)"");
            await Task.WhenAll(
                ShellMotion.To(_backdrop, 1, ""scale(1)"", 150, new CubicEaseOut()),
                ShellMotion.To(_box, 1, ""scale(1)"", 150, new CubicEaseOut()));
        }
        else
        {
            await Task.WhenAll(
                ShellMotion.To(_backdrop, 0, ""scale(1)"", 120, new CubicEaseIn()),
                ShellMotion.To(_box, 0, ""scale(0.95)"", 120, new CubicEaseIn()));
        }
    }
}
"
    };
}
