using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Drawer panel — rounded-t-[10px] bg-background with a grab handle (h-2 w-[100px] bg-muted),
// sized to its content (max 80% of the window), sliding in from its edge over a dimmed backdrop.
// XAML children go to Items.
public class DrawerContent : Border, IShellOverlayContent
{
    private readonly Border _backdrop;
    private readonly Border _panel;
    private readonly Border _handle;
    private readonly StackPanel _body = new() { Spacing = 16, Margin = new Thickness(16, 16, 16, 24) };

    [Content]
    public Controls Items => _body.Children;

    public DrawerContent()
    {
        _backdrop = new Border();
        _backdrop.Token(BackgroundProperty, ShellToken.Overlay);
        _backdrop.PointerPressed += (_, e) =>
        {
            this.FindParentOfType<Drawer>()?.SetOpen(false);
            e.Handled = true;
        };

        _handle = new Border
        {
            Height = 6,
            Width = 100,
            CornerRadius = new CornerRadius(3),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 12, 0, 0)
        };
        _handle.Token(BackgroundProperty, ShellToken.Muted);

        var stack = new DockPanel();
        DockPanel.SetDock(_handle, Dock.Top);
        stack.Children.Add(_handle);
        stack.Children.Add(new ScrollViewer { Content = _body });

        _panel = new Border
        {
            Child = stack,
            BorderThickness = new Thickness(1),
            BoxShadow = new BoxShadows(new BoxShadow { OffsetY = -4, Blur = 16, Color = ShellTheme.Shadow(0.15) })
        };
        _panel.Token(BackgroundProperty, ShellToken.Background).Token(BorderBrushProperty, ShellToken.Border);
        _panel.PointerPressed += (_, e) => e.Handled = true;

        Child = new Panel { Children = { _backdrop, _panel } };
        SizeChanged += (_, _) => ApplySide();
    }

    private DrawerSide Side => this.FindParentOfType<Drawer>()?.Side ?? DrawerSide.Bottom;

    private void ApplySide()
    {
        var side = Side;
        var horizontal = side is DrawerSide.Top or DrawerSide.Bottom;
        _panel.VerticalAlignment = side switch
        {
            DrawerSide.Top => VerticalAlignment.Top,
            DrawerSide.Bottom => VerticalAlignment.Bottom,
            _ => VerticalAlignment.Stretch
        };
        _panel.HorizontalAlignment = side switch
        {
            DrawerSide.Left => HorizontalAlignment.Left,
            DrawerSide.Right => HorizontalAlignment.Right,
            _ => HorizontalAlignment.Stretch
        };
        _panel.Width = horizontal ? double.NaN : Math.Min(320, Bounds.Width * 0.85);
        _panel.MaxHeight = horizontal && Bounds.Height > 0 ? Bounds.Height * 0.8 : double.PositiveInfinity;
        _handle.IsVisible = side == DrawerSide.Bottom;
        const double r = 10;
        _panel.CornerRadius = side switch
        {
            DrawerSide.Bottom => new CornerRadius(r, r, 0, 0),
            DrawerSide.Top => new CornerRadius(0, 0, r, r),
            DrawerSide.Left => new CornerRadius(0, r, r, 0),
            _ => new CornerRadius(r, 0, 0, r)
        };
    }

    // translate() that puts the panel just past its edge; invariant, so it parses in any culture.
    private string Offscreen()
    {
        var (x, y) = Side switch
        {
            DrawerSide.Bottom => (0, _panel.Bounds.Height + 24),
            DrawerSide.Top => (0, -_panel.Bounds.Height - 24),
            DrawerSide.Left => (-_panel.Bounds.Width - 24, 0),
            _ => (_panel.Bounds.Width + 24, 0d)
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
                ShellMotion.To(_panel, 1, "translate(0px, 0px)", 300, new CubicEaseOut()));
        }
        else
        {
            await Task.WhenAll(
                ShellMotion.To(_backdrop, 0, "scale(1)", 200, new CubicEaseIn()),
                ShellMotion.To(_panel, 1, Offscreen(), 220, new CubicEaseIn()));
        }
    }
}
