using System.CommandLine;
using Spectre.Console;
using ShellUI.Native.Templates;
using ShellUI.Native.Core.Models;
using ShellUI.Native.CLI.Services;

namespace ShellUI.Native.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // The loaders draw Unicode dots and Braille.
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var rootCommand = new RootCommand("ShellUI Native - CLI-first cross-platform component library")
        {
            Description = "Add beautiful, accessible components to your MAUI or Avalonia app. Inspired by shadcn/ui."
        };

        rootCommand.AddCommand(CreateInitCommand());
        rootCommand.AddCommand(CreateAddCommand());
        rootCommand.AddCommand(CreateListCommand());
        rootCommand.AddCommand(CreateRemoveCommand());
        rootCommand.AddCommand(CreateUpdateCommand());

        return await rootCommand.InvokeAsync(args);
    }

    static Command CreateInitCommand()
    {
        var command = new Command("init", "Initialize ShellUI Native in your project");

        var forceOption = new Option<bool>("--force", "Reinitialize even if already initialized");
        var styleOption = new Option<string>("--style", () => "default", "Choose component style (default, minimal)");
        var nonInteractiveOption = new Option<bool>("--yes", "Run in non-interactive mode with default options");

        command.AddOption(forceOption);
        command.AddOption(styleOption);
        command.AddOption(nonInteractiveOption);

        command.SetHandler(async (force, style, nonInteractive) =>
        {
            try
            {
                LogoLoader.WriteHeader("Setting up your MAUI project");
                await InitService.InitializeAsync(style, force, nonInteractive);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message.Replace("[", "[[").Replace("]", "]]")}");
            }
        }, forceOption, styleOption, nonInteractiveOption);

        return command;
    }

    static Command CreateAddCommand()
    {
        var command = new Command("add", "Add component(s) to your project");

        var componentsArg = new Argument<string[]>("components", "Component name(s) to add (space or comma-separated)")
        {
            Arity = ArgumentArity.OneOrMore
        };
        command.AddArgument(componentsArg);

        var forceOption = new Option<bool>("--force", "Overwrite existing components");
        command.AddOption(forceOption);

        command.SetHandler(async (components, force) =>
        {
            try
            {
                await ComponentInstaller.InstallComponents(components, force);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message.Replace("[", "[[").Replace("]", "]]")}");
            }
        }, componentsArg, forceOption);

        return command;
    }

    static Command CreateListCommand()
    {
        var command = new Command("list", "List available components");

        var installedOption = new Option<bool>("--installed", "Show only installed components");
        var availableOption = new Option<bool>("--available", "Show only available components");

        command.AddOption(installedOption);
        command.AddOption(availableOption);

        command.SetHandler((installed, available) =>
        {
            try
            {
                ComponentManager.ListComponents(installed, available);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message.Replace("[", "[[").Replace("]", "]]")}");
            }
        }, installedOption, availableOption);

        return command;
    }

    static Command CreateRemoveCommand()
    {
        var command = new Command("remove", "Remove component(s) from your project");

        var componentsArg = new Argument<string[]>("components", "Component name(s) to remove")
        {
            Arity = ArgumentArity.OneOrMore
        };
        command.AddArgument(componentsArg);

        command.SetHandler((components) =>
        {
            try
            {
                ComponentManager.RemoveComponents(components);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message.Replace("[", "[[").Replace("]", "]]")}");
            }
        }, componentsArg);

        return command;
    }

    static Command CreateUpdateCommand()
    {
        var command = new Command("update", "Update component(s) to latest version");

        var componentsArg = new Argument<string[]>("components", "Component name(s) to update (empty = all)")
        {
            Arity = ArgumentArity.ZeroOrMore
        };
        command.AddArgument(componentsArg);

        var allOption = new Option<bool>("--all", "Update all installed components");
        command.AddOption(allOption);

        command.SetHandler((components, all) =>
        {
            try
            {
                ComponentManager.UpdateComponents(components, all);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message.Replace("[", "[[").Replace("]", "]]")}");
            }
        }, componentsArg, allOption);

        return command;
    }
}
