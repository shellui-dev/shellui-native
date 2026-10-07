using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;

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
