using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Time field — h-10 rounded-md border border-input around the platform time picker, whose own
// frame and column dividers are stripped.
public partial class TimePicker : ContentView
{
    public static readonly BindableProperty TimeProperty =
        BindableProperty.Create(nameof(Time), typeof(TimeSpan), typeof(TimePicker),
            DateTime.Now.TimeOfDay, BindingMode.TwoWay, propertyChanged: OnTimeChanged);

    public TimeSpan Time
    {
        get => (TimeSpan)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public event EventHandler? TimeChanged;

    private readonly Microsoft.Maui.Controls.TimePicker _native;

    public TimePicker()
    {
        _native = new Microsoft.Maui.Controls.TimePicker
        {
            FontSize = 14,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent
        };
        _native.Token(Microsoft.Maui.Controls.TimePicker.TextColorProperty, ShellToken.Foreground);
        ShellPlatform.StripNativeChrome(_native, keepPadding: true);
        _native.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Microsoft.Maui.Controls.TimePicker.Time))
            {
                // .NET 10 MAUI made TimePicker.Time nullable
                Time = _native.Time ?? Time;
                TimeChanged?.Invoke(this, EventArgs.Empty);
            }
        };

        var border = new Border
        {
            Content = _native,
            HeightRequest = 40,
            Padding = new Thickness(4, 0),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        border.Token(Border.StrokeProperty, ShellToken.Input);
        Content = border;
        HorizontalOptions = LayoutOptions.Start;
    }

    private static void OnTimeChanged(BindableObject b, object o, object n)
    {
        if (b is TimePicker tp && n is TimeSpan ts && tp._native.Time.GetValueOrDefault() != ts)
            tp._native.Time = ts;
    }
}
