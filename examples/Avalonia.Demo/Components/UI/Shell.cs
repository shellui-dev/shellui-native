using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AvaloniaDemo.Components.UI;

// ShellUI Native core utilities. Installed by `shellui-native init`; every component depends on it.
public static class Shell
{
    // Joins class names, skipping null or empty ones.
    public static string Cn(params string?[] classes)
        => string.Join(" ", classes.Where(c => !string.IsNullOrWhiteSpace(c)));
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

/* Publishes both palettes as theme dictionaries of the application resources ("ShellUIPrimary" is
   the Color, "ShellUIPrimaryBrush" the Brush). Avalonia resolves the active variant's palette, so a
   theme switch repaints without re-publishing. Your own XAML can use them too:
   Foreground="{DynamicResource ShellUIMutedForegroundBrush}".
   Customize before the first window opens:
     ShellTheme.Light[ShellToken.Primary] = Color.Parse("#2563EB");
     ShellTheme.Apply(); */
public static class ShellTheme
{
    // Based on ShellUI's default neutral theme, softened from pure black/white for native text
    // rendering. Success/Warning/Info extend it (ShellUI has no semantic status colors).
    public static Dictionary<ShellToken, Color> Light { get; } = new()
    {
        [ShellToken.Background] = Color.Parse("#FCFCFC"),
        [ShellToken.Foreground] = Color.Parse("#0A0A0A"),
        [ShellToken.Card] = Color.Parse("#FFFFFF"),
        [ShellToken.CardForeground] = Color.Parse("#0A0A0A"),
        [ShellToken.Popover] = Color.Parse("#FFFFFF"),
        [ShellToken.PopoverForeground] = Color.Parse("#0A0A0A"),
        [ShellToken.Primary] = Color.Parse("#171717"),
        [ShellToken.PrimaryForeground] = Color.Parse("#FAFAFA"),
        [ShellToken.Secondary] = Color.Parse("#F0F0F0"),
        [ShellToken.SecondaryForeground] = Color.Parse("#171717"),
        [ShellToken.Muted] = Color.Parse("#F5F5F5"),
        [ShellToken.MutedForeground] = Color.Parse("#737373"),
        [ShellToken.Accent] = Color.Parse("#F0F0F0"),
        [ShellToken.AccentForeground] = Color.Parse("#171717"),
        [ShellToken.Destructive] = Color.Parse("#E54B4F"),
        [ShellToken.DestructiveForeground] = Color.Parse("#FFFFFF"),
        [ShellToken.Success] = Color.Parse("#16A34A"),
        [ShellToken.SuccessForeground] = Color.Parse("#FFFFFF"),
        [ShellToken.Warning] = Color.Parse("#D97706"),
        [ShellToken.WarningForeground] = Color.Parse("#FFFFFF"),
        [ShellToken.Info] = Color.Parse("#2563EB"),
        [ShellToken.InfoForeground] = Color.Parse("#FFFFFF"),
        [ShellToken.Border] = Color.Parse("#E4E4E4"),
        [ShellToken.Input] = Color.Parse("#E4E4E4"),
        [ShellToken.Ring] = Color.Parse("#A3A3A3"),
        [ShellToken.Overlay] = Color.Parse("#80000000"),
    };

