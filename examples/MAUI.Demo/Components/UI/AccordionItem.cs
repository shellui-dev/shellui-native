namespace MAUI.Demo.Components.UI;

public partial class AccordionItem : VerticalStackLayout
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(AccordionItem), string.Empty);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // Item-local view of parent's open set. Trigger/Content subscribe here so they don't
    // have to know about the Accordion above.
    public bool IsOpen { get; private set; }
    public event EventHandler<bool>? OpenChanged;

    private Accordion? _parent;

    public AccordionItem()
    {
        Spacing = 0;
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (_parent != null)
            _parent.ItemToggled -= OnParentToggled;

        _parent = this.FindParentOfType<Accordion>();
        if (_parent != null)
        {
            _parent.ItemToggled += OnParentToggled;
            var open = _parent.IsOpen(Value);
            if (open != IsOpen)
            {
                IsOpen = open;
                OpenChanged?.Invoke(this, IsOpen);
            }
        }
    }

    private void OnParentToggled(object? sender, (string Value, bool IsOpen) e)
    {
        if (e.Value != Value) return;
        if (IsOpen == e.IsOpen) return;
        IsOpen = e.IsOpen;
        OpenChanged?.Invoke(this, IsOpen);
    }

    public void Toggle() => _parent?.Toggle(Value);
}
