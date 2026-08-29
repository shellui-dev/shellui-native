namespace MAUI.Demo.Components.UI;

public partial class TabsTrigger : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(TabsTrigger), string.Empty);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(TabsTrigger), string.Empty,
            propertyChanged: (b, o, n) => (b as TabsTrigger)?.UpdateLabel());

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

    private readonly Label _label;
    private readonly BoxView _underline;
    private Tabs? _parent;

    public TabsTrigger()
    {
        MinimumHeightRequest = 40;

        _label = new Label
        {
            FontSize = 14,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Padding = new Thickness(12, 8)
        };
        _underline = new BoxView
        {
            HeightRequest = 2,
            Color = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill
        };
        Content = new VerticalStackLayout
        {
            Spacing = 0,
            Children = { _label, _underline }
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => _parent?.SetValue(Value);
        GestureRecognizers.Add(tap);
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (_parent != null)
            _parent.ValueChanged -= OnParentValueChanged;

        _parent = this.FindParentOfType<Tabs>();
        if (_parent != null)
        {
            _parent.ValueChanged += OnParentValueChanged;
            UpdateActiveState(_parent.Value);
        }
    }

    private void OnParentValueChanged(object? sender, (string Old, string New) e) => UpdateActiveState(e.New);

    private void UpdateActiveState(string activeValue)
    {
        var isActive = activeValue == Value;
        _label.TextColor = isActive ? Color.FromArgb("#2563EB") : Color.FromArgb("#6B7280");
        _label.FontAttributes = isActive ? FontAttributes.Bold : FontAttributes.None;
        _underline.Color = isActive ? Color.FromArgb("#2563EB") : Colors.Transparent;
    }

    private void UpdateLabel() => _label.Text = Text ?? string.Empty;
}
