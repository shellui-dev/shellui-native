using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Badge status indicator component - theme-aware
public partial class Badge : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Badge), 
            string.Empty, propertyChanged: OnTextChanged);

    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(BadgeVariant), typeof(Badge), 
            BadgeVariant.Default, propertyChanged: OnVisualPropertyChanged);

    private readonly Border _border;
    private readonly Label _label;

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public BadgeVariant Variant
    {
        get => (BadgeVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public Badge()
    {
        _label = new Label
        {
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalTextAlignment = TextAlignment.Center
        };

        _border = new Border
        {
            Content = _label,
            Padding = new Thickness(12, 4), // More padding
            StrokeThickness = 1,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start
        };

        Content = _border;
        
        // Listen for theme changes
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeChanged += (s, e) => UpdateVisualState();
        }
        
        UpdateVisualState();
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Badge badge)
            badge._label.Text = newValue as string ?? string.Empty;
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Badge badge)
            badge.UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        var isDark = ShellTheme.IsDarkMode;
        
        // Theme-aware color schemes for each variant
        var (bg, fg, border) = Variant switch
        {
            BadgeVariant.Default => (
                isDark ? Color.FromArgb("#F8FAFC") : Color.FromArgb("#0F172A"),
                isDark ? Color.FromArgb("#0F172A") : Color.FromArgb("#F8FAFC"),
                isDark ? Color.FromArgb("#F8FAFC") : Color.FromArgb("#0F172A")
            ),
            BadgeVariant.Secondary => (
                isDark ? Color.FromArgb("#334155") : Color.FromArgb("#F1F5F9"),
                isDark ? Color.FromArgb("#F8FAFC") : Color.FromArgb("#0F172A"),
                isDark ? Color.FromArgb("#475569") : Color.FromArgb("#E2E8F0")
            ),
            BadgeVariant.Destructive => (
                Color.FromArgb("#EF4444"),
                Colors.White,
                Color.FromArgb("#EF4444")
            ),
            BadgeVariant.Outline => (
                Colors.Transparent,
                isDark ? Color.FromArgb("#F8FAFC") : Color.FromArgb("#0F172A"),
                isDark ? Color.FromArgb("#475569") : Color.FromArgb("#E2E8F0")
            ),
            BadgeVariant.Success => (
                Color.FromArgb("#22C55E"),
                Colors.White,
                Color.FromArgb("#22C55E")
            ),
            BadgeVariant.Warning => (
                Color.FromArgb("#F59E0B"),
                Colors.White,
                Color.FromArgb("#F59E0B")
            ),
            _ => (
                isDark ? Color.FromArgb("#F8FAFC") : Color.FromArgb("#0F172A"),
                isDark ? Color.FromArgb("#0F172A") : Color.FromArgb("#F8FAFC"),
                isDark ? Color.FromArgb("#F8FAFC") : Color.FromArgb("#0F172A")
            )
        };

        _border.BackgroundColor = bg;
        _border.Stroke = border;
        _border.StrokeShape = new RoundRectangle { CornerRadius = 9999 }; // Full rounded (pill shape)
        _label.TextColor = fg;
    }
}

public enum BadgeVariant { Default, Secondary, Destructive, Outline, Success, Warning }
