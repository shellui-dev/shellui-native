namespace ShellUI.Native.Core.Models;

// Thrown when a component template is requested for a platform it does not implement yet.
// e.g. `shellui-native add button` in an Avalonia project before Phase 2 templates exist.
public class UnsupportedPlatformException : Exception
{
    public string ComponentName { get; }
    public NativePlatform RequestedPlatform { get; }
    public IReadOnlySet<NativePlatform> SupportedPlatforms { get; }

    public UnsupportedPlatformException(
        string componentName,
        NativePlatform requestedPlatform,
        IReadOnlySet<NativePlatform> supportedPlatforms)
        : base(BuildMessage(componentName, requestedPlatform, supportedPlatforms))
    {
        ComponentName = componentName;
        RequestedPlatform = requestedPlatform;
        SupportedPlatforms = supportedPlatforms;
    }

    private static string BuildMessage(string name, NativePlatform requested, IReadOnlySet<NativePlatform> supported)
    {
        var supportedList = supported.Count == 0 ? "(none)" : string.Join(", ", supported);
        return $"Component '{name}' does not have a template for {requested}. " +
               $"Supported platforms: {supportedList}.";
    }
}
