using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class SelectTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "select",
        DisplayName = "Select",
        Description = "Dropdown select / picker",
        Category = ComponentCategory.Form,
        FilePath = "Select.cs",
        Dependencies = new List<string>(),
        Tags = new List<string> { "form", "select", "picker", "dropdown" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

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
            Title = ""Select..."",
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
            s._picker.ItemsSource = list;
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
"
    };
}
