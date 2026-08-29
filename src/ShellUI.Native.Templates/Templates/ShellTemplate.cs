using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Shell utility class template - provides helper methods for components
public static class ShellTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "shell",
        DisplayName = "Shell Utilities",
        Description = "Core utility methods for ShellUI Native components",
        Category = ComponentCategory.Utility,
        FilePath = "Shell.cs",
        IsAvailable = false, // Auto-installed during init, not shown in list
        Tags = new List<string> { "utility", "core", "helper" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// ShellUI Native utility class
public static class Shell
{
    // Combines multiple class names, filtering out null/empty values
    public static string Cn(params string?[] classes)
        => string.Join("" "", classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
"
    };
}
