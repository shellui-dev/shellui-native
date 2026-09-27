using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

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
        SemanticProperties.SetDescription(_border, "Toggle theme");

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
