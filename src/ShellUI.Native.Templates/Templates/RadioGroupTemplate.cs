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
        Dependencies = new List<string> { "radio-group-item" },
        Tags = new List<string> { "form", "input", "radio", "choice" }
    };

    public static string Content => @"namespace YourProjectNamespace.Components.UI;

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
        _stack = new VerticalStackLayout { Spacing = 8 };
        Content = _stack;
    }

    public void SetValue(string value)
    {
        if (Value != value) { Value = value; ValueChanged?.Invoke(this, value); }
    }

    private static void OnValueChanged(BindableObject b, object o, object n) { }
}
";
}
