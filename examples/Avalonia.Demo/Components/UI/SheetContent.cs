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
        AutomationProperties.SetName(close, "Close");
        close.PointerEntered += (_, _) => close.Token(BackgroundProperty, ShellToken.Accent);
        close.PointerExited += (_, _) => { close.ClearToken(BackgroundProperty); close.Background = Brushes.Transparent; };
        close.PointerReleased += (_, e) =>
        {
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
        return FormattableString.Invariant($"translate({x}px, {y}px)");
    }

    public async Task AnimateAsync(bool open)
    {
        if (open)
        {
            ShellMotion.Set(_backdrop, 0, "scale(1)");
            ShellMotion.Set(_panel, 0, "scale(1)");
            ApplySide();
            await ShellMotion.WaitForLayoutAsync(_panel);
            ShellMotion.Set(_panel, 1, Offscreen());
            await Task.WhenAll(
                ShellMotion.To(_backdrop, 1, "scale(1)", 200, new CubicEaseOut()),
                ShellMotion.To(_panel, 1, "translate(0px, 0px)", 350, new CubicEaseOut()));
        }
        else
        {
            await Task.WhenAll(
                ShellMotion.To(_backdrop, 0, "scale(1)", 200, new CubicEaseIn()),
                ShellMotion.To(_panel, 1, Offscreen(), 250, new CubicEaseIn()));
        }
    }
}
