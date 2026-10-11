using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Keyboard key — h-5 rounded border bg-muted px-1.5 text-[11px] font-medium text-muted-foreground.
// Usage: <ui:Kbd Text="Ctrl" /> <ui:Kbd Text="K" />
public class Kbd : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Kbd, string?>(nameof(Text));

    private readonly TextBlock _label;

    static Kbd()
    {
        TextProperty.Changed.AddClassHandler<Kbd>((k, e) => k._label.Text = (string?)e.NewValue ?? string.Empty);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Kbd()
    {
        _label = new TextBlock
        {
            FontSize = 11,
            FontWeight = FontWeight.Medium,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _label.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);

        Child = _label;
        Height = 22;
        MinWidth = 22;
        Padding = new Thickness(6, 0);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusSm);
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Center;
        this.Token(BackgroundProperty, ShellToken.Muted).Token(BorderBrushProperty, ShellToken.Border);
    }
}
