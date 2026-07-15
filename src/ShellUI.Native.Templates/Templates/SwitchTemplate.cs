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
        Dependencies = new List<string>(),
        Variants = new List<string> { "default" },
        Tags = new List<string> { "form", "switch", "toggle", "input" }
    };

    public static string Content => @"using Microsoft.Maui.Controls.Shapes;
namespace YourProjectNamespace.Components.UI;

// Switch/Toggle component
public partial class Switch : ContentView
{
    public static readonly BindableProperty IsToggledProperty =
        BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(Switch), 
            false, BindingMode.TwoWay, propertyChanged: OnIsToggledChanged);

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(Switch), 
            string.Empty, propertyChanged: OnLabelChanged);

    public static new readonly BindableProperty IsEnabledProperty =
        BindableProperty.Create(nameof(IsEnabled), typeof(bool), typeof(Switch),
            true, propertyChanged: OnIsEnabledChanged);

    private readonly Border _track;
    private readonly Border _thumb;
    private readonly Label _label;
    private readonly TapGestureRecognizer _tapGesture;

    private const double TrackWidth = 44;
    private const double TrackHeight = 24;
    private const double ThumbSize = 20;
    private const double ThumbOffset = 2;

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

    public new bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    public event EventHandler<bool>? Toggled;

    public Switch()
    {
        _thumb = new Border
        {
            WidthRequest = ThumbSize,
            HeightRequest = ThumbSize,
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ThumbSize / 2 }
        };

        _track = new Border
        {
            Content = _thumb,
            WidthRequest = TrackWidth,
            HeightRequest = TrackHeight,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = TrackHeight / 2 },
            Padding = new Thickness(ThumbOffset, 0, ThumbOffset, 0),
            HorizontalOptions = LayoutOptions.Start
        };

        _label = new Label
        {
            FontSize = 14,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(12, 0, 0, 0)
        };

        _tapGesture = new TapGestureRecognizer();
        _tapGesture.Tapped += OnTapped;

        var container = new HorizontalStackLayout
        {
            Spacing = 0,
            Children = { _track, _label }
        };

        container.GestureRecognizers.Add(_tapGesture);
        Content = container;

        UpdateVisualState();
    }

    private void OnTapped(object? sender, EventArgs e)
    {
        if (IsEnabled)
        {
            IsToggled = !IsToggled;
            AnimateToggle();
        }
    }

    private void AnimateToggle()
    {
        var targetX = IsToggled ? TrackWidth - ThumbSize - ThumbOffset : ThumbOffset;
        _ = _thumb.TranslateToAsync(targetX, 0, 200, Easing.CubicOut);
    }

    private static void OnIsToggledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Switch switchControl)
        {
            switchControl.UpdateVisualState();
            switchControl.AnimateToggle();
            switchControl.Toggled?.Invoke(switchControl, (bool)newValue);
        }
    }

    private static void OnLabelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Switch switchControl)
            switchControl._label.Text = newValue as string ?? string.Empty;
    }

    private static void OnIsEnabledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Switch switchControl)
            switchControl.UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        // Design tokens matching ShellUI theme
        var trackColor = IsToggled 
            ? Color.FromArgb(""#2563EB"") 
            : Color.FromArgb(""#D1D5DB"");

        _track.BackgroundColor = trackColor;
        _thumb.BackgroundColor = Colors.White;
        _label.TextColor = Color.FromArgb(""#1F2937"");
        
        var opacity = IsEnabled ? 1.0 : 0.5;
        _track.Opacity = opacity;
        _label.Opacity = opacity;

        // Position thumb
        var thumbX = IsToggled ? TrackWidth - ThumbSize - ThumbOffset : ThumbOffset;
        _thumb.TranslationX = thumbX;
    }
}
";

}