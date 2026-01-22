namespace MAUI.Demo.Components.UI;

// ShellUI Native utility class with theming support
public static class Shell
{
    // Combines multiple class names, filtering out null/empty values
    public static string Cn(params string?[] classes)
        => string.Join(" ", classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}

// Theme-aware color tokens for ShellUI Native components
public static class ShellTheme
{
    // Check if dark mode is active
    public static bool IsDarkMode => Application.Current?.RequestedTheme == AppTheme.Dark;

    // Background colors
    public static Color Background => IsDarkMode 
        ? Color.FromArgb("#0F172A") // Dark: slate-900
        : Color.FromArgb("#FFFFFF"); // Light: white

    public static Color BackgroundMuted => IsDarkMode 
        ? Color.FromArgb("#1E293B") // Dark: slate-800
        : Color.FromArgb("#F8FAFC"); // Light: slate-50

    public static Color BackgroundCard => IsDarkMode 
        ? Color.FromArgb("#1E293B") // Dark: slate-800
        : Color.FromArgb("#FFFFFF"); // Light: white

    // Foreground/text colors
    public static Color Foreground => IsDarkMode 
        ? Color.FromArgb("#F8FAFC") // Dark: slate-50
        : Color.FromArgb("#0F172A"); // Light: slate-900

    public static Color ForegroundMuted => IsDarkMode 
        ? Color.FromArgb("#94A3B8") // Dark: slate-400
        : Color.FromArgb("#64748B"); // Light: slate-500

    // Border colors
    public static Color Border => IsDarkMode 
        ? Color.FromArgb("#334155") // Dark: slate-700
        : Color.FromArgb("#E2E8F0"); // Light: slate-200

    public static Color BorderInput => IsDarkMode 
        ? Color.FromArgb("#475569") // Dark: slate-600
        : Color.FromArgb("#CBD5E1"); // Light: slate-300

    // Primary colors (consistent across themes)
    public static Color Primary => Color.FromArgb("#3B82F6"); // blue-500
    public static Color PrimaryForeground => Colors.White;

    // Semantic colors
    public static Color Destructive => Color.FromArgb("#EF4444"); // red-500
    public static Color DestructiveForeground => Colors.White;
    
    public static Color Success => Color.FromArgb("#22C55E"); // green-500
    public static Color SuccessForeground => Colors.White;
    
    public static Color Warning => Color.FromArgb("#F59E0B"); // amber-500
    public static Color WarningForeground => Colors.White;

    // Secondary colors
    public static Color Secondary => IsDarkMode 
        ? Color.FromArgb("#334155") // Dark: slate-700
        : Color.FromArgb("#F1F5F9"); // Light: slate-100
    
    public static Color SecondaryForeground => IsDarkMode 
        ? Color.FromArgb("#F8FAFC") // Dark: slate-50
        : Color.FromArgb("#0F172A"); // Light: slate-900

    // Accent colors
    public static Color Accent => IsDarkMode 
        ? Color.FromArgb("#1E293B") // Dark: slate-800
        : Color.FromArgb("#F1F5F9"); // Light: slate-100
    
    public static Color AccentForeground => IsDarkMode 
        ? Color.FromArgb("#F8FAFC") // Dark: slate-50
        : Color.FromArgb("#0F172A"); // Light: slate-900

    // Input-specific colors
    public static Color InputBackground => IsDarkMode 
        ? Color.FromArgb("#1E293B") // Dark: slate-800
        : Color.FromArgb("#FFFFFF"); // Light: white

    public static Color InputText => IsDarkMode 
        ? Color.FromArgb("#F8FAFC") // Dark: slate-50
        : Color.FromArgb("#0F172A"); // Light: slate-900

    public static Color InputPlaceholder => IsDarkMode 
        ? Color.FromArgb("#64748B") // Dark: slate-500
        : Color.FromArgb("#94A3B8"); // Light: slate-400
}
