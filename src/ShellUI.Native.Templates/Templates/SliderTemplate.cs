using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class SliderTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "slider",
        DisplayName = "Slider",
        Description = "Range slider input",
        Category = ComponentCategory.Form,
        FilePath = "Slider.cs",
        Dependencies = new List<string> { "shell" },
        Tags = new List<string> { "form", "input", "range", "slider" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Range slider — platform slider tinted with the theme (primary range, secondary track).
public partial class Slider : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(double), typeof(Slider), 50.0, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((Slider)b).SyncValue());

    public static readonly BindableProperty MinimumProperty =
        BindableProperty.Create(nameof(Minimum), typeof(double), typeof(Slider), 0.0,
            propertyChanged: (b, o, n) => ((Slider)b).SyncRange());

    public static readonly BindableProperty MaximumProperty =
        BindableProperty.Create(nameof(Maximum), typeof(double), typeof(Slider), 100.0,
            propertyChanged: (b, o, n) => ((Slider)b).SyncRange());

    private readonly Microsoft.Maui.Controls.Slider _native;

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    // User input only: setting Value from code or a binding doesn't raise it.
    public event EventHandler<ValueChangedEventArgs>? ValueChanged;

    // Set while Value, Minimum or Maximum is pushed into the platform slider, which reports
    // those changes like a drag.
    private bool _syncing;

    public Slider()
    {
        _native = new Microsoft.Maui.Controls.Slider { Minimum = 0, Maximum = 100, Value = 50 };
        _native.Token(Microsoft.Maui.Controls.Slider.MinimumTrackColorProperty, ShellToken.Primary);
        _native.Token(Microsoft.Maui.Controls.Slider.MaximumTrackColorProperty, ShellToken.Secondary);
        _native.Token(Microsoft.Maui.Controls.Slider.ThumbColorProperty, ShellToken.Primary);
        _native.ValueChanged += (s, e) =>
        {
            Value = e.NewValue;
            if (!_syncing) ValueChanged?.Invoke(this, e);
        };
        Content = _native;
    }

    private void SyncRange()
    {
        var syncing = _syncing;
        _syncing = true;
        // Widen before narrowing so Minimum never exceeds Maximum mid-update.
        if (Maximum > _native.Maximum) { _native.Maximum = Maximum; _native.Minimum = Minimum; }
        else { _native.Minimum = Minimum; _native.Maximum = Maximum; }
        SyncValue();
        _syncing = syncing;
    }

    private void SyncValue()
    {
        if (Math.Abs(_native.Value - Value) <= 0.001) return;
        var syncing = _syncing;
        _syncing = true;
        _native.Value = Value;
        _syncing = syncing;
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;

namespace YourProjectNamespace.Components.UI;

// Range slider — h-2 rounded-full bg-secondary track, bg-primary range, h-5 w-5 rounded-full
// border-2 border-primary bg-background thumb. Custom-drawn: drag or click the track, or use the
// arrow keys (Step), Page Up/Down (10 steps), Home and End.
public class Slider : Border
{
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<Slider, double>(nameof(Value), 50.0, defaultBindingMode: BindingMode.TwoWay,
            coerce: (o, v) => o is Slider s ? Math.Clamp(v, s.Minimum, Math.Max(s.Minimum, s.Maximum)) : v);

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<Slider, double>(nameof(Minimum), 0.0);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<Slider, double>(nameof(Maximum), 100.0);

    // Keyboard increment. Pointer input is continuous.
    public static readonly StyledProperty<double> StepProperty =
        AvaloniaProperty.Register<Slider, double>(nameof(Step), 1.0);

    private const double ThumbSize = 20;
    private const double TrackHeight = 8;

    private readonly Border _track;
    private readonly Border _range;
    private readonly Border _thumb;
    private bool _dragging;

    static Slider()
    {
        ValueProperty.Changed.AddClassHandler<Slider>((s, e) =>
        {
            s.UpdateLayoutOfParts();
            if (s._byUser) s.ValueChanged?.Invoke(s, (double)e.NewValue!);
        });
        MinimumProperty.Changed.AddClassHandler<Slider>((s, _) => { s.CoerceValue(ValueProperty); s.UpdateLayoutOfParts(); });
        MaximumProperty.Changed.AddClassHandler<Slider>((s, _) => { s.CoerceValue(ValueProperty); s.UpdateLayoutOfParts(); });
        IsEnabledProperty.Changed.AddClassHandler<Slider>((s, _) => s.Opacity = s.IsEnabled ? 1.0 : 0.5);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Step
    {
        get => GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    // User input only: setting Value from code or a binding doesn't raise it.
    public event EventHandler<double>? ValueChanged;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    public Slider()
    {
        _track = new Border { Height = TrackHeight, CornerRadius = new CornerRadius(TrackHeight / 2), VerticalAlignment = VerticalAlignment.Center };
        _track.Token(BackgroundProperty, ShellToken.Secondary);
        _range = new Border { Height = TrackHeight, CornerRadius = new CornerRadius(TrackHeight / 2), HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center };
        _range.Token(BackgroundProperty, ShellToken.Primary);
        _thumb = new Border
        {
            Width = ThumbSize,
            Height = ThumbSize,
            CornerRadius = new CornerRadius(ThumbSize / 2),
            BorderThickness = new Thickness(2),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        _thumb.Token(BackgroundProperty, ShellToken.Background).Token(BorderBrushProperty, ShellToken.Primary);

        Child = new Panel { Children = { _track, _range, _thumb } };
        Height = ThumbSize;
        Background = Avalonia.Media.Brushes.Transparent;
        Cursor = new Cursor(StandardCursorType.Hand);
        ShellFocus.Ring(this, _thumb);
        SizeChanged += (_, _) => UpdateLayoutOfParts();
    }

    private double Fraction => Maximum > Minimum ? (Value - Minimum) / (Maximum - Minimum) : 0;

    // The thumb travels the track width minus its own size, so it never overhangs the ends.
    private void UpdateLayoutOfParts()
    {
        var travel = Math.Max(0, Bounds.Width - ThumbSize);
        var x = travel * Fraction;
        _thumb.Margin = new Thickness(x, 0, 0, 0);
        _range.Width = x + ThumbSize / 2;
    }

    private void SetFromPointer(PointerEventArgs e)
    {
        var travel = Bounds.Width - ThumbSize;
        if (travel <= 0) return;
        var fraction = Math.Clamp((e.GetPosition(this).X - ThumbSize / 2) / travel, 0, 1);
        ByUser(() => Value = Minimum + fraction * (Maximum - Minimum));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _dragging = true;
        e.Pointer.Capture(this);
        SetFromPointer(e);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_dragging) SetFromPointer(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_dragging) return;
        _dragging = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        _dragging = false;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        double? target = e.Key switch
        {
            Key.Left or Key.Down => Value - Step,
            Key.Right or Key.Up => Value + Step,
            Key.PageDown => Value - Step * 10,
            Key.PageUp => Value + Step * 10,
            Key.Home => Minimum,
            Key.End => Maximum,
            _ => null
        };
        if (target is not { } value) return;
        ByUser(() => Value = value);
        e.Handled = true;
    }
}
"
    };
}
