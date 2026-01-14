using System.Text.Json;
using Spectre.Console;
using ShellUI.Native.Core.Models;
using ShellUI.Native.Templates;

namespace ShellUI.Native.CLI.Services;

// Handles the 'init' command - initializes ShellUI Native in a project
public static class InitService
{
    public static async Task InitializeAsync(string style, bool force, bool nonInteractive = false)
    {
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "shellui-native.json");

        if (File.Exists(configPath) && !force)
        {
            AnsiConsole.MarkupLine("[yellow]ShellUI Native is already initialized in this project.[/]");
            AnsiConsole.MarkupLine("[dim]Use --force to reinitialize[/]");
            return;
        }

        ProjectInfo projectInfo = null!;

        try
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("green"))
                .StartAsync("Initializing ShellUI Native...", async ctx =>
                {
                    ctx.Status("Detecting project type...");
                    await Task.Delay(300);
                    projectInfo = ProjectDetector.DetectProject();
                    
                    var platformName = projectInfo.Platform switch
                    {
                        NativePlatform.MAUI => ".NET MAUI",
                        NativePlatform.WinUI => "WinUI 3",
                        NativePlatform.WPF => "WPF",
                        _ => "Unknown"
                    };
                    
                    AnsiConsole.MarkupLine($"[green]✓ Detected:[/] {platformName}");
                    AnsiConsole.MarkupLine($"[dim]Project: {projectInfo.ProjectName}[/]");
                    AnsiConsole.MarkupLine($"[dim]Namespace: {projectInfo.RootNamespace}[/]");
                });
        }
        catch
        {
            projectInfo = ProjectDetector.DetectProject();
            AnsiConsole.MarkupLine($"[green]✓ Detected:[/] {projectInfo.Platform}");
            AnsiConsole.MarkupLine($"[dim]Project: {projectInfo.ProjectName}[/]");
            AnsiConsole.MarkupLine($"[dim]Namespace: {projectInfo.RootNamespace}[/]");
        }

        if (projectInfo.Platform == NativePlatform.Unknown && !nonInteractive)
        {
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Could not auto-detect platform. Please select:[/]")
                    .AddChoices(new[] { "MAUI", "WinUI", "WPF" }));

            projectInfo.Platform = selection switch
            {
                "MAUI" => NativePlatform.MAUI,
                "WinUI" => NativePlatform.WinUI,
                "WPF" => NativePlatform.WPF,
                _ => NativePlatform.MAUI
            };
        }
        else if (projectInfo.Platform == NativePlatform.Unknown)
        {
            // Default to MAUI in non-interactive mode
            projectInfo.Platform = NativePlatform.MAUI;
            AnsiConsole.MarkupLine("[yellow]Platform not detected, defaulting to MAUI[/]");
        }

        await AnsiConsole.Status()
            .StartAsync("Setting up ShellUI Native...", async ctx =>
            {
                // Create Components/UI folder
                ctx.Status("Creating component folders...");
                var componentsPath = Path.Combine(Directory.GetCurrentDirectory(), "Components", "UI");
                var variantsPath = Path.Combine(componentsPath, "Variants");
                Directory.CreateDirectory(componentsPath);
                Directory.CreateDirectory(variantsPath);
                AnsiConsole.MarkupLine($"[green]✓ Created:[/] Components/UI/");

                // Install Shell utilities
                ctx.Status("Installing Shell utilities...");
                await InstallShellUtilityAsync(projectInfo, componentsPath);

                // Create configuration file
                ctx.Status("Creating configuration...");
                var config = new ShellUINativeConfig
                {
                    Style = style,
                    ComponentsPath = "Components/UI",
                    TargetPlatform = projectInfo.Platform,
                    Theme = new ThemeConfig
                    {
                        Enabled = true,
                        Style = "default",
                        SyncWithShellUI = true
                    }
                };

                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(configPath, json);
                AnsiConsole.MarkupLine($"[green]✓ Created:[/] shellui-native.json");

                // Create theme resources (MAUI only for now)
                if (projectInfo.Platform == NativePlatform.MAUI)
                {
                    ctx.Status("Creating theme resources...");
                    await CreateThemeResourcesAsync();
                }
            });

        AnsiConsole.MarkupLine("\n[green]✓ ShellUI Native initialized successfully![/]");
        AnsiConsole.MarkupLine("\n[blue]Next steps:[/]");
        AnsiConsole.MarkupLine("  [dim]1. Add components:[/] shellui-native add button");
        AnsiConsole.MarkupLine("  [dim]2. Browse all:[/] shellui-native list");
    }

    private static async Task InstallShellUtilityAsync(ProjectInfo projectInfo, string componentsPath)
    {
        var content = ComponentRegistry.GetComponentContent("shell");
        if (content == null) return;

        content = content.Replace("YourProjectNamespace", projectInfo.RootNamespace);
        
        var filePath = Path.Combine(componentsPath, "Shell.cs");
        await File.WriteAllTextAsync(filePath, content);
        AnsiConsole.MarkupLine($"[green]✓ Installed:[/] Shell.cs");
    }

    private static async Task CreateThemeResourcesAsync()
    {
        var resourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Styles");
        Directory.CreateDirectory(resourcesPath);

        var themePath = Path.Combine(resourcesPath, "ShellUITheme.xaml");
        await File.WriteAllTextAsync(themePath, StyleTemplates.ThemeResourceDictionary);
        AnsiConsole.MarkupLine($"[green]✓ Created:[/] Resources/Styles/ShellUITheme.xaml");
        AnsiConsole.MarkupLine("[dim]Add this resource dictionary to your App.xaml MergedDictionaries[/]");
    }
}
