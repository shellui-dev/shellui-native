namespace ShellUI.Native.Core.Models;

// Supported native platforms for ShellUI Native components
public enum NativePlatform
{
    Unknown,
    MAUI,      // .NET MAUI (iOS, Android, Windows, macOS)
    WinUI,     // WinUI 3 (Windows 11+)
    WPF        // Windows Presentation Foundation (.NET 8+)
}
