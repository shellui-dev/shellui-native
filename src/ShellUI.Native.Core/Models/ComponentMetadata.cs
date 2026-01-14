using System.Text.RegularExpressions;

namespace ShellUI.Native.Core.Models;

// Metadata describing a component template
public class ComponentMetadata
{
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public required string Description { get; set; }
    public required ComponentCategory Category { get; set; }
    
    // Relative path from Components/UI folder (e.g., "Button.cs" or "Variants/ButtonVariants.cs")
    public required string FilePath { get; set; }
    
    // Other components that must be installed alongside this one
    public List<string> Dependencies { get; set; } = new();
    
    // Whether this component appears in the CLI list command
    public bool IsAvailable { get; set; } = true;
    
    // Version computed from Directory.Build.props
    public string Version => GetCurrentVersion();
    
    // Visual variants supported by this component
    public List<string> Variants { get; set; } = new();
    
    // Searchable tags for the component
    public List<string> Tags { get; set; } = new();

    private static string GetCurrentVersion()
    {
        // Try to read version from Directory.Build.props
        try
        {
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            var dir = new DirectoryInfo(currentDir);

            while (dir != null)
            {
                var propsFile = Path.Combine(dir.FullName, "Directory.Build.props");
                if (File.Exists(propsFile))
                {
                    var content = File.ReadAllText(propsFile);
                    var match = Regex.Match(content, @"<ShellUINativeVersion>([^<]+)</ShellUINativeVersion>");
                    if (match.Success)
                    {
                        var version = match.Groups[1].Value.Trim();
                        var suffixMatch = Regex.Match(content, @"<ShellUINativeVersionSuffix>([^<]*)</ShellUINativeVersionSuffix>");
                        if (suffixMatch.Success && !string.IsNullOrEmpty(suffixMatch.Groups[1].Value.Trim()))
                        {
                            version += "-" + suffixMatch.Groups[1].Value.Trim();
                        }
                        return version;
                    }
                }
                dir = dir.Parent;
            }
        }
        catch
        {
            // Ignore errors, return fallback
        }

        return "0.1.0";
    }
}
