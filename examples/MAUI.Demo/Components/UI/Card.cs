using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Card - A container component with header, content, footer slots
// Usage: <Card><CardHeader /><CardContent /><CardFooter /></Card>
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
            Padding = 0
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => { if (IsPressable) Clicked?.Invoke(this, EventArgs.Empty); };
        _border.GestureRecognizers.Add(tap);

        Content = _border;

        if (Application.Current != null)
            Application.Current.RequestedThemeChanged += (s, e) => UpdateVisuals();

        UpdateVisuals();
    }

    private static void OnVisualChanged(BindableObject b, object o, object n) => (b as Card)?.UpdateVisuals();

    private void UpdateVisuals()
    {
        _border.BackgroundColor = ShellTheme.BackgroundCard;
        _border.StrokeShape = new RoundRectangle { CornerRadius = 8 };

        switch (Variant)
        {
            case CardVariant.Default:
                _border.Stroke = ShellTheme.Border;
                _border.StrokeThickness = 1;
                _border.Shadow = new Shadow { Opacity = 0 };
                break;
            case CardVariant.Elevated:
                _border.Stroke = Colors.Transparent;
                _border.StrokeThickness = 0;
                _border.Shadow = new Shadow
                {
                    Brush = new SolidColorBrush(Color.FromArgb("#30000000")),
                    Offset = new Point(0, 4),
                    Radius = 12,
                    Opacity = ShellTheme.IsDarkMode ? 0.4f : 0.15f
                };
                break;
        }
    }
}

// Standalone CardHeader - use ui:CardHeader to avoid XAML parsing "Card.Header" as property
public class CardHeader : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(CardHeader), "", propertyChanged: OnTextChanged);

    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(CardHeader), "", propertyChanged: OnTextChanged);

    private readonly Label _title;
    private readonly Label _description;

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Description { get => (string)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }

    public CardHeader()
    {
        _title = new Label { FontSize = 18, FontAttributes = FontAttributes.Bold };
        _description = new Label { FontSize = 14, IsVisible = false };

        Content = new VerticalStackLayout
        {
            Spacing = 4,
            Padding = new Thickness(16, 16, 16, 8),
            Children = { _title, _description }
        };

        if (Application.Current != null)
            Application.Current.RequestedThemeChanged += (s, e) => UpdateColors();
        UpdateColors();
    }

    private static void OnTextChanged(BindableObject b, object o, object n)
    {
        if (b is CardHeader h)
        {
            h._title.Text = h.Title;
            h._description.Text = h.Description;
            h._description.IsVisible = !string.IsNullOrEmpty(h.Description);
        }
    }

    private void UpdateColors()
    {
        _title.TextColor = ShellTheme.Foreground;
        _description.TextColor = ShellTheme.ForegroundMuted;
    }
}

// Standalone CardContent - body area (inherits ContentProperty from ContentView)
public class CardContent : ContentView
{
    public CardContent()
    {
        Padding = new Thickness(16, 8);
    }
}

// Standalone CardFooter - footer area with action buttons
[ContentProperty(nameof(FooterContent))]
public class CardFooter : ContentView
{
    private readonly BoxView _separator;
    private readonly HorizontalStackLayout _content;

    public IList<IView> FooterContent => _content.Children;

    public CardFooter()
    {
        _separator = new BoxView { HeightRequest = 1, HorizontalOptions = LayoutOptions.Fill };
        _content = new HorizontalStackLayout
        {
            Spacing = 8,
            Padding = new Thickness(16, 12),
            HorizontalOptions = LayoutOptions.End
        };

        Content = new VerticalStackLayout
        {
            Spacing = 0,
            Children = { _separator, _content }
        };

        if (Application.Current != null)
            Application.Current.RequestedThemeChanged += (s, e) => UpdateColors();
        UpdateColors();
    }

    private void UpdateColors() => _separator.BackgroundColor = ShellTheme.Border;
}

public enum CardVariant { Default, Elevated }
