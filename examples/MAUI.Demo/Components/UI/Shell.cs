namespace MAUI.Demo.Components.UI;

// ShellUI Native core utilities. Installed by `shellui-native init`; every component depends on it.
public static class Shell
{
    // Combines multiple class names, filtering out null/empty values
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

// Publishes the active palette as application resources ("ShellUIPrimary" for the Color,
// "ShellUIPrimaryBrush" for the Brush). Components bind to them with SetDynamicResource, so
// switching theme or overriding a token repaints everything — the MAUI equivalent of swapping
// CSS variables. Your own XAML can use them too: TextColor="{DynamicResource ShellUIMutedForeground}".
//
// Customize before the first page loads (e.g. in App's constructor):
//   ShellTheme.Light[ShellToken.Primary] = Color.FromArgb("#2563EB");
//   ShellTheme.Apply();
public static class ShellTheme
{
    // Based on ShellUI's default neutral theme, softened from pure black/white for native text
    // rendering. Success/Warning/Info extend it (ShellUI has no semantic status colors).
    public static Dictionary<ShellToken, Color> Light { get; } = new()
    {
        [ShellToken.Background] = Color.FromArgb("#FCFCFC"),
        [ShellToken.Foreground] = Color.FromArgb("#0A0A0A"),
        [ShellToken.Card] = Color.FromArgb("#FFFFFF"),
        [ShellToken.CardForeground] = Color.FromArgb("#0A0A0A"),
        [ShellToken.Popover] = Color.FromArgb("#FFFFFF"),
        [ShellToken.PopoverForeground] = Color.FromArgb("#0A0A0A"),
        [ShellToken.Primary] = Color.FromArgb("#171717"),
        [ShellToken.PrimaryForeground] = Color.FromArgb("#FAFAFA"),
        [ShellToken.Secondary] = Color.FromArgb("#F0F0F0"),
        [ShellToken.SecondaryForeground] = Color.FromArgb("#171717"),
        [ShellToken.Muted] = Color.FromArgb("#F5F5F5"),
        [ShellToken.MutedForeground] = Color.FromArgb("#737373"),
        [ShellToken.Accent] = Color.FromArgb("#F0F0F0"),
        [ShellToken.AccentForeground] = Color.FromArgb("#171717"),
        [ShellToken.Destructive] = Color.FromArgb("#E54B4F"),
        [ShellToken.DestructiveForeground] = Color.FromArgb("#FFFFFF"),
        [ShellToken.Success] = Color.FromArgb("#16A34A"),
        [ShellToken.SuccessForeground] = Color.FromArgb("#FFFFFF"),
        [ShellToken.Warning] = Color.FromArgb("#D97706"),
        [ShellToken.WarningForeground] = Color.FromArgb("#FFFFFF"),
        [ShellToken.Info] = Color.FromArgb("#2563EB"),
        [ShellToken.InfoForeground] = Color.FromArgb("#FFFFFF"),
        [ShellToken.Border] = Color.FromArgb("#E4E4E4"),
        [ShellToken.Input] = Color.FromArgb("#E4E4E4"),
        [ShellToken.Ring] = Color.FromArgb("#A3A3A3"),
        [ShellToken.Overlay] = Color.FromArgb("#80000000"),
    };