    public static Dictionary<ShellToken, Color> Dark { get; } = new()
    {
        [ShellToken.Background] = Color.Parse("#0A0A0A"),
        [ShellToken.Foreground] = Color.Parse("#FAFAFA"),
        [ShellToken.Card] = Color.Parse("#141414"),
        [ShellToken.CardForeground] = Color.Parse("#FAFAFA"),
        [ShellToken.Popover] = Color.Parse("#171717"),
        [ShellToken.PopoverForeground] = Color.Parse("#FAFAFA"),
        [ShellToken.Primary] = Color.Parse("#FAFAFA"),
        [ShellToken.PrimaryForeground] = Color.Parse("#171717"),
        [ShellToken.Secondary] = Color.Parse("#262626"),
        [ShellToken.SecondaryForeground] = Color.Parse("#FAFAFA"),
        [ShellToken.Muted] = Color.Parse("#262626"),
        [ShellToken.MutedForeground] = Color.Parse("#A3A3A3"),
        [ShellToken.Accent] = Color.Parse("#262626"),
        [ShellToken.AccentForeground] = Color.Parse("#FAFAFA"),
        [ShellToken.Destructive] = Color.Parse("#FF5B5B"),
        [ShellToken.DestructiveForeground] = Color.Parse("#0A0A0A"),
        [ShellToken.Success] = Color.Parse("#22C55E"),
        [ShellToken.SuccessForeground] = Color.Parse("#0A0A0A"),
        [ShellToken.Warning] = Color.Parse("#F59E0B"),
        [ShellToken.WarningForeground] = Color.Parse("#0A0A0A"),
        [ShellToken.Info] = Color.Parse("#3B82F6"),
        [ShellToken.InfoForeground] = Color.Parse("#FFFFFF"),
        [ShellToken.Border] = Color.Parse("#2A2A2A"),
        [ShellToken.Input] = Color.Parse("#333333"),
        [ShellToken.Ring] = Color.Parse("#737373"),
        [ShellToken.Overlay] = Color.Parse("#B3000000"),
    };

    // --radius: 0.5rem. Tailwind's rounded-sm / rounded-md / rounded-lg derive from it.
    public const double RadiusSm = 4;
    public const double RadiusMd = 6;
    public const double RadiusLg = 8;

    private static bool _initialized;

    // Raised when a palette is published or the theme variant changes, for anything resources
    // can't reach (e.g. a BoxShadow).
    public static event EventHandler? ThemeChanged;

    public static bool IsDarkMode => Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

    public static string ColorKey(ShellToken token) => $"ShellUI{token}";
    public static string BrushKey(ShellToken token) => $"ShellUI{token}Brush";

    // Current value of a token, for code that can't bind.
    public static Color Get(ShellToken token) => (IsDarkMode ? Dark : Light)[token];

    // A token at an opacity, for glows and focus rings (Tailwind's ring/35).
    public static Color Get(ShellToken token, double opacity)
    {
        var color = Get(token);
        return new Color((byte)Math.Round(color.A * opacity), color.R, color.G, color.B);
    }

    // Black at an opacity, for drop shadows (shadow-sm is black at 5%).
    public static Color Shadow(double opacity) => new((byte)Math.Round(255 * opacity), 0, 0, 0);

    public static void EnsureInitialized()
    {
        if (_initialized) return;
        var app = Application.Current;
        if (app is null) return;
        _initialized = true;
        app.ActualThemeVariantChanged += (_, _) => ThemeChanged?.Invoke(null, EventArgs.Empty);
        Apply();
    }

    // Re-publishes both palettes. Call after editing Light/Dark at runtime.
    public static void Apply()
    {
        var app = Application.Current;
        if (app is null) return;
        app.Resources.ThemeDictionaries[ThemeVariant.Light] = Build(Light);
        app.Resources.ThemeDictionaries[ThemeVariant.Dark] = Build(Dark);
        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }

    private static ResourceDictionary Build(Dictionary<ShellToken, Color> palette)
    {
        var dictionary = new ResourceDictionary();
        foreach (var (token, color) in palette)
        {
            dictionary[ColorKey(token)] = color;
            dictionary[BrushKey(token)] = new SolidColorBrush(color);
        }
        return dictionary;
    }

    // ThemeVariant.Default follows the OS setting.
    public static void SetTheme(ThemeVariant theme)
    {
        if (Application.Current is not { } app) return;
        EnsureInitialized();
        app.RequestedThemeVariant = theme;
    }

    public static void ToggleTheme() => SetTheme(IsDarkMode ? ThemeVariant.Light : ThemeVariant.Dark);
}

public static class ShellThemeExtensions
{
    // One binding per control and property. A resource binding outlives ClearValue and a later
    // SetValue, and would repaint the property on the next theme change, so it is disposed instead.
    private static readonly ConditionalWeakTable<StyledElement, Dictionary<AvaloniaProperty, IDisposable>> Bindings = new();

