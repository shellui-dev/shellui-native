namespace MAUI.Demo.Components.UI;

// Separator/divider component
public partial class Separator : ContentView
{
    public static readonly BindableProperty OrientationProperty =
        BindableProperty.Create(nameof(Orientation), typeof(SeparatorOrientation), typeof(Separator), 
            SeparatorOrientation.Horizontal, propertyChanged: OnOrientationChanged);

    private readonly BoxView _line;

    public SeparatorOrientation Orientation
    {
        get => (SeparatorOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public Separator()
    {
        _line = new BoxView
        {
            Color = Color.FromArgb("#E5E7EB"),
            HeightRequest = 1,
            WidthRequest = 1
        };

        Content = _line;
        UpdateVisualState();
    }

    private static void OnOrientationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Separator separator)
            separator.UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        // Design tokens matching ShellUI theme
        _line.Color = Color.FromArgb("#E5E7EB");

        if (Orientation == SeparatorOrientation.Horizontal)
        {
            _line.HeightRequest = 1;
            _line.WidthRequest = -1; // Fill available width
            _line.HorizontalOptions = LayoutOptions.Fill;
            _line.VerticalOptions = LayoutOptions.Center;
        }
        else
        {
            _line.WidthRequest = 1;
            _line.HeightRequest = -1; // Fill available height
            _line.HorizontalOptions = LayoutOptions.Center;
            _line.VerticalOptions = LayoutOptions.Fill;
        }
    }
}

public enum SeparatorOrientation
{
    Horizontal,
    Vertical
}
