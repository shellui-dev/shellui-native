using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AvaloniaDemo.Components.UI;

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

        ColumnDefinitions = new ColumnDefinitions("*,Auto");
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
        _label.Text = $"{Percentage:F0}%";
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
