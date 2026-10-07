using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class ThemeToggleTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "theme-toggle",
        DisplayName = "Theme Toggle",
        Description = "Light/dark mode toggle button",
        Category = ComponentCategory.Utility,
        FilePath = "ThemeToggle.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "theme", "dark-mode", "toggle" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Light/dark mode toggle — a 36x36 outline icon button (shadcn's ModeToggle). The sun and
// moon cross-fade with a quarter turn. Usage: <ui:ThemeToggle />
public partial class ThemeToggle : ContentView
{
    private const double Box = 36;

    private readonly Border _border;
    private readonly Icon _sun;
    private readonly Icon _moon;

    public event EventHandler<bool>? ThemeChanged;

    public bool IsDarkMode => ShellTheme.IsDarkMode;

    public ThemeToggle()
    {
        ShellTheme.EnsureInitialized();

        _sun = new Icon { Name = IconName.Sun, Size = 16 };
        _moon = new Icon { Name = IconName.Moon, Size = 16 };

        _border = new Border
        {
            WidthRequest = Box,
            HeightRequest = Box,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            Content = new Grid { Children = { _sun, _moon } }
        };
        _border.Token(Border.StrokeProperty, ShellToken.Input);
        _border.Token(VisualElement.BackgroundColorProperty, ShellToken.Background);
        SemanticProperties.SetDescription(_border, ""Toggle theme"");

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => _border.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        pointer.PointerExited += (_, _) => _border.Token(VisualElement.BackgroundColorProperty, ShellToken.Background);
        _border.GestureRecognizers.Add(pointer);

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { ShellFocus.FocusPressed(this); Toggle(); };
        _border.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, Toggle);

        Content = _border;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;

        ShowIcons(ShellTheme.IsDarkMode, animate: false);
        Loaded += (_, _) =>
        {
            ShellTheme.ThemeChanged -= OnThemeChanged;
            ShellTheme.ThemeChanged += OnThemeChanged;
            ShowIcons(ShellTheme.IsDarkMode, animate: false);
        };
        Unloaded += (_, _) => ShellTheme.ThemeChanged -= OnThemeChanged;
    }

    public void Toggle()
    {
        ShellTheme.ToggleTheme();
        ThemeChanged?.Invoke(this, ShellTheme.IsDarkMode);
    }

    private void OnThemeChanged(object? sender, EventArgs e) => ShowIcons(ShellTheme.IsDarkMode, animate: true);

    private void ShowIcons(bool dark, bool animate)
    {
        var (show, hide) = dark ? (_moon, _sun) : (_sun, _moon);
        if (!animate)
        {
            show.Opacity = 1; show.Rotation = 0; show.Scale = 1;
            hide.Opacity = 0; hide.Rotation = -90; hide.Scale = 0.5;
            return;
        }
        show.Rotation = 90; show.Scale = 0.5;
        _ = show.FadeToAsync(1, 200, Easing.CubicOut);
        _ = show.RotateToAsync(0, 200, Easing.CubicOut);
        _ = show.ScaleToAsync(1, 200, Easing.CubicOut);
        _ = hide.FadeToAsync(0, 200, Easing.CubicIn);
        _ = hide.RotateToAsync(-90, 200, Easing.CubicIn);
        _ = hide.ScaleToAsync(0.5, 200, Easing.CubicIn);
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;

namespace YourProjectNamespace.Components.UI;

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
        AutomationProperties.SetName(this, ""Toggle theme"");

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
        show.RenderTransform = TransformOperations.Parse(""rotate(0deg) scale(1)"");
        hide.Opacity = 0;
        hide.RenderTransform = TransformOperations.Parse(""rotate(-90deg) scale(0.5)"");
    }
}
"
    };
}
