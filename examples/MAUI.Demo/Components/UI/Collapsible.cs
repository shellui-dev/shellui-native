namespace MAUI.Demo.Components.UI;

// Expand/collapse. The trigger can wrap a Button or any view; content animates its height.
//   <ui:Collapsible Open="{Binding IsOpen}">
//       <ui:CollapsibleTrigger><ui:Button Text="Toggle" Variant="Outline" /></ui:CollapsibleTrigger>
//       <ui:CollapsibleContent><Label Text="Hidden until open" /></ui:CollapsibleContent>
//   </ui:Collapsible>
public partial class Collapsible : VerticalStackLayout
{
    public static readonly BindableProperty OpenProperty =
        BindableProperty.Create(nameof(Open), typeof(bool), typeof(Collapsible), false,
            BindingMode.TwoWay, propertyChanged: (b, o, n) => ((Collapsible)b).OnOpenChanged());

    public bool Open
    {
        get => (bool)GetValue(OpenProperty);
        set => SetValue(OpenProperty, value);
    }

    // User input only: setting Open, or calling Toggle / SetOpen, from code doesn't raise it.
    public event EventHandler<bool>? OpenChanged;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    public Collapsible()
    {
        Spacing = 8;
        Loaded += (_, _) => Refresh(animate: false);
    }

    public void SetOpen(bool value) => Open = value;

    public void Toggle() => Open = !Open;

    // A click or key on the trigger.
    internal void ToggleByUser() => ByUser(Toggle);

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    private void OnOpenChanged()
    {
        Refresh(animate: true);
        if (_byUser) OpenChanged?.Invoke(this, Open);
    }

    // The collapsible drives its content, so state never depends on when a child found its parent.
    private void Refresh(bool animate)
    {
        foreach (var content in this.FindDescendantsOfType<CollapsibleContent>(e => e is Collapsible))
            content.Apply(Open, animate);
    }
}
