namespace MAUI.Demo.Components.UI;

public enum AccordionType { Single, Multiple }

// Usage:
//   <Accordion Type="Single">
//       <AccordionItem Value="a"><AccordionTrigger>...</AccordionTrigger><AccordionContent>...</AccordionContent></AccordionItem>
//       <AccordionItem Value="b">...</AccordionItem>
//   </Accordion>
public partial class Accordion : VerticalStackLayout
{
    public static readonly BindableProperty TypeProperty =
        BindableProperty.Create(nameof(Type), typeof(AccordionType), typeof(Accordion), AccordionType.Single);

    public AccordionType Type
    {
        get => (AccordionType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    // Fired when any item's open state changes. (value, isOpen) — enough for consumers
    // to persist selection or lazy-load content.
    public event EventHandler<(string Value, bool IsOpen)>? ItemToggled;

    private readonly HashSet<string> _openValues = new();

    public Accordion()
    {
        Spacing = 0;
    }

    public bool IsOpen(string value) => _openValues.Contains(value);

    public void Toggle(string value)
    {
        var wasOpen = _openValues.Contains(value);

        if (Type == AccordionType.Single)
        {
            // Close every other item — collect first so we can announce them.
            var closing = _openValues.Where(v => v != value).ToList();
            _openValues.Clear();
            foreach (var v in closing) NotifyItem(v, false);
        }

        if (wasOpen) _openValues.Remove(value);
        else _openValues.Add(value);
        NotifyItem(value, !wasOpen);
    }

    private void NotifyItem(string value, bool isOpen)
    {
        ItemToggled?.Invoke(this, (value, isOpen));
    }
}
