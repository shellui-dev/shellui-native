using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

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
        AutomationProperties.SetName(_close, "Close");
        _close.PointerEntered += (_, _) => _close.Token(BackgroundProperty, ShellToken.Accent);
        _close.PointerExited += (_, _) => { _close.ClearToken(BackgroundProperty); _close.Background = Brushes.Transparent; };
        _close.PointerReleased += (_, e) =>
        {
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
            ShellMotion.Set(_backdrop, 0, "scale(1)");
            ShellMotion.Set(_box, 0, "scale(0.95)");
            await Task.WhenAll(
                ShellMotion.To(_backdrop, 1, "scale(1)", 150, new CubicEaseOut()),
                ShellMotion.To(_box, 1, "scale(1)", 150, new CubicEaseOut()));
        }
        else
        {
            await Task.WhenAll(
                ShellMotion.To(_backdrop, 0, "scale(1)", 120, new CubicEaseIn()),
                ShellMotion.To(_box, 0, "scale(0.95)", 120, new CubicEaseIn()));
        }
    }
}