    // Binds a Color or Brush property to a theme token, replacing any earlier token binding.
    // Returns the control for chaining.
    public static T Token<T>(this T control, AvaloniaProperty property, ShellToken token) where T : StyledElement
    {
        ShellTheme.EnsureInitialized();
        control.ClearToken(property);
        var key = typeof(IBrush).IsAssignableFrom(property.PropertyType) ? ShellTheme.BrushKey(token) : ShellTheme.ColorKey(token);
        Bindings.GetOrCreateValue(control)[property] = control.Bind(property, control.GetResourceObservable(key));
        return control;
    }

    // Removes the token binding, so a plain value (e.g. Brushes.Transparent) can be set.
    public static T ClearToken<T>(this T control, AvaloniaProperty property) where T : StyledElement
    {
        if (Bindings.TryGetValue(control, out var bindings) && bindings.Remove(property, out var binding))
            binding.Dispose();
        return control;
    }
}

/* shadcn's focus-visible ring (ring-2 ring-ring ring-offset-2): a 2px Background gap, then a 2px
   Ring band. Shown for keyboard focus only, so a click doesn't leave a ring behind. Replaces
   Fluent's focus adorner. */
public static class ShellFocus
{
    private static readonly HashSet<Border> Shown = new();

    static ShellFocus() => ShellTheme.ThemeChanged += (_, _) =>
    {
        foreach (var border in Shown) Paint(border);
    };

    public static void Ring(Control control, Border target)
    {
        control.Focusable = true;
        control.FocusAdorner = null;
        control.GotFocus += (_, e) =>
        {
            if (e.NavigationMethod is not (NavigationMethod.Tab or NavigationMethod.Directional)) return;
            Shown.Add(target);
            Paint(target);
        };
        control.LostFocus += (_, _) =>
        {
            Shown.Remove(target);
            target.BoxShadow = default;
        };
    }

    private static void Paint(Border border) => border.BoxShadow = new BoxShadows(
        new BoxShadow { Spread = 2, Color = ShellTheme.Get(ShellToken.Background) },
        new[] { new BoxShadow { Spread = 4, Color = ShellTheme.Get(ShellToken.Ring) } });
}

// Fade + transform transitions for overlays opening and closing.
public static class ShellMotion
{
    // Jumps to the values without animating (the starting point of an entrance).
    public static void Set(Visual visual, double opacity, string transform)
    {
        visual.Transitions = null;
        visual.Opacity = opacity;
        visual.RenderTransform = TransformOperations.Parse(transform);
    }

    // Animates from the current values; completes when the transition has run.
    public static Task To(Visual visual, double opacity, string transform, int milliseconds, Easing easing)
    {
        var duration = TimeSpan.FromMilliseconds(milliseconds);
        visual.Transitions = new Transitions
        {
            new DoubleTransition { Property = Visual.OpacityProperty, Duration = duration, Easing = easing },
            new TransformOperationsTransition { Property = Visual.RenderTransformProperty, Duration = duration, Easing = easing }
        };
        visual.Opacity = opacity;
        visual.RenderTransform = TransformOperations.Parse(transform);
        return Task.Delay(duration);
    }

    // Waits until a freshly shown control has a size (slide-ins start one size away).
    public static async Task WaitForLayoutAsync(Control control)
    {
        for (var i = 0; i < 20 && (control.Bounds.Width <= 0 || control.Bounds.Height <= 0); i++)
            await Task.Delay(16);
    }
}

// Open overlays and popups, newest last. Escape closes the top one; with nothing open the key
// keeps its normal behavior.
public static class ShellDismiss
{
    private static readonly List<(object Key, Action Close)> Open = new();
    private static readonly ConditionalWeakTable<TopLevel, object> Hooked = new();

    public static void Push(object key, Action close, Visual from)
    {
        Open.RemoveAll(entry => ReferenceEquals(entry.Key, key));
        Open.Add((key, close));
        if (TopLevel.GetTopLevel(from) is not { } top || Hooked.TryGetValue(top, out _)) return;
        Hooked.Add(top, new object());
        // Tunnel, so Escape reaches us before a focused control (e.g. a TextBox) handles it.
        top.AddHandler(InputElement.KeyDownEvent, (_, e) =>
        {
            if (e.Key == Key.Escape && DismissTop()) e.Handled = true;
        }, RoutingStrategies.Tunnel);
    }

    public static void Remove(object key) => Open.RemoveAll(entry => ReferenceEquals(entry.Key, key));

