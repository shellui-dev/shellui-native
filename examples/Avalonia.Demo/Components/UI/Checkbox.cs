using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Checkbox — h-4 w-4 rounded-sm border border-primary; checked: bg-primary + check icon.
// The whole row (box + label) is the hit target.
public class Checkbox : Border
{
    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<Checkbox, bool>(nameof(IsChecked), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<Checkbox, string?>(nameof(Label));

    public static readonly StyledProperty<bool> HasErrorProperty =
        AvaloniaProperty.Register<Checkbox, bool>(nameof(HasError));

    private readonly Border _box;
    private readonly Icon _check;
    private readonly TextBlock _label;

    static Checkbox()
    {
        IsCheckedProperty.Changed.AddClassHandler<Checkbox>((c, e) =>
        {
            c.UpdateVisualState();
            c.CheckedChanged?.Invoke(c, (bool)e.NewValue!);
        });
        LabelProperty.Changed.AddClassHandler<Checkbox>((c, _) => c.UpdateLabel());
        HasErrorProperty.Changed.AddClassHandler<Checkbox>((c, _) => c.UpdateVisualState());
        IsEnabledProperty.Changed.AddClassHandler<Checkbox>((c, _) => c.Opacity = c.IsEnabled ? 1.0 : 0.5);
    }

    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool HasError
    {
        get => GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public event EventHandler<bool>? CheckedChanged;

    public Checkbox()
    {
        _check = new Icon { Kind = IconName.Check, Size = 12, StrokeWidth = 3, Token = ShellToken.PrimaryForeground };
        _box = new Border
        {
            Child = _check,
            Width = 16,
            Height = 16,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusSm),
            VerticalAlignment = VerticalAlignment.Center
        };
        _label = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, IsVisible = false };
        _label.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);

        Child = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, MinHeight = 24, Children = { _box, _label } };
        Background = Brushes.Transparent;
        HorizontalAlignment = HorizontalAlignment.Left;
        Cursor = new Cursor(StandardCursorType.Hand);
        ShellFocus.Ring(this, _box);
        UpdateVisualState();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left || !IsEffectivelyEnabled) return;
        IsChecked = !IsChecked;
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key != Key.Space) return;
        IsChecked = !IsChecked;
        e.Handled = true;
    }

    private void UpdateLabel()
    {
        _label.Text = Label ?? string.Empty;
        _label.IsVisible = !string.IsNullOrEmpty(Label);
    }

    private void UpdateVisualState()
    {
        _box.Token(BorderBrushProperty, HasError ? ShellToken.Destructive : ShellToken.Primary);
        if (IsChecked)
            _box.Token(BackgroundProperty, ShellToken.Primary);
        else
        {
            _box.ClearToken(BackgroundProperty);
            _box.Background = Brushes.Transparent;
        }
        _check.IsVisible = IsChecked;
    }
}
