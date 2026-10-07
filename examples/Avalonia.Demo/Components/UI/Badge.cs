using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Status pill — rounded-full border px-2.5 py-0.5 text-xs font-semibold.
public class Badge : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Badge, string?>(nameof(Text));

    public static readonly StyledProperty<BadgeVariant> VariantProperty =
        AvaloniaProperty.Register<Badge, BadgeVariant>(nameof(Variant));

    private readonly TextBlock _label;

    static Badge()
    {
        TextProperty.Changed.AddClassHandler<Badge>((b, e) => b._label.Text = (string?)e.NewValue ?? string.Empty);
        VariantProperty.Changed.AddClassHandler<Badge>((b, _) => b.UpdateVisualState());
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public BadgeVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public Badge()
    {
        _label = new TextBlock
        {
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        Child = _label;
        Padding = new Thickness(10, 2);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(999);
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Center;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        // (background, foreground, border); null background = transparent.
        var (bg, fg, border) = Variant switch
        {
            BadgeVariant.Secondary => ((ShellToken?)ShellToken.Secondary, ShellToken.SecondaryForeground, ShellToken.Secondary),
            BadgeVariant.Destructive => (ShellToken.Destructive, ShellToken.DestructiveForeground, ShellToken.Destructive),
            BadgeVariant.Outline => (null, ShellToken.Foreground, ShellToken.Border),
            BadgeVariant.Success => (ShellToken.Success, ShellToken.SuccessForeground, ShellToken.Success),
            BadgeVariant.Warning => (ShellToken.Warning, ShellToken.WarningForeground, ShellToken.Warning),
            BadgeVariant.Info => (ShellToken.Info, ShellToken.InfoForeground, ShellToken.Info),
            _ => (ShellToken.Primary, ShellToken.PrimaryForeground, ShellToken.Primary)
        };

        if (bg.HasValue)
            this.Token(BackgroundProperty, bg.Value);
        else
        {
            this.ClearToken(BackgroundProperty);
            Background = Brushes.Transparent;
        }
        this.Token(BorderBrushProperty, border);
        _label.Token(TextBlock.ForegroundProperty, fg);
    }
}

public enum BadgeVariant { Default, Secondary, Destructive, Outline, Success, Warning, Info }
