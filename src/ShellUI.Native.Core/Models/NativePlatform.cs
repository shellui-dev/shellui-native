namespace ShellUI.Native.Core.Models;

// Supported native platforms for ShellUI Native components
public enum NativePlatform
{
    Unknown,
    MAUI,      // .NET MAUI (iOS, Android, Windows, macOS) - primary target
    Avalonia,  // Avalonia UI (Windows, macOS, Linux, iOS, Android, WASM) - primary cross-desktop target
    WinUI,     // WinUI 3 (Windows 11+) - only pursued if Fluent-native Windows apps are required
    WPF        // Windows Presentation Foundation (.NET 8+) - detection only, not an active build target
}
