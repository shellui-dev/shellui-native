using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

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

    // Wraps like MAUI's Label; TextWrapping="NoWrap" keeps it on one line.
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
