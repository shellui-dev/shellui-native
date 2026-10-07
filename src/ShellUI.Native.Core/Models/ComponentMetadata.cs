using System.Reflection;

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

    // Directory.Build.props stamps the package version into the assembly; an installed tool has no props file to read.
    private static string GetCurrentVersion()
    {
        var informational = typeof(ComponentMetadata).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        return string.IsNullOrEmpty(informational) ? "0.0.0" : informational.Split('+')[0];
    }
}
