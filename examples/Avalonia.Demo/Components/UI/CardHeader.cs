using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Card header — p-6 space-y-1.5; title font-semibold, description text-sm text-muted-foreground.
public class CardHeader : Border
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<CardHeader, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<CardHeader, string?>(nameof(Description));

    private readonly TextBlock _title;
    private readonly TextBlock _description;

    static CardHeader()
    {
        TitleProperty.Changed.AddClassHandler<CardHeader>((h, _) => h.UpdateText());
        DescriptionProperty.Changed.AddClassHandler<CardHeader>((h, _) => h.UpdateText());
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public CardHeader()
    {
        _title = new TextBlock { FontSize = 18, FontWeight = FontWeight.SemiBold, TextWrapping = TextWrapping.Wrap };
        _title.Token(TextBlock.ForegroundProperty, ShellToken.CardForeground);
        _description = new TextBlock { FontSize = 14, TextWrapping = TextWrapping.Wrap, IsVisible = false };
        _description.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);

        Child = new StackPanel { Spacing = 6, Children = { _title, _description } };
        Padding = new Thickness(24, 24, 24, 16);
    }

    private void UpdateText()
    {
        _title.Text = Title ?? string.Empty;
        _description.Text = Description ?? string.Empty;
        _description.IsVisible = !string.IsNullOrEmpty(Description);
    }
}
