using System;
using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaDemo.Components.UI;

[assembly: SupportedOSPlatform("browser")]

namespace AvaloniaPreview;

internal static partial class Program
{
    // The preview to show, from ?c=button.
    public static string Component { get; private set; } = "";

    // From ?theme=dark or ?theme=light; anything else follows the browser.
    public static ThemeVariant Theme { get; private set; } = ThemeVariant.Default;

    // main.js passes the page's address as the first argument.
    private static Task Main(string[] args)
    {
        var query = args.Length > 0 && Uri.TryCreate(args[0], UriKind.Absolute, out var uri) ? uri.Query : "";
        Component = Read(query, "c");
        Theme = ParseTheme(Read(query, "theme"));

        // Without ICU data the only culture is the invariant one, whose currency sign is a
        // placeholder. Money in the previews reads as dollars instead.
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat.CurrencySymbol = "$";
        CultureInfo.CurrentCulture = CultureInfo.DefaultThreadCurrentCulture = culture;
        return BuildAvaloniaApp().WithInterFont().StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>();

    // Called by main.js when the page hosting the frame changes its theme.
    [JSExport]
    internal static void SetTheme(string theme)
    {
        Theme = ParseTheme(theme);
        if (Application.Current is null) return;
        Dispatcher.UIThread.Post(() => ShellTheme.SetTheme(Theme));
    }

    // The height the preview asks for, with the frame's padding, so a page can size its frame.
    [JSExport]
    internal static double ContentHeight() =>
        App.Host?.Child is { } preview ? Math.Ceiling(preview.DesiredSize.Height + App.Host.Padding.Top + App.Host.Padding.Bottom) : 0;

    private static ThemeVariant ParseTheme(string theme) => theme switch
    {
        "dark" => ThemeVariant.Dark,
        "light" => ThemeVariant.Light,
        _ => ThemeVariant.Default,
    };

    private static string Read(string query, string key)
    {
        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts[0] == key) return parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";
        }
        return "";
    }
}
