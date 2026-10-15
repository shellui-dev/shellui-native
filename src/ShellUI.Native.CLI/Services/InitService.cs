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

        var projectInfo = ProjectDetector.DetectProject();
        var platformName = projectInfo.Platform switch
        {
            NativePlatform.MAUI => ".NET MAUI",
            NativePlatform.Avalonia => "Avalonia UI",
            NativePlatform.WinUI => "WinUI 3",
            NativePlatform.WPF => "WPF",
            _ => "Unknown"
        };
        AnsiConsole.MarkupLine($"[green]✓ Detected:[/] {platformName}");
        AnsiConsole.MarkupLine($"[dim]Project: {Markup.Escape(projectInfo.ProjectName)}[/]");
        AnsiConsole.MarkupLine($"[dim]Namespace: {Markup.Escape(projectInfo.RootNamespace)}[/]");

        if (projectInfo.Platform == NativePlatform.Unknown && !nonInteractive)
        {
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Could not auto-detect platform. Please select:[/]")
                    .AddChoices(new[] { "MAUI", "Avalonia", "WinUI", "WPF" }));

            projectInfo.Platform = selection switch
            {
                "MAUI" => NativePlatform.MAUI,
                "Avalonia" => NativePlatform.Avalonia,
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

        // ShellUI (Blazor) already uses Components/UI and <App>.Components.UI; its Button.razor and our
        // Button.cs would be the same class.
        var besideBlazor = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "shellui.json"));
        var componentsFolder = besideBlazor ? TemplateText.BesideBlazorComponentsPath : TemplateText.DefaultComponentsPath;
        var componentsNamespace = TemplateText.ComponentsNamespace(projectInfo.RootNamespace, componentsFolder);
        if (besideBlazor)
            AnsiConsole.MarkupLine($"[green]✓ Found ShellUI (Blazor):[/] native components go to {componentsFolder}/ ({Markup.Escape(componentsNamespace)}) so the two sets don't clash");

        await LogoLoader.RunAsync("Setting up ShellUI Native...", async status =>
            {
                status("Creating component folders...");
                var componentsPath = Path.Combine(Directory.GetCurrentDirectory(), componentsFolder);
                var variantsPath = Path.Combine(componentsPath, "Variants");
                Directory.CreateDirectory(componentsPath);
                Directory.CreateDirectory(variantsPath);
                AnsiConsole.MarkupLine($"[green]✓ Created:[/] {componentsFolder}/");

                // Install Shell utilities
                status("Installing Shell utilities...");
                var shellInstalled = await InstallShellUtilityAsync(projectInfo, componentsPath, componentsFolder);

                // Create configuration file
                status("Creating configuration...");
                var config = new ShellUINativeConfig
                {
                    Style = style,
                    ComponentsPath = componentsFolder,
                    TargetPlatform = projectInfo.Platform,
                    Theme = new ThemeConfig
                    {
                        Enabled = true,
                        Style = "default",
                        SyncWithShellUI = true
                    }
                };

                // Recorded like any component, so `update shell` and `update --all` include it.
                if (shellInstalled)
                    config.InstalledComponents.Add(new InstalledComponent
                    {
                        Name = "shell",
                        Version = ComponentRegistry.GetMetadata("shell")!.Version,
                        Platform = projectInfo.Platform
                    });

                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(configPath, json);
                AnsiConsole.MarkupLine($"[green]✓ Created:[/] shellui-native.json");

                // Create theme resources (MAUI only for now)
                if (projectInfo.Platform == NativePlatform.MAUI)
                {
                    status("Creating theme resources...");
                    await CreateThemeResourcesAsync();
                }
            });

        AnsiConsole.MarkupLine("\n[green]✓ ShellUI Native initialized successfully![/]");
        AnsiConsole.MarkupLine("\n[blue]Next steps:[/]");
        if (projectInfo.Platform == NativePlatform.Avalonia)
        {
            AnsiConsole.MarkupLine($"  [dim]1. Load the theme in App.Initialize, after AvaloniaXamlLoader.Load(this):[/] {Markup.Escape(componentsNamespace)}.ShellTheme.EnsureInitialized();");
            AnsiConsole.MarkupLine($"  [dim]2. Use the components in XAML:[/] xmlns:ui=\"using:{Markup.Escape(componentsNamespace)}\"");
            AnsiConsole.MarkupLine("  [dim]3. Add components:[/] shellui-native add theme-toggle");
        }
        else
        {
            AnsiConsole.MarkupLine("  [dim]1. Add components:[/] shellui-native add button");
            AnsiConsole.MarkupLine("  [dim]2. Browse all:[/] shellui-native list");
        }
    }

    private static async Task<bool> InstallShellUtilityAsync(ProjectInfo projectInfo, string componentsPath, string componentsFolder)
    {
        if (!ComponentRegistry.SupportsPlatform("shell", projectInfo.Platform))
        {
            AnsiConsole.MarkupLine(
                $"[yellow]Shell utility has no template for {projectInfo.Platform} yet — skipping.[/] " +
                $"[dim]Add components once per-platform templates land.[/]");
            return false;
        }

        var content = ComponentRegistry.GetComponentContent("shell", projectInfo.Platform);
        if (content == null) return false;

        content = TemplateText.Apply(content, projectInfo.RootNamespace, componentsFolder);

        var filePath = Path.Combine(componentsPath, "Shell.cs");
        await File.WriteAllTextAsync(filePath, content);
        AnsiConsole.MarkupLine($"[green]✓ Installed:[/] Shell.cs");
        return true;
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
