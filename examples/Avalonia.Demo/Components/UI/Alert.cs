using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Callout with icon, title and message — rounded-lg border p-4, icon + text tinted per variant.
public class Alert : Border
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Alert, string?>(nameof(Title));

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<Alert, string?>(nameof(Message));

    public static readonly StyledProperty<AlertVariant> VariantProperty =
        AvaloniaProperty.Register<Alert, AlertVariant>(nameof(Variant));

    private readonly Icon _icon;
    private readonly TextBlock _titleLabel;
    private readonly TextBlock _messageLabel;

    static Alert()
    {
        TitleProperty.Changed.AddClassHandler<Alert>((a, _) => a.UpdateVisualState());
        MessageProperty.Changed.AddClassHandler<Alert>((a, _) => a.UpdateVisualState());
        VariantProperty.Changed.AddClassHandler<Alert>((a, _) => a.UpdateVisualState());
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public AlertVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public Alert()
    {
        _icon = new Icon { Size = 16, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 2, 0, 0) };
        _titleLabel = new TextBlock { FontSize = 14, FontWeight = FontWeight.SemiBold, TextWrapping = TextWrapping.Wrap };
        _messageLabel = new TextBlock { FontSize = 14, TextWrapping = TextWrapping.Wrap };

        var text = new StackPanel { Spacing = 4, Children = { _titleLabel, _messageLabel } };
        Grid.SetColumn(text, 1);
        Child = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*"), ColumnSpacing = 12, Children = { _icon, text } };
        Padding = new Thickness(16, 12);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusLg);
        this.Token(BackgroundProperty, ShellToken.Card);
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        var (icon, accent) = Variant switch
        {
            AlertVariant.Destructive => (IconName.CircleAlert, ShellToken.Destructive),
            AlertVariant.Success => (IconName.CircleCheck, ShellToken.Success),
            AlertVariant.Warning => (IconName.TriangleAlert, ShellToken.Warning),
            AlertVariant.Info => (IconName.Info, ShellToken.Info),
            _ => (IconName.Info, ShellToken.Foreground)
        };

        _icon.Kind = icon;
        _icon.Token = accent;
        _titleLabel.Text = Title ?? string.Empty;
        _titleLabel.IsVisible = !string.IsNullOrEmpty(Title);
        _messageLabel.Text = Message ?? string.Empty;
        _messageLabel.IsVisible = !string.IsNullOrEmpty(Message);

        // Tinted variants color the title; the message stays readable in the muted foreground.
        _titleLabel.Token(TextBlock.ForegroundProperty, accent);
        _messageLabel.Token(TextBlock.ForegroundProperty,
            Variant == AlertVariant.Default ? ShellToken.MutedForeground : ShellToken.Foreground);
        this.Token(BorderBrushProperty, Variant == AlertVariant.Default ? ShellToken.Border : accent);
    }
}

public enum AlertVariant
{
    Default,
    Destructive,
    Success,
    Warning,
    Info
}
