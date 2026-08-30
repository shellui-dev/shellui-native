namespace MAUI.Demo.Components.UI;

// v1 wraps the native Picker directly — no outer Border. The native control (WinUI
// ComboBox on Windows) draws its own chrome that overflows a wrapping Border on
// desktop, so we defer styling to the platform. A shadcn-style Popover-based Select
// with our own visuals is tracked as a follow-up.
public partial class Select : ContentView
{
    public static readonly BindableProperty SelectedIndexProperty =
        BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(Select),
            -1, BindingMode.TwoWay, propertyChanged: OnSelectedChanged);

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IList<string>), typeof(Select),
            null, propertyChanged: OnItemsChanged);

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public IList<string>? ItemsSource
    {
        get => (IList<string>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public string? SelectedItem => SelectedIndex >= 0 && ItemsSource != null && SelectedIndex < ItemsSource.Count
        ? ItemsSource[SelectedIndex] : null;

    public event EventHandler? SelectedIndexChanged;

    private readonly Microsoft.Maui.Controls.Picker _picker;

    public Select()
    {
        _picker = new Microsoft.Maui.Controls.Picker
        {
            Title = "Select...",
            HeightRequest = 40
        };
        _picker.SelectedIndexChanged += (s, e) =>
        {
            SelectedIndex = _picker.SelectedIndex;
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        };
        Content = _picker;
    }

    private static void OnItemsChanged(BindableObject b, object o, object n)
    {
        if (b is Select s && n is IList<string> list)
        {
            // Picker.ItemsSource is System.Collections.IList (non-generic). List<string>
            // implements both, but the IList<string> parameter type doesn't — copy into a
            // List<string> to cover the case where the caller passed an array or ObservableCollection<T>.
            s._picker.ItemsSource = list as System.Collections.IList ?? new List<string>(list);
            if (s.SelectedIndex >= 0 && s.SelectedIndex < list.Count)
                s._picker.SelectedIndex = s.SelectedIndex;
        }
    }

    private static void OnSelectedChanged(BindableObject b, object o, object n)
    {
        if (b is Select s && (int)n != s._picker.SelectedIndex)
            s._picker.SelectedIndex = (int)n;
    }
}
