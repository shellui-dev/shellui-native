using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Badge component template
public static class BadgeTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "badge",
        DisplayName = "Badge",
        Description = "Small status indicator with variant colors",
        Category = ComponentCategory.DataDisplay,
        FilePath = "Badge.cs",
        Dependencies = new List<string>(),
        Variants = new List<string> { "default", "secondary", "destructive", "outline", "success", "warning" },
        Tags = new List<string> { "status", "indicator", "badge", "tag", "chip" }
    };

    public static string Content => @"namespace YourProjectNamespace.Components.UI;

// Badge status indicator component
public partial class Badge : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Badge), 
            string.Empty, propertyChanged: OnTextChanged);

    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(BadgeVariant), typeof(Badge), 
            BadgeVariant.Default, propertyChanged: OnVisualPropertyChanged);

    private readonly Border _border;
    private readonly Label _label;

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public BadgeVariant Variant
    {
        get => (BadgeVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public Badge()
    {
        _label = new Label
        {
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        _border = new Border
        {
            Content = _label,
            Padding = new Thickness(10, 2),
            StrokeThickness = 1,
            HeightRequest = 22
        };

        Content = _border;
        UpdateVisualState();
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Badge badge)
            badge._label.Text = newValue as string ?? string.Empty;
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Badge badge)
            badge.UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        // Define color schemes for each variant
        var (bg, fg, border) = Variant switch
        {
            BadgeVariant.Default => (
                Color.FromArgb(""#1F2937""),
                Color.FromArgb(""#FFFFFF""),
                Color.FromArgb(""#1F2937"")
            ),
            BadgeVariant.Secondary => (
                Color.FromArgb(""#F3F4F6""),
                Color.FromArgb(""#374151""),
                Color.FromArgb(""#F3F4F6"")
            ),
            BadgeVariant.Destructive => (
                Color.FromArgb(""#EF4444""),
                Color.FromArgb(""#FFFFFF""),
                Color.FromArgb(""#EF4444"")
            ),
            BadgeVariant.Outline => (
                Colors.Transparent,
                Color.FromArgb(""#374151""),
                Color.FromArgb(""#E5E7EB"")
            ),
            BadgeVariant.Success => (
                Color.FromArgb(""#22C55E""),
                Color.FromArgb(""#FFFFFF""),
                Color.FromArgb(""#22C55E"")
            ),
            BadgeVariant.Warning => (
                Color.FromArgb(""#F59E0B""),
                Color.FromArgb(""#FFFFFF""),
                Color.FromArgb(""#F59E0B"")
            ),
            _ => (
                Color.FromArgb(""#1F2937""),
                Color.FromArgb(""#FFFFFF""),
                Color.FromArgb(""#1F2937"")
            )
        };

        _border.BackgroundColor = bg;
        _border.Stroke = border;
        _border.StrokeShape = new RoundRectangle { CornerRadius = 9999 }; // Full rounded
        _label.TextColor = fg;
    }
}

public enum BadgeVariant { Default, Secondary, Destructive, Outline, Success, Warning }
";
}
