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
            t.ValueChanged?.Invoke(t, ((string?)e.OldValue ?? string.Empty, (string?)e.NewValue ?? string.Empty));
        });
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // (oldValue, newValue)
    public event EventHandler<(string Old, string New)>? ValueChanged;

    public Tabs()
    {
        Spacing = 8;
        Children.CollectionChanged += (_, _) => Refresh(animate: false);
    }

    public void SetValue(string? value) => Value = value;

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
