using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Button component template for MAUI
public static class ButtonTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "button",
        DisplayName = "Button",
        Description = "Interactive button component with multiple variants and sizes",
        Category = ComponentCategory.Form,
        FilePath = "Button.cs",
        Dependencies = new List<string> { "button-variants" },
        Variants = new List<string> { "default", "destructive", "outline", "secondary", "ghost" },
        Tags = new List<string> { "form", "input", "interactive", "button", "action" }
    };

    public static string Content => @"using YourProjectNamespace.Components.UI.Variants;

namespace YourProjectNamespace.Components.UI;

// Interactive button with variant and size support
public partial class Button : ContentView
{
    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(ButtonVariant), typeof(Button), 
            ButtonVariant.Default, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(ButtonSize), typeof(Button), 
            ButtonSize.Default, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(Button), 
            false, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Button), 
            string.Empty, propertyChanged: OnTextChanged);

    private readonly Microsoft.Maui.Controls.Button _innerButton;
    private readonly ActivityIndicator _loadingIndicator;
    private readonly Label _textLabel;

    public ButtonVariant Variant
    {
        get => (ButtonVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public ButtonSize Size
    {
        get => (ButtonSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public event EventHandler? Clicked;

    public Button()
    {
        _loadingIndicator = new ActivityIndicator
        {
            IsRunning = false,
            IsVisible = false,
            WidthRequest = 16,
            HeightRequest = 16,
            Margin = new Thickness(0, 0, 8, 0)
        };

        _textLabel = new Label
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        _innerButton = new Microsoft.Maui.Controls.Button();
        _innerButton.Clicked += OnInnerButtonClicked;

        var contentLayout = new HorizontalStackLayout
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children = { _loadingIndicator, _textLabel }
        };

        Content = new Border
        {
            Content = contentLayout,
            StrokeThickness = 0
        };

        UpdateVisualState();
    }

    private void OnInnerButtonClicked(object? sender, EventArgs e)
    {
        if (!IsLoading && IsEnabled)
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Button button)
            button.UpdateVisualState();
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Button button)
            button._textLabel.Text = newValue as string ?? string.Empty;
    }

    private void UpdateVisualState()
    {
        var style = ButtonVariants.GetStyle(Variant, Size);
        
        if (Content is Border border)
        {
            border.BackgroundColor = style.BackgroundColor;
            border.Stroke = style.BorderColor;
            border.StrokeThickness = style.BorderThickness;
            border.StrokeShape = new RoundRectangle { CornerRadius = style.CornerRadius };
            border.Padding = style.Padding;
            border.HeightRequest = style.Height;
            border.MinimumWidthRequest = style.MinWidth;
        }

        _textLabel.TextColor = style.TextColor;
        _textLabel.FontSize = style.FontSize;
        _textLabel.FontAttributes = FontAttributes.Bold;

        _loadingIndicator.Color = style.TextColor;
        _loadingIndicator.IsVisible = IsLoading;
        _loadingIndicator.IsRunning = IsLoading;

        Opacity = IsEnabled ? 1.0 : 0.5;
    }
}
";
}
