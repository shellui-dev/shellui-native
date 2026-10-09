using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// AccordionItem - one expandable row. Its IsOpen is derived from the parent Accordion.
public static class AccordionItemTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "accordion-item",
        DisplayName = "Accordion Item",
        Description = "Single row of an Accordion - contains AccordionTrigger + AccordionContent",
        Category = ComponentCategory.Layout,
        FilePath = "AccordionItem.cs",
        Dependencies = new List<string> { "shell", "element-extensions" },
        Tags = new List<string> { "layout", "accordion", "item" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// One section: trigger + content, with a bottom divider (border-b, none on the last item).
[ContentProperty(nameof(Children))]
public partial class AccordionItem : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(AccordionItem), string.Empty);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public bool IsOpen { get; private set; }
    public event EventHandler<bool>? OpenChanged;

    private readonly VerticalStackLayout _stack;
    private readonly BoxView _divider;

    public new IList<IView> Children => _stack.Children;

    public AccordionItem()
    {
        _stack = new VerticalStackLayout { Spacing = 0 };
        _divider = new BoxView { HeightRequest = 1, BackgroundColor = Colors.Transparent };
        _divider.Token(BoxView.ColorProperty, ShellToken.Border);
        Content = new VerticalStackLayout { Spacing = 0, Children = { _stack, _divider } };
    }

    public void Toggle() => this.FindParentOfType<Accordion>()?.Toggle(Value);

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
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace YourProjectNamespace.Components.UI;

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
"
    };
}
