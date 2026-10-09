using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;

namespace AvaloniaDemo.Components.UI;

public enum AccordionType { Single, Multiple }

// Stacked expandable sections with dividers and rotating chevrons.
//   <ui:Accordion Type="Single" Value="a">
//       <ui:AccordionItem Value="a">
//           <ui:AccordionTrigger Text="Is it accessible?" />
//           <ui:AccordionContent><TextBlock Text="Yes." /></ui:AccordionContent>
//       </ui:AccordionItem>
//   </ui:Accordion>
public class Accordion : StackPanel
{
    public static readonly StyledProperty<AccordionType> TypeProperty =
        AvaloniaProperty.Register<Accordion, AccordionType>(nameof(Type));

    // Initially open item (Single) — or comma-separated items (Multiple).
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<Accordion, string?>(nameof(Value));

    private readonly HashSet<string> _openValues = new();

    static Accordion()
    {
        ValueProperty.Changed.AddClassHandler<Accordion>((a, _) => a.OnValueChanged());
    }

    public AccordionType Type
    {
        get => GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // Fired when any item's open state changes: (value, isOpen).
    public event EventHandler<(string Value, bool IsOpen)>? ItemToggled;

    public Accordion()
    {
        Children.CollectionChanged += (_, _) => Refresh(animate: false);
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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Refresh(animate: false);
    }

    private void OnValueChanged()
    {
        _openValues.Clear();
        foreach (var v in (Value ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
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
            items[i].ApplyOpen(_openValues.Contains(items[i].Value ?? string.Empty), animate);
            items[i].SetDividerVisible(i < items.Count - 1);
        }
    }
}
