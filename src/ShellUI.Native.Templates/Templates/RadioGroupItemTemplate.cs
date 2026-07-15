using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class RadioGroupItemTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "radio-group-item",
        DisplayName = "Radio Group Item",
        Description = "Single radio option",
        Category = ComponentCategory.Form,
        FilePath = "RadioGroupItem.cs",
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "form", "radio", "option" }
    };

    public static string Content => @"using Microsoft.Maui.Controls.Shapes;
namespace YourProjectNamespace.Components.UI;

public partial class RadioGroupItem : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(RadioGroupItem), string.Empty);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(RadioGroupItem), string.Empty,
            propertyChanged: (b, o, n) => (b as RadioGroupItem)?.UpdateLabel());

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private readonly Border _indicator;
    private readonly Label _label;
    private readonly HorizontalStackLayout _layout;

    public RadioGroupItem()
    {
        _indicator = new Border
        {
            WidthRequest = 20,
            HeightRequest = 20,
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = 10 }
        };
        _label = new Label { VerticalOptions = LayoutOptions.Center };
        _layout = new HorizontalStackLayout
        {
            Spacing = 12,
            Children = { _indicator, _label }
        };
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;
        GestureRecognizers.Add(tap);
        Content = _layout;
        UpdateVisualState();
    }

    private void OnTapped(object? s, TappedEventArgs e)
    {
        FindParentOfType<RadioGroup>()?.SetValue(Value);
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        var group = FindParentOfType<RadioGroup>();
        if (group != null)
            group.ValueChanged += (s, v) => UpdateVisualState();
    }

    private void UpdateLabel() => _label.Text = Text ?? string.Empty;

    private void UpdateVisualState()
    {
        var isChecked = FindParentOfType<RadioGroup>()?.Value == Value;
        _indicator.Stroke = Color.FromArgb(""#E5E7EB"");
        _indicator.BackgroundColor = isChecked ? Color.FromArgb(""#3B82F6"") : Colors.Transparent;
    }
}
";
}
