namespace MAUI.Demo.Components.UI;

public enum AccordionType { Single, Multiple }

// Stacked expandable sections with dividers and rotating chevrons.
//   <ui:Accordion Type="Single" Value="a">
//       <ui:AccordionItem Value="a">
//           <ui:AccordionTrigger Text="Is it accessible?" />
//           <ui:AccordionContent><Label Text="Yes." /></ui:AccordionContent>
//       </ui:AccordionItem>
//   </ui:Accordion>
public partial class Accordion : VerticalStackLayout
{
    public static readonly BindableProperty TypeProperty =
        BindableProperty.Create(nameof(Type), typeof(AccordionType), typeof(Accordion), AccordionType.Single);

    // Initially open item (Single) — or comma-separated items (Multiple).
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(Accordion), string.Empty,
            propertyChanged: (b, o, n) => ((Accordion)b).OnValueChanged((string?)n));

    public AccordionType Type
    {
        get => (AccordionType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // Fired when any item's open state changes: (value, isOpen).
    public event EventHandler<(string Value, bool IsOpen)>? ItemToggled;

    private readonly HashSet<string> _openValues = new();

    public Accordion()
    {
        Spacing = 0;
        Loaded += (_, _) => Refresh(animate: false);
    }

    public bool IsOpen(string value) => _openValues.Contains(value);

    public void Toggle(string value)
    {
        var wasOpen = _openValues.Contains(value);
        var changed = new List<(string, bool)>();

        if (Type == AccordionType.Single)
        {
            foreach (var other in _openValues.Where(v => v != value).ToList())
            {
                _openValues.Remove(other);
                changed.Add((other, false));
            }
        }

        if (wasOpen) _openValues.Remove(value);
        else _openValues.Add(value);
        changed.Add((value, !wasOpen));

        Refresh(animate: true);
        foreach (var change in changed) ItemToggled?.Invoke(this, change);
    }

    private void OnValueChanged(string? value)
    {
        _openValues.Clear();
        foreach (var v in (value ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            _openValues.Add(v);
            if (Type == AccordionType.Single) break;
        }
        Refresh(animate: IsLoaded);
    }

    // The accordion drives its items, so state never depends on when a child found its parent.
    private void Refresh(bool animate)
    {
        var items = this.FindDescendantsOfType<AccordionItem>(e => e is Accordion).ToList();
        for (var i = 0; i < items.Count; i++)
        {
            items[i].ApplyOpen(_openValues.Contains(items[i].Value), animate);
            items[i].SetDividerVisible(i < items.Count - 1);
        }
    }
}
