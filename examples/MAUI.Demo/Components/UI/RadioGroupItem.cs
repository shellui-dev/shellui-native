using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Radio option — h-4 w-4 rounded-full border border-primary; checked shows a filled dot.
public partial class RadioGroupItem : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(RadioGroupItem), string.Empty);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(RadioGroupItem), string.Empty,
            propertyChanged: (b, o, n) => ((RadioGroupItem)b)._label.Text = n as string ?? string.Empty);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsChecked { get; private set; }

    private readonly Border _ring;
    private readonly Border _dot;
    private readonly Label _label;

    public RadioGroupItem()
    {
        _dot = new Border
        {
            WidthRequest = 8,
            HeightRequest = 8,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 4 },
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Scale = 0
        };
        _dot.Token(VisualElement.BackgroundColorProperty, ShellToken.Primary);

        _ring = new Border
        {
            Content = _dot,
            WidthRequest = 16,
            HeightRequest = 16,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            VerticalOptions = LayoutOptions.Center
        };
        _ring.Token(Border.StrokeProperty, ShellToken.Primary);

        _label = new Label { FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center };
        _label.Token(Label.TextColorProperty, ShellToken.Foreground);

        var row = new HorizontalStackLayout { Spacing = 8, Children = { _ring, _label } };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (!IsEnabled) return; ShellFocus.FocusPressed(this); Select(); };
        row.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, () => { if (IsEnabled) Select(); });

        Content = row;
        HorizontalOptions = LayoutOptions.Start;
    }

    private void Select() => this.FindParentOfType<RadioGroup>()?.SetValue(Value);

    internal void SetChecked(bool isChecked)
    {
        if (IsChecked == isChecked && _dot.Scale == (isChecked ? 1 : 0)) return;
        IsChecked = isChecked;
        _ = _dot.ScaleToAsync(isChecked ? 1 : 0, 120, Easing.CubicOut);
    }
}
