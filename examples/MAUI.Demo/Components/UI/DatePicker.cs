namespace MAUI.Demo.Components.UI;

// v1 hosts the native DatePicker directly — no outer Border. On Windows the native
// control (WinUI CalendarDatePicker) draws chrome that overflows a wrapping Border.
// A custom calendar-popup DatePicker is tracked as a follow-up.
public partial class DatePicker : ContentView
{
    public static readonly BindableProperty DateProperty =
        BindableProperty.Create(nameof(Date), typeof(DateTime), typeof(DatePicker), 
            DateTime.Today, BindingMode.TwoWay, propertyChanged: OnDateChanged);

    public static readonly BindableProperty MinimumDateProperty =
        BindableProperty.Create(nameof(MinimumDate), typeof(DateTime?), typeof(DatePicker), null);

    public static readonly BindableProperty MaximumDateProperty =
        BindableProperty.Create(nameof(MaximumDate), typeof(DateTime?), typeof(DatePicker), null);

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

    private readonly Microsoft.Maui.Controls.DatePicker _nativePicker;

    public DatePicker()
    {
        _nativePicker = new Microsoft.Maui.Controls.DatePicker
        {
            HeightRequest = 40
        };
        _nativePicker.DateSelected += (s, e) =>
        {
            Date = e.NewDate;
            DateChanged?.Invoke(this, new DateChangedEventArgs(e.OldDate, e.NewDate));
        };
        Content = _nativePicker;
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == DateProperty.PropertyName) _nativePicker.Date = Date;
        else if (propertyName == MinimumDateProperty.PropertyName) _nativePicker.MinimumDate = MinimumDate ?? DateTime.MinValue;
        else if (propertyName == MaximumDateProperty.PropertyName) _nativePicker.MaximumDate = MaximumDate ?? DateTime.MaxValue;
    }

    private static void OnDateChanged(BindableObject b, object o, object n)
    {
        if (b is DatePicker dp && n is DateTime dt && dp._nativePicker.Date != dt)
            dp._nativePicker.Date = dt;
    }
}
