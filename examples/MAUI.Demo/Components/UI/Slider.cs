namespace MAUI.Demo.Components.UI;

public partial class Slider : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(double), typeof(Slider), 50.0, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => (b as Slider)?.OnValueChanged());

    public static readonly BindableProperty MinimumProperty =
        BindableProperty.Create(nameof(Minimum), typeof(double), typeof(Slider), 0.0);

    public static readonly BindableProperty MaximumProperty =
        BindableProperty.Create(nameof(Maximum), typeof(double), typeof(Slider), 100.0);

    private readonly Microsoft.Maui.Controls.Slider _nativeSlider;

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

    public event EventHandler<ValueChangedEventArgs>? ValueChanged;

    public Slider()
    {
        _nativeSlider = new Microsoft.Maui.Controls.Slider();
        _nativeSlider.ValueChanged += (s, e) =>
        {
            Value = e.NewValue;
            ValueChanged?.Invoke(this, e);
        };
        Content = _nativeSlider;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        _nativeSlider.Minimum = Minimum;
        _nativeSlider.Maximum = Maximum;
        _nativeSlider.Value = Value;
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == MinimumProperty.PropertyName) _nativeSlider.Minimum = Minimum;
        else if (propertyName == MaximumProperty.PropertyName) _nativeSlider.Maximum = Maximum;
    }

    private void OnValueChanged()
    {
        if (Math.Abs(_nativeSlider.Value - Value) > 0.001)
            _nativeSlider.Value = Value;
    }
}
