using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Checkbox component with label support
public partial class Checkbox : ContentView
{
    public static readonly BindableProperty IsCheckedProperty =
        BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(Checkbox), 
            false, BindingMode.TwoWay, propertyChanged: OnIsCheckedChanged);

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(Checkbox), 
            string.Empty, propertyChanged: OnLabelChanged);

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(nameof(HasError), typeof(bool), typeof(Checkbox), 
            false, propertyChanged: OnVisualPropertyChanged);

    public static new readonly BindableProperty IsEnabledProperty =
        BindableProperty.Create(nameof(IsEnabled), typeof(bool), typeof(Checkbox), 
            true, propertyChanged: OnIsEnabledChanged);

    private readonly Border _checkboxBorder;
    private readonly Label _checkmark;
    private readonly Label _label;
    private readonly TapGestureRecognizer _tapGesture;

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public new bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    public event EventHandler<bool>? CheckedChanged;

    public Checkbox()
    {
        _checkmark = new Label
        {
            Text = "✓",
            FontSize = 14,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            IsVisible = false
        };

        _checkboxBorder = new Border
        {
            Content = _checkmark,
            WidthRequest = 20,
            HeightRequest = 20,
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = 4 }
        };

        _label = new Label
        {
            FontSize = 14,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(8, 0, 0, 0)
        };

        _tapGesture = new TapGestureRecognizer();
        _tapGesture.Tapped += OnTapped;

        var container = new HorizontalStackLayout
        {
            Spacing = 0,
            Children = { _checkboxBorder, _label }
        };

        container.GestureRecognizers.Add(_tapGesture);
        Content = container;

        UpdateVisualState();
    }

    private void OnTapped(object? sender, EventArgs e)
    {
        if (IsEnabled)
            IsChecked = !IsChecked;
    }

    private static void OnIsCheckedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Checkbox checkbox)
        {
            checkbox.UpdateVisualState();
            checkbox.CheckedChanged?.Invoke(checkbox, (bool)newValue);
        }
    }

    private static void OnLabelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Checkbox checkbox)
            checkbox._label.Text = newValue as string ?? string.Empty;
    }

    private static void OnIsEnabledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Checkbox checkbox)
            checkbox.UpdateVisualState();
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Checkbox checkbox)
            checkbox.UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        // Design tokens matching ShellUI theme
        var borderColor = HasError 
            ? Color.FromArgb("#EF4444") 
            : (IsChecked ? Color.FromArgb("#2563EB") : Color.FromArgb("#E5E7EB"));
        
        var backgroundColor = IsChecked 
            ? Color.FromArgb("#2563EB") 
            : Colors.Transparent;

        _checkboxBorder.BackgroundColor = backgroundColor;
        _checkboxBorder.Stroke = borderColor;
        _checkmark.IsVisible = IsChecked;
        _label.TextColor = Color.FromArgb("#1F2937");
        _label.Opacity = IsEnabled ? 1.0 : 0.5;
        _checkboxBorder.Opacity = IsEnabled ? 1.0 : 0.5;
    }
}
