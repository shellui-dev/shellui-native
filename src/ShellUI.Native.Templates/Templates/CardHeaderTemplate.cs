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
        Dependencies = new List<string>(),
        Variants = new List<string>(),
        Tags = new List<string> { "layout", "card", "header" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Card header section
public partial class CardHeader : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(CardHeader), 
            string.Empty, propertyChanged: OnTitleChanged);

    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(CardHeader), 
            string.Empty, propertyChanged: OnDescriptionChanged);

    private readonly Label _titleLabel;
    private readonly Label _descriptionLabel;
    private readonly VerticalStackLayout _stack;

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
        _titleLabel = new Label
        {
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(""#1F2937"")
        };

        _descriptionLabel = new Label
        {
            FontSize = 14,
            TextColor = Color.FromArgb(""#6B7280""),
            IsVisible = false
        };

        _stack = new VerticalStackLayout
        {
            Spacing = 4,
            Padding = new Thickness(16, 16, 16, 8),
            Children = { _titleLabel, _descriptionLabel }
        };

        Content = _stack;
    }

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CardHeader header)
            header._titleLabel.Text = newValue as string ?? string.Empty;
    }

    private static void OnDescriptionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CardHeader header)
        {
            var text = newValue as string ?? string.Empty;
            header._descriptionLabel.Text = text;
            header._descriptionLabel.IsVisible = !string.IsNullOrEmpty(text);
        }
    }
}
"
    };
}
