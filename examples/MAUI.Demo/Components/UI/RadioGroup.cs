namespace MAUI.Demo.Components.UI;

// Single-choice group. Usage:
//   <ui:RadioGroup Value="a"><ui:RadioGroupItem Value="a" Text="Option A" />...</ui:RadioGroup>
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

    // User input only: setting Value from code or a binding doesn't raise it.
    public event EventHandler<string>? ValueChanged;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    private readonly VerticalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public RadioGroup()
    {
        _stack = new VerticalStackLayout { Spacing = 12 };
        Content = _stack;
        Loaded += (_, _) => RefreshItems();
    }

    public void SetValue(string value) => Value = value;

    // A click or key on an item.
    internal void SelectByUser(string value) => ByUser(() => Value = value);

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    private static void OnValueChanged(BindableObject b, object o, object n)
    {
        if (b is not RadioGroup group) return;
        group.RefreshItems();
        if (group._byUser) group.ValueChanged?.Invoke(group, n as string ?? string.Empty);
    }

    // The group drives its items, so item state never depends on when an item found its parent.
    internal void RefreshItems()
    {
        foreach (var item in this.FindDescendantsOfType<RadioGroupItem>(e => e is RadioGroup))
            item.SetChecked(item.Value == Value);
    }
}