    // Closes the most recently opened overlay. False when nothing is open.
    public static bool DismissTop()
    {
        if (Open.Count == 0) return false;
        var top = Open[^1];
        Open.RemoveAt(Open.Count - 1);
        top.Close();
        return true;
    }
}

// Floating panels that close on a click outside (Dropdown, Popover). Only one is open at a time:
// opening another closes the previous one.
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

    // shadow-md
    public static BoxShadows PanelShadow() => new(new BoxShadow { OffsetY = 4, Blur = 12, Color = ShellTheme.Shadow(0.12) });
}

/* Base for a component with a trigger and floating content. XAML children go to Items: the
   content part is shown in a Popup in the window's overlay layer, everything else (the trigger)
   renders in place. The Popup keeps the content in the logical tree, so it inherits the theme
   and DataContext, and its parts find the component with FindParentOfType. */
public abstract class ShellFloatingHost : Border
{
    private readonly Panel _inline = new();
    protected readonly Popup Popup = new() { ShouldUseOverlayLayer = true };

    protected Control? FloatingContent { get; private set; }

    [Content]
    public Controls Items { get; } = new();

    protected ShellFloatingHost()
    {
        _inline.Children.Add(Popup);
        Child = _inline;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
        Items.CollectionChanged += OnItemsChanged;
    }

    protected abstract bool IsContent(Control child);

    // Rebuilt from Items on every change; Clear reports no old items, so diffing isn't worth it.
    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Popup.Child = null;
        FloatingContent = null;
        _inline.Children.RemoveAll(_inline.Children.Where(c => c != Popup).ToList());
        foreach (var child in Items)
        {
            if (IsContent(child)) Popup.Child = FloatingContent = child;
            else _inline.Children.Insert(_inline.Children.Count - 1, child);
        }
    }
}

/* A panel floating under an anchor in the window's overlay layer (Dropdown, Popover, Select,
   DatePicker, TimePicker). A click outside or Escape closes it, and only one is open at a time.
   The owner puts Popup in its tree, so the panel inherits the owner's theme and DataContext. */
public sealed class ShellAnchoredPopup : IShellPopup
{
    private bool _open;

    public Popup Popup { get; }

    public bool IsOpen => _open;

    // Space between the anchor and the panel, on whichever side the panel opens.
    public double Gap { get; set; } = 4;

    // Raised when it closes for any reason: Close(), a click outside, Escape or another menu opening.
    public event EventHandler? Closed;

    public ShellAnchoredPopup(Control anchor, Popup? popup = null)
    {
        Popup = popup ?? new Popup { ShouldUseOverlayLayer = true };
        Popup.PlacementTarget = anchor;
        Popup.Placement = PlacementMode.BottomEdgeAlignedLeft;
        Popup.IsLightDismissEnabled = true;
        // The click that dismisses (even on the trigger) does nothing else, so it can't reopen.
        Popup.OverlayDismissEventPassThrough = false;
        Popup.Closed += (_, _) =>
        {
            if (_open) Finish();
        };
    }

    public async void Open()
    {
        if (_open || Popup.Child is not Control panel || Popup.PlacementTarget is not { } anchor) return;
        _open = true;
        ShellPopups.Opened(this);
        ShellDismiss.Push(this, Close, anchor);
        panel.RenderTransformOrigin = new RelativePoint(0.5, 0, RelativeUnit.Relative);
        ShellMotion.Set(panel, 0, "scale(0.95)");
        Popup.VerticalOffset = Gap;
        Popup.IsOpen = true;
        // Avalonia flips a panel that doesn't fit below to above the anchor but keeps the offset's
        // sign, which pushes it onto the anchor; mirror the offset once the flip is known.
        Dispatcher.UIThread.Post(() =>
        {
            if (_open && panel.TranslatePoint(default, anchor) is { Y: < 0 }) Popup.VerticalOffset = -Gap;
        }, DispatcherPriority.Loaded);
        await ShellMotion.To(panel, 1, "scale(1)", 120, new CubicEaseOut());
    }

    public async void Close()
    {
        if (!_open) return;
        Finish();
        if (Popup.Child is Control panel) await ShellMotion.To(panel, 0, "scale(0.95)", 90, new CubicEaseIn());
        if (!_open) Popup.IsOpen = false;
    }

