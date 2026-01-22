using Microsoft.Maui.Controls.Shapes;
using MauiIcons.Core;
using MauiIcons.Fluent;

namespace MAUI.Demo.Components.UI;

// Theme toggle switch using MauiIcons Fluent
public partial class ThemeToggle : ContentView
{
    private readonly Border _track;
    private readonly Border _thumb;
    private readonly MauiIcon _icon;
    private bool _isDark;

    public event EventHandler<bool>? ThemeChanged;

    public bool IsDarkMode
    {
        get => _isDark;
        set
        {
            if (_isDark != value)
            {
                _isDark = value;
                AnimateToggle();
                ApplyTheme();
                ThemeChanged?.Invoke(this, value);
            }
        }
    }

    public ThemeToggle()
    {
        _isDark = Application.Current?.RequestedTheme == AppTheme.Dark;

        // Using MauiIcon for cross-platform icons
        _icon = new MauiIcon
        {
            Icon = _isDark ? FluentIcons.WeatherMoon24 : FluentIcons.WeatherSunny24,
            IconSize = 14,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        _thumb = new Border
        {
            WidthRequest = 26,
            HeightRequest = 26,
            StrokeThickness = 0,
            Content = _icon,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start
        };

        _track = new Border
        {
            WidthRequest = 56,
            HeightRequest = 32,
            StrokeThickness = 1,
            Padding = new Thickness(3),
            Content = _thumb
        };

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) => IsDarkMode = !IsDarkMode;
        _track.GestureRecognizers.Add(tapGesture);

        Content = _track;
        UpdateVisualState();
    }

    private void ApplyTheme()
    {
        if (Application.Current != null)
            Application.Current.UserAppTheme = _isDark ? AppTheme.Dark : AppTheme.Light;
    }

    private async void AnimateToggle()
    {
        await _thumb.ScaleToAsync(0.85, 60, Easing.CubicOut);
        _thumb.Margin = _isDark ? new Thickness(24, 0, 0, 0) : new Thickness(0);
        UpdateVisualState();
        await _thumb.ScaleToAsync(1.0, 60, Easing.CubicOut);
    }

    private void UpdateVisualState()
    {
        // Track styling
        _track.BackgroundColor = _isDark ? Color.FromArgb("#1E293B") : Color.FromArgb("#F1F5F9");
        _track.Stroke = _isDark ? Color.FromArgb("#475569") : Color.FromArgb("#CBD5E1");
        _track.StrokeShape = new RoundRectangle { CornerRadius = 16 };

        // Thumb styling
        _thumb.BackgroundColor = _isDark ? Color.FromArgb("#3B82F6") : Colors.White;
        _thumb.StrokeShape = new RoundRectangle { CornerRadius = 13 };
        _thumb.Shadow = new Shadow
        {
            Brush = new SolidColorBrush(Color.FromArgb("#40000000")),
            Offset = new Point(0, 1),
            Radius = 3,
            Opacity = 0.2f
        };
        _thumb.Margin = _isDark ? new Thickness(24, 0, 0, 0) : new Thickness(0);

        // Icon - using MauiIcons FluentIcons directly
        _icon.Icon = _isDark ? FluentIcons.WeatherMoon24 : FluentIcons.WeatherSunny24;
        _icon.IconColor = _isDark ? Colors.White : Color.FromArgb("#F59E0B");
    }
}
