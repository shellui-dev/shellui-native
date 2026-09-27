namespace MAUI.Demo.Components.UI;

// Panel shown while its Value is the active tab; fades in on switch.
[ContentProperty(nameof(Content))]
public partial class TabsContent : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(TabsContent), string.Empty);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public TabsContent()
    {
        IsVisible = false;
    }

    internal void SetActive(bool active, bool animate)
    {
        if (IsVisible == active) return;
        IsVisible = active;
        if (active && animate)
        {
            Opacity = 0;
            _ = this.FadeToAsync(1, 150, Easing.CubicOut);
        }
        else
        {
            Opacity = 1;
        }
    }
}
