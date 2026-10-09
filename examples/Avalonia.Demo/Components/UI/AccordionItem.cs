using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// One section: trigger + content, with a bottom divider (border-b, none on the last item).
public class AccordionItem : Border
{
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<AccordionItem, string?>(nameof(Value));

    private readonly StackPanel _stack = new();
    private readonly Border _divider = new() { Height = 1 };

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    [Content]
    public Controls Items => _stack.Children;

    public bool IsOpen { get; private set; }

    public event EventHandler<bool>? OpenChanged;

    public AccordionItem()
    {
        _divider.Token(BackgroundProperty, ShellToken.Border);
        Child = new StackPanel { Children = { _stack, _divider } };
    }

    public void Toggle() => this.FindParentOfType<Accordion>()?.Toggle(Value ?? string.Empty);

    internal void SetDividerVisible(bool visible) => _divider.IsVisible = visible;

    internal void ApplyOpen(bool open, bool animate)
    {
        var changed = IsOpen != open;
        IsOpen = open;
        foreach (var trigger in this.FindDescendantsOfType<AccordionTrigger>(e => e is AccordionItem))
            trigger.SetOpen(open, animate);
        foreach (var content in this.FindDescendantsOfType<AccordionContent>(e => e is AccordionItem))
            content.Apply(open, animate);
        if (changed) OpenChanged?.Invoke(this, open);
    }
}
