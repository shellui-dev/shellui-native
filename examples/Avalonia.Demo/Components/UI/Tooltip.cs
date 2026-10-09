using System.Threading.Tasks;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;

namespace AvaloniaDemo.Components.UI;

// Hover tooltip — rounded-md bg-primary px-3 py-1.5 text-xs text-primary-foreground, shown next to
// the wrapped control after a short delay, or at once when it gets keyboard focus.
// Usage: <ui:Tooltip Text="Add to library"><ui:Button Icon="Plus" Size="Icon" Variant="Outline" /></ui:Tooltip>
public class Tooltip : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Tooltip, string?>(nameof(Text));

    // Top (default), Bottom, Left or Right.
    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        AvaloniaProperty.Register<Tooltip, PlacementMode>(nameof(Placement), PlacementMode.Top);

    public static readonly StyledProperty<Control?> ContentProperty =
        AvaloniaProperty.Register<Tooltip, Control?>(nameof(Content));

    // Milliseconds the pointer must rest before the tooltip shows.
    public static readonly StyledProperty<int> DelayProperty =
        AvaloniaProperty.Register<Tooltip, int>(nameof(Delay), 400);

    private const double Gap = 6;

    // The wrapped control and the popup, which has to sit in the tree next to it.
    private readonly Panel _root = new();
    private readonly Popup _popup;
    private readonly TextBlock _label;
    private int _version;

    static Tooltip()
    {
        TextProperty.Changed.AddClassHandler<Tooltip>((t, _) =>
        {
            t._label.Text = t.Text ?? string.Empty;
            AutomationProperties.SetHelpText(t, t.Text);
        });
        ContentProperty.Changed.AddClassHandler<Tooltip>((t, e) =>
        {
            if (e.OldValue is Control old) t._root.Children.Remove(old);
            if (e.NewValue is Control content) t._root.Children.Insert(0, content);
        });
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // The wrapped control.
    [Content]
    public Control? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public PlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public int Delay
    {
        get => GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    public Tooltip()
    {
        _label = new TextBlock { FontSize = 12 };
        _label.Token(TextBlock.ForegroundProperty, ShellToken.PrimaryForeground);
        var bubble = new Border
        {
            Child = _label,
            Padding = new Thickness(12, 6),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            IsHitTestVisible = false
        };
        bubble.Token(BackgroundProperty, ShellToken.Primary);

        _popup = new Popup
        {
            ShouldUseOverlayLayer = true,
            IsLightDismissEnabled = false,
            PlacementTarget = this,
            Child = bubble,
            IsHitTestVisible = false
        };
        _root.Children.Add(_popup);
        Child = _root;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;

        // Tunnel, so a child that handles the press (Button) still hides the tooltip.
        AddHandler(PointerPressedEvent, (_, _) => Hide(), RoutingStrategies.Tunnel);
        AddHandler(GotFocusEvent, (_, e) =>
        {
            if (e.NavigationMethod is NavigationMethod.Tab or NavigationMethod.Directional) Show();
        });
        AddHandler(LostFocusEvent, (_, _) => Hide());
        AddHandler(KeyDownEvent, (_, e) =>
        {
            if (e.Key == Key.Escape && _popup.IsOpen) { Hide(); e.Handled = true; }
        }, RoutingStrategies.Tunnel);
    }

    protected override async void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        var version = ++_version;
        await Task.Delay(Delay);
        if (version == _version && IsPointerOver) Show();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        Hide();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Hide();
    }

    private void Show()
    {
        if (_popup.IsOpen || string.IsNullOrEmpty(Text)) return;
        _popup.Placement = Placement;
        SetOffset(1);
        _popup.IsOpen = true;
        // Avalonia flips a bubble with no room on its side but keeps the offset's sign; mirror it.
        Dispatcher.UIThread.Post(() =>
        {
            if (_popup.IsOpen && _popup.Child is Visual bubble && IsFlipped(bubble)) SetOffset(-1);
        }, DispatcherPriority.Loaded);
    }

    private void Hide()
    {
        _version++;
        _popup.IsOpen = false;
    }

    private void SetOffset(int sign)
    {
        var (x, y) = Placement switch
        {
            PlacementMode.Bottom => (0.0, Gap),
            PlacementMode.Left => (-Gap, 0.0),
            PlacementMode.Right => (Gap, 0.0),
            _ => (0.0, -Gap)
        };
        _popup.HorizontalOffset = x * sign;
        _popup.VerticalOffset = y * sign;
    }

    private bool IsFlipped(Visual bubble)
    {
        if (bubble.TranslatePoint(default, this) is not { } at) return false;
        return Placement switch
        {
            PlacementMode.Bottom => at.Y < 0,
            PlacementMode.Left => at.X >= 0,
            PlacementMode.Right => at.X < 0,
            _ => at.Y >= 0
        };
    }
}
