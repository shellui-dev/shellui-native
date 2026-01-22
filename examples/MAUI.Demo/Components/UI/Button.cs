using Microsoft.Maui.Controls.Shapes;
using MauiIcons.Core;
using MauiIcons.Fluent;
using MAUI.Demo.Components.UI.Variants;

namespace MAUI.Demo.Components.UI;

// Interactive button with variant, size, icon support, and click animations
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

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(FluentIcons?), typeof(Button), 
            null, propertyChanged: OnIconChanged);

    public static readonly BindableProperty IconPositionProperty =
        BindableProperty.Create(nameof(IconPosition), typeof(IconPosition), typeof(Button), 
            IconPosition.Left, propertyChanged: OnIconChanged);

    private readonly ActivityIndicator _loadingIndicator;
    private readonly Label _textLabel;
    private readonly MauiIcon _iconView;
    private readonly Border _border;
    private readonly HorizontalStackLayout _contentLayout;
    private ButtonStyle _currentStyle = new();

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

    public FluentIcons? Icon
    {
        get => (FluentIcons?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IconPosition IconPosition
    {
        get => (IconPosition)GetValue(IconPositionProperty);
        set => SetValue(IconPositionProperty, value);
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

        _iconView = new MauiIcon
        {
            IconSize = 18,
            IsVisible = false,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
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
            Spacing = 8,
            Children = { _loadingIndicator, _iconView, _textLabel }
        };

        _border = new Border
        {
            Content = _contentLayout,
            StrokeThickness = 0
        };

        // Add pointer events for hover/press states
        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerEntered += OnPointerEntered;
        pointerGesture.PointerExited += OnPointerExited;
        pointerGesture.PointerPressed += OnPointerPressed;
        pointerGesture.PointerReleased += OnPointerReleased;
        _border.GestureRecognizers.Add(pointerGesture);

        // Add tap gesture for click handling
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnTapped;
        _border.GestureRecognizers.Add(tapGesture);

        Content = _border;
        
        // Listen for theme changes
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeChanged += (s, e) => UpdateVisualState();
        }
        
        UpdateVisualState();
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (!IsLoading && IsEnabled)
        {
            _border.Opacity = 0.9;
        }
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        _border.Opacity = 1.0;
        _border.Scale = 1.0;
    }

    private async void OnPointerPressed(object? sender, PointerEventArgs e)
    {
        if (!IsLoading && IsEnabled)
        {
            await _border.ScaleToAsync(0.96, 50, Easing.CubicOut);
        }
    }

    private async void OnPointerReleased(object? sender, PointerEventArgs e)
    {
        await _border.ScaleToAsync(1.0, 100, Easing.CubicOut);
    }

    private async void OnTapped(object? sender, TappedEventArgs e)
    {
        if (!IsLoading && IsEnabled)
        {
            await _border.ScaleToAsync(0.95, 50, Easing.CubicOut);
            await _border.ScaleToAsync(1.0, 100, Easing.CubicOut);
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

    private static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Button button)
            button.UpdateIconState();
    }

    private void UpdateIconState()
    {
        var hasIcon = Icon.HasValue;
        _iconView.IsVisible = hasIcon && !IsLoading;
        
        if (hasIcon)
        {
            _iconView.Icon = Icon!.Value;
            
            // Reorder children based on icon position
            _contentLayout.Children.Clear();
            _contentLayout.Children.Add(_loadingIndicator);
            
            if (IconPosition == IconPosition.Left)
            {
                _contentLayout.Children.Add(_iconView);
                _contentLayout.Children.Add(_textLabel);
            }
            else
            {
                _contentLayout.Children.Add(_textLabel);
                _contentLayout.Children.Add(_iconView);
            }
        }
    }

    private void UpdateVisualState()
    {
        _currentStyle = ButtonVariants.GetStyle(Variant, Size);
        
        _border.BackgroundColor = _currentStyle.BackgroundColor;
        _border.Stroke = _currentStyle.BorderColor;
        _border.StrokeThickness = _currentStyle.BorderThickness;
        _border.StrokeShape = new RoundRectangle { CornerRadius = _currentStyle.CornerRadius };
        _border.Padding = _currentStyle.Padding;
        _border.HeightRequest = _currentStyle.Height;
        _border.MinimumWidthRequest = _currentStyle.MinWidth;

        _textLabel.TextColor = _currentStyle.TextColor;
        _textLabel.FontSize = _currentStyle.FontSize;
        _textLabel.FontAttributes = FontAttributes.Bold;

        // Icon color matches text color
        _iconView.IconColor = _currentStyle.TextColor;
        _iconView.IconSize = _currentStyle.FontSize + 2;
        
        _loadingIndicator.Color = _currentStyle.TextColor;
        _loadingIndicator.IsVisible = IsLoading;
        _loadingIndicator.IsRunning = IsLoading;
        
        // Hide icon when loading
        if (Icon.HasValue)
            _iconView.IsVisible = !IsLoading;

        Opacity = IsEnabled ? 1.0 : 0.5;
    }
}

public enum IconPosition { Left, Right }
