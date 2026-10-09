using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class ToastTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "toast",
        DisplayName = "Toast",
        Description = "Sonner-style stacked notifications (Toaster + Toast API)",
        Category = ComponentCategory.Feedback,
        FilePath = "Toast.cs",
        Dependencies = new List<string> { "shell", "icon", "button" },
        Tags = new List<string> { "toast", "sonner", "notification", "snackbar" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;
using YourProjectNamespace.Components.UI.Variants;

namespace YourProjectNamespace.Components.UI;

// Sonner-style toasts. Call from anywhere — toasts float in the page layer:
//   Toast.Show(""Event created"", ""Sunday, December 03 at 9:00 AM"");
//   Toast.Success(""Profile saved"");
//   Toast.Error(""Upload failed"", actionText: ""Retry"", action: Retry);
// Toasts stack in a corner, slide in, pause while hovered and dismiss themselves.
// Optional: declare <ui:Toaster Position=""TopRight"" MaxVisible=""5"" /> anywhere on a page to
// configure them; without one, a default Toaster (bottom-right, 3 visible) is created on demand.
public static class Toast
{
    private static WeakReference<Toaster>? _toaster;

    internal static void Register(Toaster toaster) => _toaster = new WeakReference<Toaster>(toaster);

    internal static void Unregister(Toaster toaster)
    {
        if (_toaster != null && _toaster.TryGetTarget(out var current) && current == toaster)
            _toaster = null;
    }

    public static string Show(string message, string? description = null, ToastVariant variant = ToastVariant.Default,
        TimeSpan? duration = null, string? actionText = null, Action? action = null)
    {
        var id = Guid.NewGuid().ToString(""N"");
        if (_toaster == null || !_toaster.TryGetTarget(out var toaster) || !toaster.IsAttached)
        {
            toaster = new Toaster();
            if (!toaster.Attach())
            {
                System.Diagnostics.Debug.WriteLine(""ShellUI: Toast.Show called before any page is on screen."");
                return id;
            }
            Register(toaster);
        }
        toaster.Add(new ToastItem(id, message, description, variant, duration ?? TimeSpan.FromSeconds(4), actionText, action));
        return id;
    }

    public static string Success(string message, string? description = null, TimeSpan? duration = null,
        string? actionText = null, Action? action = null)
        => Show(message, description, ToastVariant.Success, duration, actionText, action);

    public static string Error(string message, string? description = null, TimeSpan? duration = null,
        string? actionText = null, Action? action = null)
        => Show(message, description, ToastVariant.Error, duration, actionText, action);

    public static string Warning(string message, string? description = null, TimeSpan? duration = null,
        string? actionText = null, Action? action = null)
        => Show(message, description, ToastVariant.Warning, duration, actionText, action);

    public static string Info(string message, string? description = null, TimeSpan? duration = null,
        string? actionText = null, Action? action = null)
        => Show(message, description, ToastVariant.Info, duration, actionText, action);

    public static void Dismiss(string id)
    {
        if (_toaster != null && _toaster.TryGetTarget(out var toaster))
            toaster.Remove(id);
    }
}

public enum ToastVariant { Default, Success, Error, Warning, Info }

public enum ToasterPosition { BottomRight, BottomCenter, TopRight, TopCenter }

internal sealed record ToastItem(string Id, string Message, string? Description, ToastVariant Variant,
    TimeSpan Duration, string? ActionText, Action? Action);

// Configures where toasts appear. Its stack lives in the page layer, so the Toaster itself takes
// no space and can be declared anywhere on the page.
public partial class Toaster : ContentView
{
    public static readonly BindableProperty PositionProperty =
        BindableProperty.Create(nameof(Position), typeof(ToasterPosition), typeof(Toaster), ToasterPosition.BottomRight,
            propertyChanged: (b, o, n) => ((Toaster)b).UpdatePosition());

    public static readonly BindableProperty MaxVisibleProperty =
        BindableProperty.Create(nameof(MaxVisible), typeof(int), typeof(Toaster), 3);

    public ToasterPosition Position
    {
        get => (ToasterPosition)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public int MaxVisible
    {
        get => (int)GetValue(MaxVisibleProperty);
        set => SetValue(MaxVisibleProperty, value);
    }

    private readonly VerticalStackLayout _stack;

    public Toaster()
    {
        _stack = new VerticalStackLayout { Spacing = 8, Margin = new Thickness(16), ZIndex = 2000 };
        UpdatePosition();
        Loaded += (_, _) => { if (Attach()) Toast.Register(this); };
        Unloaded += (_, _) =>
        {
            ShellPortal.Detach(_stack);
            Toast.Unregister(this);
        };
    }

    internal bool IsAttached => _stack.Parent != null;

    // Places the toast stack in the page layer (of this Toaster's page, or the page on screen).
    internal bool Attach()
    {
        // The stack keeps itself clear of the system bars with its margin (see Fit).
        ShellPortal.EdgeToEdge(_stack);
        if (!ShellPortal.Attach(this, _stack)) return false;
        if (_stack.Parent is Grid layer)
        {
            void Fit()
            {
                _stack.WidthRequest = Math.Max(0, Math.Min(356, layer.Width - 32));
                var safe = ShellPortal.GetSafeInsets(layer);
                _stack.Margin = new Thickness(16 + safe.Left, 16 + safe.Top, 16 + safe.Right, 16 + safe.Bottom);
            }
            layer.SizeChanged += (_, _) => Fit();
            Fit();
        }
        return true;
    }

    private bool IsTop => Position is ToasterPosition.TopRight or ToasterPosition.TopCenter;

    private void UpdatePosition()
    {
        _stack.VerticalOptions = IsTop ? LayoutOptions.Start : LayoutOptions.End;
        _stack.HorizontalOptions = Position is ToasterPosition.BottomCenter or ToasterPosition.TopCenter
            ? LayoutOptions.Center
            : LayoutOptions.End;
    }

    internal void Add(ToastItem item)
    {
        var view = new ToastView(item, this);
        // Newest toast sits nearest the edge it enters from.
        if (IsTop) _stack.Children.Insert(0, view);
        else _stack.Children.Add(view);

        var views = _stack.Children.OfType<ToastView>().ToList();
        var overflow = views.Count - Math.Max(1, MaxVisible);
        foreach (var old in (IsTop ? views.AsEnumerable().Reverse() : views).Take(Math.Max(0, overflow)))
            _ = old.DismissAsync();

        _ = view.EnterAsync(IsTop);
    }

    internal void Remove(string id)
    {
        var view = _stack.Children.OfType<ToastView>().FirstOrDefault(v => v.ToastId == id);
        if (view != null) _ = view.DismissAsync();
    }

    internal void Detach(ToastView view) => _stack.Children.Remove(view);
}

// One toast — rounded-lg border bg-popover p-4 shadow-lg, variant icon, optional action, close X.
internal sealed class ToastView : ContentView
{
    public string ToastId { get; }

    private readonly ToastItem _item;
    private readonly Toaster _host;
    private readonly Border _card;
    private readonly Border _close;
    private bool _dismissing;
    private bool _hovered;
    private DateTime _deadline;

    public ToastView(ToastItem item, Toaster host)
    {
        ToastId = item.Id;
        _item = item;
        _host = host;

        var (icon, token) = item.Variant switch
        {
            ToastVariant.Success => (IconName.CircleCheck, ShellToken.Success),
            ToastVariant.Error => (IconName.CircleX, ShellToken.Destructive),
            ToastVariant.Warning => (IconName.TriangleAlert, ShellToken.Warning),
            ToastVariant.Info => (IconName.Info, ShellToken.Info),
            _ => (IconName.None, ShellToken.Foreground)
        };

        var title = new Label { Text = item.Message, FontSize = 14, FontAttributes = FontAttributes.Bold };
        title.Token(Label.TextColorProperty, ShellToken.PopoverForeground);
        var text = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, Children = { title } };
        if (!string.IsNullOrEmpty(item.Description))
        {
            var description = new Label { Text = item.Description, FontSize = 13 };
            description.Token(Label.TextColorProperty, ShellToken.MutedForeground);
            text.Children.Add(description);
        }

        var row = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };
        if (icon != IconName.None)
            row.Add(new Icon { Name = icon, Size = 18, Token = token, VerticalOptions = LayoutOptions.Center }, 0, 0);
        row.Add(text, 1, 0);

        if (!string.IsNullOrEmpty(item.ActionText))
        {
            var action = new Button { Text = item.ActionText, Size = ButtonSize.Sm, VerticalOptions = LayoutOptions.Center };
            action.Clicked += (_, _) => { item.Action?.Invoke(); _ = DismissAsync(); };
            row.Add(action, 2, 0);
        }

        _close = new Border
        {
            Content = new Icon { Name = IconName.X, Size = 14, Token = ShellToken.MutedForeground },
            WidthRequest = 22,
            HeightRequest = 22,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 11 },
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(-8, -8, 0, 0),
            Opacity = 0
        };
        _close.Token(VisualElement.BackgroundColorProperty, ShellToken.Popover);
        _close.Token(Border.StrokeProperty, ShellToken.Border);
        SemanticProperties.SetDescription(_close, ""Close"");
        var closeTap = new TapGestureRecognizer();
        closeTap.Tapped += (_, _) => _ = DismissAsync();
        _close.GestureRecognizers.Add(closeTap);

        _card = new Border
        {
            Content = row,
            Padding = new Thickness(16),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusLg },
            Shadow = new Shadow { Brush = new SolidColorBrush(Colors.Black), Offset = new Point(0, 4), Radius = 16, Opacity = 0.12f }
        };
        _card.Token(VisualElement.BackgroundColorProperty, ShellToken.Popover);
        _card.Token(Border.StrokeProperty, ShellToken.Border);

        // Hovering pauses the timer and reveals the close button (Sonner behavior).
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { _hovered = true; _ = _close.FadeToAsync(1, 120); };
        pointer.PointerExited += (_, _) =>
        {
            _hovered = false;
            _deadline = DateTime.UtcNow + TimeSpan.FromSeconds(1.5);
            _ = _close.FadeToAsync(0, 120);
        };

        var root = new Grid { Children = { _card, _close } };
        root.GestureRecognizers.Add(pointer);
        Content = root;
        Opacity = 0;
    }

    public async Task EnterAsync(bool fromTop)
    {
        TranslationY = fromTop ? -16 : 16;
        await Task.WhenAll(this.FadeToAsync(1, 200, Easing.CubicOut), this.TranslateToAsync(0, 0, 250, Easing.CubicOut));

        _deadline = DateTime.UtcNow + _item.Duration;
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(200), () =>
        {
            if (_dismissing) return false;
            if (_hovered) return true;
            if (DateTime.UtcNow < _deadline) return true;
            _ = DismissAsync();
            return false;
        });
    }

    public async Task DismissAsync()
    {
        if (_dismissing) return;
        _dismissing = true;
        await Task.WhenAll(this.FadeToAsync(0, 150, Easing.CubicIn), this.TranslateToAsync(24, 0, 150, Easing.CubicIn));
        _host.Detach(this);
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using YourProjectNamespace.Components.UI.Variants;

namespace YourProjectNamespace.Components.UI;

// Sonner-style toasts. Call from anywhere — toasts float above the window's content:
//   Toast.Show(""Event created"", ""Sunday, December 03 at 9:00 AM"");
//   Toast.Success(""Profile saved"");
//   Toast.Error(""Upload failed"", actionText: ""Retry"", action: Retry);
// Toasts stack in a corner, slide in, pause while hovered and dismiss themselves.
// Optional: declare <ui:Toaster Position=""TopRight"" MaxVisible=""5"" /> anywhere in a window to
// configure them; without one, a default Toaster (bottom-right, 3 visible) is created on demand
// in the active window.
public static class Toast
{
    private static WeakReference<Toaster>? _toaster;

    internal static void Register(Toaster toaster) => _toaster = new WeakReference<Toaster>(toaster);

    internal static void Unregister(Toaster toaster)
    {
        if (_toaster != null && _toaster.TryGetTarget(out var current) && current == toaster)
            _toaster = null;
    }

    public static string Show(string message, string? description = null, ToastVariant variant = ToastVariant.Default,
        TimeSpan? duration = null, string? actionText = null, Action? action = null)
    {
        var id = Guid.NewGuid().ToString(""N"");
        if (_toaster == null || !_toaster.TryGetTarget(out var toaster) || !toaster.IsAttached)
        {
            toaster = new Toaster();
            if (ActiveTopLevel() is not { } top || !toaster.Attach(top))
            {
                System.Diagnostics.Debug.WriteLine(""ShellUI: Toast.Show called before any window is on screen."");
                return id;
            }
            Register(toaster);
        }
        toaster.Add(new ToastItem(id, message, description, variant, duration ?? TimeSpan.FromSeconds(4), actionText, action));
        return id;
    }

    public static string Success(string message, string? description = null, TimeSpan? duration = null,
        string? actionText = null, Action? action = null)
        => Show(message, description, ToastVariant.Success, duration, actionText, action);

    public static string Error(string message, string? description = null, TimeSpan? duration = null,
        string? actionText = null, Action? action = null)
        => Show(message, description, ToastVariant.Error, duration, actionText, action);

    public static string Warning(string message, string? description = null, TimeSpan? duration = null,
        string? actionText = null, Action? action = null)
        => Show(message, description, ToastVariant.Warning, duration, actionText, action);

    public static string Info(string message, string? description = null, TimeSpan? duration = null,
        string? actionText = null, Action? action = null)
        => Show(message, description, ToastVariant.Info, duration, actionText, action);

    public static void Dismiss(string id)
    {
        if (_toaster != null && _toaster.TryGetTarget(out var toaster))
            toaster.Remove(id);
    }

    private static TopLevel? ActiveTopLevel() => Application.Current?.ApplicationLifetime switch
    {
        IClassicDesktopStyleApplicationLifetime desktop => desktop.Windows.FirstOrDefault(w => w.IsActive) ?? desktop.MainWindow,
        ISingleViewApplicationLifetime single when single.MainView is { } view => TopLevel.GetTopLevel(view),
        _ => null
    };
}

public enum ToastVariant { Default, Success, Error, Warning, Info }

public enum ToasterPosition { BottomRight, BottomCenter, TopRight, TopCenter }

internal sealed record ToastItem(string Id, string Message, string? Description, ToastVariant Variant,
    TimeSpan Duration, string? ActionText, Action? Action);

// Configures where toasts appear. Its stack sits in the window's overlay layer, so the Toaster
// itself takes no space and can be declared anywhere in the window.
public class Toaster : Control
{
    public static readonly StyledProperty<ToasterPosition> PositionProperty =
        AvaloniaProperty.Register<Toaster, ToasterPosition>(nameof(Position));

    public static readonly StyledProperty<int> MaxVisibleProperty =
        AvaloniaProperty.Register<Toaster, int>(nameof(MaxVisible), 3);

    // Fills the overlay layer; only the toasts take input.
    private readonly Panel _layer = new() { ZIndex = 1000 };
    private readonly StackPanel _stack = new() { Spacing = 8, Margin = new Thickness(16) };
    private OverlayLayer? _overlay;

    static Toaster()
    {
        PositionProperty.Changed.AddClassHandler<Toaster>((t, _) => t.UpdatePosition());
    }

    public ToasterPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public int MaxVisible
    {
        get => GetValue(MaxVisibleProperty);
        set => SetValue(MaxVisibleProperty, value);
    }

    public Toaster()
    {
        _layer.Children.Add(_stack);
        IsHitTestVisible = false;
        UpdatePosition();
    }

    internal bool IsAttached => _overlay != null;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TopLevel.GetTopLevel(this) is { } top && Attach(top)) Toast.Register(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Detach();
        Toast.Unregister(this);
    }

    // Puts the toast stack in the overlay layer of `top`, sized to it.
    internal bool Attach(TopLevel top)
    {
        if (_overlay != null) return true;
        if (OverlayLayer.GetOverlayLayer(top) is not { } overlay) return false;
        _overlay = overlay;
        overlay.Children.Add(_layer);
        overlay.SizeChanged += OnOverlaySizeChanged;
        Fit(overlay.Bounds.Size);
        return true;
    }

    private void Detach()
    {
        if (_overlay == null) return;
        _overlay.SizeChanged -= OnOverlaySizeChanged;
        _overlay.Children.Remove(_layer);
        _overlay = null;
    }

    private void OnOverlaySizeChanged(object? sender, SizeChangedEventArgs e) => Fit(e.NewSize);

    private void Fit(Size size)
    {
        _layer.Width = size.Width;
        _layer.Height = size.Height;
        _stack.Width = Math.Max(0, Math.Min(356, size.Width - 32));
    }

    private bool IsTop => Position is ToasterPosition.TopRight or ToasterPosition.TopCenter;

    private void UpdatePosition()
    {
        _stack.VerticalAlignment = IsTop ? VerticalAlignment.Top : VerticalAlignment.Bottom;
        _stack.HorizontalAlignment = Position is ToasterPosition.BottomCenter or ToasterPosition.TopCenter
            ? HorizontalAlignment.Center
            : HorizontalAlignment.Right;
    }

    internal void Add(ToastItem item)
    {
        var view = new ToastView(item, this);
        // Newest toast sits nearest the edge it enters from.
        if (IsTop) _stack.Children.Insert(0, view);
        else _stack.Children.Add(view);

        var views = _stack.Children.OfType<ToastView>().Where(v => !v.IsDismissing).ToList();
        var overflow = views.Count - Math.Max(1, MaxVisible);
        foreach (var old in (IsTop ? views.AsEnumerable().Reverse() : views).Take(Math.Max(0, overflow)))
            _ = old.DismissAsync();

        _ = view.EnterAsync(IsTop);
    }

    internal void Remove(string id)
    {
        var view = _stack.Children.OfType<ToastView>().FirstOrDefault(v => v.ToastId == id);
        if (view != null) _ = view.DismissAsync();
    }

    internal void Detach(ToastView view) => _stack.Children.Remove(view);
}

