using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Tests;

// Sanity checks on the platform enum — mostly documenting invariants that other
// switch statements and detection code depend on.
public class NativePlatformTests
{
    [Fact]
    public void Unknown_is_the_default_value()
    {
        // If this changes, InitService's "default to MAUI in non-interactive mode"
        // branch and every switch expression that uses `_ => Unknown` breaks silently.
        Assert.Equal(NativePlatform.Unknown, default(NativePlatform));
    }

    [Fact]
    public void Contains_all_four_active_platforms()
    {
        var members = Enum.GetValues<NativePlatform>().ToHashSet();
        Assert.Contains(NativePlatform.Unknown, members);
        Assert.Contains(NativePlatform.MAUI, members);
        Assert.Contains(NativePlatform.Avalonia, members);
        Assert.Contains(NativePlatform.WinUI, members);
        Assert.Contains(NativePlatform.WPF, members);
    }
}
