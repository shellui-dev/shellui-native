using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class RadioGroupTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "radio-group",
        DisplayName = "Radio Group",
        Description = "Radio button group",
        Category = ComponentCategory.Form,
        FilePath = "RadioGroup.cs",
        Dependencies = new List<string> { "element-extensions", "radio-group-item" },
        Tags = new List<string> { "form", "input", "radio", "choice" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Single-choice group. Usage:
//   <ui:RadioGroup Value=""a""><ui:RadioGroupItem Value=""a"" Text=""Option A"" />...</ui:RadioGroup>
[ContentProperty(nameof(Children))]
public partial class RadioGroup : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(RadioGroup),
            string.Empty, BindingMode.TwoWay, propertyChanged: OnValueChanged);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public event EventHandler<string>? ValueChanged;

    private readonly VerticalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public RadioGroup()
    {
        _stack = new VerticalStackLayout { Spacing = 12 };
        Content = _stack;
        Loaded += (_, _) => RefreshItems();
    }

    public void SetValue(string value) => Value = value;

    private static void OnValueChanged(BindableObject b, object o, object n)
    {
        if (b is not RadioGroup group) return;
        group.RefreshItems();
        group.ValueChanged?.Invoke(group, n as string ?? string.Empty);
    }

    // The group drives its items, so item state never depends on when an item found its parent.
    internal void RefreshItems()
    {
        foreach (var item in this.FindDescendantsOfType<RadioGroupItem>(e => e is RadioGroup))
            item.SetChecked(item.Value == Value);
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace YourProjectNamespace.Components.UI;

// Single-choice group. Usage:
//   <ui:RadioGroup Value=""a""><ui:RadioGroupItem Value=""a"" Text=""Option A"" />...</ui:RadioGroup>
public class RadioGroup : StackPanel
{
    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<RadioGroup, string?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    static RadioGroup()
    {
        ValueProperty.Changed.AddClassHandler<RadioGroup>((g, e) =>
        {
            g.RefreshItems();
            g.ValueChanged?.Invoke(g, (string?)e.NewValue ?? string.Empty);
        });
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public event EventHandler<string>? ValueChanged;

    public RadioGroup()
    {
        Spacing = 12;
        Children.CollectionChanged += (_, _) => RefreshItems();
    }

    public void SetValue(string? value) => Value = value;

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
"
    };
}