    private void Finish()
    {
        _open = false;
        ShellPopups.Closed(this);
        ShellDismiss.Remove(this);
        Closed?.Invoke(this, EventArgs.Empty);
    }
}

// Shared host for trigger + floating panel (Dropdown, Popover). Clicking outside closes it.
public abstract class ShellPopoverHost : ShellFloatingHost
{
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<ShellPopoverHost, bool>(nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    private readonly ShellAnchoredPopup _anchored;

    static ShellPopoverHost()
    {
        IsOpenProperty.Changed.AddClassHandler<ShellPopoverHost>((h, _) => h.OnOpenChanged());
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public event EventHandler<bool>? IsOpenChanged;

    protected ShellPopoverHost()
    {
        _anchored = new ShellAnchoredPopup(this, Popup);
        _anchored.Closed += (_, _) => IsOpen = false;
    }

    protected virtual PlacementMode Placement => PlacementMode.BottomEdgeAlignedLeft;
    protected virtual double Offset => 4;

    public void SetOpen(bool value) => IsOpen = value;
    public void Toggle() => IsOpen = !IsOpen;
    public void Close() => IsOpen = false;

    private void OnOpenChanged()
    {
        IsOpenChanged?.Invoke(this, IsOpen);
        if (!IsOpen)
        {
            _anchored.Close();
            return;
        }
        if (FloatingContent is null) return;
        Popup.Placement = Placement;
        _anchored.Gap = Offset;
        _anchored.Open();
    }
}

// Content of a modal overlay (DialogContent, DrawerContent, SheetContent): animates itself in and out.
public interface IShellOverlayContent
{
    Task AnimateAsync(bool open);
}

/* Shared host for Dialog / Drawer / Sheet. Children: an optional *Trigger (rendered in place) and
   the *Content, which covers the window while Open, so the host can sit next to the button that
   opens it. While open, keyboard focus stays inside the content (Tab cycles through it); on close
   it returns to the control that had it, usually the trigger. */
public abstract class ShellOverlayHost : ShellFloatingHost
{
    public static readonly StyledProperty<bool> OpenProperty =
        AvaloniaProperty.Register<ShellOverlayHost, bool>(nameof(Open), defaultBindingMode: BindingMode.TwoWay);

    private TopLevel? _top;
    private IInputElement? _returnFocus;
    private int _version;

    static ShellOverlayHost()
    {
        OpenProperty.Changed.AddClassHandler<ShellOverlayHost>((h, _) => h.OnOpenChanged());
    }

    public bool Open
    {
        get => GetValue(OpenProperty);
        set => SetValue(OpenProperty, value);
    }

    public event EventHandler<bool>? OpenChanged;

    protected ShellOverlayHost()
    {
        Popup.IsLightDismissEnabled = false;
        Popup.Placement = PlacementMode.Center;
    }

    protected override bool IsContent(Control child) => child is IShellOverlayContent;

    public void SetOpen(bool value) => Open = value;

    // What Escape does while this overlay is on top.
    protected virtual void Dismiss() => Open = false;

    private async void OnOpenChanged()
    {
        var version = ++_version;
        if (Open) ShellDismiss.Push(this, Dismiss, this);
        else ShellDismiss.Remove(this);
        OpenChanged?.Invoke(this, Open);
        if (FloatingContent is not { } content) return;

        if (Open)
        {
            if (TopLevel.GetTopLevel(this) is not { } top) return;
            if (_top is null)
            {
                _top = top;
                _top.SizeChanged += OnWindowSizeChanged;
            }
            Fit(content);
            Popup.PlacementTarget = top;
            _returnFocus = top.FocusManager?.GetFocusedElement();
            Popup.IsOpen = true;
            TrapFocus(content);
            if (content is IShellOverlayContent animated) await animated.AnimateAsync(true);
        }
        else
        {
            if (_returnFocus is InputElement previous && TopLevel.GetTopLevel(previous) != null) previous.Focus();
            _returnFocus = null;
            if (content is IShellOverlayContent animated) await animated.AnimateAsync(false);
            if (version != _version) return; // reopened while closing
            Popup.IsOpen = false;
            if (_top != null) _top.SizeChanged -= OnWindowSizeChanged;
            _top = null;
        }
    }

    // Tab and Shift+Tab wrap inside the content, and focus starts on its first tab stop (the
    // content itself when it has none), so nothing behind the backdrop can be reached.
    private static void TrapFocus(Control content)
    {
        KeyboardNavigation.SetTabNavigation(content, KeyboardNavigationMode.Cycle);
        var first = content.GetVisualDescendants().OfType<InputElement>().FirstOrDefault(e =>
            e.Focusable && e.IsEffectivelyEnabled && e.IsEffectivelyVisible && KeyboardNavigation.GetIsTabStop(e));
        if (first is null)
        {
            content.Focusable = true;
            content.FocusAdorner = null;
            first = content;
        }
        first.Focus();
    }

    private void OnWindowSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (FloatingContent is { } content) Fit(content);
    }

    // The content (backdrop + panel) covers the whole window.
    private void Fit(Control content)
    {
        if (_top is null) return;
        content.Width = _top.ClientSize.Width;
        content.Height = _top.ClientSize.Height;
    }
}

// Implemented by compositional triggers (DialogTrigger, DropdownTrigger, ...).
public interface IShellTrigger
{
    void Activate();
}

// Marks a control that is itself a tab stop (Button), so a trigger wrapping it doesn't add a second one.
public interface IShellFocusable { }

/* Wraps the control that opens a component: <ui:DialogTrigger><ui:Button Text="Open" /></ui:DialogTrigger>.
   A ShellUI Button inside handles its own click and then activates the trigger, the Avalonia
   take on shadcn's asChild; any other content activates it through the trigger itself. */
public abstract class ShellTriggerView : Border, IShellTrigger
{
    static ShellTriggerView()
    {
        ChildProperty.Changed.AddClassHandler<ShellTriggerView>((t, _) => t.Focusable = t.Child is not IShellFocusable);
    }

