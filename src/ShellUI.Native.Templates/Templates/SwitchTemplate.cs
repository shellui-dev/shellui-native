using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Switch/Toggle component template
public static class SwitchTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "switch",
        DisplayName = "Switch",
        Description = "Toggle switch component with label support",
        Category = ComponentCategory.Form,
        FilePath = "Switch.cs",
        Dependencies = new List<string> { "shell" },
        Variants = new List<string> { "default" },
        Tags = new List<string> { "form", "switch", "toggle", "input" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Switch — h-6 w-11 rounded-full track (primary when on, input when off),
// h-5 w-5 thumb in bg-background that slides translate-x-5.
public partial class Switch : ContentView
{
    public static readonly BindableProperty IsToggledProperty =
        BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(Switch),
            false, BindingMode.TwoWay, propertyChanged: OnIsToggledChanged);

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(Switch),
            string.Empty, propertyChanged: (b, o, n) => ((Switch)b).UpdateLabel());

    private const double TrackWidth = 44;
    private const double TrackHeight = 24;
    private const double Inset = 2;
    private const double ThumbSize = TrackHeight - Inset * 2;          // 20
    private const double Travel = TrackWidth - Inset * 2 - ThumbSize;  // 20

    private readonly Border _track;
    private readonly Border _thumb;
    private readonly Label _label;

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public event EventHandler<bool>? Toggled;

    public Switch()
    {
        _thumb = new Border
        {
            WidthRequest = ThumbSize,
            HeightRequest = ThumbSize,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ThumbSize / 2 },
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            Shadow = new Shadow { Brush = new SolidColorBrush(Colors.Black), Offset = new Point(0, 1), Radius = 3, Opacity = 0.2f }
        };
        _thumb.Token(VisualElement.BackgroundColorProperty, ShellToken.Background);

        _track = new Border
        {
            Content = _thumb,
            WidthRequest = TrackWidth,
            HeightRequest = TrackHeight,
            Padding = new Thickness(Inset),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = TrackHeight / 2 },
            VerticalOptions = LayoutOptions.Center
        };

        _label = new Label { FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center, IsVisible = false };
        _label.Token(Microsoft.Maui.Controls.Label.TextColorProperty, ShellToken.Foreground);

        var row = new HorizontalStackLayout { Spacing = 8, Children = { _track, _label } };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (!IsEnabled) return; ShellFocus.FocusPressed(this); IsToggled = !IsToggled; };
        row.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, () => { if (IsEnabled) IsToggled = !IsToggled; });

        Content = row;
        HorizontalOptions = LayoutOptions.Start;
        UpdateVisualState(animate: false);
    }

    private static void OnIsToggledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not Switch control) return;
        control.UpdateVisualState(animate: true);
        control.Toggled?.Invoke(control, (bool)newValue);
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

    private void UpdateVisualState(bool animate)
    {
        _track.Token(VisualElement.BackgroundColorProperty, IsToggled ? ShellToken.Primary : ShellToken.Input);
        var x = IsToggled ? Travel : 0;
        if (animate) _ = _thumb.TranslateToAsync(x, 0, 150, Easing.CubicOut);
        else _thumb.TranslationX = x;
    }
}
"
    };

}