    public static Dictionary<ShellToken, Color> Dark { get; } = new()
    {
        [ShellToken.Background] = Color.FromArgb("#0A0A0A"),
        [ShellToken.Foreground] = Color.FromArgb("#FAFAFA"),
        [ShellToken.Card] = Color.FromArgb("#141414"),
        [ShellToken.CardForeground] = Color.FromArgb("#FAFAFA"),
        [ShellToken.Popover] = Color.FromArgb("#171717"),
        [ShellToken.PopoverForeground] = Color.FromArgb("#FAFAFA"),
        [ShellToken.Primary] = Color.FromArgb("#FAFAFA"),
        [ShellToken.PrimaryForeground] = Color.FromArgb("#171717"),
        [ShellToken.Secondary] = Color.FromArgb("#262626"),
        [ShellToken.SecondaryForeground] = Color.FromArgb("#FAFAFA"),
        [ShellToken.Muted] = Color.FromArgb("#262626"),
        [ShellToken.MutedForeground] = Color.FromArgb("#A3A3A3"),
        [ShellToken.Accent] = Color.FromArgb("#262626"),
        [ShellToken.AccentForeground] = Color.FromArgb("#FAFAFA"),
        [ShellToken.Destructive] = Color.FromArgb("#FF5B5B"),
        [ShellToken.DestructiveForeground] = Color.FromArgb("#0A0A0A"),
        [ShellToken.Success] = Color.FromArgb("#22C55E"),
        [ShellToken.SuccessForeground] = Color.FromArgb("#0A0A0A"),
        [ShellToken.Warning] = Color.FromArgb("#F59E0B"),
        [ShellToken.WarningForeground] = Color.FromArgb("#0A0A0A"),
        [ShellToken.Info] = Color.FromArgb("#3B82F6"),
        [ShellToken.InfoForeground] = Color.FromArgb("#FFFFFF"),
        [ShellToken.Border] = Color.FromArgb("#2A2A2A"),
        [ShellToken.Input] = Color.FromArgb("#333333"),
        [ShellToken.Ring] = Color.FromArgb("#737373"),
        [ShellToken.Overlay] = Color.FromArgb("#B3000000"),
    };

    // --radius: 0.5rem. Tailwind's rounded-sm / rounded-md / rounded-lg derive from it.
    public const float RadiusSm = 4;
    public const float RadiusMd = 6;
    public const float RadiusLg = 8;

    // Opt-in: make Android's status and navigation bars follow the theme — transparent bars over
    // the edge-to-edge page, a Background-colored status strip drawn by the page layer (so
    // backdrops and panels cover it), and bar icons flipped with the theme. Set before
    // EnsureInitialized (e.g. in App's constructor).
    public static bool SyncSystemBars { get; set; }

    private static bool _initialized;

    // Raised after a palette is published. Resources cover colors and brushes; use this for
    // anything they can't reach (shadows, native-control tweaks).
    public static event EventHandler? ThemeChanged;

    public static bool IsDarkMode => Application.Current?.RequestedTheme == AppTheme.Dark;

    public static string ColorKey(ShellToken token) => $"ShellUI{token}";
    public static string BrushKey(ShellToken token) => $"ShellUI{token}Brush";

    // Current value of a token — for code that can't bind (e.g. building a Shadow).
    public static Color Get(ShellToken token) => (IsDarkMode ? Dark : Light)[token];

    public static void EnsureInitialized()
    {
        if (_initialized) return;
        var app = Application.Current;
        if (app is null) return;
        _initialized = true;
        app.RequestedThemeChanged += (_, _) => Apply();
        // Set up each page's overlay layer as it appears. Doing it on first use instead would
        // re-parent the page content under the user's finger (Android drops the scroll position).
        app.PageAppearing += (_, page) => page.Dispatcher.Dispatch(() => ShellPortal.GetLayer(page));
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
        if (SyncSystemBars) ApplySystemBars(retries: 10);
        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }

