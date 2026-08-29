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
        Dependencies = new List<string>(),
        Variants = new List<string> { "default", "muted", "destructive" },
        Tags = new List<string> { "typography", "text", "label", "heading" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Typography label with variants
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
    {
        if (bindable is ShellLabel label)
            label.UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        // Font size based on Size enum
        _label.FontSize = Size switch
        {
            LabelSize.Xs => 12,
            LabelSize.Sm => 14,
            LabelSize.Default => 16,
            LabelSize.Lg => 18,
            LabelSize.Xl => 20,
            LabelSize.Xxl => 24,
            LabelSize.Xxxl => 30,
            _ => 16
        };

        // Font weight
        _label.FontAttributes = Weight switch
        {
            LabelWeight.Light => FontAttributes.None,
            LabelWeight.Normal => FontAttributes.None,
            LabelWeight.Medium => FontAttributes.None,
            LabelWeight.Semibold => FontAttributes.Bold,
            LabelWeight.Bold => FontAttributes.Bold,
            _ => FontAttributes.None
        };

        // Text color based on variant
        _label.TextColor = Variant switch
        {
            LabelVariant.Default => Color.FromArgb(""#1F2937""),
            LabelVariant.Muted => Color.FromArgb(""#6B7280""),
            LabelVariant.Destructive => Color.FromArgb(""#EF4444""),
            LabelVariant.Success => Color.FromArgb(""#22C55E""),
            LabelVariant.Warning => Color.FromArgb(""#F59E0B""),
            _ => Color.FromArgb(""#1F2937"")
        };
    }
}

public enum LabelSize { Xs, Sm, Default, Lg, Xl, Xxl, Xxxl }
public enum LabelWeight { Light, Normal, Medium, Semibold, Bold }
public enum LabelVariant { Default, Muted, Destructive, Success, Warning }
"
    };
}
