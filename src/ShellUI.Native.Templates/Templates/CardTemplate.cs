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
        Dependencies = new List<string> { "card-header", "card-content", "card-footer" },
        Variants = new List<string> { "default", "bordered", "elevated" },
        Tags = new List<string> { "layout", "container", "card", "panel", "surface" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;
namespace YourProjectNamespace.Components.UI;

// Card container - compositional children via Dependencies (CardHeader, CardContent, CardFooter)
// Usage: <Card><CardHeader /><CardContent /><CardFooter /></Card>
[ContentProperty(nameof(Children))]
public partial class Card : ContentView
{
    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(CardVariant), typeof(Card), 
            CardVariant.Default, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty IsPressableProperty =
        BindableProperty.Create(nameof(IsPressable), typeof(bool), typeof(Card), 
            false, propertyChanged: OnVisualPropertyChanged);

    private readonly Border _border;
    private readonly VerticalStackLayout _contentStack;

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

    /// <summary>Children collection for CardHeader, CardContent, CardFooter (compositional pattern)</summary>
    public new IList<IView> Children => _contentStack.Children;

    public event EventHandler? Clicked;

    public Card()
    {
        _contentStack = new VerticalStackLayout { Spacing = 0 };
        _border = new Border { Content = _contentStack, Padding = 0 };

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            if (IsPressable) Clicked?.Invoke(this, EventArgs.Empty);
        };
        _border.GestureRecognizers.Add(tapGesture);
        Content = _border;
        UpdateVisualState();
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Card card) card.UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        var backgroundColor = Color.FromArgb(""#FFFFFF"");
        var borderColor = Color.FromArgb(""#E5E7EB"");
        _border.BackgroundColor = backgroundColor;
        _border.StrokeShape = new RoundRectangle { CornerRadius = 8 };

        switch (Variant)
        {
            case CardVariant.Default:
                _border.Stroke = borderColor;
                _border.StrokeThickness = 1;
                _border.Shadow = null;
                break;
            case CardVariant.Bordered:
                _border.Stroke = borderColor;
                _border.StrokeThickness = 2;
                _border.Shadow = null;
                break;
            case CardVariant.Elevated:
                _border.Stroke = Colors.Transparent;
                _border.StrokeThickness = 0;
                _border.Shadow = new Shadow
                {
                    Brush = new SolidColorBrush(Color.FromArgb(""#20000000"")),
                    Offset = new Point(0, 4),
                    Radius = 8,
                    Opacity = 0.15f
                };
                break;
        }
    }
}

public enum CardVariant { Default, Bordered, Elevated }
"
    };
}
