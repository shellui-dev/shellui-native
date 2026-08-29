namespace MAUI.Demo.Components.UI;

// Usage:
//   <Tabs Value="tab1">
//       <TabsList>
//           <TabsTrigger Value="tab1" Text="One" />
//           <TabsTrigger Value="tab2" Text="Two" />
//       </TabsList>
//       <TabsContent Value="tab1">...</TabsContent>
//       <TabsContent Value="tab2">...</TabsContent>
//   </Tabs>
public partial class Tabs : VerticalStackLayout
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(Tabs), string.Empty,
            BindingMode.TwoWay,
            propertyChanged: (b, o, n) => (b as Tabs)?.OnValueChanged((string?)o ?? string.Empty, (string?)n ?? string.Empty));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // (oldValue, newValue). Triggers/Content subscribe to update their active state.
    public event EventHandler<(string Old, string New)>? ValueChanged;

    public Tabs()
    {
        Spacing = 0;
    }

    public void SetValue(string value)
    {
        if (Value != value) Value = value;
    }

    private void OnValueChanged(string oldValue, string newValue) => ValueChanged?.Invoke(this, (oldValue, newValue));
}
