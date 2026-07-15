namespace MAUI.Demo.Components.UI;

public partial class DropdownItem : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(DropdownItem), string.Empty,
            propertyChanged: (b, o, n) => (b as DropdownItem)?.UpdateContent());

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public event EventHandler? Clicked;

    private readonly Label _label;

    public DropdownItem()
    {
        _label = new Label
        {
            FontSize = 14,
            VerticalOptions = LayoutOptions.Center,
            Padding = new Thickness(8, 10)
        };
        Content = _label;
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnTapped;
        GestureRecognizers.Add(tap);
        UpdateContent();
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        this.FindParentOfType<Dropdown>()?.CloseAsync();
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateContent() => _label.Text = Text ?? string.Empty;
}
