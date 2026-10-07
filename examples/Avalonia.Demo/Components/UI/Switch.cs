using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;

namespace AvaloniaDemo.Components.UI;

// Switch — h-6 w-11 rounded-full track (primary when on, input when off),
// h-5 w-5 thumb in bg-background that slides translate-x-5.
public class Switch : Border
{
    public static readonly StyledProperty<bool> IsToggledProperty =
        AvaloniaProperty.Register<Switch, bool>(nameof(IsToggled), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<Switch, string?>(nameof(Label));

    private const double TrackWidth = 44;
    private const double TrackHeight = 24;
    private const double Inset = 2;
    private const double ThumbSize = TrackHeight - Inset * 2;          // 20
    private const double Travel = TrackWidth - Inset * 2 - ThumbSize;  // 20

    private readonly Border _track;
    private readonly Border _thumb;
    private readonly TextBlock _label;

    static Switch()
    {
        IsToggledProperty.Changed.AddClassHandler<Switch>((s, e) =>
        {
            s.UpdateVisualState();
            s.Toggled?.Invoke(s, (bool)e.NewValue!);
        });
        LabelProperty.Changed.AddClassHandler<Switch>((s, _) => s.UpdateLabel());
        IsEnabledProperty.Changed.AddClassHandler<Switch>((s, _) => s.Opacity = s.IsEnabled ? 1.0 : 0.5);
    }

    public bool IsToggled
    {
        get => GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public event EventHandler<bool>? Toggled;

    public Switch()
    {
        _thumb = new Border
        {
            Width = ThumbSize,
            Height = ThumbSize,
            CornerRadius = new CornerRadius(ThumbSize / 2),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            BoxShadow = new BoxShadows(new BoxShadow { OffsetY = 1, Blur = 3, Color = ShellTheme.Shadow(0.2) })
        };
        _thumb.Token(BackgroundProperty, ShellToken.Background);

        _track = new Border
        {
            Child = _thumb,
            Width = TrackWidth,
            Height = TrackHeight,
            Padding = new Thickness(Inset),
            CornerRadius = new CornerRadius(TrackHeight / 2),
            VerticalAlignment = VerticalAlignment.Center
        };

        _label = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, IsVisible = false };
        _label.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);

        Child = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { _track, _label } };
        Background = Brushes.Transparent;
        HorizontalAlignment = HorizontalAlignment.Left;
        Cursor = new Cursor(StandardCursorType.Hand);
        ShellFocus.Ring(this, _track);
        UpdateVisualState();
        // Added after the first state, so the thumb doesn't slide into place on load.
        _thumb.Transitions = new Transitions
        {
            new TransformOperationsTransition { Property = RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(150), Easing = new CubicEaseOut() }
        };
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left || !IsEffectivelyEnabled) return;
        IsToggled = !IsToggled;
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is not (Key.Space or Key.Enter)) return;
        IsToggled = !IsToggled;
        e.Handled = true;
    }

    private void UpdateLabel()
    {
        _label.Text = Label ?? string.Empty;
        _label.IsVisible = !string.IsNullOrEmpty(Label);
    }

    private void UpdateVisualState()
    {
        _track.Token(BackgroundProperty, IsToggled ? ShellToken.Primary : ShellToken.Input);
        _thumb.RenderTransform = TransformOperations.Parse($"translateX({(IsToggled ? Travel : 0)}px)");
    }
}
