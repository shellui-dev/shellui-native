using MAUI.Demo.Components.UI.Variants;

namespace MAUI.Demo.Components.UI;

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

    private readonly Border _border;
    private readonly ActivityIndicator _loadingIndicator;
    private readonly Label _textLabel;
    private readonly HorizontalStackLayout _contentLayout;

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
            HorizontalOptions = LayoutOptions.Center,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalTextAlignment = TextAlignment.Center
        };

        _contentLayout = new HorizontalStackLayout
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children = { _loadingIndicator, _textLabel }
        };

        _border = new Border
        {
            Content = _contentLayout,
            StrokeThickness = 0
        };

        // Add tap gesture
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnTapped;
        _border.GestureRecognizers.Add(tapGesture);

        Content = _border;
        UpdateVisualState();
    }

    private void OnTapped(object? sender, TappedEventArgs e)
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

        _border.BackgroundColor = style.BackgroundColor;
        _border.Stroke = style.BorderColor;
        _border.StrokeThickness = style.BorderThickness;
        _border.StrokeShape = new RoundRectangle { CornerRadius = style.CornerRadius };
        _border.Padding = style.Padding;
        _border.HeightRequest = style.Height;
        _border.MinimumWidthRequest = style.MinWidth;

        _textLabel.TextColor = style.TextColor;
        _textLabel.FontSize = style.FontSize;
        _textLabel.FontAttributes = FontAttributes.Bold;

        _loadingIndicator.Color = style.TextColor;
        _loadingIndicator.IsVisible = IsLoading;
        _loadingIndicator.IsRunning = IsLoading;

        Opacity = IsEnabled ? 1.0 : 0.5;
    }
}
