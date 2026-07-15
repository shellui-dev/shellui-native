namespace MAUI.Demo.Components.UI.Variants;

public enum ButtonVariant
{
    Default,
    Destructive,
    Outline,
    Secondary,
    Ghost
}

public enum ButtonSize
{
    Default,
    Sm,
    Lg,
    Icon
}

// Style properties for button rendering
public class ButtonStyle
{
    public Color BackgroundColor { get; set; } = Colors.Transparent;
    public Color TextColor { get; set; } = Colors.Black;
    public Color BorderColor { get; set; } = Colors.Transparent;
    public double BorderThickness { get; set; } = 0;
    public CornerRadius CornerRadius { get; set; } = new(6);
    public Thickness Padding { get; set; } = new(16, 10);
    public double Height { get; set; } = 40;
    public double MinWidth { get; set; } = 80;
    public double FontSize { get; set; } = 14;
}

public static class ButtonVariants
{
    // Theme-aware design tokens
    private static bool IsDarkMode => Application.Current?.RequestedTheme == AppTheme.Dark;

    private static Color PrimaryColor => Color.FromArgb("#3B82F6"); // blue-500
    private static Color PrimaryForeground => Colors.White;
    
    private static Color DestructiveColor => Color.FromArgb("#EF4444"); // red-500
    private static Color DestructiveForeground => Colors.White;
    
    private static Color SecondaryColor => IsDarkMode 
        ? Color.FromArgb("#334155")  // slate-700
        : Color.FromArgb("#F1F5F9"); // slate-100
    private static Color SecondaryForeground => IsDarkMode 
        ? Color.FromArgb("#F8FAFC")  // slate-50
        : Color.FromArgb("#0F172A"); // slate-900
    
    private static Color BorderColor => IsDarkMode 
        ? Color.FromArgb("#475569")  // slate-600
        : Color.FromArgb("#E2E8F0"); // slate-200
    
    private static Color ForegroundColor => IsDarkMode 
        ? Color.FromArgb("#F8FAFC")  // slate-50
        : Color.FromArgb("#0F172A"); // slate-900

    public static ButtonStyle GetStyle(ButtonVariant variant = ButtonVariant.Default, ButtonSize size = ButtonSize.Default)
    {
        var style = new ButtonStyle();

        // Apply variant styles (theme-aware)
        switch (variant)
        {
            case ButtonVariant.Default:
                style.BackgroundColor = PrimaryColor;
                style.TextColor = PrimaryForeground;
                break;
            case ButtonVariant.Destructive:
                style.BackgroundColor = DestructiveColor;
                style.TextColor = DestructiveForeground;
                break;
            case ButtonVariant.Outline:
                style.BackgroundColor = Colors.Transparent;
                style.TextColor = ForegroundColor;
                style.BorderColor = BorderColor;
                style.BorderThickness = 1;
                break;
            case ButtonVariant.Secondary:
                style.BackgroundColor = SecondaryColor;
                style.TextColor = SecondaryForeground;
                break;
            case ButtonVariant.Ghost:
                style.BackgroundColor = Colors.Transparent;
                style.TextColor = ForegroundColor;
                break;
        }

        // Apply size styles (standard sizes)
        switch (size)
        {
            case ButtonSize.Sm:
                style.Height = 36;
                style.Padding = new Thickness(14, 8);
                style.FontSize = 13;
                style.CornerRadius = new CornerRadius(6);
                break;
            case ButtonSize.Default:
                style.Height = 40;
                style.Padding = new Thickness(16, 10);
                style.FontSize = 14;
                style.CornerRadius = new CornerRadius(6);
                break;
            case ButtonSize.Lg:
                style.Height = 48;
                style.Padding = new Thickness(24, 12);
                style.FontSize = 16;
                style.CornerRadius = new CornerRadius(8);
                break;
            case ButtonSize.Icon:
                style.Height = 40;
                style.MinWidth = 40;
                style.Padding = new Thickness(10);
                style.CornerRadius = new CornerRadius(6);
                break;
        }

        return style;
    }
}
