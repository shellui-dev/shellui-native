using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Checkbox — h-4 w-4 rounded-sm border border-primary; checked: bg-primary + check icon.
// The whole row (box + label) is the hit target.
public partial class Checkbox : ContentView
{
    public static readonly BindableProperty IsCheckedProperty =
        BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(Checkbox),
            false, BindingMode.TwoWay, propertyChanged: OnIsCheckedChanged);

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(Checkbox),
            string.Empty, propertyChanged: (b, o, n) => ((Checkbox)b).UpdateLabel());

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(nameof(HasError), typeof(bool), typeof(Checkbox),
            false, propertyChanged: (b, o, n) => ((Checkbox)b).UpdateVisualState());

    private readonly Border _box;
    private readonly Icon _check;
    private readonly Label _label;

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

    public event EventHandler<bool>? CheckedChanged;

    public Checkbox()
    {
        _check = new Icon { Name = IconName.Check, Size = 12, StrokeWidth = 3, Token = ShellToken.PrimaryForeground };

        _box = new Border
        {
            Content = _check,
            WidthRequest = 16,
            HeightRequest = 16,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusSm },
            VerticalOptions = LayoutOptions.Center
        };

        _label = new Label { FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center, IsVisible = false };
        _label.Token(Microsoft.Maui.Controls.Label.TextColorProperty, ShellToken.Foreground);

        var row = new HorizontalStackLayout
        {
            Spacing = 8,
            MinimumHeightRequest = 24,
            Children = { _box, _label }
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (!IsEnabled) return; ShellFocus.FocusPressed(this); IsChecked = !IsChecked; };
        row.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, () => { if (IsEnabled) IsChecked = !IsChecked; });

        Content = row;
        HorizontalOptions = LayoutOptions.Start;
        UpdateVisualState();
    }

    private static void OnIsCheckedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not Checkbox checkbox) return;
        checkbox.UpdateVisualState();
        checkbox.CheckedChanged?.Invoke(checkbox, (bool)newValue);
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName)
            Opacity = IsEnabled ? 1.0 : 0.5;
    }

    private void UpdateLabel()
    {
        _label.Text = Label ?? string.Empty;
        _label.IsVisible = !string.IsNullOrEmpty(Label);
    }

    private void UpdateVisualState()
    {
        _box.Token(Border.StrokeProperty, HasError ? ShellToken.Destructive : ShellToken.Primary);
        if (IsChecked)
            _box.Token(VisualElement.BackgroundColorProperty, ShellToken.Primary);
        else
        {
            _box.ClearValue(VisualElement.BackgroundColorProperty);
            _box.BackgroundColor = Colors.Transparent;
        }
        _check.IsVisible = IsChecked;
    }
}
