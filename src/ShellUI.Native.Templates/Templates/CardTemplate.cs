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
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;

namespace YourProjectNamespace.Components.UI;

// Card container — rounded-lg border bg-card shadow-sm.
// The parts stack inside; XAML children go to Items.
// Usage: <ui:Card><ui:CardHeader Title=""..."" /><ui:CardContent>...</ui:CardContent><ui:CardFooter>...</ui:CardFooter></ui:Card>
public class Card : Border
{
    public static readonly StyledProperty<CardVariant> VariantProperty =
        AvaloniaProperty.Register<Card, CardVariant>(nameof(Variant));

    public static readonly StyledProperty<bool> IsPressableProperty =
        AvaloniaProperty.Register<Card, bool>(nameof(IsPressable));

    private const double Radius = ShellTheme.RadiusLg + 4;

    private readonly StackPanel _items = new();

    static Card()
    {
        VariantProperty.Changed.AddClassHandler<Card>((c, _) => c.UpdateShadow());
        IsPressableProperty.Changed.AddClassHandler<Card>((c, _) =>
            c.Cursor = c.IsPressable ? new Cursor(StandardCursorType.Hand) : null);
    }

    public CardVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public bool IsPressable
    {
        get => GetValue(IsPressableProperty);
        set => SetValue(IsPressableProperty, value);
    }

    public event EventHandler? Clicked;

    [Content]
    public Controls Items => _items.Children;

    public Card()
    {
        Child = _items;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(Radius);
        this.Token(BackgroundProperty, ShellToken.Card).Token(BorderBrushProperty, ShellToken.Border);
        UpdateShadow();
    }

    private void UpdateShadow() => BoxShadow = new BoxShadows(Variant == CardVariant.Elevated
        ? new BoxShadow { OffsetY = 4, Blur = 12, Color = ShellTheme.Shadow(0.10) }
        : new BoxShadow { OffsetY = 1, Blur = 2, Color = ShellTheme.Shadow(0.05) });

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!IsPressable || e.InitialPressMouseButton != MouseButton.Left) return;
        Clicked?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }
}

public enum CardVariant { Default, Elevated }
"
    };
}
