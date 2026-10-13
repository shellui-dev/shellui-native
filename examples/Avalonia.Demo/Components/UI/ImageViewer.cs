using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Image viewer — a rounded-md thumbnail that opens the image over the window on a dark backdrop,
// with zoom out, reset, zoom in and close controls. The wheel and the + / - / 0 keys zoom too; a
// click on the backdrop or Escape closes it. Give it a size: the thumbnail fills it.
//   <ui:ImageViewer Source="/Assets/photo.jpg" Description="The harbour at dusk" Width="160" Height="120" />
// ThumbnailSource shows a smaller file in the page and Source only in the viewer.
public class ImageViewer : ShellOverlayHost, IShellFocusable
{
    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<ImageViewer, IImage?>(nameof(Source));

    public static readonly StyledProperty<IImage?> ThumbnailSourceProperty =
        AvaloniaProperty.Register<ImageViewer, IImage?>(nameof(ThumbnailSource));

    // What the image shows, for screen readers.
    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<ImageViewer, string?>(nameof(Description));

    // How the thumbnail fills its box.
    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<ImageViewer, Stretch>(nameof(Stretch), Stretch.UniformToFill);

    public const double MinZoom = 0.5;
    public const double MaxZoom = 4;

    private readonly Image _thumbnailImage = new() { Stretch = Stretch.UniformToFill, RenderTransformOrigin = RelativePoint.Center };
    private readonly Border _thumbnail;
    private readonly ImageViewerContent _content;

    static ImageViewer()
    {
        SourceProperty.Changed.AddClassHandler<ImageViewer>((v, _) => v.UpdateSources());
        ThumbnailSourceProperty.Changed.AddClassHandler<ImageViewer>((v, _) => v.UpdateSources());
        DescriptionProperty.Changed.AddClassHandler<ImageViewer>((v, _) => v.UpdateSources());
        StretchProperty.Changed.AddClassHandler<ImageViewer>((v, e) => v._thumbnailImage.Stretch = (Stretch)e.NewValue!);
    }

    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public IImage? ThumbnailSource
    {
        get => GetValue(ThumbnailSourceProperty);
        set => SetValue(ThumbnailSourceProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    // 1 is the size that fits the window.
    public double Zoom { get; private set; } = 1;

    public ImageViewer()
    {
        _thumbnailImage.Transitions = new Transitions
        {
            new TransformOperationsTransition { Property = RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(200), Easing = new CubicEaseOut() }
        };
        _thumbnail = new Border
        {
            Child = _thumbnailImage,
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            ClipToBounds = true,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        // The thumbnail grows a little under the pointer.
        _thumbnail.PointerEntered += (_, _) => _thumbnailImage.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Parse("scale(1.05)");
        _thumbnail.PointerExited += (_, _) => _thumbnailImage.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Parse("scale(1)");
        _thumbnail.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left || !IsEffectivelyEnabled) return;
            SetOpen(true);
            e.Handled = true;
        };

        _content = new ImageViewerContent(this);
        Items.Add(_thumbnail);
        Items.Add(_content);
        ShellFocus.Ring(this, _thumbnail);
        OpenChanged += (_, open) =>
        {
            if (open) SetZoom(1);
        };
    }

    public void ZoomIn() => SetZoom(Zoom + 0.25);
    public void ZoomOut() => SetZoom(Zoom - 0.25);
    public void ResetZoom() => SetZoom(1);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (Open || !ReferenceEquals(e.Source, this) || e.Key is not (Key.Enter or Key.Space)) return;
        SetOpen(true);
        e.Handled = true;
    }

    internal void SetZoom(double zoom)
    {
        Zoom = Math.Clamp(zoom, MinZoom, MaxZoom);
        _content.ShowZoom(Zoom);
    }

    private void UpdateSources()
    {
        _thumbnailImage.Source = ThumbnailSource ?? Source;
        _content.Image.Source = Source;
        AutomationProperties.SetName(this, string.IsNullOrEmpty(Description) ? "View image" : $"View {Description}");
        AutomationProperties.SetName(_content.Image, Description ?? string.Empty);
    }
}

// The backdrop, image and controls of an open ImageViewer.
internal sealed class ImageViewerContent : Border, IShellOverlayContent
{
    public Image Image { get; } = new()
    {
        Stretch = Stretch.Uniform,
        StretchDirection = StretchDirection.DownOnly,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        RenderTransformOrigin = RelativePoint.Center
    };

