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
        Dependencies = new List<string> { "shell" },
        Variants = new List<string> { "default", "secondary", "destructive", "outline", "success", "warning" },
        Tags = new List<string> { "status", "indicator", "badge", "tag", "chip" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Status pill — rounded-full border px-2.5 py-0.5 text-xs font-semibold.
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
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalTextAlignment = TextAlignment.Center
        };

        _border = new Border
        {
            Content = _label,
            Padding = new Thickness(10, 2),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 999 }
        };

        Content = _border;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        UpdateVisualState();
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Badge badge)
            badge._label.Text = newValue as string ?? string.Empty;
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        => (bindable as Badge)?.UpdateVisualState();

    private void UpdateVisualState()
    {
        // (background, foreground, border); null background = transparent.
        var (bg, fg, border) = Variant switch
        {
            BadgeVariant.Secondary => ((ShellToken?)ShellToken.Secondary, ShellToken.SecondaryForeground, ShellToken.Secondary),
            BadgeVariant.Destructive => (ShellToken.Destructive, ShellToken.DestructiveForeground, ShellToken.Destructive),
            BadgeVariant.Outline => (null, ShellToken.Foreground, ShellToken.Border),
            BadgeVariant.Success => (ShellToken.Success, ShellToken.SuccessForeground, ShellToken.Success),
            BadgeVariant.Warning => (ShellToken.Warning, ShellToken.WarningForeground, ShellToken.Warning),
            BadgeVariant.Info => (ShellToken.Info, ShellToken.InfoForeground, ShellToken.Info),
            _ => (ShellToken.Primary, ShellToken.PrimaryForeground, ShellToken.Primary)
        };

        if (bg.HasValue)
            _border.Token(VisualElement.BackgroundColorProperty, bg.Value);
        else
        {
            _border.ClearValue(VisualElement.BackgroundColorProperty);
            _border.BackgroundColor = Colors.Transparent;
        }
        _border.Token(Border.StrokeProperty, border);
        _label.Token(Label.TextColorProperty, fg);
    }
}

public enum BadgeVariant { Default, Secondary, Destructive, Outline, Success, Warning, Info }
",
        [NativePlatform.Avalonia] = @"using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace YourProjectNamespace.Components.UI;

// Status pill — rounded-full border px-2.5 py-0.5 text-xs font-semibold.
public class Badge : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Badge, string?>(nameof(Text));

    public static readonly StyledProperty<BadgeVariant> VariantProperty =
        AvaloniaProperty.Register<Badge, BadgeVariant>(nameof(Variant));

    private readonly TextBlock _label;

    static Badge()
    {
        TextProperty.Changed.AddClassHandler<Badge>((b, e) => b._label.Text = (string?)e.NewValue ?? string.Empty);
        VariantProperty.Changed.AddClassHandler<Badge>((b, _) => b.UpdateVisualState());
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public BadgeVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public Badge()
    {
        _label = new TextBlock
        {
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        Child = _label;
        Padding = new Thickness(10, 2);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(999);
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Center;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        // (background, foreground, border); null background = transparent.
        var (bg, fg, border) = Variant switch
        {
            BadgeVariant.Secondary => ((ShellToken?)ShellToken.Secondary, ShellToken.SecondaryForeground, ShellToken.Secondary),
            BadgeVariant.Destructive => (ShellToken.Destructive, ShellToken.DestructiveForeground, ShellToken.Destructive),
            BadgeVariant.Outline => (null, ShellToken.Foreground, ShellToken.Border),
            BadgeVariant.Success => (ShellToken.Success, ShellToken.SuccessForeground, ShellToken.Success),
            BadgeVariant.Warning => (ShellToken.Warning, ShellToken.WarningForeground, ShellToken.Warning),
            BadgeVariant.Info => (ShellToken.Info, ShellToken.InfoForeground, ShellToken.Info),
            _ => (ShellToken.Primary, ShellToken.PrimaryForeground, ShellToken.Primary)
        };

        if (bg.HasValue)
            this.Token(BackgroundProperty, bg.Value);
        else
        {
            this.ClearToken(BackgroundProperty);
            Background = Brushes.Transparent;
        }
        this.Token(BorderBrushProperty, border);
        _label.Token(TextBlock.ForegroundProperty, fg);
    }
}

public enum BadgeVariant { Default, Secondary, Destructive, Outline, Success, Warning, Info }
"
    };
}
