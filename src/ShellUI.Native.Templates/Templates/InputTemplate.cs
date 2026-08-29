using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Input component template for text entry
public static class InputTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "input",
        DisplayName = "Input",
        Description = "Text input field with placeholder, validation states, and icon support",
        Category = ComponentCategory.Form,
        FilePath = "Input.cs",
        Dependencies = new List<string>(),
        Variants = new List<string> { "default", "error", "success" },
        Tags = new List<string> { "form", "input", "text", "field", "entry" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;
namespace YourProjectNamespace.Components.UI;

// Text input component with validation states
public partial class Input : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Input), 
            string.Empty, BindingMode.TwoWay, propertyChanged: OnTextChanged);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(Input), 
            string.Empty, propertyChanged: OnPlaceholderChanged);

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(Input), 
            false, propertyChanged: OnIsPasswordChanged);

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(nameof(HasError), typeof(bool), typeof(Input), 
            false, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty IsReadOnlyProperty =
        BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(Input), 
            false, propertyChanged: OnIsReadOnlyChanged);

    public static readonly BindableProperty MaxLengthProperty =
        BindableProperty.Create(nameof(MaxLength), typeof(int), typeof(Input), 
            int.MaxValue, propertyChanged: OnMaxLengthChanged);

    private readonly Entry _entry;
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

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public event EventHandler<TextChangedEventArgs>? TextChanged;
    public event EventHandler? Completed;

    public Input()
    {
        _entry = new Entry
        {
            BackgroundColor = Colors.Transparent,
            FontSize = 14,
            VerticalOptions = LayoutOptions.Center
        };
        
        _entry.TextChanged += (s, e) => 
        {
            Text = e.NewTextValue;
            TextChanged?.Invoke(this, e);
        };
        _entry.Completed += (s, e) => Completed?.Invoke(this, e);

        _border = new Border
        {
            Content = _entry,
            Padding = new Thickness(12, 0),
            HeightRequest = 40,
            StrokeThickness = 1
        };

        Content = _border;
        UpdateVisualState();
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Input input && input._entry.Text != newValue as string)
            input._entry.Text = newValue as string ?? string.Empty;
    }

    private static void OnPlaceholderChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Input input)
            input._entry.Placeholder = newValue as string ?? string.Empty;
    }

    private static void OnIsPasswordChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Input input)
            input._entry.IsPassword = (bool)newValue;
    }

    private static void OnIsReadOnlyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Input input)
            input._entry.IsReadOnly = (bool)newValue;
    }

    private static void OnMaxLengthChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Input input)
            input._entry.MaxLength = (int)newValue;
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Input input)
            input.UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        // Design tokens matching ShellUI theme
        var backgroundColor = Color.FromArgb(""#FFFFFF"");
        var borderColor = HasError ? Color.FromArgb(""#EF4444"") : Color.FromArgb(""#E5E7EB"");
        var focusBorderColor = HasError ? Color.FromArgb(""#EF4444"") : Color.FromArgb(""#2563EB"");

        _border.BackgroundColor = backgroundColor;
        _border.Stroke = borderColor;
        _border.StrokeShape = new RoundRectangle { CornerRadius = 6 };

        _entry.PlaceholderColor = Color.FromArgb(""#9CA3AF"");
        _entry.TextColor = Color.FromArgb(""#1F2937"");

        Opacity = IsEnabled ? 1.0 : 0.5;
    }

    public void Focus() => _entry.Focus();
    public void Unfocus() => _entry.Unfocus();
}
"
    };
}