// One toast — rounded-lg border bg-popover p-4 shadow-lg, variant icon, optional action, close X
// shown on hover.
internal sealed class ToastView : Panel
{
    public string ToastId { get; }
    public bool IsDismissing { get; private set; }

    private readonly ToastItem _item;
    private readonly Toaster _host;
    private readonly Border _close;
    private DispatcherTimer? _timer;
    private DateTime _deadline;

    public ToastView(ToastItem item, Toaster host)
    {
        ToastId = item.Id;
        _item = item;
        _host = host;

        var (icon, token) = item.Variant switch
        {
            ToastVariant.Success => (IconName.CircleCheck, ShellToken.Success),
            ToastVariant.Error => (IconName.CircleX, ShellToken.Destructive),
            ToastVariant.Warning => (IconName.TriangleAlert, ShellToken.Warning),
            ToastVariant.Info => (IconName.Info, ShellToken.Info),
            _ => (IconName.None, ShellToken.Foreground)
        };

        var title = new TextBlock { Text = item.Message, FontSize = 14, FontWeight = FontWeight.SemiBold, TextWrapping = TextWrapping.Wrap };
        title.Token(TextBlock.ForegroundProperty, ShellToken.PopoverForeground);
        var text = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center, Children = { title } };
        if (!string.IsNullOrEmpty(item.Description))
        {
            var description = new TextBlock { Text = item.Description, FontSize = 13, TextWrapping = TextWrapping.Wrap };
            description.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
            text.Children.Add(description);
        }

