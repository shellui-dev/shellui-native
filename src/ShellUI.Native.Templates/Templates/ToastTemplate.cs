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

// Sonner-style toasts. Put one Toaster where it can fill the page (e.g. last child of the page's
// root Grid), then call from anywhere:
//   Toast.Show(""Event created"", ""Sunday, December 03 at 9:00 AM"");
//   Toast.Success(""Profile saved"");
//   Toast.Error(""Upload failed"", actionText: ""Retry"", action: Retry);
// Toasts stack in a corner, slide in, pause while hovered and dismiss themselves.
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
        if (_toaster != null && _toaster.TryGetTarget(out var toaster))
            toaster.Add(new ToastItem(id, message, description, variant, duration ?? TimeSpan.FromSeconds(4), actionText, action));
        else
            System.Diagnostics.Debug.WriteLine(""ShellUI: Toast.Show called but no <ui:Toaster /> is on the page."");
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

// Host for toasts. Input passes through everywhere except the toasts themselves.
public partial class Toaster : Grid
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
        InputTransparent = true;
        CascadeInputTransparent = false;
        ZIndex = 2000;
        _stack = new VerticalStackLayout { Spacing = 8, Margin = new Thickness(16) };
        Children.Add(_stack);
        SizeChanged += (_, _) => _stack.WidthRequest = Math.Max(0, Math.Min(356, Width - 32));
        UpdatePosition();
        Loaded += (_, _) => Toast.Register(this);
        Unloaded += (_, _) => Toast.Unregister(this);
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
"
    };
}
