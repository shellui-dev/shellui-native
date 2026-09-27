namespace MAUI.Demo.Components.UI;

// 1px divider in the Border token.
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
        _line = new BoxView();
        _line.Token(BoxView.ColorProperty, ShellToken.Border);
        Content = _line;
        UpdateVisualState();
    }

    private static void OnOrientationChanged(BindableObject bindable, object oldValue, object newValue)
        => (bindable as Separator)?.UpdateVisualState();

    private void UpdateVisualState()
    {
        if (Orientation == SeparatorOrientation.Horizontal)
        {
            _line.HeightRequest = 1;
            _line.WidthRequest = -1;
            _line.HorizontalOptions = LayoutOptions.Fill;
            _line.VerticalOptions = LayoutOptions.Center;
        }
        else
        {
            _line.WidthRequest = 1;
            _line.HeightRequest = -1;
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
