using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Hover tooltip — rounded-md bg-primary px-3 py-1.5 text-xs text-primary-foreground, shown above
// the wrapped view after a short delay (pointer devices; touch has no hover).
// Usage: <ui:Tooltip Text="Add to library"><ui:Button Icon="Plus" Size="Icon" Variant="Outline" /></ui:Tooltip>
[ContentProperty(nameof(Content))]
public partial class Tooltip : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Tooltip), string.Empty,
            propertyChanged: (b, o, n) => ((Tooltip)b)._label.Text = n as string ?? string.Empty);

    public static readonly BindableProperty PlacementProperty =
        BindableProperty.Create(nameof(Placement), typeof(ShellPopupPlacement), typeof(Tooltip), ShellPopupPlacement.Top);

    // Milliseconds the pointer must rest before the tooltip shows.
    public static readonly BindableProperty DelayProperty =
        BindableProperty.Create(nameof(Delay), typeof(int), typeof(Tooltip), 400);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ShellPopupPlacement Placement
    {
        get => (ShellPopupPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public int Delay
    {
        get => (int)GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    private readonly Label _label;
    private readonly Border _bubble;
    private ShellPopupHandle? _handle;
    private int _hoverVersion;

    public Tooltip()
    {
        _label = new Label { FontSize = 12, LineBreakMode = LineBreakMode.NoWrap };
        _label.Token(Label.TextColorProperty, ShellToken.PrimaryForeground);
        _bubble = new Border
        {
            Content = _label,
            Padding = new Thickness(12, 6),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            InputTransparent = true
        };
        _bubble.Token(VisualElement.BackgroundColorProperty, ShellToken.Primary);

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += async (_, _) =>
        {
            var version = ++_hoverVersion;
            await Task.Delay(Delay);
            if (version == _hoverVersion) Show();
        };
        pointer.PointerExited += (_, _) => Hide();
        pointer.PointerPressed += (_, _) => Hide();
        GestureRecognizers.Add(pointer);

        HorizontalOptions = LayoutOptions.Start;
        Unloaded += (_, _) => Hide();
    }

    private void Show()
    {
        if (_handle != null || string.IsNullOrEmpty(Text)) return;
        SemanticProperties.SetHint(this, Text);
        _handle = ShellPortal.ShowPopup(this, _bubble, new ShellPopupOptions
        {
            Placement = Placement,
            Align = ShellPopupAlign.Center,
            Offset = 6,
            Modal = false
        });
    }

    private void Hide()
    {
        _hoverVersion++;
        var handle = _handle;
        _handle = null;
        if (handle != null) _ = handle.CloseAsync();
    }
}
