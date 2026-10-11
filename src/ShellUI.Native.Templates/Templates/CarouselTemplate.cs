using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class CarouselTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "carousel",
        DisplayName = "Carousel",
        Description = "Swipeable slides with arrows, dots and auto-play",
        Category = ComponentCategory.DataDisplay,
        FilePath = "Carousel.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "carousel", "slider", "slides", "gallery" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Carousel — one slide at a time in a rounded-lg frame; swipe, use the round arrow buttons or
// tap a dot to move. Each child view is a slide.
//   <ui:Carousel AspectRatio=""2"" Loop=""True"">
//       <Image Source=""one.jpg"" Aspect=""AspectFill"" />
//       <Image Source=""two.jpg"" Aspect=""AspectFill"" />
//   </ui:Carousel>
[ContentProperty(nameof(Slides))]
public partial class Carousel : ContentView
{
    public static readonly BindableProperty PositionProperty =
        BindableProperty.Create(nameof(Position), typeof(int), typeof(Carousel), 0, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((Carousel)b).OnPositionChanged());

    // At the last slide, Next wraps to the first (and Previous the other way).
    public static readonly BindableProperty LoopProperty =
        BindableProperty.Create(nameof(Loop), typeof(bool), typeof(Carousel), true,
            propertyChanged: (b, o, n) => ((Carousel)b).UpdateControls());

    public static readonly BindableProperty ShowArrowsProperty =
        BindableProperty.Create(nameof(ShowArrows), typeof(bool), typeof(Carousel), true,
            propertyChanged: (b, o, n) => ((Carousel)b).UpdateControls());

    public static readonly BindableProperty ShowDotsProperty =
        BindableProperty.Create(nameof(ShowDots), typeof(bool), typeof(Carousel), true,
            propertyChanged: (b, o, n) => ((Carousel)b).UpdateControls());

    public static readonly BindableProperty AutoPlayProperty =
        BindableProperty.Create(nameof(AutoPlay), typeof(bool), typeof(Carousel), false,
            propertyChanged: (b, o, n) => ((Carousel)b).UpdateTimer());

    // Milliseconds between slides while AutoPlay is on.
    public static readonly BindableProperty AutoPlayIntervalProperty =
        BindableProperty.Create(nameof(AutoPlayInterval), typeof(int), typeof(Carousel), 3000,
            propertyChanged: (b, o, n) => ((Carousel)b).UpdateTimer());

    // Width / height of the frame. 0 turns it off: set HeightRequest on the carousel instead.
    public static readonly BindableProperty AspectRatioProperty =
        BindableProperty.Create(nameof(AspectRatio), typeof(double), typeof(Carousel), 16d / 9d,
            propertyChanged: (b, o, n) => ((Carousel)b).Resize());

    public int Position
    {
        get => (int)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public bool Loop
    {
        get => (bool)GetValue(LoopProperty);
        set => SetValue(LoopProperty, value);
    }

    public bool ShowArrows
    {
        get => (bool)GetValue(ShowArrowsProperty);
        set => SetValue(ShowArrowsProperty, value);
    }

    public bool ShowDots
    {
        get => (bool)GetValue(ShowDotsProperty);
        set => SetValue(ShowDotsProperty, value);
    }

    public bool AutoPlay
    {
        get => (bool)GetValue(AutoPlayProperty);
        set => SetValue(AutoPlayProperty, value);
    }

    public int AutoPlayInterval
    {
        get => (int)GetValue(AutoPlayIntervalProperty);
        set => SetValue(AutoPlayIntervalProperty, value);
    }

    public double AspectRatio
    {
        get => (double)GetValue(AspectRatioProperty);
        set => SetValue(AspectRatioProperty, value);
    }

    public IList<IView> Slides => _track.Children;

    public event EventHandler<int>? PositionChanged;

    private const string SlideAnimation = ""CarouselSlide"";

    private readonly Grid _track;
    private readonly Border _frame;
    private readonly Border _previous;
    private readonly Border _next;
    private readonly HorizontalStackLayout _dots;
    private IDispatcherTimer? _timer;
    // Where the track is, in slides: 1.5 is halfway between the second and third slide.
    private double _offset;
    private double _dragX;

    public Carousel()
    {
        _track = new Grid { IsClippedToBounds = true, BackgroundColor = Colors.Transparent };
        _track.ChildAdded += (_, _) => OnSlidesChanged();
        _track.ChildRemoved += (_, _) => OnSlidesChanged();
        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPan;
        _track.GestureRecognizers.Add(pan);

        _frame = new Border
        {
            Content = _track,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusLg },
            BackgroundColor = Colors.Transparent
        };

        _previous = Arrow(IconName.ChevronLeft, LayoutOptions.Start, ""Previous slide"", Previous);
        _next = Arrow(IconName.ChevronRight, LayoutOptions.End, ""Next slide"", Next);
        _dots = new HorizontalStackLayout
        {
            Spacing = 0,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 0, 0, 8)
        };

