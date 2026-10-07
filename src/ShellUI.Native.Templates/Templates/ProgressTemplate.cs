using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Progress component template
public static class ProgressTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "progress",
        DisplayName = "Progress",
        Description = "Progress bar indicator with percentage support",
        Category = ComponentCategory.DataDisplay,
        FilePath = "Progress.cs",
        Dependencies = new List<string> { "shell" },
        Variants = new List<string> { "default", "success", "warning", "destructive" },
        Tags = new List<string> { "progress", "loading", "indicator", "bar" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Progress bar — h-2 rounded-full track, fill animates to the new value.
public partial class Progress : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(double), typeof(Progress),
            0.0, propertyChanged: OnValueChanged);

    public static readonly BindableProperty MaximumProperty =
        BindableProperty.Create(nameof(Maximum), typeof(double), typeof(Progress),
            100.0, propertyChanged: OnValueChanged);

    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(ProgressVariant), typeof(Progress),
            ProgressVariant.Default, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty ShowLabelProperty =
        BindableProperty.Create(nameof(ShowLabel), typeof(bool), typeof(Progress),
            false, propertyChanged: OnVisualPropertyChanged);

    private const double TrackHeight = 8;

    private readonly Grid _track;
    private readonly BoxView _trackFill;
    private readonly Border _fill;
    private readonly Label _label;

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public ProgressVariant Variant
    {
        get => (ProgressVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public bool ShowLabel
    {
        get => (bool)GetValue(ShowLabelProperty);
        set => SetValue(ShowLabelProperty, value);
    }

    public double Percentage => Maximum > 0 ? Math.Clamp(Value / Maximum, 0, 1) * 100 : 0;

    public Progress()
    {
        // Track tint is the fill color at 20% (bg-primary/20), so it follows the variant.
        _trackFill = new BoxView { Opacity = 0.2, CornerRadius = TrackHeight / 2, BackgroundColor = Colors.Transparent };
        _fill = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = (float)(TrackHeight / 2) },
            HorizontalOptions = LayoutOptions.Start,
            WidthRequest = 0
        };
        _track = new Grid { HeightRequest = TrackHeight, Children = { _trackFill, _fill } };
        _track.SizeChanged += (_, _) => UpdateFill(animate: false);

        _label = new Label
        {
            FontSize = 12,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(12, 0, 0, 0),
            IsVisible = false
        };
        _label.Token(Label.TextColorProperty, ShellToken.MutedForeground);

        var container = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        container.Add(_track, 0, 0);
        container.Add(_label, 1, 0);
        Content = container;

        UpdateVisualState();
    }

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
        => (bindable as Progress)?.UpdateFill(animate: true);

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        => (bindable as Progress)?.UpdateVisualState();

    private void UpdateVisualState()
    {
        var token = Variant switch
        {
            ProgressVariant.Success => ShellToken.Success,
            ProgressVariant.Warning => ShellToken.Warning,
            ProgressVariant.Destructive => ShellToken.Destructive,
            _ => ShellToken.Primary
        };
        _fill.Token(VisualElement.BackgroundColorProperty, token);
        _trackFill.Token(BoxView.ColorProperty, token);
        _label.IsVisible = ShowLabel;
        UpdateFill(animate: false);
    }

    private void UpdateFill(bool animate)
    {
        _label.Text = $""{Percentage:F0}%"";
        if (_track.Width <= 0) return;
        var target = _track.Width * Percentage / 100;
        this.AbortAnimation(""ProgressFill"");
        if (!animate)
        {
            _fill.WidthRequest = target;
            return;
        }
        new Animation(v => _fill.WidthRequest = v, Math.Max(_fill.WidthRequest, 0), target, Easing.CubicOut)
            .Commit(this, ""ProgressFill"", 16, 300);
    }
}

public enum ProgressVariant
{
    Default,
    Success,
    Warning,
    Destructive
}
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;

namespace YourProjectNamespace.Components.UI;

// Progress bar — h-2 rounded-full track, fill animates to the new value.
public class Progress : Grid
{
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<Progress, double>(nameof(Value));

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<Progress, double>(nameof(Maximum), 100.0);

    public static readonly StyledProperty<ProgressVariant> VariantProperty =
        AvaloniaProperty.Register<Progress, ProgressVariant>(nameof(Variant));

    public static readonly StyledProperty<bool> ShowLabelProperty =
        AvaloniaProperty.Register<Progress, bool>(nameof(ShowLabel));

    private const double TrackHeight = 8;

    private readonly Panel _track;
    private readonly Border _trackFill;
    private readonly Border _fill;
    private readonly TextBlock _label;
    private readonly Transitions _fillTransitions = new()
    {
        new DoubleTransition { Property = WidthProperty, Duration = TimeSpan.FromMilliseconds(300), Easing = new CubicEaseOut() }
    };

    static Progress()
    {
        ValueProperty.Changed.AddClassHandler<Progress>((p, _) => p.UpdateFill(animate: true));
        MaximumProperty.Changed.AddClassHandler<Progress>((p, _) => p.UpdateFill(animate: true));
        VariantProperty.Changed.AddClassHandler<Progress>((p, _) => p.UpdateVisualState());
        ShowLabelProperty.Changed.AddClassHandler<Progress>((p, _) => p.UpdateVisualState());
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public ProgressVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public bool ShowLabel
    {
        get => GetValue(ShowLabelProperty);
        set => SetValue(ShowLabelProperty, value);
    }

    public double Percentage => Maximum > 0 ? Math.Clamp(Value / Maximum, 0, 1) * 100 : 0;

    public Progress()
    {
        // Track tint is the fill color at 20% (bg-primary/20), so it follows the variant.
        _trackFill = new Border { Opacity = 0.2, CornerRadius = new CornerRadius(TrackHeight / 2) };
        _fill = new Border { CornerRadius = new CornerRadius(TrackHeight / 2), HorizontalAlignment = HorizontalAlignment.Left, Width = 0 };
        _track = new Panel { Height = TrackHeight, VerticalAlignment = VerticalAlignment.Center, Children = { _trackFill, _fill } };
        _track.SizeChanged += (_, _) => UpdateFill(animate: false);

        _label = new TextBlock { FontSize = 12, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0), IsVisible = false };
        _label.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);

        ColumnDefinitions = new ColumnDefinitions(""*,Auto"");
        SetColumn(_label, 1);
        Children.Add(_track);
        Children.Add(_label);
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        var token = Variant switch
        {
            ProgressVariant.Success => ShellToken.Success,
            ProgressVariant.Warning => ShellToken.Warning,
            ProgressVariant.Destructive => ShellToken.Destructive,
            _ => ShellToken.Primary
        };
        _fill.Token(Border.BackgroundProperty, token);
        _trackFill.Token(Border.BackgroundProperty, token);
        _label.IsVisible = ShowLabel;
        UpdateFill(animate: false);
    }

    private void UpdateFill(bool animate)
    {
        _label.Text = $""{Percentage:F0}%"";
        if (_track.Bounds.Width <= 0) return;
        // Resizes jump to the new width; only value changes animate.
        _fill.Transitions = animate ? _fillTransitions : null;
        _fill.Width = _track.Bounds.Width * Percentage / 100;
    }
}

public enum ProgressVariant
{
    Default,
    Success,
    Warning,
    Destructive
}
"
    };

}