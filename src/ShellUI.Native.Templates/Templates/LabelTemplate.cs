using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Label component template
public static class LabelTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "label",
        DisplayName = "Label",
        Description = "Typography label with size and weight variants",
        Category = ComponentCategory.Typography,
        FilePath = "Label.cs",
        Dependencies = new List<string> { "shell" },
        Variants = new List<string> { "default", "muted", "destructive" },
        Tags = new List<string> { "typography", "text", "label", "heading" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Typography label with size / weight / variant — theme-aware.
public partial class ShellLabel : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(ShellLabel),
            string.Empty, propertyChanged: OnTextChanged);

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(LabelSize), typeof(ShellLabel),
            LabelSize.Default, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty WeightProperty =
        BindableProperty.Create(nameof(Weight), typeof(LabelWeight), typeof(ShellLabel),
            LabelWeight.Normal, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(LabelVariant), typeof(ShellLabel),
            LabelVariant.Default, propertyChanged: OnVisualPropertyChanged);

    private readonly Label _label;

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public LabelSize Size
    {
        get => (LabelSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public LabelWeight Weight
    {
        get => (LabelWeight)GetValue(WeightProperty);
        set => SetValue(WeightProperty, value);
    }

    public LabelVariant Variant
    {
        get => (LabelVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public ShellLabel()
    {
        _label = new Label();
        Content = _label;
        UpdateVisualState();
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ShellLabel label)
            label._label.Text = newValue as string ?? string.Empty;
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        => (bindable as ShellLabel)?.UpdateVisualState();

    private void UpdateVisualState()
    {
        _label.FontSize = Size switch
        {
            LabelSize.Xs => 12,
            LabelSize.Sm => 14,
            LabelSize.Lg => 18,
            LabelSize.Xl => 20,
            LabelSize.Xxl => 24,
            LabelSize.Xxxl => 30,
            _ => 16
        };

        // MAUI exposes only regular/bold without a custom font family.
        _label.FontAttributes = Weight is LabelWeight.Semibold or LabelWeight.Bold
            ? FontAttributes.Bold
            : FontAttributes.None;

        _label.Token(Label.TextColorProperty, Variant switch
        {
            LabelVariant.Muted => ShellToken.MutedForeground,
            LabelVariant.Destructive => ShellToken.Destructive,
            LabelVariant.Success => ShellToken.Success,
            LabelVariant.Warning => ShellToken.Warning,
            _ => ShellToken.Foreground
        });
    }
}

public enum LabelSize { Xs, Sm, Default, Lg, Xl, Xxl, Xxxl }
public enum LabelWeight { Light, Normal, Medium, Semibold, Bold }
public enum LabelVariant { Default, Muted, Destructive, Success, Warning }
",
        [NativePlatform.Avalonia] = @"using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace YourProjectNamespace.Components.UI;

// Typography label with size / weight / variant — theme-aware.
// Named ShellLabel because Avalonia already has a Label control.
public class ShellLabel : TextBlock
{
    public static readonly StyledProperty<LabelSize> SizeProperty =
        AvaloniaProperty.Register<ShellLabel, LabelSize>(nameof(Size), LabelSize.Default);

    public static readonly StyledProperty<LabelWeight> WeightProperty =
        AvaloniaProperty.Register<ShellLabel, LabelWeight>(nameof(Weight), LabelWeight.Normal);

    public static readonly StyledProperty<LabelVariant> VariantProperty =
        AvaloniaProperty.Register<ShellLabel, LabelVariant>(nameof(Variant));

    static ShellLabel()
    {
        SizeProperty.Changed.AddClassHandler<ShellLabel>((l, _) => l.UpdateVisualState());
        WeightProperty.Changed.AddClassHandler<ShellLabel>((l, _) => l.UpdateVisualState());
        VariantProperty.Changed.AddClassHandler<ShellLabel>((l, _) => l.UpdateVisualState());
    }

    public LabelSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public LabelWeight Weight
    {
        get => GetValue(WeightProperty);
        set => SetValue(WeightProperty, value);
    }

    public LabelVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    // Wraps like MAUI's Label; TextWrapping=""NoWrap"" keeps it on one line.
    public ShellLabel()
    {
        TextWrapping = TextWrapping.Wrap;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        FontSize = Size switch
        {
            LabelSize.Xs => 12,
            LabelSize.Sm => 14,
            LabelSize.Lg => 18,
            LabelSize.Xl => 20,
            LabelSize.Xxl => 24,
            LabelSize.Xxxl => 30,
            _ => 16
        };

        FontWeight = Weight switch
        {
            LabelWeight.Light => FontWeight.Light,
            LabelWeight.Medium => FontWeight.Medium,
            LabelWeight.Semibold => FontWeight.SemiBold,
            LabelWeight.Bold => FontWeight.Bold,
            _ => FontWeight.Normal
        };

        this.Token(ForegroundProperty, Variant switch
        {
            LabelVariant.Muted => ShellToken.MutedForeground,
            LabelVariant.Destructive => ShellToken.Destructive,
            LabelVariant.Success => ShellToken.Success,
            LabelVariant.Warning => ShellToken.Warning,
            _ => ShellToken.Foreground
        });
    }
}

public enum LabelSize { Xs, Sm, Default, Lg, Xl, Xxl, Xxxl }
public enum LabelWeight { Light, Normal, Medium, Semibold, Bold }
public enum LabelVariant { Default, Muted, Destructive, Success, Warning }
"
    };
}
