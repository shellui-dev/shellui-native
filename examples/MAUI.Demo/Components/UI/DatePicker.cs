using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Date field — h-10 rounded-md border border-input around the platform date picker, whose own
// frame is stripped. A custom calendar popover is tracked as a follow-up.
public partial class DatePicker : ContentView
{
    public static readonly BindableProperty DateProperty =
        BindableProperty.Create(nameof(Date), typeof(DateTime), typeof(DatePicker),
            DateTime.Today, BindingMode.TwoWay, propertyChanged: OnDateChanged);

    public static readonly BindableProperty MinimumDateProperty =
        BindableProperty.Create(nameof(MinimumDate), typeof(DateTime?), typeof(DatePicker), null,
            propertyChanged: (b, o, n) => ((DatePicker)b)._native.MinimumDate = (DateTime?)n ?? DateTime.MinValue);

    public static readonly BindableProperty MaximumDateProperty =
        BindableProperty.Create(nameof(MaximumDate), typeof(DateTime?), typeof(DatePicker), null,
            propertyChanged: (b, o, n) => ((DatePicker)b)._native.MaximumDate = (DateTime?)n ?? DateTime.MaxValue);

    public DateTime Date
    {
        get => (DateTime)GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    public DateTime? MinimumDate
    {
        get => (DateTime?)GetValue(MinimumDateProperty);
        set => SetValue(MinimumDateProperty, value);
    }

    public DateTime? MaximumDate
    {
        get => (DateTime?)GetValue(MaximumDateProperty);
        set => SetValue(MaximumDateProperty, value);
    }

    public event EventHandler<DateChangedEventArgs>? DateChanged;

    private readonly Microsoft.Maui.Controls.DatePicker _native;

    public DatePicker()
    {
        _native = new Microsoft.Maui.Controls.DatePicker
        {
            FontSize = 14,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent
        };
        _native.Token(Microsoft.Maui.Controls.DatePicker.TextColorProperty, ShellToken.Foreground);
        ShellPlatform.StripNativeChrome(_native, keepPadding: true);
        _native.DateSelected += (s, e) =>
        {
            // .NET 10 MAUI made DateChangedEventArgs.NewDate / OldDate nullable
            var newDate = e.NewDate ?? Date;
            var oldDate = e.OldDate ?? Date;
            Date = newDate;
            DateChanged?.Invoke(this, new DateChangedEventArgs(oldDate, newDate));
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

    private static void OnDateChanged(BindableObject b, object o, object n)
    {
        if (b is DatePicker dp && n is DateTime dt && dp._native.Date.GetValueOrDefault() != dt)
            dp._native.Date = dt;
    }
}
