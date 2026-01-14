namespace MAUI.Demo.Components.UI;

// ShellUI Native utility class
public static class Shell
{
    // Combines multiple class names, filtering out null/empty values
    public static string Cn(params string?[] classes)
        => string.Join(" ", classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
