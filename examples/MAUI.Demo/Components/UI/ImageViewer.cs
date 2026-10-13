using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Image viewer — a rounded-md thumbnail that opens the image over the page on a dark backdrop,
// with zoom out, reset, zoom in and close controls. Pinching zooms too; a tap on the backdrop
// or Escape closes it. Give it a size: the thumbnail fills it.
//   <ui:ImageViewer Source="photo.jpg" Description="The harbour at dusk" WidthRequest="160" HeightRequest="120" />
// ThumbnailSource shows a smaller file in the page and loads Source only in the viewer.
public partial class ImageViewer : ShellOverlayHost
{
    public static readonly BindableProperty SourceProperty =
        BindableProperty.Create(nameof(Source), typeof(ImageSource), typeof(ImageViewer), null,
            propertyChanged: (b, o, n) => ((ImageViewer)b).UpdateSources());

    public static readonly BindableProperty ThumbnailSourceProperty =
        BindableProperty.Create(nameof(ThumbnailSource), typeof(ImageSource), typeof(ImageViewer), null,
            propertyChanged: (b, o, n) => ((ImageViewer)b).UpdateSources());

    // What the image shows, for screen readers.
    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(ImageViewer), string.Empty,
            propertyChanged: (b, o, n) => ((ImageViewer)b).UpdateSources());

    // How the thumbnail fills its box.
    public static readonly BindableProperty AspectProperty =
        BindableProperty.Create(nameof(Aspect), typeof(Aspect), typeof(ImageViewer), Aspect.AspectFill,
            propertyChanged: (b, o, n) => ((ImageViewer)b)._thumbnail.Image.Aspect = (Aspect)n);

    public ImageSource? Source
    {
        get => (ImageSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public ImageSource? ThumbnailSource
    {
        get => (ImageSource?)GetValue(ThumbnailSourceProperty);
        set => SetValue(ThumbnailSourceProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public Aspect Aspect
    {
        get => (Aspect)GetValue(AspectProperty);
        set => SetValue(AspectProperty, value);
    }

    public const double MinZoom = 0.5;
    public const double MaxZoom = 4;

    // 1 is the size that fits the page.
    public double Zoom { get; private set; } = 1;

    private readonly ImageViewerThumbnail _thumbnail;
    private readonly ImageViewerContent _content;

    public ImageViewer()
    {
        _thumbnail = new ImageViewerThumbnail(() => SetOpen(true));
        _content = new ImageViewerContent(this);
        Children.Add(_thumbnail);
        Children.Add(_content);
        OpenChanged += (_, open) =>
        {
            if (open) SetZoom(1, animate: false);
        };
    }

    protected override bool IsTrigger(Element child) => child is ImageViewerThumbnail;

    public void ZoomIn() => SetZoom(Zoom + 0.25);
    public void ZoomOut() => SetZoom(Zoom - 0.25);
    public void ResetZoom() => SetZoom(1);

    internal void SetZoom(double zoom, bool animate = true)
    {
        Zoom = Math.Clamp(zoom, MinZoom, MaxZoom);
        _content.ShowZoom(Zoom, animate);
    }

    private void UpdateSources()
    {
        _thumbnail.Image.Source = ThumbnailSource ?? Source;
        _content.Image.Source = Source;
        SemanticProperties.SetDescription(_thumbnail, string.IsNullOrEmpty(Description) ? "View image" : $"View {Description}");
        SemanticProperties.SetDescription(_content.Image, Description ?? string.Empty);
    }
}

// The thumbnail of an ImageViewer; it grows a little under the pointer.
internal sealed partial class ImageViewerThumbnail : ContentView
{
    public Image Image { get; } = new() { Aspect = Aspect.AspectFill };

    public ImageViewerThumbnail(Action open)
    {
        var frame = new Border
        {
            Content = Image,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd }
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => _ = Image.ScaleToAsync(1.05, 200, Easing.CubicOut);
        pointer.PointerExited += (_, _) => _ = Image.ScaleToAsync(1, 200, Easing.CubicOut);
        frame.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { ShellFocus.FocusPressed(this); open(); };
        frame.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, open);
        Content = frame;
    }
}

// The backdrop, image and controls of an open ImageViewer.
internal sealed partial class ImageViewerContent : ContentView, IShellOverlayContent
{
    public Image Image { get; } = new() { Aspect = Aspect.AspectFit, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };

    private readonly ImageViewer _viewer;
    private readonly BoxView _backdrop;
    private readonly Border _zoomOut;
    private readonly Border _zoomIn;
    private readonly Label _percent;
    private double _pinchStart = 1;

    public ImageViewerContent(ImageViewer viewer)
    {
        _viewer = viewer;
        // bg-black/80 in both themes.
        _backdrop = new BoxView { Color = Color.FromRgba(0, 0, 0, 0.8) };
        var tapBackdrop = new TapGestureRecognizer();
        tapBackdrop.Tapped += (_, _) => viewer.SetOpen(false);
        _backdrop.GestureRecognizers.Add(tapBackdrop);

        // Swallow taps on the image so they don't fall through to the backdrop.
        Image.GestureRecognizers.Add(new TapGestureRecognizer());
        var pinch = new PinchGestureRecognizer();
        pinch.PinchUpdated += (_, e) =>
        {
            if (e.Status == GestureStatus.Started) _pinchStart = viewer.Zoom;
            else if (e.Status == GestureStatus.Running) viewer.SetZoom(_pinchStart * e.Scale, animate: false);
        };
        Image.GestureRecognizers.Add(pinch);

        _percent = new Label { FontSize = 12, TextColor = Colors.White, VerticalTextAlignment = TextAlignment.Center, HorizontalTextAlignment = TextAlignment.Center };
        _zoomOut = Control(new Icon { Name = IconName.Minus, Size = 16, Color = Colors.White }, "Zoom out", viewer.ZoomOut);
        _zoomIn = Control(new Icon { Name = IconName.Plus, Size = 16, Color = Colors.White }, "Zoom in", viewer.ZoomIn);
        var reset = Control(_percent, "Reset zoom", viewer.ResetZoom);
        reset.WidthRequest = 52;
        var close = Control(new Icon { Name = IconName.X, Size = 16, Color = Colors.White }, "Close", () => viewer.SetOpen(false));

        var controls = new HorizontalStackLayout
        {
            Spacing = 8,
            Margin = new Thickness(16),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Children = { _zoomOut, reset, _zoomIn, close }
        };

        var root = new Grid { Children = { _backdrop, Image, controls } };
        ShellPortal.EdgeToEdge(this, root);
        // max-h-[90vh] max-w-[90vw]
        root.SizeChanged += (_, _) =>
        {
            Image.MaximumWidthRequest = root.Width * 0.9;
            Image.MaximumHeightRequest = root.Height * 0.9;
        };
        Content = root;
        ShowZoom(1, animate: false);
    }

    public void ShowZoom(double zoom, bool animate)
    {
        _percent.Text = $"{zoom * 100:0}%";
        _zoomOut.Opacity = zoom <= ImageViewer.MinZoom ? 0.5 : 1;
        _zoomIn.Opacity = zoom >= ImageViewer.MaxZoom ? 0.5 : 1;
        Image.AbortAnimation("ScaleTo");
        if (animate) _ = Image.ScaleToAsync(zoom, 200, Easing.CubicOut);
        else Image.Scale = zoom;
    }

    // h-9 w-9 rounded-md bg-white/10 text-white hover:bg-white/20
    private static Border Control(View content, string description, Action action)
    {
        content.HorizontalOptions = LayoutOptions.Center;
        content.VerticalOptions = LayoutOptions.Center;
        var control = new Border
        {
            Content = content,
            WidthRequest = 36,
            HeightRequest = 36,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Color.FromRgba(255, 255, 255, 0.1)
        };
        SemanticProperties.SetDescription(control, description);
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => control.BackgroundColor = Color.FromRgba(255, 255, 255, 0.2);
        pointer.PointerExited += (_, _) => control.BackgroundColor = Color.FromRgba(255, 255, 255, 0.1);
        control.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => action();
        control.GestureRecognizers.Add(tap);
        return control;
    }

    public async Task AnimateAsync(bool open)
    {
        if (open)
        {
            Opacity = 0;
            await this.FadeToAsync(1, 150, Easing.CubicOut);
        }
        else
        {
            await this.FadeToAsync(0, 120, Easing.CubicIn);
        }
    }
}
