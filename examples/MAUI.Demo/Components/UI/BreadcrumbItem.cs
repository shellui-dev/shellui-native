namespace MAUI.Demo.Components.UI;

public partial class BreadcrumbItem : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(BreadcrumbItem), string.Empty,
            propertyChanged: (b, o, n) => (b as BreadcrumbItem)?.UpdateVisualState());

    public static readonly BindableProperty IsCurrentProperty =
        BindableProperty.Create(nameof(IsCurrent), typeof(bool), typeof(BreadcrumbItem), false,
            propertyChanged: (b, o, n) => (b as BreadcrumbItem)?.UpdateVisualState());

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // IsCurrent=true renders as the active page (bolder, foreground color, no tap effect).
    public bool IsCurrent
    {
        get => (bool)GetValue(IsCurrentProperty);
        set => SetValue(IsCurrentProperty, value);
    }

    public event EventHandler? Clicked;

    private readonly Label _text;
    private readonly Label _separator;

    public BreadcrumbItem()
    {
        _text = new Label { FontSize = 14, VerticalOptions = LayoutOptions.Center };
        _separator = new Label
        {
            Text = "/",
            FontSize = 14,
            TextColor = Color.FromArgb("#9CA3AF"),
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(8, 0)
        };
        Content = new HorizontalStackLayout { Spacing = 0, Children = { _text, _separator } };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => { if (!IsCurrent) Clicked?.Invoke(this, EventArgs.Empty); };
        GestureRecognizers.Add(tap);

        UpdateVisualState();
    }

    // Called by parent Breadcrumb — the last item hides its trailing separator.
    public void SetSeparatorVisible(bool visible) => _separator.IsVisible = visible;

    private void UpdateVisualState()
    {
        _text.Text = Text ?? string.Empty;
        _text.TextColor = IsCurrent ? Color.FromArgb("#1F2937") : Color.FromArgb("#6B7280");
        _text.FontAttributes = IsCurrent ? FontAttributes.Bold : FontAttributes.None;
    }
}
