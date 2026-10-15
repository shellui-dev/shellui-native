namespace MAUI.Demo.Components.UI;

// Range slider — platform slider tinted with the theme (primary range, secondary track).
public partial class Slider : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(double), typeof(Slider), 50.0, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((Slider)b).SyncValue());

    public static readonly BindableProperty MinimumProperty =
        BindableProperty.Create(nameof(Minimum), typeof(double), typeof(Slider), 0.0,
            propertyChanged: (b, o, n) => ((Slider)b).SyncRange());

    public static readonly BindableProperty MaximumProperty =
        BindableProperty.Create(nameof(Maximum), typeof(double), typeof(Slider), 100.0,
            propertyChanged: (b, o, n) => ((Slider)b).SyncRange());

    private readonly Microsoft.Maui.Controls.Slider _native;

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    // User input only: setting Value from code or a binding doesn't raise it.
    public event EventHandler<ValueChangedEventArgs>? ValueChanged;

    // Set while Value, Minimum or Maximum is pushed into the platform slider, which reports
    // those changes like a drag.
    private bool _syncing;

    public Slider()
    {
        _native = new Microsoft.Maui.Controls.Slider { Minimum = 0, Maximum = 100, Value = 50 };
        _native.Token(Microsoft.Maui.Controls.Slider.MinimumTrackColorProperty, ShellToken.Primary);
        _native.Token(Microsoft.Maui.Controls.Slider.MaximumTrackColorProperty, ShellToken.Secondary);
        _native.Token(Microsoft.Maui.Controls.Slider.ThumbColorProperty, ShellToken.Primary);
        _native.ValueChanged += (s, e) =>
        {
            Value = e.NewValue;
            if (!_syncing) ValueChanged?.Invoke(this, e);
        };
        Content = _native;
    }

    private void SyncRange()
    {
        var syncing = _syncing;
        _syncing = true;
        // Widen before narrowing so Minimum never exceeds Maximum mid-update.
        if (Maximum > _native.Maximum) { _native.Maximum = Maximum; _native.Minimum = Minimum; }
        else { _native.Minimum = Minimum; _native.Maximum = Maximum; }
        SyncValue();
        _syncing = syncing;
    }

    private void SyncValue()
    {
        if (Math.Abs(_native.Value - Value) <= 0.001) return;
        var syncing = _syncing;
        _syncing = true;
        _native.Value = Value;
        _syncing = syncing;
    }
}