    protected ShellTriggerView()
    {
        Background = Brushes.Transparent;
        HorizontalAlignment = HorizontalAlignment.Left;
        Cursor = new Cursor(StandardCursorType.Hand);
        ShellFocus.Ring(this, this);
    }

    public void Activate() => OnActivated();

    protected abstract void OnActivated();

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left || !new Rect(Bounds.Size).Contains(e.GetPosition(this))) return;
        Activate();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is not (Key.Enter or Key.Space)) return;
        Activate();
        e.Handled = true;
    }

    // Called by interactive children (Button) after they handle a click.
    public static void ActivateAncestor(StyledElement from)
    {
        for (var p = from.Parent; p != null; p = p.Parent)
        {
            if (p is not IShellTrigger trigger) continue;
            trigger.Activate();
            return;
        }
    }
}

// Native-control helpers: Fluent draws its own chrome inside the controls a ShellUI Border wraps.
public static class ShellPlatform
{
    // Fluent's TextBox template takes its background and border brushes from these resources in
    // every state (hover, focus, disabled); overriding them on the TextBox clears that chrome.
    private static readonly string[] TextBoxBrushes =
    {
        "TextControlBackground", "TextControlBackgroundPointerOver", "TextControlBackgroundFocused", "TextControlBackgroundDisabled",
        "TextControlBorderBrush", "TextControlBorderBrushPointerOver", "TextControlBorderBrushFocused", "TextControlBorderBrushDisabled",
    };

    // Leaves the wrapping Border as the only frame, with text, caret and placeholder in the theme tokens.
    public static void StripNativeChrome(TextBox textBox)
    {
        foreach (var key in TextBoxBrushes)
            textBox.Resources[key] = Brushes.Transparent;
        textBox.Resources["TextControlBorderThemeThickness"] = new Thickness(0);
        textBox.Resources["TextControlBorderThemeThicknessFocused"] = new Thickness(0);
        textBox.Resources["TextControlThemePadding"] = new Thickness(0);
        textBox.BorderThickness = new Thickness(0);
        textBox.Padding = new Thickness(0);
        textBox.MinHeight = 0;
        textBox.Token(TextBox.ForegroundProperty, ShellToken.Foreground)
            .Token(TextBox.CaretBrushProperty, ShellToken.Foreground)
            .Token(TextBox.PlaceholderForegroundProperty, ShellToken.MutedForeground);
    }
}
