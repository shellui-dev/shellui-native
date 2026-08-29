namespace MAUI.Demo.Components.UI;

[ContentProperty(nameof(Content))]
public partial class TabsContent : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(TabsContent), string.Empty,
            propertyChanged: (b, o, n) => (b as TabsContent)?.RefreshVisibility());

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private Tabs? _parent;

    public TabsContent()
    {
        IsVisible = false;
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
            RefreshVisibility();
        }
    }

    private void OnParentValueChanged(object? sender, (string Old, string New) e) => RefreshVisibility();

    private void RefreshVisibility()
    {
        IsVisible = _parent != null && _parent.Value == Value;
    }
}
