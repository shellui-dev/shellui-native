using System.Text.Json;
using Spectre.Console;
using ShellUI.Native.Core.Models;
using ShellUI.Native.Templates;

namespace ShellUI.Native.CLI.Services;

// Handles list, remove, and update commands
public static class ComponentManager
{
    public static void ListComponents(bool installedOnly, bool availableOnly)
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "shellui-native.json");
        ShellUINativeConfig? config = null;

        if (File.Exists(configPath))
        {
            var json = File.ReadAllText(configPath);
            config = JsonSerializer.Deserialize<ShellUINativeConfig>(json);
        }

        var installedNames = config?.InstalledComponents.Select(c => c.Name).ToHashSet() ?? new HashSet<string>();

        // Get available components (excluding non-available utility components)
        var components = ComponentRegistry.Components.Values
            .Where(c => c.IsAvailable)
            .OrderBy(c => c.Category)
            .ThenBy(c => c.Name);

        if (installedOnly)
        {
            components = components.Where(c => installedNames.Contains(c.Name)).OrderBy(c => c.Category).ThenBy(c => c.Name);
        }
        else if (availableOnly)
        {
            components = components.Where(c => !installedNames.Contains(c.Name)).OrderBy(c => c.Category).ThenBy(c => c.Name);
        }

        var table = new Table();
        table.AddColumn("Component");
        table.AddColumn("Category");
        table.AddColumn("Description");
        table.AddColumn("Status");

        foreach (var component in components)
        {
            var status = installedNames.Contains(component.Name) 
                ? "[green]installed[/]" 
                : "[dim]available[/]";

            table.AddRow(
                $"[bold]{component.Name}[/]",
                component.Category.ToString(),
                component.Description.Length > 40 
                    ? component.Description[..37] + "..." 
                    : component.Description,
                status
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[dim]Total: {components.Count()} components[/]");
    }

    public static void RemoveComponents(string[] components)
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "shellui-native.json");
        
        if (!File.Exists(configPath))
        {
            AnsiConsole.MarkupLine("[red]ShellUI Native not initialized![/]");
            return;
        }

        var json = File.ReadAllText(configPath);
        var config = JsonSerializer.Deserialize<ShellUINativeConfig>(json);
        
        if (config == null)
        {
            AnsiConsole.MarkupLine("[red]Failed to read shellui-native.json[/]");
            return;
        }

        var removedCount = 0;
        
        foreach (var componentName in components)
        {
            var metadata = ComponentRegistry.GetMetadata(componentName);
            if (metadata == null)
            {
                AnsiConsole.MarkupLine($"[yellow]Component '{componentName}' not found in registry[/]");
                continue;
            }

            var componentPath = Path.Combine(Directory.GetCurrentDirectory(), config.ComponentsPath, metadata.FilePath);
            
            if (File.Exists(componentPath))
            {
                File.Delete(componentPath);
                config.InstalledComponents.RemoveAll(c => c.Name == componentName);
                AnsiConsole.MarkupLine($"[green]✓ Removed '{componentName}'[/]");
                removedCount++;
            }
            else
            {
                AnsiConsole.MarkupLine($"[yellow]Component '{componentName}' not found in project[/]");
            }
        }

        // Save updated config
        var updatedJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(configPath, updatedJson);

        AnsiConsole.MarkupLine($"\n[dim]Removed {removedCount} component(s)[/]");
    }

    public static void UpdateComponents(string[] components, bool all)
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "shellui-native.json");
        
        if (!File.Exists(configPath))
        {
            AnsiConsole.MarkupLine("[red]ShellUI Native not initialized![/]");
            return;
        }

        var json = File.ReadAllText(configPath);
        var config = JsonSerializer.Deserialize<ShellUINativeConfig>(json);
        
        if (config == null)
        {
            AnsiConsole.MarkupLine("[red]Failed to read shellui-native.json[/]");
            return;
        }

        var projectInfo = ProjectDetector.DetectProject();

        IEnumerable<string> componentsToUpdate;
        
        if (all || components.Length == 0)
        {
            componentsToUpdate = config.InstalledComponents.Select(c => c.Name);
        }
        else
        {
            componentsToUpdate = components;
        }

        var updatedCount = 0;

        foreach (var componentName in componentsToUpdate)
        {
            var metadata = ComponentRegistry.GetMetadata(componentName);
            if (metadata == null)
            {
                AnsiConsole.MarkupLine($"[yellow]Component '{componentName}' not found in registry[/]");
                continue;
            }

            var installed = config.InstalledComponents.FirstOrDefault(c => c.Name == componentName);
            if (installed == null)
            {
                AnsiConsole.MarkupLine($"[yellow]Component '{componentName}' is not installed[/]");
                continue;
            }

            // Check if update needed
            if (installed.Version == metadata.Version && !installed.IsCustomized)
            {
                AnsiConsole.MarkupLine($"[dim]'{componentName}' is already up to date (v{metadata.Version})[/]");
                continue;
            }

            var content = ComponentRegistry.GetComponentContent(componentName);
            if (content == null) continue;

            content = content.Replace("YourProjectNamespace", projectInfo.RootNamespace);

            var componentPath = Path.Combine(Directory.GetCurrentDirectory(), config.ComponentsPath, metadata.FilePath);
            File.WriteAllText(componentPath, content);

            installed.Version = metadata.Version;
            installed.InstalledAt = DateTime.UtcNow;
            installed.IsCustomized = false;

            AnsiConsole.MarkupLine($"[green]✓ Updated '{componentName}' to v{metadata.Version}[/]");
            updatedCount++;
        }

        // Save updated config
        var updatedJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(configPath, updatedJson);

        AnsiConsole.MarkupLine($"\n[dim]Updated {updatedCount} component(s)[/]");
    }
}
