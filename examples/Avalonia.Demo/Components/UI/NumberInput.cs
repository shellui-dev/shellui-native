using System;
using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Number field with − and + buttons — h-10 rounded-md border, the value centered between them.
// The value is clamped to Minimum / Maximum; the buttons dim at the limits. Up and Down step
// the value while the field has focus.
// Usage: <ui:NumberInput Value="{Binding Quantity}" Minimum="1" Maximum="10" />
public class NumberInput : Border
{
    public static readonly StyledProperty<decimal?> ValueProperty =
        AvaloniaProperty.Register<NumberInput, decimal?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay,
            coerce: (o, value) => ((NumberInput)o).Clamp(value));

    public static readonly StyledProperty<decimal?> MinimumProperty =
        AvaloniaProperty.Register<NumberInput, decimal?>(nameof(Minimum));

    public static readonly StyledProperty<decimal?> MaximumProperty =
        AvaloniaProperty.Register<NumberInput, decimal?>(nameof(Maximum));

    public static readonly StyledProperty<decimal> StepProperty =
        AvaloniaProperty.Register<NumberInput, decimal>(nameof(Step), 1m);

    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<NumberInput, string?>(nameof(Placeholder));

    private readonly TextBox _textBox;
    private readonly Border _decrease;
    private readonly Border _increase;

    static NumberInput()
    {
        ValueProperty.Changed.AddClassHandler<NumberInput>((n, _) =>
        {
            n.UpdateText();
            n.UpdateButtons();
            if (n._byUser) n.ValueChanged?.Invoke(n, n.Value);
        });
        MinimumProperty.Changed.AddClassHandler<NumberInput>((n, _) => n.OnLimitsChanged());
        MaximumProperty.Changed.AddClassHandler<NumberInput>((n, _) => n.OnLimitsChanged());
        PlaceholderProperty.Changed.AddClassHandler<NumberInput>((n, e) => n._textBox.PlaceholderText = (string?)e.NewValue);
        IsEnabledProperty.Changed.AddClassHandler<NumberInput>((n, _) => n.Opacity = n.IsEnabled ? 1.0 : 0.5);
    }

    public decimal? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public decimal? Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public decimal? Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public decimal Step
    {
        get => GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    // User input only: setting Value from code or a binding doesn't raise it.
    public event EventHandler<decimal?>? ValueChanged;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    public NumberInput()
    {
        _textBox = new TextBox
        {
            FontSize = 14,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        ShellPlatform.StripNativeChrome(_textBox);
        _textBox.GotFocus += (_, _) => this.Token(BorderBrushProperty, ShellToken.Ring);
        _textBox.LostFocus += (_, _) =>
        {
            this.Token(BorderBrushProperty, ShellToken.Input);
            Commit();
        };
        _textBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter) Commit();
            else if (e.Key == Key.Up) StepBy(1);
            else if (e.Key == Key.Down) StepBy(-1);
            else return;
            e.Handled = true;
        };
        Grid.SetColumn(_textBox, 2);

        _decrease = StepButton(IconName.Minus, "Decrease", -1);
        _increase = StepButton(IconName.Plus, "Increase", 1);
        Grid.SetColumn(_increase, 4);

        Child = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("40,1,*,1,40"),
            Children = { _decrease, Divider(1), _textBox, Divider(3), _increase }
        };
        Height = 40;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        // The step buttons' hover tint stays inside the rounded corners.
        ClipToBounds = true;
        this.Token(BorderBrushProperty, ShellToken.Input);
        UpdateText();
        UpdateButtons();
    }

    private static Border Divider(int column)
    {
        var line = new Border();
        line.Token(BackgroundProperty, ShellToken.Input);
        Grid.SetColumn(line, column);
        return line;
    }

    private Border StepButton(IconName icon, string name, int direction)
    {
        var glyph = new Icon { Kind = icon, Size = 16, Token = ShellToken.MutedForeground, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        var button = new Border { Child = glyph, Background = Brushes.Transparent, Cursor = new Cursor(StandardCursorType.Hand) };
        AutomationProperties.SetName(button, name);
        button.PointerEntered += (_, _) =>
        {
            if (button.Opacity < 0.9) return;
            button.Token(BackgroundProperty, ShellToken.Muted);
            glyph.Token = ShellToken.Foreground;
        };
        button.PointerExited += (_, _) =>
        {
            button.ClearToken(BackgroundProperty);
            button.Background = Brushes.Transparent;
            glyph.Token = ShellToken.MutedForeground;
        };
        button.PointerPressed += (_, e) => e.Handled = true;
        button.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;
            StepBy(direction);
            e.Handled = true;
        };
        return button;
    }

    private void StepBy(int direction)
    {
        if (!IsEffectivelyEnabled) return;
        Commit();
        ByUser(() => Value = (Value ?? Minimum ?? 0) + direction * Step);
    }

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    private decimal? Clamp(decimal? value)
    {
        if (value is not { } number) return null;
        if (Minimum is { } min && number < min) number = min;
        if (Maximum is { } max && number > max) number = max;
        return number;
    }

    // A typed number is clamped and kept, empty clears the value, anything else is reverted.
    private void Commit() => ByUser(CommitText);

    private void CommitText()
    {
        var text = _textBox.Text?.Trim();
        if (string.IsNullOrEmpty(text)) Value = null;
        else if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out var typed) ||
                 decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out typed))
            Value = typed;
        UpdateText();
    }

    private void OnLimitsChanged()
    {
        CoerceValue(ValueProperty);
        UpdateButtons();
    }

    private void UpdateText()
    {
        var text = Value?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        if (_textBox.Text != text) _textBox.Text = text;
    }

    private void UpdateButtons()
    {
        _decrease.Opacity = Value is { } v && Minimum is { } min && v <= min ? 0.4 : 1;
        _increase.Opacity = Value is { } w && Maximum is { } max && w >= max ? 0.4 : 1;
    }
}
