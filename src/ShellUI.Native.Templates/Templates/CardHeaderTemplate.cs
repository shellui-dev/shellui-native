using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// CardHeader component template
public static class CardHeaderTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "card-header",
        DisplayName = "Card Header",
        Description = "Header section for Card component with title and description",
        Category = ComponentCategory.Layout,
        FilePath = "CardHeader.cs",
        Dependencies = new List<string> { "shell" },
        Variants = new List<string>(),
        Tags = new List<string> { "layout", "card", "header" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Card header — p-6 space-y-1.5; title font-semibold, description text-sm text-muted-foreground.
public partial class CardHeader : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(CardHeader), string.Empty,
            propertyChanged: (b, o, n) => (b as CardHeader)?.UpdateText());

    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(CardHeader), string.Empty,
            propertyChanged: (b, o, n) => (b as CardHeader)?.UpdateText());

    private readonly Label _title;
    private readonly Label _description;

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public CardHeader()
    {
        _title = new Label { FontSize = 18, FontAttributes = FontAttributes.Bold };
        _title.Token(Label.TextColorProperty, ShellToken.CardForeground);
        _description = new Label { FontSize = 14, IsVisible = false };
        _description.Token(Label.TextColorProperty, ShellToken.MutedForeground);

        Content = new VerticalStackLayout
        {
            Spacing = 6,
            Padding = new Thickness(24, 24, 24, 16),
            Children = { _title, _description }
        };
    }

    private void UpdateText()
    {
        _title.Text = Title ?? string.Empty;
        _description.Text = Description ?? string.Empty;
        _description.IsVisible = !string.IsNullOrEmpty(Description);
    }
}
",
        [NativePlatform.Avalonia] = @"using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace YourProjectNamespace.Components.UI;

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
"
    };
}