    private readonly ImageViewer _viewer;
    private readonly Border _zoomOut;
    private readonly Border _zoomIn;
    private readonly TextBlock _percent;

    public ImageViewerContent(ImageViewer viewer)
    {
        _viewer = viewer;
        // bg-black/80 in both themes.
        var backdrop = new Border { Background = new SolidColorBrush(Colors.Black, 0.8) };
        backdrop.PointerPressed += (_, e) =>
        {
            viewer.SetOpen(false);
            e.Handled = true;
        };

        Image.Transitions = new Transitions
        {
            new TransformOperationsTransition { Property = RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(200), Easing = new CubicEaseOut() }
        };
        // Clicks on the image must not reach the backdrop behind it.
        Image.PointerPressed += (_, e) => e.Handled = true;

        _percent = new TextBlock { FontSize = 12, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        _zoomOut = Control(new Icon { Kind = IconName.Minus, Size = 16, Color = Colors.White }, "Zoom out", viewer.ZoomOut);
        var reset = Control(_percent, "Reset zoom", viewer.ResetZoom);
        reset.Width = 52;
        _zoomIn = Control(new Icon { Kind = IconName.Plus, Size = 16, Color = Colors.White }, "Zoom in", viewer.ZoomIn);
        var close = Control(new Icon { Kind = IconName.X, Size = 16, Color = Colors.White }, "Close", () => viewer.SetOpen(false));

        var controls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(16),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Children = { _zoomOut, reset, _zoomIn, close }
        };

        Child = new Panel { Children = { backdrop, Image, controls } };
        ClipToBounds = true;
        // max-h-[90vh] max-w-[90vw]
        SizeChanged += (_, e) =>
        {
            Image.MaxWidth = e.NewSize.Width * 0.9;
            Image.MaxHeight = e.NewSize.Height * 0.9;
        };
        ShowZoom(1);
    }

    public void ShowZoom(double zoom)
    {
        _percent.Text = $"{zoom * 100:0}%";
        _zoomOut.Opacity = zoom <= ImageViewer.MinZoom ? 0.5 : 1;
        _zoomIn.Opacity = zoom >= ImageViewer.MaxZoom ? 0.5 : 1;
        Image.RenderTransform = Avalonia.Media.Transformation.TransformOperations.Parse(
            FormattableString.Invariant($"scale({zoom})"));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.Key)
        {
            case Key.OemPlus or Key.Add:
                _viewer.ZoomIn();
                break;
            case Key.OemMinus or Key.Subtract:
                _viewer.ZoomOut();
                break;
            case Key.D0 or Key.NumPad0:
                _viewer.ResetZoom();
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if (e.Delta.Y > 0) _viewer.ZoomIn();
        else if (e.Delta.Y < 0) _viewer.ZoomOut();
        e.Handled = true;
    }

    // h-9 w-9 rounded-md bg-white/10 text-white hover:bg-white/20; a tab stop (Enter / Space).
    private static Border Control(Control content, string name, Action action)
    {
        content.HorizontalAlignment = HorizontalAlignment.Center;
        content.VerticalAlignment = VerticalAlignment.Center;
        var normal = new SolidColorBrush(Colors.White, 0.1);
        var hover = new SolidColorBrush(Colors.White, 0.2);
        var control = new Border
        {
            Child = content,
            Width = 36,
            Height = 36,
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            Background = normal,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        AutomationProperties.SetName(control, name);
        ShellFocus.Ring(control, control);
        control.PointerEntered += (_, _) => control.Background = hover;
        control.PointerExited += (_, _) => control.Background = normal;
        control.PointerPressed += (_, e) => e.Handled = true;
        control.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;
            action();
            e.Handled = true;
        };
        control.KeyDown += (_, e) =>
        {
            if (e.Key is not (Key.Enter or Key.Space)) return;
            action();
            e.Handled = true;
        };
        return control;
    }

    public async Task AnimateAsync(bool open)
    {
        if (open)
        {
            ShellMotion.Set(this, 0, "scale(1)");
            await ShellMotion.To(this, 1, "scale(1)", 150, new CubicEaseOut());
        }
        else
        {
            await ShellMotion.To(this, 0, "scale(1)", 120, new CubicEaseIn());
        }
    }
}
