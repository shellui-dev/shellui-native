using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Tests;

public class UnsupportedPlatformExceptionTests
{
    [Fact]
    public void Message_lists_supported_platforms()
    {
        var ex = new UnsupportedPlatformException(
            componentName: "button",
            requestedPlatform: NativePlatform.Avalonia,
            supportedPlatforms: new HashSet<NativePlatform> { NativePlatform.MAUI });

        Assert.Contains("button", ex.Message);
        Assert.Contains("Avalonia", ex.Message);
        Assert.Contains("MAUI", ex.Message);
    }

    [Fact]
    public void Message_handles_empty_supported_set()
    {
        var ex = new UnsupportedPlatformException(
            componentName: "hypothetical",
            requestedPlatform: NativePlatform.WPF,
            supportedPlatforms: new HashSet<NativePlatform>());

        Assert.Contains("hypothetical", ex.Message);
        Assert.Contains("(none)", ex.Message);
    }

    [Fact]
    public void Properties_expose_original_arguments()
    {
        var supported = new HashSet<NativePlatform> { NativePlatform.MAUI, NativePlatform.Avalonia };
        var ex = new UnsupportedPlatformException("dialog", NativePlatform.WinUI, supported);

        Assert.Equal("dialog", ex.ComponentName);
        Assert.Equal(NativePlatform.WinUI, ex.RequestedPlatform);
        Assert.Equal(supported, ex.SupportedPlatforms);
    }
}
