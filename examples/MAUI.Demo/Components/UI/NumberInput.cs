using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Number field with − and + buttons — h-10 rounded-md border, the value centered between them.
// The value is clamped to Minimum / Maximum; the buttons dim at the limits.
// Usage: <ui:NumberInput Value="{Binding Quantity}" Minimum="1" Maximum="10" />
public partial class NumberInput : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(decimal?), typeof(NumberInput), null, BindingMode.TwoWay,
            coerceValue: (b, v) => ((NumberInput)b).Clamp((decimal?)v),
            propertyChanged: (b, o, n) => ((NumberInput)b).OnValueChanged());

    public static readonly BindableProperty MinimumProperty =
        BindableProperty.Create(nameof(Minimum), typeof(decimal?), typeof(NumberInput), null,
            propertyChanged: (b, o, n) => ((NumberInput)b).OnLimitsChanged());

    public static readonly BindableProperty MaximumProperty =
        BindableProperty.Create(nameof(Maximum), typeof(decimal?), typeof(NumberInput), null,
            propertyChanged: (b, o, n) => ((NumberInput)b).OnLimitsChanged());

    public static readonly BindableProperty StepProperty =
        BindableProperty.Create(nameof(Step), typeof(decimal), typeof(NumberInput), 1m);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(NumberInput), string.Empty,
            propertyChanged: (b, o, n) => ((NumberInput)b)._entry.Placeholder = (string)n);

    public decimal? Value
    {
        get => (decimal?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public decimal? Minimum
    {
        get => (decimal?)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public decimal? Maximum
    {
        get => (decimal?)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public decimal Step
    {
        get => (decimal)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public event EventHandler<decimal?>? ValueChanged;

    private readonly Border _frame;
    private readonly Entry _entry;
    private readonly Grid _decrease;
    private readonly Grid _increase;

    public NumberInput()
    {
        _entry = new Entry
        {
            Keyboard = Keyboard.Numeric,
            FontSize = 14,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent
        };
        _entry.Token(Entry.TextColorProperty, ShellToken.Foreground);
        _entry.Token(Entry.PlaceholderColorProperty, ShellToken.MutedForeground);
        ShellPlatform.StripNativeChrome(_entry);
        ShellFocus.Track(_entry);
        _entry.Focused += (_, _) => _frame!.Token(Border.StrokeProperty, ShellToken.Ring);
        _entry.Unfocused += (_, _) => { _frame!.Token(Border.StrokeProperty, ShellToken.Input); Commit(); };
        _entry.Completed += (_, _) => Commit();

        _decrease = StepButton(IconName.Minus, "Decrease", -1);
        _increase = StepButton(IconName.Plus, "Increase", 1);

        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(40), new ColumnDefinition(1), new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(1), new ColumnDefinition(40)
            }
        };
        row.Add(_decrease, 0, 0);
        row.Add(Divider(), 1, 0);
        row.Add(_entry, 2, 0);
        row.Add(Divider(), 3, 0);
        row.Add(_increase, 4, 0);

        _frame = new Border
        {
            Content = row,
            HeightRequest = 40,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        _frame.Token(Border.StrokeProperty, ShellToken.Input);
        Content = _frame;
        UpdateText();
        UpdateButtons();
    }

    private static BoxView Divider()
    {
        var line = new BoxView { BackgroundColor = Colors.Transparent };
        line.Token(BoxView.ColorProperty, ShellToken.Input);
        return line;
    }

    private Grid StepButton(IconName icon, string description, int direction)
    {
        var glyph = new Icon { Name = icon, Size = 16, Token = ShellToken.MutedForeground, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        var button = new Grid { BackgroundColor = Colors.Transparent, Children = { glyph } };
        SemanticProperties.SetDescription(button, description);
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { if (button.Opacity > 0.9) { button.Token(VisualElement.BackgroundColorProperty, ShellToken.Muted); glyph.Token = ShellToken.Foreground; } };
        pointer.PointerExited += (_, _) =>
        {
            button.ClearValue(VisualElement.BackgroundColorProperty);
            button.BackgroundColor = Colors.Transparent;
            glyph.Token = ShellToken.MutedForeground;
        };
        button.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            if (!IsEnabled) return;
            Commit();
            Value = (Value ?? Minimum ?? 0) + direction * Step;
        };
        button.GestureRecognizers.Add(tap);
        return button;
    }

    private decimal? Clamp(decimal? value)
    {
        if (value is not { } number) return null;
        if (Minimum is { } min && number < min) number = min;
        if (Maximum is { } max && number > max) number = max;
        return number;
    }

    // Reads what the user typed: a number is clamped and kept, empty clears the value, and
    // anything else puts the last good value back.
    private void Commit()
    {
        var text = _entry.Text?.Trim();
        if (string.IsNullOrEmpty(text)) Value = null;
        else if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out var typed) ||
                 decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out typed))
            Value = typed;
        UpdateText();
    }

    private void OnLimitsChanged()
    {
        Value = Clamp(Value);
        UpdateButtons();
    }

    private void OnValueChanged()
    {
        UpdateText();
        UpdateButtons();
        ValueChanged?.Invoke(this, Value);
    }

    private void UpdateText()
    {
        var text = Value?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        if (_entry.Text != text) _entry.Text = text;
    }

    private void UpdateButtons()
    {
        _decrease.Opacity = Value is { } v && Minimum is { } min && v <= min ? 0.4 : 1;
        _increase.Opacity = Value is { } w && Maximum is { } max && w >= max ? 0.4 : 1;
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName && _entry != null)
        {
            Opacity = IsEnabled ? 1.0 : 0.5;
            _entry.IsEnabled = IsEnabled;
        }
    }
}
