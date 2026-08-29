using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Button variants and styling template
public static class ButtonVariantsTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "button-variants",
        DisplayName = "Button Variants",
        Description = "Button variant enums and style definitions",
        Category = ComponentCategory.Utility,
        FilePath = "Variants/ButtonVariants.cs",
        IsAvailable = false // Installed as dependency of button
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI.Variants;

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
    // Design tokens - these mirror ShellUI Blazor CSS variables
    private static readonly Color PrimaryColor = Color.FromArgb(""#2563EB"");
    private static readonly Color PrimaryForeground = Colors.White;
    private static readonly Color DestructiveColor = Color.FromArgb(""#EF4444"");
    private static readonly Color DestructiveForeground = Colors.White;
    private static readonly Color SecondaryColor = Color.FromArgb(""#F4F4F5"");
    private static readonly Color SecondaryForeground = Color.FromArgb(""#18181B"");
    private static readonly Color BorderColor = Color.FromArgb(""#E4E4E7"");
    private static readonly Color ForegroundColor = Color.FromArgb(""#18181B"");
    private static readonly Color AccentColor = Color.FromArgb(""#F4F4F5"");

    public static ButtonStyle GetStyle(ButtonVariant variant = ButtonVariant.Default, ButtonSize size = ButtonSize.Default)
    {
        var style = new ButtonStyle();

        // Apply variant styles
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

        // Apply size styles
        switch (size)
        {
            case ButtonSize.Sm:
                style.Height = 36;
                style.Padding = new Thickness(12, 8);
                style.FontSize = 13;
                style.CornerRadius = new CornerRadius(4);
                break;
            case ButtonSize.Default:
                style.Height = 40;
                style.Padding = new Thickness(16, 10);
                style.FontSize = 14;
                style.CornerRadius = new CornerRadius(6);
                break;
            case ButtonSize.Lg:
                style.Height = 44;
                style.Padding = new Thickness(24, 12);
                style.FontSize = 16;
                style.CornerRadius = new CornerRadius(6);
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
"
    };
}
