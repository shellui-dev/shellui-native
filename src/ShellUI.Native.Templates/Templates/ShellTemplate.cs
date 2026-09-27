using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Shell utility class template - provides helper methods for components
public static class ShellTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "shell",
        DisplayName = "Shell Utilities",
        Description = "Core utilities and theme tokens (light/dark palettes) for ShellUI Native components",
        Category = ComponentCategory.Utility,
        FilePath = "Shell.cs",
        IsAvailable = false, // Auto-installed during init, not shown in list
        Tags = new List<string> { "utility", "core", "helper" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// ShellUI Native core utilities. Installed by `shellui-native init`; every component depends on it.
public static class Shell
{
    // Combines multiple class names, filtering out null/empty values
    public static string Cn(params string?[] classes)
        => string.Join("" "", classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}

// Design tokens mirroring ShellUI's CSS variables (--background, --primary, ...).
public enum ShellToken
{
    Background, Foreground,
    Card, CardForeground,
    Popover, PopoverForeground,
    Primary, PrimaryForeground,
    Secondary, SecondaryForeground,
    Muted, MutedForeground,
    Accent, AccentForeground,
    Destructive, DestructiveForeground,
    Success, SuccessForeground,
    Warning, WarningForeground,
    Info, InfoForeground,
    Border, Input, Ring,
    Overlay
}

// Publishes the active palette as application resources (""ShellUIPrimary"" for the Color,
// ""ShellUIPrimaryBrush"" for the Brush). Components bind to them with SetDynamicResource, so
// switching theme or overriding a token repaints everything — the MAUI equivalent of swapping
// CSS variables. Your own XAML can use them too: TextColor=""{DynamicResource ShellUIMutedForeground}"".
//
// Customize before the first page loads (e.g. in App's constructor):
//   ShellTheme.Light[ShellToken.Primary] = Color.FromArgb(""#2563EB"");
//   ShellTheme.Apply();
public static class ShellTheme
{
    // Based on ShellUI's default neutral theme, softened from pure black/white for native text
    // rendering. Success/Warning/Info extend it (ShellUI has no semantic status colors).
    public static Dictionary<ShellToken, Color> Light { get; } = new()
    {
        [ShellToken.Background] = Color.FromArgb(""#FCFCFC""),
        [ShellToken.Foreground] = Color.FromArgb(""#0A0A0A""),
        [ShellToken.Card] = Color.FromArgb(""#FFFFFF""),
        [ShellToken.CardForeground] = Color.FromArgb(""#0A0A0A""),
        [ShellToken.Popover] = Color.FromArgb(""#FFFFFF""),
        [ShellToken.PopoverForeground] = Color.FromArgb(""#0A0A0A""),
        [ShellToken.Primary] = Color.FromArgb(""#171717""),
        [ShellToken.PrimaryForeground] = Color.FromArgb(""#FAFAFA""),
        [ShellToken.Secondary] = Color.FromArgb(""#F0F0F0""),
        [ShellToken.SecondaryForeground] = Color.FromArgb(""#171717""),
        [ShellToken.Muted] = Color.FromArgb(""#F5F5F5""),
        [ShellToken.MutedForeground] = Color.FromArgb(""#737373""),
        [ShellToken.Accent] = Color.FromArgb(""#F0F0F0""),
        [ShellToken.AccentForeground] = Color.FromArgb(""#171717""),
        [ShellToken.Destructive] = Color.FromArgb(""#E54B4F""),
        [ShellToken.DestructiveForeground] = Color.FromArgb(""#FFFFFF""),
        [ShellToken.Success] = Color.FromArgb(""#16A34A""),
        [ShellToken.SuccessForeground] = Color.FromArgb(""#FFFFFF""),
        [ShellToken.Warning] = Color.FromArgb(""#D97706""),
        [ShellToken.WarningForeground] = Color.FromArgb(""#FFFFFF""),
        [ShellToken.Info] = Color.FromArgb(""#2563EB""),
        [ShellToken.InfoForeground] = Color.FromArgb(""#FFFFFF""),
        [ShellToken.Border] = Color.FromArgb(""#E4E4E4""),
        [ShellToken.Input] = Color.FromArgb(""#E4E4E4""),
        [ShellToken.Ring] = Color.FromArgb(""#A3A3A3""),
        [ShellToken.Overlay] = Color.FromArgb(""#80000000""),
    };

    public static Dictionary<ShellToken, Color> Dark { get; } = new()
    {
        [ShellToken.Background] = Color.FromArgb(""#0A0A0A""),
        [ShellToken.Foreground] = Color.FromArgb(""#FAFAFA""),
        [ShellToken.Card] = Color.FromArgb(""#141414""),
        [ShellToken.CardForeground] = Color.FromArgb(""#FAFAFA""),
        [ShellToken.Popover] = Color.FromArgb(""#171717""),
        [ShellToken.PopoverForeground] = Color.FromArgb(""#FAFAFA""),
        [ShellToken.Primary] = Color.FromArgb(""#FAFAFA""),
        [ShellToken.PrimaryForeground] = Color.FromArgb(""#171717""),
        [ShellToken.Secondary] = Color.FromArgb(""#262626""),
        [ShellToken.SecondaryForeground] = Color.FromArgb(""#FAFAFA""),
        [ShellToken.Muted] = Color.FromArgb(""#262626""),
        [ShellToken.MutedForeground] = Color.FromArgb(""#A3A3A3""),
        [ShellToken.Accent] = Color.FromArgb(""#262626""),
        [ShellToken.AccentForeground] = Color.FromArgb(""#FAFAFA""),
        [ShellToken.Destructive] = Color.FromArgb(""#FF5B5B""),
        [ShellToken.DestructiveForeground] = Color.FromArgb(""#0A0A0A""),
        [ShellToken.Success] = Color.FromArgb(""#22C55E""),
        [ShellToken.SuccessForeground] = Color.FromArgb(""#0A0A0A""),
        [ShellToken.Warning] = Color.FromArgb(""#F59E0B""),
        [ShellToken.WarningForeground] = Color.FromArgb(""#0A0A0A""),
        [ShellToken.Info] = Color.FromArgb(""#3B82F6""),
        [ShellToken.InfoForeground] = Color.FromArgb(""#FFFFFF""),
        [ShellToken.Border] = Color.FromArgb(""#2A2A2A""),
        [ShellToken.Input] = Color.FromArgb(""#333333""),
        [ShellToken.Ring] = Color.FromArgb(""#737373""),
        [ShellToken.Overlay] = Color.FromArgb(""#B3000000""),
    };

    // --radius: 0.5rem. Tailwind's rounded-sm / rounded-md / rounded-lg derive from it.
    public const float RadiusSm = 4;
    public const float RadiusMd = 6;
    public const float RadiusLg = 8;

    private static bool _initialized;

    // Raised after a palette is published. Resources cover colors and brushes; use this for
    // anything they can't reach (shadows, native-control tweaks).
    public static event EventHandler? ThemeChanged;

    public static bool IsDarkMode => Application.Current?.RequestedTheme == AppTheme.Dark;

    public static string ColorKey(ShellToken token) => $""ShellUI{token}"";
    public static string BrushKey(ShellToken token) => $""ShellUI{token}Brush"";

    // Current value of a token — for code that can't bind (e.g. building a Shadow).
    public static Color Get(ShellToken token) => (IsDarkMode ? Dark : Light)[token];

    public static void EnsureInitialized()
    {
        if (_initialized) return;
        var app = Application.Current;
        if (app is null) return;
        _initialized = true;
        app.RequestedThemeChanged += (_, _) => Apply();
        Apply();
    }

    // Re-publishes the active palette. Call after editing Light/Dark at runtime.
    public static void Apply()
    {
        var app = Application.Current;
        if (app is null) return;
        foreach (var (token, color) in IsDarkMode ? Dark : Light)
        {
            app.Resources[ColorKey(token)] = color;
            app.Resources[BrushKey(token)] = new SolidColorBrush(color);
        }
        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void SetTheme(AppTheme theme)
    {
        if (Application.Current is not { } app) return;
        EnsureInitialized();
        app.UserAppTheme = theme;
        Apply();
    }

    public static void ToggleTheme() => SetTheme(IsDarkMode ? AppTheme.Light : AppTheme.Dark);
}

public static class ShellThemeExtensions
{
    // Binds a Color or Brush property to a theme token. Returns the element for chaining.
    // Clears any local value first: a value set in code outranks a dynamic resource.
    public static T Token<T>(this T element, BindableProperty property, ShellToken token) where T : Element
    {
        ShellTheme.EnsureInitialized();
        var key = property.ReturnType == typeof(Brush) ? ShellTheme.BrushKey(token) : ShellTheme.ColorKey(token);
        element.ClearValue(property);
        element.SetDynamicResource(property, key);
        return element;
    }
}

// Native-control helpers. Form components wrap platform controls (Entry, Editor, DatePicker, ...)
// in a ShellUI Border; the platform control must not draw a second frame inside it.
public static class ShellPlatform
{
    public static void StripNativeChrome(View view, bool keepPadding = false)
    {
        view.HandlerChanged += (_, _) => Strip(view, keepPadding);
        Strip(view, keepPadding);
    }

    private static void Strip(View view, bool keepPadding)
    {
#if WINDOWS
        if (view.Handler?.PlatformView is not Microsoft.UI.Xaml.Controls.Control control) return;
        // WinUI templates paint borders/backgrounds from theme resources per visual state
        // (focused, hover, ...), so local Background/BorderBrush alone is not enough. The
        // control's own dictionary can be shared/read-only, so it gets a fresh one. Cosmetic
        // only: never let a styling failure take the app down.
        try
        {
            // WinUI rejects one brush instance under several keys, so each entry gets its own.
            static Microsoft.UI.Xaml.Media.SolidColorBrush Transparent() => new(Microsoft.UI.Colors.Transparent);
            var zero = new Microsoft.UI.Xaml.Thickness(0);
            var resources = new Microsoft.UI.Xaml.ResourceDictionary();
            foreach (var prefix in new[] { ""TextControl"", ""CalendarDatePicker"", ""TimePickerButton"", ""ComboBox"" })
            {
                foreach (var state in new[] { """", ""PointerOver"", ""Pressed"", ""Focused"", ""Disabled"" })
                {
                    resources[$""{prefix}Background{state}""] = Transparent();
                    resources[$""{prefix}BorderBrush{state}""] = Transparent();
                }
            }
            resources[""TextControlBorderThemeThickness""] = zero;
            resources[""TextControlBorderThemeThicknessFocused""] = zero;
            resources[""CalendarDatePickerBorderThemeThickness""] = zero;
            resources[""TimePickerBorderThemeThickness""] = zero;
            resources[""TimePickerSpacerFill""] = Transparent();
            resources[""ComboBoxBorderThemeThickness""] = zero;
            control.Resources = resources;
            control.Background = Transparent();
            control.BorderBrush = Transparent();
            control.BorderThickness = zero;
            control.MinHeight = 0;
            if (!keepPadding) control.Padding = zero;
            control.UseSystemFocusVisuals = false;
            control.FocusVisualPrimaryThickness = zero;
            control.FocusVisualSecondaryThickness = zero;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($""ShellUI: could not strip native chrome: {ex.Message}"");
        }
#endif
    }
}

// Web-like focus for drawn controls. ShellUI buttons, checkboxes, tabs, ... are drawn views,
// not native controls, so by default they never take focus: the caret stays in the last text
// input, keys keep typing there, and WinUI scrolls back to that input on layout changes.
// MakeFocusable turns a control into a keyboard tab stop (Enter/Space activate it) and
// FocusPressed moves focus to the control the user pressed — which is already on screen.
public static class ShellFocus
{
    private static WeakReference<VisualElement>? _active;

    // Text inputs register so non-Windows platforms can blur them on press.
    public static void Track(VisualElement input)
    {
        input.Focused += (_, _) => _active = new WeakReference<VisualElement>(input);
        input.Unfocused += (_, _) =>
        {
            if (_active != null && _active.TryGetTarget(out var current) && current == input)
                _active = null;
        };
    }

    // `when` is checked once the native view exists (e.g. skip if the content is already focusable).
    public static void MakeFocusable(View view, Action activate, Func<bool>? when = null)
    {
#if WINDOWS
        view.HandlerChanged += (_, _) =>
        {
            if (view.Handler?.PlatformView is not Microsoft.UI.Xaml.UIElement element || element.IsTabStop) return;
            if (when != null && !when()) return;
            element.IsTabStop = true;
            element.UseSystemFocusVisuals = true;
            element.KeyDown += (_, e) =>
            {
                if (e.Key is Windows.System.VirtualKey.Enter or Windows.System.VirtualKey.Space)
                {
                    e.Handled = true;
                    activate();
                }
            };
        };
#endif
    }

    public static void FocusPressed(View pressed)
    {
#if WINDOWS
        // Unfocus() on Windows hands focus to the next text input and scrolls to it, so only
        // move focus onto a control that can take it.
        if (pressed.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement { IsTabStop: true } element)
            element.Focus(Microsoft.UI.Xaml.FocusState.Pointer);
#else
        BlurActiveInput();
#endif
    }

    public static void BlurActiveInput()
    {
        if (_active != null && _active.TryGetTarget(out var input) && input.IsFocused)
            input.Unfocus();
        _active = null;
    }
}

// Lays out an anchor (child 0) and floats every other child just below it without adding to
// the measured size — dropdown / popover / select panels overlap the content that follows
// instead of pushing it down. Raise ZIndex while open so the panel draws above later siblings.
public class ShellAnchorLayout : Layout
{
    public static readonly BindableProperty AlignEndProperty =
        BindableProperty.Create(nameof(AlignEnd), typeof(bool), typeof(ShellAnchorLayout), false);

    // Panel width matches the anchor (e.g. Select) instead of the panel's own size.
    public static readonly BindableProperty MatchAnchorWidthProperty =
        BindableProperty.Create(nameof(MatchAnchorWidth), typeof(bool), typeof(ShellAnchorLayout), false);

    public bool AlignEnd
    {
        get => (bool)GetValue(AlignEndProperty);
        set => SetValue(AlignEndProperty, value);
    }

    public bool MatchAnchorWidth
    {
        get => (bool)GetValue(MatchAnchorWidthProperty);
        set => SetValue(MatchAnchorWidthProperty, value);
    }

    public double Offset { get; set; } = 4;

    protected override Microsoft.Maui.Layouts.ILayoutManager CreateLayoutManager() => new Manager(this);

    private sealed class Manager(ShellAnchorLayout layout) : Microsoft.Maui.Layouts.ILayoutManager
    {
        public Size Measure(double widthConstraint, double heightConstraint)
        {
            if (layout.Count == 0) return Size.Zero;
            var anchor = layout[0].Measure(widthConstraint, heightConstraint);
            for (var i = 1; i < layout.Count; i++)
            {
                var floatWidth = layout.MatchAnchorWidth ? anchor.Width : widthConstraint;
                layout[i].Measure(floatWidth, double.PositiveInfinity);
            }
            return anchor;
        }

        public Size ArrangeChildren(Rect bounds)
        {
            if (layout.Count == 0) return bounds.Size;
            var anchorView = layout[0];
            var anchorWidth = anchorView.HorizontalLayoutAlignment == Microsoft.Maui.Primitives.LayoutAlignment.Fill
                ? bounds.Width
                : Math.Min(anchorView.DesiredSize.Width, bounds.Width);
            var anchorX = bounds.X;
            anchorView.Arrange(new Rect(anchorX, bounds.Y, anchorWidth, anchorView.DesiredSize.Height));
            var top = bounds.Y + anchorView.DesiredSize.Height + layout.Offset;
            for (var i = 1; i < layout.Count; i++)
            {
                var child = layout[i];
                var width = layout.MatchAnchorWidth ? anchorWidth : child.DesiredSize.Width;
                var x = layout.AlignEnd ? anchorX + anchorWidth - width : anchorX;
                child.Arrange(new Rect(x, top, width, child.DesiredSize.Height));
            }
            return bounds.Size;
        }
    }
}

// Floating panels (Dropdown, Popover, Select). Only one is open at a time: opening another
// closes the previous one, like clicking outside a menu on the web.
public interface IShellPopup
{
    void Close();
}

public static class ShellPopups
{
    private static WeakReference<IShellPopup>? _open;

    public static void Opened(IShellPopup popup)
    {
        if (_open != null && _open.TryGetTarget(out var previous) && !ReferenceEquals(previous, popup))
            previous.Close();
        _open = new WeakReference<IShellPopup>(popup);
    }

    public static void Closed(IShellPopup popup)
    {
        if (_open != null && _open.TryGetTarget(out var current) && ReferenceEquals(current, popup))
            _open = null;
    }

    // ZIndex only orders siblings, so a floating panel must raise every ancestor (up to the
    // scroll view / page) above the content that follows it. Ancestors are ref-counted: two
    // panels in the same row share them, and one closing must not lower the other.
    // Returns the undo action.
    private static readonly Dictionary<VisualElement, (int Original, int Count)> _raised = new();

    public static Action RaiseAboveSiblings(VisualElement host)
    {
        var views = new List<VisualElement>();
        for (Element? e = host; e is VisualElement ve && e is not Page && e is not ScrollView; e = e.Parent)
        {
            if (ve.Parent is not Layout) continue;
            views.Add(ve);
            if (_raised.TryGetValue(ve, out var entry))
                _raised[ve] = (entry.Original, entry.Count + 1);
            else
            {
                _raised[ve] = (ve.ZIndex, 1);
                ve.ZIndex = 1000;
            }
        }
        var undone = false;
        return () =>
        {
            if (undone) return;
            undone = true;
            foreach (var view in views)
            {
                if (!_raised.TryGetValue(view, out var entry)) continue;
                if (entry.Count > 1) _raised[view] = (entry.Original, entry.Count - 1);
                else
                {
                    _raised.Remove(view);
                    view.ZIndex = entry.Original;
                }
            }
        };
    }

    // Shared open/close motion: fade + zoom-in-95 from the top edge.
    public static async Task AnimateAsync(VisualElement panel, bool open)
    {
        panel.AbortAnimation(""ShellPopup"");
        panel.AnchorY = 0;
        if (open)
        {
            panel.Opacity = 0;
            panel.Scale = 0.95;
            panel.IsVisible = true;
            await Task.WhenAll(panel.FadeToAsync(1, 120, Easing.CubicOut), panel.ScaleToAsync(1, 120, Easing.CubicOut));
        }
        else
        {
            await Task.WhenAll(panel.FadeToAsync(0, 90, Easing.CubicIn), panel.ScaleToAsync(0.95, 90, Easing.CubicIn));
            panel.IsVisible = false;
        }
    }

    public static Shadow PanelShadow() => new()
    {
        Brush = new SolidColorBrush(Colors.Black),
        Offset = new Point(0, 4),
        Radius = 12,
        Opacity = 0.12f
    };
}

// Content of a modal overlay (DialogContent, DrawerContent, SheetContent): animates itself in/out.
public interface IShellOverlayContent
{
    Task AnimateAsync(bool open);
}

// Shared host for Dialog / Drawer / Sheet. Children: an optional *Trigger (rendered inline) and
// the *Content (rendered in a full-size layer that is hidden until Open).
// The overlay fills this host, so place the host where it can fill the page — e.g. as the last
// child of the page's root Grid. While closed the host is input-transparent: it never blocks
// the content underneath.
public abstract class ShellOverlayHost : Grid
{
    public static readonly BindableProperty OpenProperty =
        BindableProperty.Create(nameof(Open), typeof(bool), typeof(ShellOverlayHost), false,
            propertyChanged: (b, o, n) => ((ShellOverlayHost)b).OnOpenChanged());

    public bool Open
    {
        get => (bool)GetValue(OpenProperty);
        set => SetValue(OpenProperty, value);
    }

    public event EventHandler<bool>? OpenChanged;

    private readonly VerticalStackLayout _triggerContainer;
    private readonly Grid _overlayLayer;
    private int _version;

    protected ShellOverlayHost()
    {
        InputTransparent = true;
        CascadeInputTransparent = false;
        _triggerContainer = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Start };
        _overlayLayer = new Grid { IsVisible = false, ZIndex = 1000 };
        Children.Add(_triggerContainer);
        Children.Add(_overlayLayer);
    }

    protected abstract bool IsTrigger(Element child);

    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);
        if (child == _triggerContainer || child == _overlayLayer) return;
        Dispatcher.Dispatch(() =>
        {
            if (child is not IView view) return;
            Children.Remove(view);
            if (IsTrigger(child)) _triggerContainer.Children.Add(view);
            else _overlayLayer.Children.Add(view);
        });
    }

    public void SetOpen(bool value) => Open = value;

    private async void OnOpenChanged()
    {
        var version = ++_version;
        OpenChanged?.Invoke(this, Open);
        var content = _overlayLayer.Children.OfType<IShellOverlayContent>().FirstOrDefault();
        if (Open)
        {
            InputTransparent = false;
            _overlayLayer.IsVisible = true;
            if (content != null) await content.AnimateAsync(true);
        }
        else
        {
            InputTransparent = true;
            if (content != null) await content.AnimateAsync(false);
            if (version == _version) _overlayLayer.IsVisible = false;
        }
    }

    // Waits until a freshly shown view has been laid out (its size is needed for slide-ins).
    public static async Task WaitForLayoutAsync(VisualElement view)
    {
        for (var i = 0; i < 20 && (view.Width <= 0 || view.Height <= 0); i++)
            await Task.Delay(16);
    }
}

// Implemented by compositional triggers (DialogTrigger, CollapsibleTrigger, ...). A ShellUI
// Button inside a trigger activates it when clicked — the button handles its own tap, so the
// trigger's gesture never sees it. This is the MAUI take on shadcn's `asChild`.
public interface IShellTrigger
{
    void Activate();
}

// Marks a control that is itself a keyboard tab stop (Button), so a trigger wrapping it
// doesn't add a second one.
public interface IShellFocusable { }

public abstract class ShellTriggerView : ContentView, IShellTrigger
{
    private long _lastActivation;

    protected ShellTriggerView()
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { ShellFocus.FocusPressed(this); Activate(); };
        GestureRecognizers.Add(tap);
        // Wrapping arbitrary content (a label, a row) — the trigger is the tab stop.
        ShellFocus.MakeFocusable(this, Activate, when: () => Content is not IShellFocusable);
    }

    public void Activate()
    {
        // A tap can reach us both through a child Button and our own gesture on platforms
        // that bubble taps. Collapse the pair into one activation.
        var now = Environment.TickCount64;
        if (now - _lastActivation < 250) return;
        _lastActivation = now;
        OnActivated();
    }

    protected abstract void OnActivated();

    // Called by interactive children (Button) after they handle a click.
    public static void ActivateAncestor(Element from)
    {
        for (var p = from.Parent; p != null; p = p.Parent)
        {
            if (p is IShellTrigger trigger)
            {
                trigger.Activate();
                return;
            }
        }
    }
}
"
    };
}
