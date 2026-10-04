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
        Dependencies = new List<string> { "shell" },
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
    Ghost,
    Link
}

public enum ButtonSize
{
    Default,
    Sm,
    Lg,
    Icon
}

// Token-based style for one variant + size. Null background/border = transparent.
public class ButtonStyle
{
    public ShellToken? Background { get; set; }
    public ShellToken? HoverBackground { get; set; }
    public ShellToken Foreground { get; set; } = ShellToken.Foreground;
    public ShellToken? Border { get; set; }
    public double HoverOpacity { get; set; } = 1.0;
    public bool UnderlineOnHover { get; set; }
    public float CornerRadius { get; set; } = ShellTheme.RadiusMd;
    public Thickness Padding { get; set; } = new(16, 0);
    public double Height { get; set; } = 40;
    public double Width { get; set; } = -1;
    public double FontSize { get; set; } = 14;
    public double IconSize { get; set; } = 16;
}

// Mirrors ShellUI's buttonVariants (cva): variant → colors, size → box.
public static class ButtonVariants
{
    public static ButtonStyle GetStyle(ButtonVariant variant = ButtonVariant.Default, ButtonSize size = ButtonSize.Default)
    {
        var style = new ButtonStyle();

        switch (variant)
        {
            case ButtonVariant.Default:      // bg-primary text-primary-foreground hover:bg-primary/90
                style.Background = ShellToken.Primary;
                style.Foreground = ShellToken.PrimaryForeground;
                style.HoverOpacity = 0.9;
                break;
            case ButtonVariant.Destructive:  // bg-destructive text-destructive-foreground hover:bg-destructive/90
                style.Background = ShellToken.Destructive;
                style.Foreground = ShellToken.DestructiveForeground;
                style.HoverOpacity = 0.9;
                break;
            case ButtonVariant.Outline:      // border border-input bg-background hover:bg-accent
                style.Background = ShellToken.Background;
                style.HoverBackground = ShellToken.Accent;
                style.Border = ShellToken.Input;
                style.Foreground = ShellToken.Foreground;
                break;
            case ButtonVariant.Secondary:    // bg-secondary text-secondary-foreground hover:bg-secondary/80
                style.Background = ShellToken.Secondary;
                style.Foreground = ShellToken.SecondaryForeground;
                style.HoverOpacity = 0.8;
                break;
            case ButtonVariant.Ghost:        // hover:bg-accent hover:text-accent-foreground
                style.HoverBackground = ShellToken.Accent;
                style.Foreground = ShellToken.Foreground;
                break;
            case ButtonVariant.Link:         // text-primary underline-offset-4 hover:underline
                style.Foreground = ShellToken.Primary;
                style.UnderlineOnHover = true;
                break;
        }

        switch (size)
        {
            case ButtonSize.Sm:              // h-9 px-3
                style.Height = 36;
                style.Padding = new Thickness(12, 0);
                style.FontSize = 13;
                break;
            case ButtonSize.Lg:              // h-11 px-8
                style.Height = 44;
                style.Padding = new Thickness(32, 0);
                style.FontSize = 15;
                break;
            case ButtonSize.Icon:            // h-10 w-10
                style.Height = 40;
                style.Width = 40;
                style.Padding = new Thickness(0);
                break;
        }

        return style;
    }
}
"
    };
}
