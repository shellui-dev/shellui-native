namespace ShellUI.Native.Core.Models;

// Configuration stored in shellui-native.json
public class ShellUINativeConfig
{
    public string Schema { get; set; } = "https://native.shellui.dev/schema.json";
    public string Style { get; set; } = "default";
    public string ComponentsPath { get; set; } = "Components/UI";
    public NativePlatform TargetPlatform { get; set; } = NativePlatform.Unknown;
    public ThemeConfig Theme { get; set; } = new();
    public List<InstalledComponent> InstalledComponents { get; set; } = new();
}

// Theme configuration settings
public class ThemeConfig
{
    public bool Enabled { get; set; } = true;
    public string Style { get; set; } = "default";
    
    // Whether to sync design tokens with ShellUI Blazor
    public bool SyncWithShellUI { get; set; } = true;
}

// Tracks an installed component in the config
public class InstalledComponent
{
    public required string Name { get; set; }
    public required string Version { get; set; }
    public NativePlatform Platform { get; set; } = NativePlatform.MAUI;
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
    public bool IsCustomized { get; set; }
}
