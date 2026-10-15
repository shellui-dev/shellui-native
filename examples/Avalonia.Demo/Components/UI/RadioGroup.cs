using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace AvaloniaDemo.Components.UI;

// Single-choice group. Usage:
//   <ui:RadioGroup Value="a"><ui:RadioGroupItem Value="a" Text="Option A" />...</ui:RadioGroup>
public class RadioGroup : StackPanel
{
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<RadioGroup, string?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    static RadioGroup()
    {
        ValueProperty.Changed.AddClassHandler<RadioGroup>((g, e) =>
        {
            g.RefreshItems();
            if (g._byUser) g.ValueChanged?.Invoke(g, (string?)e.NewValue ?? string.Empty);
        });
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // User input only: setting Value from code or a binding doesn't raise it.
    public event EventHandler<string>? ValueChanged;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    public RadioGroup()
    {
        Spacing = 12;
        Children.CollectionChanged += (_, _) => RefreshItems();
    }

    public void SetValue(string? value) => Value = value;

    // A click or key on an item.
    internal void SelectByUser(string? value) => ByUser(() => Value = value);

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    // Items inside a nested panel may arrive after this group's own children.
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        RefreshItems();
    }

    // The group drives its items, so item state never depends on when an item found its parent.
    internal void RefreshItems()
    {
        foreach (var item in this.FindDescendantsOfType<RadioGroupItem>(e => e is RadioGroup))
            item.SetChecked(item.Value == Value);
    }
}