    private static void ApplySystemBars(int retries)
    {
#if ANDROID
        var window = Platform.CurrentActivity?.Window;
        if (window is null)
        {
            // No activity yet (theme applied from App's constructor): try again shortly.
            if (retries > 0)
                Application.Current?.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () => ApplySystemBars(retries - 1));
            return;
        }
#pragma warning disable CA1422 // Android 15+ draws edge-to-edge and ignores these; older versions need them.
        window.SetStatusBarColor(Android.Graphics.Color.Transparent);
        window.SetNavigationBarColor(Android.Graphics.Color.Transparent);
#pragma warning restore CA1422
        if (AndroidX.Core.View.WindowCompat.GetInsetsController(window, window.DecorView) is { } insets)
        {
            insets.AppearanceLightStatusBars = !IsDarkMode;
            insets.AppearanceLightNavigationBars = !IsDarkMode;
        }
#endif
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
            foreach (var prefix in new[] { "TextControl", "CalendarDatePicker", "TimePickerButton", "ComboBox" })
            {
                foreach (var state in new[] { "", "PointerOver", "Pressed", "Focused", "Disabled" })
                {
                    resources[$"{prefix}Background{state}"] = Transparent();
                    resources[$"{prefix}BorderBrush{state}"] = Transparent();
                }
            }
            resources["TextControlBorderThemeThickness"] = zero;
            resources["TextControlBorderThemeThicknessFocused"] = zero;
            resources["CalendarDatePickerBorderThemeThickness"] = zero;
            resources["TimePickerBorderThemeThickness"] = zero;
            resources["TimePickerSpacerFill"] = Transparent();
            resources["ComboBoxBorderThemeThickness"] = zero;
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
            System.Diagnostics.Debug.WriteLine($"ShellUI: could not strip native chrome: {ex.Message}");
        }
#elif ANDROID
        // EditText (Entry, Editor, and the date/time pickers) draws a Material underline as its
        // background and adds its own padding.
        if (view.Handler?.PlatformView is Android.Widget.EditText edit)
        {
            edit.Background = null;
            if (!keepPadding) edit.SetPadding(0, 0, 0, 0);
        }
#elif IOS || MACCATALYST
        if (view.Handler?.PlatformView is UIKit.UITextField field)
        {
            field.BorderStyle = UIKit.UITextBorderStyle.None;
            field.BackgroundColor = UIKit.UIColor.Clear;
        }
        else if (view.Handler?.PlatformView is UIKit.UITextView text)
        {
            text.BackgroundColor = UIKit.UIColor.Clear;
            if (!keepPadding)
            {
                text.TextContainerInset = UIKit.UIEdgeInsets.Zero;
                text.TextContainer.LineFragmentPadding = 0;
            }
        }
#endif
    }

    // For a text field that only captures input while the component draws the text itself
    // (InputOtp): hides the platform caret and selection so nothing native shows through.
    public static void HideCaret(View view)
    {
        view.HandlerChanged += (_, _) => Hide(view);
        Hide(view);
    }

    private static void Hide(View view)
    {
#if WINDOWS
        // Opacity 0 still hit-tests in WinUI, so the field keeps taking taps and focus.
        if (view.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement element) element.Opacity = 0;
#elif ANDROID
        if (view.Handler?.PlatformView is Android.Widget.EditText edit)
        {
            edit.SetCursorVisible(false);
            edit.SetHighlightColor(Android.Graphics.Color.Transparent);
            edit.LongClickable = false;
        }
#elif IOS || MACCATALYST
        if (view.Handler?.PlatformView is UIKit.UITextField field) field.TintColor = UIKit.UIColor.Clear;
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

// Page-level layer for anything that must draw above the page: modal overlays, floating panels,
// tooltips, toasts — the MAUI take on a React portal. Created on first use as the last child of
// the page's root Grid (a non-Grid root is wrapped in one, once). The layer itself never takes
// input; only what is placed in it does.
public static class ShellPortal
{
    private static readonly BindableProperty LayerProperty =
        BindableProperty.CreateAttached("ShellPortalLayer", typeof(Grid), typeof(ShellPortal), null);

    // Logical owner of a view that was moved into the layer (e.g. DialogContent -> its Dialog).
    // FindParentOfType follows it, so portalled content still finds its component.
    public static readonly BindableProperty OwnerProperty =
        BindableProperty.CreateAttached("Owner", typeof(Element), typeof(ShellPortal), null);

    public static Element? GetOwner(BindableObject view) => (Element?)view.GetValue(OwnerProperty);

    // Parent in the logical sense: the portal owner when there is one, else the visual parent.
    public static Element? LogicalParent(Element element) => GetOwner(element) ?? element.Parent;

    public static Grid? GetLayer(Element? element)
    {
        var page = FindPage(element);
        if (page is null) return null;
        if (page.GetValue(LayerProperty) is Grid existing) return existing;

        if (page.Content is not Grid root)
        {
            var content = page.Content;
            var scroll = content as ScrollView;
            var (scrollX, scrollY) = (scroll?.ScrollX ?? 0, scroll?.ScrollY ?? 0);
            // Edge-to-edge like the page itself, so wrapping doesn't change how the content is inset.
            root = new Grid { SafeAreaEdges = SafeAreaEdges.None };
            page.Content = root;
            if (content != null) root.Children.Add(content);
            // Normally this runs as the page appears (see ShellTheme.EnsureInitialized); if it
            // runs later, re-parenting must not lose where the user had scrolled to.
            if (scroll != null && (scrollX > 0 || scrollY > 0))
                scroll.Dispatcher.Dispatch(() => _ = scroll.ScrollToAsync(scrollX, scrollY, false));
        }

        // The layer covers the whole window (backdrops dim under the system bars); content placed
        // in it uses GetSafeInsets to stay clear of them.
        var layer = new Grid
        {
            ZIndex = 10000,
            InputTransparent = true,
            CascadeInputTransparent = false,
            SafeAreaEdges = SafeAreaEdges.None
        };
        Grid.SetRowSpan(layer, Math.Max(1, root.RowDefinitions.Count));
        Grid.SetColumnSpan(layer, Math.Max(1, root.ColumnDefinitions.Count));
        root.Children.Add(layer);
        page.SetValue(LayerProperty, layer);
#if ANDROID
        if (ShellTheme.SyncSystemBars) AddStatusBarScrim(layer);
#endif
        return layer;
    }

#if ANDROID
    // With SyncSystemBars the system bars are transparent and the page draws under them. This
    // strip is the status bar's background: page content scrolls beneath it, while backdrops and
    // panels (added to the layer after it) cover it.
    private static void AddStatusBarScrim(Grid layer)
    {
        var scrim = new BoxView
        {
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Start,
            HeightRequest = 0,
            InputTransparent = true
        };
        scrim.Token(BoxView.ColorProperty, ShellToken.Background);
        layer.Children.Add(scrim);
        void Fit() => scrim.HeightRequest = GetSafeInsets(layer).Top;
        layer.SizeChanged += (_, _) => Fit();
        layer.Loaded += (_, _) => Fit();
        Fit();
    }
#endif

    private static ContentPage? FindPage(Element? element)
    {
        for (var e = element; e != null; e = LogicalParent(e))
            if (e is ContentPage found) return found;

        // Not in a tree (e.g. Toast.Show from a view model): use the page on screen.
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        while (true)
        {
            switch (page)
            {
                case Microsoft.Maui.Controls.Shell shell: page = shell.CurrentPage; continue;
                case NavigationPage nav: page = nav.CurrentPage; continue;
                case FlyoutPage flyout: page = flyout.Detail; continue;
                case TabbedPage tabs: page = tabs.CurrentPage; continue;
            }
            break;
        }
        return page as ContentPage;
    }

    // Moves `view` into the layer above the page that contains `owner`. Returns false when there
    // is no page yet (caller should fall back to showing the view in place).
    public static bool Attach(Element owner, View view)
    {
        var layer = GetLayer(owner);
        if (layer is null) return false;
        if (view.Parent == layer) return true;
        Detach(view);
        view.SetValue(OwnerProperty, owner);
        view.BindingContext = owner.BindingContext;
        layer.Children.Add(view);
        return true;
    }

    public static void Detach(View view)
    {
        if (view.Parent is Layout layout) layout.Children.Remove(view);
        else if (view.Parent is ContentView host && host.Content == view) host.Content = null;
    }

    // Opts overlay chrome out of MAUI's automatic safe-area padding: backdrops and panels run
    // under the system bars and pad their own content with GetSafeInsets instead.
    public static void EdgeToEdge(params View[] views)
    {
        foreach (var view in views)
        {
            switch (view)
            {
                case Layout layout: layout.SafeAreaEdges = SafeAreaEdges.None; break;
                case ScrollView scroll: scroll.SafeAreaEdges = SafeAreaEdges.None; break;
                case Border border: border.SafeAreaEdges = SafeAreaEdges.None; break;
                case ContentView content: content.SafeAreaEdges = SafeAreaEdges.None; break;
            }
        }
    }

    // How far the system bars / notch reach into `view`, in device-independent units. Zero on
    // an edge the view doesn't touch (e.g. the top of a page below a navigation bar).
    public static Thickness GetSafeInsets(VisualElement view)
    {
#if ANDROID
        if (view.Handler?.PlatformView is Android.Views.View native &&
            AndroidX.Core.View.ViewCompat.GetRootWindowInsets(native) is { } windowInsets)
        {
            var bars = windowInsets.GetInsets(AndroidX.Core.View.WindowInsetsCompat.Type.SystemBars() |
                                              AndroidX.Core.View.WindowInsetsCompat.Type.DisplayCutout());
            var density = native.Resources?.DisplayMetrics?.Density ?? 1f;
            if (bars != null)
            {
                var at = new int[2];
                native.GetLocationInWindow(at);
                var window = native.RootView;
                double right = bars.Right, bottom = bars.Bottom;
                if (window != null && native.Width > 0 && native.Height > 0)
                {
                    right = Math.Max(0, bars.Right - (window.Width - at[0] - native.Width));
                    bottom = Math.Max(0, bars.Bottom - (window.Height - at[1] - native.Height));
                }
                return new Thickness(
                    Math.Max(0, bars.Left - at[0]) / density,
                    Math.Max(0, bars.Top - at[1]) / density,
                    right / density,
                    bottom / density);
            }
        }
#elif IOS || MACCATALYST
        if (view.Handler?.PlatformView is UIKit.UIView { Window: { } window })
        {
            var i = window.SafeAreaInsets;
            return new Thickness(i.Left, i.Top, i.Right, i.Bottom);
        }
#endif
        return new Thickness(0);
    }

    // Top-left of `view` in the layer's coordinates. Asks the platform where both really are
    // (scrolling, transforms and safe areas included); falls back to walking the tree.
    public static Point GetPosition(VisualElement view, Grid layer)
    {
#if ANDROID
        if (view.Handler?.PlatformView is Android.Views.View nativeView && layer.Handler?.PlatformView is Android.Views.View nativeLayer)
        {
            var a = new int[2];
            var l = new int[2];
            nativeView.GetLocationInWindow(a);
            nativeLayer.GetLocationInWindow(l);
            var density = nativeView.Resources?.DisplayMetrics?.Density ?? 1f;
            return new Point((a[0] - l[0]) / density, (a[1] - l[1]) / density);
        }
#elif IOS || MACCATALYST
        if (view.Handler?.PlatformView is UIKit.UIView nativeView && layer.Handler?.PlatformView is UIKit.UIView nativeLayer)
        {
            var p = nativeView.ConvertPointToView(CoreGraphics.CGPoint.Empty, nativeLayer);
            return new Point(p.X, p.Y);
        }
#elif WINDOWS
        if (view.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement nativeView && layer.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement nativeLayer)
        {
            var p = nativeView.TransformToVisual(nativeLayer).TransformPoint(new Windows.Foundation.Point(0, 0));
            return new Point(p.X, p.Y);
        }
#endif
        double x = 0, y = 0;
        var root = layer.Parent;
        for (Element? e = view; e is VisualElement ve && e != root; e = e.Parent)
        {
            x += ve.X + ve.TranslationX;
            y += ve.Y + ve.TranslationY;
            if (ve.Parent is ScrollView scroll)
            {
                x -= scroll.ScrollX;
                y -= scroll.ScrollY;
            }
        }
        return new Point(x - layer.X, y - layer.Y);
    }

    // Floats `panel` next to `anchor` in the page layer. Returns a handle to close it, or null
    // when there is no page to float over.
    public static ShellPopupHandle? ShowPopup(View anchor, View panel, ShellPopupOptions? options = null)
    {
        options ??= new ShellPopupOptions();
        var layer = GetLayer(anchor);
        if (layer is null) return null;

        View? catcher = null;
        var handle = new ShellPopupHandle(panel, () => catcher);
        if (options.Modal)
        {
            // Transparent full-page catcher: a click outside the panel dismisses it.
            var surface = new Grid { BackgroundColor = Colors.Transparent };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => options.OnDismiss?.Invoke();
            surface.GestureRecognizers.Add(tap);
            catcher = surface;
            layer.Children.Add(surface);
            if (options.OnDismiss != null) ShellDismiss.Push(handle, options.OnDismiss);
        }

        Detach(panel);
        panel.SetValue(OwnerProperty, options.Owner ?? anchor);
        panel.BindingContext = (options.Owner ?? anchor).BindingContext;
        panel.HorizontalOptions = LayoutOptions.Start;
        panel.VerticalOptions = LayoutOptions.Start;
        panel.Margin = new Thickness(0);
        panel.Opacity = 0;
        panel.IsVisible = true;
        if (options.MatchAnchorWidth) panel.WidthRequest = anchor.Width;
        layer.Children.Add(panel);

        _ = PlaceAsync(anchor, panel, layer, options, handle);
        return handle;
    }

    // Platforms disagree on what an unarranged view measures to (e.g. a ScrollView's height), so
    // wait for the real layout and position from the size the panel actually got.
    private static async Task PlaceAsync(View anchor, View panel, Grid layer, ShellPopupOptions options, ShellPopupHandle handle)
    {
        for (var i = 0; i < 30 && (panel.Width <= 0 || panel.Height <= 0); i++)
            await Task.Delay(16);
        if (handle.IsClosed) return;

        var size = new Size(panel.Width, panel.Height);
        var origin = GetPosition(anchor, layer);
        const double edge = 8;
        var safe = GetSafeInsets(layer);
        var minTop = safe.Top + edge;
        var maxBottom = layer.Height - safe.Bottom - edge;

        var x = options.Align switch
        {
            ShellPopupAlign.Center => origin.X + (anchor.Width - size.Width) / 2,
            ShellPopupAlign.End => origin.X + anchor.Width - size.Width,
            _ => origin.X
        };
        var below = origin.Y + anchor.Height + options.Offset;
        var above = origin.Y - size.Height - options.Offset;
        var fitsBelow = below + size.Height <= maxBottom;
        var fitsAbove = above >= minTop;
        var top = options.Placement == ShellPopupPlacement.Top
            ? (fitsAbove || !fitsBelow ? above : below)
            : (fitsBelow || !fitsAbove ? below : above);
        var placedAbove = top < origin.Y;

        x = Math.Max(safe.Left + edge, Math.Min(x, layer.Width - safe.Right - size.Width - edge));
        top = Math.Max(minTop, Math.Min(top, maxBottom - size.Height));
        panel.Margin = new Thickness(x, top, 0, 0);

        // Grow from the edge nearest the anchor.
        panel.AnchorY = placedAbove ? 1 : 0;
        panel.Scale = 0.95;
        await Task.WhenAll(panel.FadeToAsync(1, 120, Easing.CubicOut), panel.ScaleToAsync(1, 120, Easing.CubicOut));
    }
}

public enum ShellPopupPlacement { Bottom, Top }
public enum ShellPopupAlign { Start, Center, End }

public sealed class ShellPopupOptions
{
    public ShellPopupPlacement Placement { get; init; } = ShellPopupPlacement.Bottom;
    public ShellPopupAlign Align { get; init; } = ShellPopupAlign.Start;
    public double Offset { get; init; } = 4;
    public bool MatchAnchorWidth { get; init; }
    // Modal popups get a click-outside catcher (menus, selects); tooltips and hover cards don't.
    public bool Modal { get; init; } = true;
    public Action? OnDismiss { get; init; }
    // Logical owner for FindParentOfType / BindingContext; defaults to the anchor.
    public Element? Owner { get; init; }
}

public sealed class ShellPopupHandle
{
    private readonly View _panel;
    private readonly Func<View?> _catcher;
    private bool _closed;

    internal bool IsClosed => _closed;

    internal ShellPopupHandle(View panel, Func<View?> catcher)
    {
        _panel = panel;
        _catcher = catcher;
    }

    public async Task CloseAsync()
    {
        if (_closed) return;
        _closed = true;
        ShellDismiss.Remove(this);
        if (_catcher() is { } catcher) ShellPortal.Detach(catcher);
        _panel.AbortAnimation("FadeTo");
        _panel.AbortAnimation("ScaleTo");
        await Task.WhenAll(_panel.FadeToAsync(0, 90, Easing.CubicIn), _panel.ScaleToAsync(0.95, 90, Easing.CubicIn));
        ShellPortal.Detach(_panel);
    }
}

// Open overlays and popups, newest last. Escape (Windows) and the Android back button close the
// top one; with nothing open the key keeps its normal behavior.
public static class ShellDismiss
{
    private static readonly List<(object Key, Action Close)> _open = new();

    public static void Push(object key, Action close)
    {
        _open.RemoveAll(entry => ReferenceEquals(entry.Key, key));
        _open.Add((key, close));
        Hook();
    }

    public static void Remove(object key)
    {
        _open.RemoveAll(entry => ReferenceEquals(entry.Key, key));
        Sync();
    }

    // Closes the most recently opened overlay. False when nothing is open.
    public static bool DismissTop()
    {
        if (_open.Count == 0) return false;
        var top = _open[^1];
        _open.RemoveAt(_open.Count - 1);
        Sync();
        top.Close();
        return true;
    }

#if WINDOWS
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Microsoft.UI.Xaml.UIElement, object> _hooked = new();

    private static void Hook()
    {
        if (Application.Current is not { } app) return;
        foreach (var window in app.Windows)
        {
            if (window.Handler?.PlatformView is not Microsoft.UI.Xaml.Window { Content: { } root }) continue;
            if (_hooked.TryGetValue(root, out _)) continue;
            _hooked.Add(root, new object());
            root.PreviewKeyDown += (_, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Escape && DismissTop()) e.Handled = true;
            };
        }
    }

    private static void Sync() { }
#elif ANDROID
    // Enabled only while something is open, so back still navigates otherwise.
    private sealed class BackCallback : AndroidX.Activity.OnBackPressedCallback
    {
        public BackCallback() : base(false) { }
        public override void HandleOnBackPressed() => DismissTop();
    }

    private static BackCallback? _back;
    private static WeakReference<Android.App.Activity>? _backActivity;

    private static void Hook()
    {
        if (Platform.CurrentActivity is AndroidX.Activity.ComponentActivity activity &&
            (_back is null || _backActivity is null || !_backActivity.TryGetTarget(out var hooked) || hooked != activity))
        {
            _back?.Remove();
            _back = new BackCallback();
            _backActivity = new WeakReference<Android.App.Activity>(activity);
            activity.OnBackPressedDispatcher.AddCallback(_back);
        }
        Sync();
    }

    private static void Sync()
    {
        if (_back != null) _back.Enabled = _open.Count > 0;
    }
#else
    private static void Hook() { }
    private static void Sync() { }
#endif
}

