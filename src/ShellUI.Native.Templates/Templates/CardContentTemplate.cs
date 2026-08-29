using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// CardContent component template
public static class CardContentTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "card-content",
        DisplayName = "Card Content",
        Description = "Main content section for Card component",
        Category = ComponentCategory.Layout,
        FilePath = "CardContent.cs",
        Dependencies = new List<string>(),
        Variants = new List<string>(),
        Tags = new List<string> { "layout", "card", "content" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Card main content section
public partial class CardContent : ContentView
{
    public static readonly BindableProperty NoPaddingProperty =
        BindableProperty.Create(nameof(NoPadding), typeof(bool), typeof(CardContent), 
            false, propertyChanged: OnNoPaddingChanged);

    private readonly ContentView _innerContent;

    public bool NoPadding
    {
        get => (bool)GetValue(NoPaddingProperty);
        set => SetValue(NoPaddingProperty, value);
    }

    public CardContent()
    {
        _innerContent = new ContentView
        {
            Padding = new Thickness(16, 8)
        };

        Content = _innerContent;
    }

    /*
     * Proxy content to inner container
     * Allows normal XAML content binding to work correctly
     */
    public new View? Content
    {
        get => _innerContent.Content;
        set
        {
            if (value != _innerContent)
                _innerContent.Content = value;
        }
    }

    private static void OnNoPaddingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CardContent content)
        {
            content._innerContent.Padding = (bool)newValue 
                ? new Thickness(0) 
                : new Thickness(16, 8);
        }
    }
}
"
    };
}
