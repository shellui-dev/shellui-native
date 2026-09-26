namespace MAUI.Demo.Components.UI;

// Modal dialog - compositional. Usage: <Dialog><DialogTrigger>...</DialogTrigger><DialogContent>...</DialogContent></Dialog>
public partial class Dialog : Grid
{
    public static readonly BindableProperty OpenProperty =
        BindableProperty.Create(nameof(Open), typeof(bool), typeof(Dialog), false,
            propertyChanged: (b, o, n) => (b as Dialog)?.OnOpenChanged());

    public bool Open
    {
        get => (bool)GetValue(OpenProperty);
        set => SetValue(OpenProperty, value);
    }

    public event EventHandler<bool>? OpenChanged;

    private readonly VerticalStackLayout _triggerContainer;
    private readonly Grid _overlayLayer;

    public Dialog()
    {
        // The host fills its area (page-root overlays fill the whole page). While closed it must
        // not swallow input meant for content underneath; CascadeInputTransparent = false keeps
        // an inline trigger inside it clickable. Toggled off while open so the backdrop catches taps.
        InputTransparent = true;
        CascadeInputTransparent = false;
        RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _triggerContainer = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Start };
        _overlayLayer = new Grid { IsVisible = false, ZIndex = 1000 };
        Children.Add(_triggerContainer);
        Children.Add(_overlayLayer);
        Grid.SetRow(_triggerContainer, 0);
        Grid.SetRow(_overlayLayer, 0);
    }

    protected override void OnChildAdded(Element child)  
    {
        base.OnChildAdded(child);
        if (child == _triggerContainer || child == _overlayLayer) return;
        Dispatcher.Dispatch(() =>
        {
            if (child is IView view)
            {
                Children.Remove(view);
                if (child is DialogTrigger dt)
                    _triggerContainer.Children.Add(dt);
                else if (child is DialogContent dc)
                    _overlayLayer.Children.Add(dc);
            }
        });
    }

    public void SetOpen(bool value)
    {
        if (Open != value) { Open = value; OnOpenChanged(); }
    }

    private void OnOpenChanged()
    {
        _overlayLayer.IsVisible = Open;
        InputTransparent = !Open;
        OpenChanged?.Invoke(this, Open);
    }
}
