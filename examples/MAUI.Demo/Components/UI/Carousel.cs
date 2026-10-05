using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Carousel — one slide at a time in a rounded-lg frame; swipe, use the round arrow buttons or
// tap a dot to move. Each child view is a slide.
//   <ui:Carousel AspectRatio="2" Loop="True">
//       <Image Source="one.jpg" Aspect="AspectFill" />
//       <Image Source="two.jpg" Aspect="AspectFill" />
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

    private const string SlideAnimation = "CarouselSlide";

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

        _previous = Arrow(IconName.ChevronLeft, LayoutOptions.Start, "Previous slide", Previous);
        _next = Arrow(IconName.ChevronRight, LayoutOptions.End, "Next slide", Next);
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
