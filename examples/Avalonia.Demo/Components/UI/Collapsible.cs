using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace AvaloniaDemo.Components.UI;

// Expand/collapse. The trigger can wrap a Button or any control; content animates its height.
//   <ui:Collapsible Open="{Binding IsOpen}">
//       <ui:CollapsibleTrigger><ui:Button Text="Toggle" Variant="Outline" /></ui:CollapsibleTrigger>
//       <ui:CollapsibleContent><TextBlock Text="Hidden until open" /></ui:CollapsibleContent>
//   </ui:Collapsible>
public class Collapsible : StackPanel
{
    public static readonly StyledProperty<bool> OpenProperty =
        AvaloniaProperty.Register<Collapsible, bool>(nameof(Open), defaultBindingMode: BindingMode.TwoWay);

    static Collapsible()
    {
        OpenProperty.Changed.AddClassHandler<Collapsible>((c, _) =>
        {
            c.Refresh(animate: c.IsLoaded);
            if (c._byUser) c.OpenChanged?.Invoke(c, c.Open);
        });
    }

    public bool Open
    {
        get => GetValue(OpenProperty);
        set => SetValue(OpenProperty, value);
    }

    // User input only: setting Open, or calling Toggle / SetOpen, from code doesn't raise it.
    public event EventHandler<bool>? OpenChanged;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    public Collapsible()
    {
        Spacing = 8;
        Children.CollectionChanged += (_, _) => Refresh(animate: false);
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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Refresh(animate: false);
    }

    // The collapsible drives its content, so state never depends on when a child found its parent.
    private void Refresh(bool animate)
    {
        foreach (var content in this.FindDescendantsOfType<CollapsibleContent>(e => e is Collapsible))
            content.Apply(Open, animate);
    }
}
