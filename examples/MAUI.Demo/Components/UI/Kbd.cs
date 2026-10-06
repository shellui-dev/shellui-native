using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Keyboard key — h-5 rounded border bg-muted px-1.5 text-[11px] font-medium text-muted-foreground.
// Usage: <ui:Kbd Text="Ctrl" /> <ui:Kbd Text="K" />
public partial class Kbd : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Kbd), string.Empty,
            propertyChanged: (b, o, n) => ((Kbd)b)._label.Text = n as string ?? string.Empty);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private readonly Label _label;

    public Kbd()
    {
        _label = new Label
        {
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.NoWrap
        };
        _label.Token(Label.TextColorProperty, ShellToken.MutedForeground);

        var key = new Border
        {
            Content = _label,
            HeightRequest = 22,
            MinimumWidthRequest = 22,
            Padding = new Thickness(6, 0),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusSm }
        };
        key.Token(VisualElement.BackgroundColorProperty, ShellToken.Muted);
        key.Token(Border.StrokeProperty, ShellToken.Border);

        Content = key;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
    }
}