// Floating panels with a click-outside catcher (Dropdown, Popover, Select). Only one is open at
// a time: opening another closes the previous one.
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

    public static Shadow PanelShadow() => new()
    {
        Brush = new SolidColorBrush(Colors.Black),
        Offset = new Point(0, 4),
        Radius = 12,
        Opacity = 0.12f
    };
}

// Shared host for trigger + floating content (Dropdown, Popover, HoverCard). The trigger renders
// in place; the content child is held back and floated in the page layer while open.
public abstract class ShellPopoverHost : Grid, IShellPopup
{
    public static readonly BindableProperty IsOpenProperty =
        BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(ShellPopoverHost), false,
            propertyChanged: (b, o, n) => ((ShellPopoverHost)b).OnOpenChanged());

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public event EventHandler<bool>? IsOpenChanged;

    private View? _content;
    private ShellPopupHandle? _handle;

    protected ShellPopoverHost()
    {
        HorizontalOptions = LayoutOptions.Start;
    }

    protected abstract bool IsContent(Element child);

    // Click-outside catcher + single-open behavior. Hover cards turn this off.
    protected virtual bool Modal => true;
    protected virtual ShellPopupPlacement Placement => ShellPopupPlacement.Bottom;
    protected virtual ShellPopupAlign Align => ShellPopupAlign.Start;

    protected View? PopupContent => _content;

    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);
        if (!IsContent(child) || child is not View view) return;
        _content = view;
        view.SetValue(ShellPortal.OwnerProperty, this);
        // Hold the content out of the tree until it is opened.
        Dispatcher.Dispatch(() => { if (view.Parent == this) Children.Remove(view); });
    }

    public void SetOpen(bool value) => IsOpen = value;
    public void Toggle() => IsOpen = !IsOpen;
    public void Close() => IsOpen = false;

    private void OnOpenChanged()
    {
        IsOpenChanged?.Invoke(this, IsOpen);
        if (IsOpen)
        {
            if (_content is null) return;
            if (Modal) ShellPopups.Opened(this);
            _handle = ShellPortal.ShowPopup(this, _content, new ShellPopupOptions
            {
                Placement = Placement,
                Align = Align,
                Modal = Modal,
                Owner = this,
                OnDismiss = Close
            });
        }
        else
        {
            if (Modal) ShellPopups.Closed(this);
            var handle = _handle;
            _handle = null;
            if (handle != null) _ = handle.CloseAsync();
        }
    }
}

