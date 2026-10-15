namespace MAUI.Demo.Components.UI;

// Usage:
//   <ui:Tabs Value="account">
//       <ui:TabsList>
//           <ui:TabsTrigger Value="account" Text="Account" />
//           <ui:TabsTrigger Value="password" Text="Password" />
//       </ui:TabsList>
//       <ui:TabsContent Value="account">...</ui:TabsContent>
//       <ui:TabsContent Value="password">...</ui:TabsContent>
//   </ui:Tabs>
public partial class Tabs : VerticalStackLayout
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(Tabs), string.Empty,
            BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((Tabs)b).OnValueChanged((string?)o ?? string.Empty, (string?)n ?? string.Empty));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // (oldValue, newValue)
    // User input only: setting Value from code or a binding doesn't raise it.
    public event EventHandler<(string Old, string New)>? ValueChanged;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    public Tabs()
    {
        Spacing = 8;
        Loaded += (_, _) => Refresh(animate: false);
    }

    public void SetValue(string value) => Value = value;

    // A click or key on a trigger.
    internal void SelectByUser(string value) => ByUser(() => Value = value);

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    private void OnValueChanged(string oldValue, string newValue)
    {
        Refresh(animate: true);
        if (_byUser) ValueChanged?.Invoke(this, (oldValue, newValue));
    }

    // Tabs drives its triggers and panels, so state never depends on when a child found its parent.
    private void Refresh(bool animate)
    {
        foreach (var trigger in this.FindDescendantsOfType<TabsTrigger>(e => e is Tabs))
            trigger.SetActive(trigger.Value == Value);
        foreach (var content in this.FindDescendantsOfType<TabsContent>(e => e is Tabs))
            content.SetActive(content.Value == Value, animate);
    }
}
