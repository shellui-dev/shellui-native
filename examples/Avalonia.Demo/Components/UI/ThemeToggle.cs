using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;

namespace AvaloniaDemo.Components.UI;

// Light/dark mode toggle — a 36x36 outline icon button (shadcn's ModeToggle). The sun and
// moon cross-fade with a quarter turn. Usage: <ui:ThemeToggle />
public class ThemeToggle : Border
{
    private const double Box = 36;

    private readonly Icon _sun;
    private readonly Icon _moon;

    public event EventHandler<bool>? ThemeChanged;

    public bool IsDarkMode => ShellTheme.IsDarkMode;

    public ThemeToggle()
    {
        ShellTheme.EnsureInitialized();

        _sun = new Icon { Kind = IconName.Sun, Size = 16 };
        _moon = new Icon { Kind = IconName.Moon, Size = 16 };

        Width = Box;
        Height = Box;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Center;
        Cursor = new Cursor(StandardCursorType.Hand);
        Focusable = true;
        Child = new Panel { Children = { _sun, _moon } };
        this.Token(BorderBrushProperty, ShellToken.Input);
        this.Token(BackgroundProperty, ShellToken.Background);
        AutomationProperties.SetName(this, "Toggle theme");

        ShowIcons(ShellTheme.IsDarkMode);
        foreach (var icon in new[] { _sun, _moon })
        {
            icon.Transitions = new Transitions
            {
                new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(200), Easing = new CubicEaseOut() },
                new TransformOperationsTransition { Property = RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(200), Easing = new CubicEaseOut() },
            };
        }
    }

    public void Toggle()
    {
        ShellTheme.ToggleTheme();
        ThemeChanged?.Invoke(this, ShellTheme.IsDarkMode);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ShellTheme.ThemeChanged += OnThemeChanged;
        ShowIcons(ShellTheme.IsDarkMode);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ShellTheme.ThemeChanged -= OnThemeChanged;
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        this.Token(BackgroundProperty, ShellToken.Accent);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        this.Token(BackgroundProperty, ShellToken.Background);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            Toggle();
            e.Handled = true;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is Key.Enter or Key.Space)
        {
            Toggle();
            e.Handled = true;
        }
    }

    private void OnThemeChanged(object? sender, EventArgs e) => ShowIcons(ShellTheme.IsDarkMode);

    private void ShowIcons(bool dark)
    {
        var (show, hide) = dark ? (_moon, _sun) : (_sun, _moon);
        show.Opacity = 1;
        show.RenderTransform = TransformOperations.Parse("rotate(0deg) scale(1)");
        hide.Opacity = 0;
        hide.RenderTransform = TransformOperations.Parse("rotate(-90deg) scale(0.5)");
    }
}
