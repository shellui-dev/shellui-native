using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Tab button — rounded-md px-3 text-sm font-medium; active: bg-background text-foreground
// shadow-sm; inactive: text-muted-foreground.
public partial class TabsTrigger : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(TabsTrigger), string.Empty);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(TabsTrigger), string.Empty,
            propertyChanged: (b, o, n) => ((TabsTrigger)b)._label.Text = n as string ?? string.Empty);

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

    public bool IsActive { get; private set; }

    private readonly Border _pill;
    private readonly Label _label;

    public TabsTrigger()
    {
        _label = new Label
        {
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Center
        };
        _pill = new Border
        {
            Content = _label,
            Padding = new Thickness(12, 0),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        Content = _pill;

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (!IsEnabled) return; ShellFocus.FocusPressed(this); Select(); };
        _pill.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, () => { if (IsEnabled) Select(); });

        SetActive(false);
    }

    private void Select() => this.FindParentOfType<Tabs>()?.SetValue(Value);

    internal void SetActive(bool active)
    {
        IsActive = active;
        _label.Token(Label.TextColorProperty, active ? ShellToken.Foreground : ShellToken.MutedForeground);
        if (active)
        {
            _pill.Token(VisualElement.BackgroundColorProperty, ShellToken.Background);
            _pill.Shadow = new Shadow { Brush = new SolidColorBrush(Colors.Black), Offset = new Point(0, 1), Radius = 2, Opacity = 0.1f };
        }
        else
        {
            _pill.ClearValue(VisualElement.BackgroundColorProperty);
            _pill.BackgroundColor = Colors.Transparent;
            _pill.ClearValue(VisualElement.ShadowProperty);
        }
    }
}
