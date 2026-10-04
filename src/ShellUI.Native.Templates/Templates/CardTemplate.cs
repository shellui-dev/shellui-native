using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Card component template - container for content
public static class CardTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "card",
        DisplayName = "Card",
        Description = "Container component for grouping related content with optional header and footer",
        Category = ComponentCategory.Layout,
        FilePath = "Card.cs",
        Dependencies = new List<string> { "shell", "card-header", "card-content", "card-footer" },
        Variants = new List<string> { "default", "bordered", "elevated" },
        Tags = new List<string> { "layout", "container", "card", "panel", "surface" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Card container — rounded-lg border bg-card shadow-sm.
// Usage: <ui:Card><ui:CardHeader Title=""..."" /><ui:CardContent>...</ui:CardContent><ui:CardFooter>...</ui:CardFooter></ui:Card>
[ContentProperty(nameof(CardContent))]
public partial class Card : ContentView
{
    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(CardVariant), typeof(Card),
            CardVariant.Default, propertyChanged: OnVisualChanged);

    public static readonly BindableProperty IsPressableProperty =
        BindableProperty.Create(nameof(IsPressable), typeof(bool), typeof(Card), false);

    private readonly Border _border;
    private readonly VerticalStackLayout _container;

    public CardVariant Variant
    {
        get => (CardVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public bool IsPressable
    {
        get => (bool)GetValue(IsPressableProperty);
        set => SetValue(IsPressableProperty, value);
    }

    public event EventHandler? Clicked;

    public IList<IView> CardContent => _container.Children;

    public Card()
    {
        _container = new VerticalStackLayout { Spacing = 0 };

        _border = new Border
        {
            Content = _container,
            Padding = 0,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusLg + 4 }
        };
        _border.Token(VisualElement.BackgroundColorProperty, ShellToken.Card);
        _border.Token(Border.StrokeProperty, ShellToken.Border);

        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => { if (IsPressable) Clicked?.Invoke(this, EventArgs.Empty); };
        _border.GestureRecognizers.Add(tap);

        Content = _border;
        UpdateVisuals();
    }

    private static void OnVisualChanged(BindableObject b, object o, object n) => (b as Card)?.UpdateVisuals();

    private void UpdateVisuals()
    {
        _border.Shadow = new Shadow
        {
            Brush = new SolidColorBrush(Colors.Black),
            Offset = Variant == CardVariant.Elevated ? new Point(0, 4) : new Point(0, 1),
            Radius = Variant == CardVariant.Elevated ? 12 : 2,
            Opacity = Variant == CardVariant.Elevated ? 0.10f : 0.05f
        };
    }
}

public enum CardVariant { Default, Elevated }
"
    };
}
