using System.Text.Json;
using Spectre.Console;
using ShellUI.Native.Core.Models;
using ShellUI.Native.Templates;

namespace ShellUI.Native.CLI.Services;

// Handles the 'add' command - installs components to the project
public static class ComponentInstaller
{
    public static async Task InstallComponents(string[] components, bool force)
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "shellui-native.json");
        
        if (!File.Exists(configPath))
        {
            AnsiConsole.MarkupLine("[red]ShellUI Native not initialized![/]");
            AnsiConsole.MarkupLine("[yellow]Run 'shellui-native init' first[/]");
            return;
        }

        var configJson = await File.ReadAllTextAsync(configPath);
        var config = JsonSerializer.Deserialize<ShellUINativeConfig>(configJson);
        
        if (config == null)
        {
            AnsiConsole.MarkupLine("[red]Failed to read shellui-native.json[/]");
            return;
        }

        var projectInfo = ProjectDetector.DetectProject();

        // Parse comma-separated components
        var componentList = new List<string>();
        foreach (var comp in components)
        {
            componentList.AddRange(comp.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }

        var successCount = 0;
        var skippedCount = 0;
        var failedComponents = new List<string>();
        var installedSet = new HashSet<string>();
        
        // Show dependency information
        foreach (var componentName in componentList)
        {
            var metadata = ComponentRegistry.GetMetadata(componentName);
            if (metadata != null && metadata.Dependencies.Count > 0)
            {
                AnsiConsole.MarkupLine($"[green]●[/] [bold]{componentName}[/] requires: [yellow]{string.Join(", ", metadata.Dependencies)}[/]");
            }
        }
        
        AnsiConsole.MarkupLine("");
        
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("green"))
            .StartAsync("Installing components...", async ctx =>
            {
                foreach (var componentName in componentList)
                {
                    ctx.Status($"Installing {componentName}...");
                    await InstallComponentWithDependenciesAsync(
                        componentName, config, projectInfo, force, 
                        installedSet, successCount, skippedCount, failedComponents);
                }
            });

        // Update config
        var updatedJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(configPath, updatedJson);

        // Summary
        AnsiConsole.MarkupLine("");
        if (successCount > 0)
            AnsiConsole.MarkupLine($"[green]Installed {successCount} component(s) successfully![/]");
        if (skippedCount > 0)
            AnsiConsole.MarkupLine($"[yellow]Skipped {skippedCount} component(s) (already exists, use --force to overwrite)[/]");
        if (failedComponents.Count > 0)
            AnsiConsole.MarkupLine($"[red]Failed: {string.Join(", ", failedComponents)}[/]");
    }

    private static async Task InstallComponentWithDependenciesAsync(
        string componentName, 
        ShellUINativeConfig config, 
        ProjectInfo projectInfo, 
        bool force,
        HashSet<string> installedSet,
        int successCount,
        int skippedCount,
        List<string> failedComponents)
    {
        if (installedSet.Contains(componentName))
            return;
        
        if (!ComponentRegistry.Exists(componentName))
        {
            AnsiConsole.MarkupLine($"[red]Component '{componentName}' not found[/]");
            failedComponents.Add(componentName);
            return;
        }

        var metadata = ComponentRegistry.GetMetadata(componentName);
        if (metadata == null)
        {
            AnsiConsole.MarkupLine($"[red]Failed to get metadata for '{componentName}'[/]");
            failedComponents.Add(componentName);
            return;
        }

        // Install dependencies first
        if (metadata.Dependencies.Count > 0)
        {
            AnsiConsole.MarkupLine($"[dim]Installing dependencies for [bold]{componentName}[/]: {string.Join(", ", metadata.Dependencies)}[/]");
            foreach (var dep in metadata.Dependencies)
            {
                if (!installedSet.Contains(dep))
                {
                    await InstallComponentWithDependenciesAsync(dep, config, projectInfo, force, installedSet, successCount, skippedCount, failedComponents);
                }
            }
        }

        // Install the component
        var result = await InstallComponentInternalAsync(componentName, metadata, config, projectInfo, force);
        
        if (result == InstallResult.Success)
        {
            successCount++;
            installedSet.Add(componentName);
        }
        else if (result == InstallResult.Skipped)
        {
            skippedCount++;
            installedSet.Add(componentName);
        }
        else
        {
            failedComponents.Add(componentName);
        }
    }

    private static async Task<InstallResult> InstallComponentInternalAsync(
        string componentName,
        ComponentMetadata metadata,
        ShellUINativeConfig config,
        ProjectInfo projectInfo,
        bool force)
    {
        var componentPath = Path.Combine(Directory.GetCurrentDirectory(), config.ComponentsPath, metadata.FilePath);
        
        if (File.Exists(componentPath) && !force)
        {
            AnsiConsole.MarkupLine($"[yellow]Skipped '{componentName}' (already exists)[/]");
            return InstallResult.Skipped;
        }

        var content = ComponentRegistry.GetComponentContent(componentName);
        if (content == null)
        {
            AnsiConsole.MarkupLine($"[red]Failed to get content for '{componentName}'[/]");
            return InstallResult.Failed;
        }

        // Replace namespace placeholder
        content = content.Replace("YourProjectNamespace", projectInfo.RootNamespace);

        // Ensure directory exists
        var directory = Path.GetDirectoryName(componentPath);
        if (directory != null)
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(componentPath, content);

        // Update config
        var existing = config.InstalledComponents.FirstOrDefault(c => c.Name == componentName);
        if (existing != null)
        {
            existing.Version = metadata.Version;
            existing.InstalledAt = DateTime.UtcNow;
            existing.IsCustomized = false;
        }
        else
        {
            config.InstalledComponents.Add(new InstalledComponent
            {
                Name = componentName,
                Version = metadata.Version,
                Platform = config.TargetPlatform,
                InstalledAt = DateTime.UtcNow,
                IsCustomized = false
            });
        }

        AnsiConsole.MarkupLine($"[green]✓ Installed '{componentName}'[/] [dim]({metadata.FilePath})[/]");
        return InstallResult.Success;
    }

    private enum InstallResult
    {
        Success,
        Skipped,
        Failed
    }
}