// Content of a modal overlay (DialogContent, DrawerContent, SheetContent): animates itself in/out.
public interface IShellOverlayContent
{
    Task AnimateAsync(bool open);
}

// Shared host for Dialog / Drawer / Sheet / AlertDialog. Children: an optional *Trigger (rendered
// in place) and the *Content, which is held back and shown in the page layer while Open — so the
// host can be declared anywhere on the page, next to the button that opens it.
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

    private View? _content;
    private int _triggers;
    private int _version;

    protected ShellOverlayHost()
    {
        // With no trigger the host is an empty, invisible placeholder that takes no space.
        IsVisible = false;
        HorizontalOptions = LayoutOptions.Start;
    }

    protected abstract bool IsTrigger(Element child);

    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);
        if (IsTrigger(child))
        {
            _triggers++;
            IsVisible = true;
            return;
        }
        if (child is not View view) return;
        _content = view;
        view.SetValue(ShellPortal.OwnerProperty, this);
        Dispatcher.Dispatch(() => { if (view.Parent == this) Children.Remove(view); });
    }

    public void SetOpen(bool value) => Open = value;

    // What Escape / the Android back button does while this overlay is on top.
    protected virtual void Dismiss() => Open = false;

    private async void OnOpenChanged()
    {
        var version = ++_version;
        if (Open) ShellDismiss.Push(this, Dismiss);
        else ShellDismiss.Remove(this);
        OpenChanged?.Invoke(this, Open);
        if (_content is null) return;
        var animated = _content as IShellOverlayContent;
        if (Open)
        {
            if (!ShellPortal.Attach(this, _content)) return;
            if (animated != null) await animated.AnimateAsync(true);
        }
        else
        {
            if (animated != null) await animated.AnimateAsync(false);
            if (version == _version) ShellPortal.Detach(_content);
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
        for (var p = ShellPortal.LogicalParent(from); p != null; p = ShellPortal.LogicalParent(p))
        {
            if (p is IShellTrigger trigger)
            {
                trigger.Activate();
                return;
            }
        }
    }
}
