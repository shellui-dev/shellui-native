using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class TextareaTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "textarea",
        DisplayName = "Textarea",
        Description = "Multi-line text input",
        Category = ComponentCategory.Form,
        FilePath = "Textarea.cs",
        Dependencies = new List<string>(),
        Tags = new List<string> { "form", "input", "multiline", "textarea" }
    };

    public static string Content => @"using Microsoft.Maui.Controls.Shapes;
namespace YourProjectNamespace.Components.UI;

public partial class Textarea : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Textarea), 
            string.Empty, BindingMode.TwoWay, propertyChanged: OnTextChanged);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(Textarea), 
            string.Empty, propertyChanged: OnPlaceholderChanged);

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(nameof(HasError), typeof(bool), typeof(Textarea), 
            false, propertyChanged: OnVisualChanged);

    public static readonly BindableProperty MaxLengthProperty =
        BindableProperty.Create(nameof(MaxLength), typeof(int), typeof(Textarea), 
            int.MaxValue, propertyChanged: OnMaxLengthChanged);

    private readonly Editor _editor;
    private readonly Border _border;

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public event EventHandler<TextChangedEventArgs>? TextChanged;

    public Textarea()
    {
        _editor = new Editor
        {
            BackgroundColor = Colors.Transparent,
            FontSize = 14,
            MinimumHeightRequest = 80,
            AutoSize = EditorAutoSizeOption.TextChanges
        };
        _editor.TextChanged += (s, e) => { Text = e.NewTextValue; TextChanged?.Invoke(this, e); };
        _border = new Border
        {
            Content = _editor,
            Padding = new Thickness(12, 8),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 6 }
        };
        Content = _border;
        UpdateVisualState();
    }

    private static void OnTextChanged(BindableObject b, object o, object n)
    {
        if (b is Textarea t && t._editor.Text != n as string)
            t._editor.Text = n as string ?? string.Empty;
    }

    private static void OnPlaceholderChanged(BindableObject b, object o, object n)
    {
        if (b is Textarea t) t._editor.Placeholder = n as string ?? string.Empty;
    }

    private static void OnMaxLengthChanged(BindableObject b, object o, object n)
    {
        if (b is Textarea t) t._editor.MaxLength = (int)n;
    }

    private static void OnVisualChanged(BindableObject b, object o, object n) => (b as Textarea)?.UpdateVisualState();

    private void UpdateVisualState()
    {
        _border.Stroke = HasError ? Color.FromArgb(""#EF4444"") : Color.FromArgb(""#E5E7EB"");
    }
}
";
}
