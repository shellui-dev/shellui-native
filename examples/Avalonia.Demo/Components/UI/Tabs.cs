using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace AvaloniaDemo.Components.UI;

// Usage:
//   <ui:Tabs Value="account">
//       <ui:TabsList>
//           <ui:TabsTrigger Value="account" Text="Account" />
//           <ui:TabsTrigger Value="password" Text="Password" />
//       </ui:TabsList>
//       <ui:TabsContent Value="account">...</ui:TabsContent>
//       <ui:TabsContent Value="password">...</ui:TabsContent>
//   </ui:Tabs>
public class Tabs : StackPanel
{
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<Tabs, string?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    static Tabs()
    {
        ValueProperty.Changed.AddClassHandler<Tabs>((t, e) =>
        {
            t.Refresh(animate: t.IsLoaded);
            if (t._byUser) t.ValueChanged?.Invoke(t, ((string?)e.OldValue ?? string.Empty, (string?)e.NewValue ?? string.Empty));
        });
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
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
        Children.CollectionChanged += (_, _) => Refresh(animate: false);
    }

    public void SetValue(string? value) => Value = value;

    // A click or key on a trigger.
    internal void SelectByUser(string? value) => ByUser(() => Value = value);

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Refresh(animate: false);
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
