namespace MAUI.Demo.Components.UI;

// Typography label with variants - theme-aware
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
        
        // Listen for theme changes
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeChanged += (s, e) => UpdateVisualState();
        }
        
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

        // Theme-aware text color based on variant
        _label.TextColor = Variant switch
        {
            LabelVariant.Default => ShellTheme.Foreground,
            LabelVariant.Muted => ShellTheme.ForegroundMuted,
            LabelVariant.Destructive => ShellTheme.Destructive,
            LabelVariant.Success => ShellTheme.Success,
            LabelVariant.Warning => ShellTheme.Warning,
            _ => ShellTheme.Foreground
        };
    }
}

public enum LabelSize { Xs, Sm, Default, Lg, Xl, Xxl, Xxxl }
public enum LabelWeight { Light, Normal, Medium, Semibold, Bold }
public enum LabelVariant { Default, Muted, Destructive, Success, Warning }
