using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class TimePickerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "time-picker",
        DisplayName = "Time Picker",
        Description = "Time selection picker",
        Category = ComponentCategory.Form,
        FilePath = "TimePicker.cs",
        Dependencies = new List<string>(),
        Tags = new List<string> { "form", "time", "picker" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// v1 hosts the native TimePicker directly — no outer Border. On Windows the native
// control (WinUI TimePicker with hour/minute/am-pm columns) draws chrome that overflows
// a wrapping Border. A custom time-popup TimePicker is tracked as a follow-up.
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

    private readonly Microsoft.Maui.Controls.TimePicker _picker;

    public TimePicker()
    {
        _picker = new Microsoft.Maui.Controls.TimePicker
        {
            HeightRequest = 40
        };
        _picker.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Microsoft.Maui.Controls.TimePicker.Time))
            {
                Time = _picker.Time;
                TimeChanged?.Invoke(this, EventArgs.Empty);
            }
        };
        Content = _picker;
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == TimeProperty.PropertyName && _picker.Time != Time)
            _picker.Time = Time;
    }

    private static void OnTimeChanged(BindableObject b, object o, object n)
    {
        if (b is TimePicker tp && n is TimeSpan ts && tp._picker.Time != ts)
            tp._picker.Time = ts;
    }
}
"
    };
}