        var row = new Grid { ColumnDefinitions = new ColumnDefinitions(""Auto,*,Auto""), ColumnSpacing = 12 };
        if (icon != IconName.None)
            row.Children.Add(new Icon { Kind = icon, Size = 18, Token = token, VerticalAlignment = VerticalAlignment.Center });
        Grid.SetColumn(text, 1);
        row.Children.Add(text);

        if (!string.IsNullOrEmpty(item.ActionText))
        {
            var action = new Button { Text = item.ActionText, Size = ButtonSize.Sm, VerticalAlignment = VerticalAlignment.Center };
            action.Clicked += (_, _) => { item.Action?.Invoke(); _ = DismissAsync(); };
            Grid.SetColumn(action, 2);
            row.Children.Add(action);
        }

        _close = new Border
        {
            Child = new Icon { Kind = IconName.X, Size = 14, Token = ShellToken.MutedForeground },
            Width = 22,
            Height = 22,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(11),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(-8, -8, 0, 0),
            Opacity = 0,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        _close.Token(Border.BackgroundProperty, ShellToken.Popover).Token(Border.BorderBrushProperty, ShellToken.Border);
        AutomationProperties.SetName(_close, ""Close"");
        _close.PointerReleased += (_, e) => { _ = DismissAsync(); e.Handled = true; };

        var card = new Border
        {
            Child = row,
            Padding = new Thickness(16),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusLg),
            BoxShadow = new BoxShadows(new BoxShadow { OffsetY = 4, Blur = 16, Color = ShellTheme.Shadow(0.12) })
        };
        card.Token(Border.BackgroundProperty, ShellToken.Popover).Token(Border.BorderBrushProperty, ShellToken.Border);

        Children.Add(card);
        Children.Add(_close);
        Opacity = 0;
    }

    // Hovering pauses the timer and reveals the close button (Sonner behavior).
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _ = ShellMotion.To(_close, 1, ""scale(1)"", 120, new CubicEaseOut());
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _deadline = DateTime.UtcNow + TimeSpan.FromSeconds(1.5);
        _ = ShellMotion.To(_close, 0, ""scale(1)"", 120, new CubicEaseIn());
    }

    public async Task EnterAsync(bool fromTop)
    {
        ShellMotion.Set(this, 0, fromTop ? ""translateY(-16px)"" : ""translateY(16px)"");
        await ShellMotion.To(this, 1, ""translateY(0px)"", 250, new CubicEaseOut());

        _deadline = DateTime.UtcNow + _item.Duration;
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Background, (_, _) =>
        {
            if (IsPointerOver || DateTime.UtcNow < _deadline) return;
            _ = DismissAsync();
        });
        _timer.Start();
    }

    public async Task DismissAsync()
    {
        if (IsDismissing) return;
        IsDismissing = true;
        _timer?.Stop();
        await ShellMotion.To(this, 0, ""translateX(24px)"", 150, new CubicEaseIn());
        _host.Detach(this);
    }
}
"
    };
}
