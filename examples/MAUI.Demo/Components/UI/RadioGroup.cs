namespace MAUI.Demo.Components.UI;

[ContentProperty(nameof(Children))]
public partial class RadioGroup : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(RadioGroup), 
            string.Empty, BindingMode.TwoWay, propertyChanged: OnValueChanged);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public event EventHandler<string>? ValueChanged;

    private readonly VerticalStackLayout _stack;

    public IList<IView> Children => _stack.Children;

    public RadioGroup()
    {
        _stack = new VerticalStackLayout { Spacing = 8 };
        Content = _stack;
    }

    public void SetValue(string value)
    {
        if (Value != value) { Value = value; ValueChanged?.Invoke(this, value); }
    }

    private static void OnValueChanged(BindableObject b, object o, object n) { }
}