        Content = new Grid { Children = { _frame, _previous, _next, _dots } };
        SizeChanged += (_, _) => Resize();
        Loaded += (_, _) => UpdateTimer();
        Unloaded += (_, _) => StopTimer();
        UpdateControls();
    }

    private int Count => _track.Children.Count;
    private int Current => Math.Clamp(Position, 0, Math.Max(0, Count - 1));

    public void Next()
    {
        if (Current < Count - 1) Position = Current + 1;
        else if (Loop && Count > 1) Position = 0;
    }

    public void Previous()
    {
        if (Current > 0) Position = Current - 1;
        else if (Loop && Count > 1) Position = Count - 1;
    }

    private Border Arrow(IconName icon, LayoutOptions side, string description, Action press)
    {
        var button = new Border
        {
            Content = new Icon { Name = icon, Size = 16, Token = ShellToken.Foreground },
            WidthRequest = 32,
            HeightRequest = 32,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            HorizontalOptions = side,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(12, 0),
            Shadow = new Shadow { Brush = new SolidColorBrush(Colors.Black), Offset = new Point(0, 2), Radius = 6, Opacity = 0.15f }
        };
        button.Token(VisualElement.BackgroundColorProperty, ShellToken.Background);
        button.Token(Border.StrokeProperty, ShellToken.Border);
        SemanticProperties.SetDescription(button, description);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => press();
        button.GestureRecognizers.Add(tap);
        return button;
    }

    private void OnSlidesChanged()
    {
        _dots.Children.Clear();
        for (var i = 0; i < Count; i++)
        {
            var index = i;
            var dot = new Border
            {
                WidthRequest = 8,
                HeightRequest = 8,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 4 },
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            dot.Token(VisualElement.BackgroundColorProperty, ShellToken.Primary);
            // 8px dot in a 16px tap target.
            var target = new Grid { WidthRequest = 16, HeightRequest = 16, BackgroundColor = Colors.Transparent, Children = { dot } };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => Position = index;
            target.GestureRecognizers.Add(tap);
            _dots.Children.Add(target);
        }
        _offset = Current;
        Render();
        UpdateControls();
    }

    private void OnPositionChanged()
    {
        UpdateControls();
        this.AbortAnimation(SlideAnimation);
        new Animation(v => { _offset = v; Render(); }, _offset, Current)
            .Commit(this, SlideAnimation, 16, 300, Easing.CubicOut);
        PositionChanged?.Invoke(this, Current);
        // A manual move restarts the auto-play countdown.
        UpdateTimer();
    }

    private void Resize()
    {
        if (AspectRatio > 0 && Width > 0) _frame.HeightRequest = Width / AspectRatio;
        else if (AspectRatio <= 0) _frame.HeightRequest = -1;
        Render();
    }

    // Slides are stacked in one cell and spread out sideways by translation.
    private void Render()
    {
        var width = _track.Width > 0 ? _track.Width : Width;
        if (width <= 0) return;
        for (var i = 0; i < Count; i++)
        {
            if (_track.Children[i] is View slide)
                slide.TranslationX = (i - _offset) * width;
        }
    }

    private void UpdateControls()
    {
        var current = Current;
        var many = Count > 1;
        _previous.IsVisible = ShowArrows && many;
        _next.IsVisible = ShowArrows && many;
        _previous.Opacity = Loop || current > 0 ? 0.9 : 0.4;
        _next.Opacity = Loop || current < Count - 1 ? 0.9 : 0.4;
        _dots.IsVisible = ShowDots && many;
        for (var i = 0; i < _dots.Children.Count; i++)
        {
            // One color at two strengths, so the dots stay readable over any slide.
            if (_dots.Children[i] is Grid { Children: [Border dot] })
                dot.Opacity = i == current ? 1 : 0.3;
        }
    }

    private void OnPan(object? sender, PanUpdatedEventArgs e)
    {
        var width = _track.Width;
        if (width <= 0 || Count < 2) return;
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                this.AbortAnimation(SlideAnimation);
                _dragX = 0;
                break;
            case GestureStatus.Running:
                // Follow the finger, with a little give past the first and last slide.
                _dragX = e.TotalX;
                _offset = Math.Clamp(Current - _dragX / width, -0.25, Count - 0.75);
                Render();
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                var moved = _dragX / width;
                _dragX = 0;
                var current = Current;
                if (moved <= -0.2 && current < Count - 1) Position = current + 1;
                else if (moved >= 0.2 && current > 0) Position = current - 1;
                else
                {
                    // Not far enough: settle back.
                    new Animation(v => { _offset = v; Render(); }, _offset, current)
                        .Commit(this, SlideAnimation, 16, 200, Easing.CubicOut);
                }
                break;
        }
    }

    private void UpdateTimer()
    {
        StopTimer();
        if (!AutoPlay || Count < 2 || Handler is null) return;
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(Math.Max(500, AutoPlayInterval));
        _timer.Tick += (_, _) =>
        {
            if (Current < Count - 1) Position = Current + 1;
            else Position = 0;
        };
        _timer.Start();
    }

    private void StopTimer()
    {
        _timer?.Stop();
        _timer = null;
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Threading;

namespace YourProjectNamespace.Components.UI;

// Carousel — one slide at a time in a rounded-lg frame; drag, use the round arrow buttons, the
// Left / Right keys or click a dot to move. Each child control is a slide.
//   <ui:Carousel AspectRatio=""2"" Loop=""True"">
//       <Image Source=""/Assets/one.jpg"" Stretch=""UniformToFill"" />
//       <Image Source=""/Assets/two.jpg"" Stretch=""UniformToFill"" />
//   </ui:Carousel>
public class Carousel : Border
{
    public static readonly StyledProperty<int> PositionProperty =
        AvaloniaProperty.Register<Carousel, int>(nameof(Position), defaultBindingMode: BindingMode.TwoWay);

    // At the last slide, Next wraps to the first (and Previous the other way).
    public static readonly StyledProperty<bool> LoopProperty =
        AvaloniaProperty.Register<Carousel, bool>(nameof(Loop), true);

    public static readonly StyledProperty<bool> ShowArrowsProperty =
        AvaloniaProperty.Register<Carousel, bool>(nameof(ShowArrows), true);

    public static readonly StyledProperty<bool> ShowDotsProperty =
        AvaloniaProperty.Register<Carousel, bool>(nameof(ShowDots), true);

    public static readonly StyledProperty<bool> AutoPlayProperty =
        AvaloniaProperty.Register<Carousel, bool>(nameof(AutoPlay));

    // Milliseconds between slides while AutoPlay is on.
    public static readonly StyledProperty<int> AutoPlayIntervalProperty =
        AvaloniaProperty.Register<Carousel, int>(nameof(AutoPlayInterval), 3000);

    // Width / height of the frame. 0 turns it off: set Height on the carousel instead.
    public static readonly StyledProperty<double> AspectRatioProperty =
        AvaloniaProperty.Register<Carousel, double>(nameof(AspectRatio), 16d / 9d);

    private const double DragThreshold = 6;

    private readonly Panel _track;
    private readonly Border _previous;
    private readonly Border _next;
    private readonly StackPanel _dots;
    private readonly Easing _easing = new CubicEaseOut();
    private DispatcherTimer? _timer;
    // Where the track is, in slides: 1.5 is halfway between the second and third slide.
    private double _offset;
    private int _animation;
    private Point? _pressedAt;
    private bool _dragging;
    private double _dragX;

    static Carousel()
    {
        PositionProperty.Changed.AddClassHandler<Carousel>((c, _) => c.OnPositionChanged());
        LoopProperty.Changed.AddClassHandler<Carousel>((c, _) => c.UpdateControls());
        ShowArrowsProperty.Changed.AddClassHandler<Carousel>((c, _) => c.UpdateControls());
        ShowDotsProperty.Changed.AddClassHandler<Carousel>((c, _) => c.UpdateControls());
        AutoPlayProperty.Changed.AddClassHandler<Carousel>((c, _) => c.UpdateTimer());
        AutoPlayIntervalProperty.Changed.AddClassHandler<Carousel>((c, _) => c.UpdateTimer());
        AffectsMeasure<Carousel>(AspectRatioProperty);
    }

    public int Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public bool Loop
    {
        get => GetValue(LoopProperty);
        set => SetValue(LoopProperty, value);
    }

    public bool ShowArrows
    {
        get => GetValue(ShowArrowsProperty);
        set => SetValue(ShowArrowsProperty, value);
    }

    public bool ShowDots
    {
        get => GetValue(ShowDotsProperty);
        set => SetValue(ShowDotsProperty, value);
    }

    public bool AutoPlay
    {
        get => GetValue(AutoPlayProperty);
        set => SetValue(AutoPlayProperty, value);
    }

    public int AutoPlayInterval
    {
        get => GetValue(AutoPlayIntervalProperty);
        set => SetValue(AutoPlayIntervalProperty, value);
    }

    public double AspectRatio
    {
        get => GetValue(AspectRatioProperty);
        set => SetValue(AspectRatioProperty, value);
    }

    [Content]
    public Controls Slides => _track.Children;

    public event EventHandler<int>? PositionChanged;

    public Carousel()
    {
        // Transparent, not null, so a drag can start between the slides' own content.
        _track = new Panel { ClipToBounds = true, Background = Brushes.Transparent };
        _track.Children.CollectionChanged += (_, _) => OnSlidesChanged();
        _track.SizeChanged += (_, _) => Render();
        _track.PointerPressed += OnTrackPressed;
        _track.PointerMoved += OnTrackMoved;
        _track.PointerReleased += OnTrackReleased;
        _track.PointerCaptureLost += (_, _) => EndDrag(cancelled: true);

        _previous = Arrow(IconName.ChevronLeft, HorizontalAlignment.Left, ""Previous slide"", Previous);
        _next = Arrow(IconName.ChevronRight, HorizontalAlignment.Right, ""Next slide"", Next);
        foreach (var arrow in new[] { _previous, _next })
        {
            arrow.PointerEntered += (_, _) => UpdateControls();
            arrow.PointerExited += (_, _) => UpdateControls();
        }
        _dots = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 8)
        };

        Child = new Panel { Children = { _track, _previous, _next, _dots } };
        CornerRadius = new CornerRadius(ShellTheme.RadiusLg);
        // Slides stay inside the rounded corners.
        ClipToBounds = true;
        UpdateControls();
    }

    private int Count => _track.Children.Count;
    private int Current => Math.Clamp(Position, 0, Math.Max(0, Count - 1));

    public void Next()
    {
        if (Current < Count - 1) Position = Current + 1;
        else if (Loop && Count > 1) Position = 0;
    }

    public void Previous()
    {
        if (Current > 0) Position = Current - 1;
        else if (Loop && Count > 1) Position = Count - 1;
    }

    // The frame takes the width it is given and derives its height from AspectRatio.
    protected override Size MeasureOverride(Size availableSize)
    {
        if (AspectRatio <= 0) return base.MeasureOverride(availableSize);
        var width = double.IsInfinity(availableSize.Width) ? base.MeasureOverride(availableSize).Width : availableSize.Width;
        var size = new Size(width, width / AspectRatio);
        base.MeasureOverride(size);
        return size;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateTimer();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopTimer();
        _animation++;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled || e.Source is TextBox) return;
        if (e.Key == Key.Left) Previous();
        else if (e.Key == Key.Right) Next();
        else return;
        e.Handled = true;
    }

    // Round outline button over the slides; a tab stop that activates with Enter or Space.
    private static Border Arrow(IconName icon, HorizontalAlignment side, string name, Action press)
    {
        var shadow = new BoxShadows(new BoxShadow { OffsetY = 2, Blur = 6, Color = ShellTheme.Shadow(0.15) });
        var button = new Border
        {
            Child = new Icon { Kind = icon, Size = 16, Token = ShellToken.Foreground, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
            Width = 32,
            Height = 32,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            HorizontalAlignment = side,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(12, 0),
            BoxShadow = shadow,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        button.Token(BackgroundProperty, ShellToken.Background).Token(BorderBrushProperty, ShellToken.Border);
        AutomationProperties.SetName(button, name);
        ShellFocus.Ring(button, button);
        // The focus ring is painted as a shadow and cleared on blur: put the drop shadow back.
        button.LostFocus += (_, _) => button.BoxShadow = shadow;
        button.PointerPressed += (_, e) => e.Handled = true;
        button.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;
            press();
            e.Handled = true;
        };
        button.KeyDown += (_, e) =>
        {
            if (e.Key is not (Key.Enter or Key.Space)) return;
            press();
            e.Handled = true;
        };
        return button;
    }

    private void OnSlidesChanged()
    {
        _dots.Children.Clear();
        for (var i = 0; i < Count; i++)
        {
            var index = i;
            var dot = new Border
            {
                Width = 8,
                Height = 8,
                CornerRadius = new CornerRadius(4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            dot.Token(BackgroundProperty, ShellToken.Primary);
            // 8px dot in a 16px click target.
            var target = new Border { Width = 16, Height = 16, Background = Brushes.Transparent, Child = dot, Cursor = new Cursor(StandardCursorType.Hand) };
            AutomationProperties.SetName(target, $""Go to slide {i + 1}"");
            target.PointerPressed += (_, e) => e.Handled = true;
            target.PointerReleased += (_, e) =>
            {
                if (e.InitialPressMouseButton != MouseButton.Left) return;
                Position = index;
                e.Handled = true;
            };
            _dots.Children.Add(target);
        }
        _animation++;
        _offset = Current;
        Render();
        UpdateControls();
        UpdateTimer();
    }

    private void OnPositionChanged()
    {
        UpdateControls();
        AnimateTo(Current, 300);
        PositionChanged?.Invoke(this, Current);
        // A manual move restarts the auto-play countdown.
        UpdateTimer();
    }

    // Moves the track to a slide over the next frames; a newer move or a drag takes over.
    private void AnimateTo(double target, int milliseconds)
    {
        var run = ++_animation;
        var from = _offset;
        if (TopLevel.GetTopLevel(this) is not { } top || from == target)
        {
            _offset = target;
            Render();
            return;
        }

        TimeSpan? started = null;
        void Frame(TimeSpan now)
        {
            if (run != _animation) return;
            started ??= now;
            var progress = Math.Min(1, (now - started.Value).TotalMilliseconds / milliseconds);
            _offset = from + (target - from) * _easing.Ease(progress);
            Render();
            if (progress < 1) top.RequestAnimationFrame(Frame);
        }
        top.RequestAnimationFrame(Frame);
    }

    // Slides are stacked in one cell and spread out sideways by translation.
    private void Render()
    {
        var width = _track.Bounds.Width;
        if (width <= 0) return;
        for (var i = 0; i < Count; i++)
        {
            var x = (i - _offset) * width;
            if (_track.Children[i].RenderTransform is TranslateTransform shift) shift.X = x;
            else _track.Children[i].RenderTransform = new TranslateTransform(x, 0);
        }
    }

    private void UpdateControls()
    {
        var current = Current;
        var many = Count > 1;
        var canGoBack = Loop || current > 0;
        var canGoOn = Loop || current < Count - 1;
        _previous.IsVisible = ShowArrows && many;
        _next.IsVisible = ShowArrows && many;
        _previous.Opacity = !canGoBack ? 0.4 : _previous.IsPointerOver ? 1 : 0.9;
        _next.Opacity = !canGoOn ? 0.4 : _next.IsPointerOver ? 1 : 0.9;
        _previous.Focusable = canGoBack;
        _next.Focusable = canGoOn;
        _dots.IsVisible = ShowDots && many;
        for (var i = 0; i < _dots.Children.Count; i++)
        {
            // One color at two strengths, so the dots stay readable over any slide.
            if (_dots.Children[i] is Border { Child: { } dot })
                dot.Opacity = i == current ? 1 : 0.3;
        }
    }

    private void OnTrackPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Count < 2 || !e.GetCurrentPoint(_track).Properties.IsLeftButtonPressed) return;
        _pressedAt = e.GetPosition(_track);
        _dragging = false;
        _dragX = 0;
    }

    private void OnTrackMoved(object? sender, PointerEventArgs e)
    {
        var width = _track.Bounds.Width;
        if (_pressedAt is not { } start || width <= 0) return;
        _dragX = e.GetPosition(_track).X - start.X;
        if (!_dragging)
        {
            // A small move is still a click on the slide's own content.
            if (Math.Abs(_dragX) < DragThreshold) return;
            _dragging = true;
            _animation++;
            e.Pointer.Capture(_track);
        }
        // Follow the pointer, with a little give past the first and last slide.
        _offset = Math.Clamp(Current - _dragX / width, -0.25, Count - 0.75);
        Render();
        e.Handled = true;
    }

    private void OnTrackReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragging) e.Handled = true;
        EndDrag(cancelled: false);
        e.Pointer.Capture(null);
    }

    private void EndDrag(bool cancelled)
    {
        var width = _track.Bounds.Width;
        var wasDragging = _dragging;
        var moved = width > 0 ? _dragX / width : 0;
        _pressedAt = null;
        _dragging = false;
        _dragX = 0;
        if (!wasDragging) return;

        var current = Current;
        if (!cancelled && moved <= -0.2 && current < Count - 1) Position = current + 1;
        else if (!cancelled && moved >= 0.2 && current > 0) Position = current - 1;
        // Not far enough: settle back.
        else AnimateTo(current, 200);
    }

    private void UpdateTimer()
    {
        StopTimer();
        if (!AutoPlay || Count < 2 || TopLevel.GetTopLevel(this) is null) return;
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(Math.Max(500, AutoPlayInterval)), DispatcherPriority.Normal,
            (_, _) => Position = Current < Count - 1 ? Current + 1 : 0);
        _timer.Start();
    }

    private void StopTimer()
    {
        _timer?.Stop();
        _timer = null;
    }
}
"
    };
}
