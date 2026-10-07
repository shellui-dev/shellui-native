![ShellUI Native](https://raw.githubusercontent.com/shellui-dev/shellui-native/main/assets/icon.svg)

# ShellUI Native CLI

Command-line interface for ShellUI Native: shadcn-style, copy-and-own components for .NET MAUI, with the same design tokens as [ShellUI](https://shellui.dev/) for Blazor.

## Installation

```bash
dotnet tool install -g ShellUI.Native.CLI --prerelease
```

Requires the .NET 10 SDK.

## Commands

```bash
shellui-native init [--yes] [--force]     # Initialize project
shellui-native add <components>           # Add components
shellui-native list [--installed]         # List components
shellui-native remove <components>        # Remove components
shellui-native update [--all]             # Update components
```

## Quick Start

```bash
# Navigate to your MAUI project
cd YourProject

# Initialize ShellUI Native
shellui-native init --yes

# Add components
shellui-native add button input card
```

Components are written to `Components/UI/` as plain C# files you own. Avalonia support is the next phase.

## More Information

- [Component reference](https://github.com/shellui-dev/shellui-native/blob/main/docs/COMPONENTS.md)
- [Release notes](https://github.com/shellui-dev/shellui-native/blob/main/docs/RELEASE_NOTES.md)
- Docs site: [native.shellui.dev](https://native.shellui.dev) (coming soon)

## The Shell family

| | Project | |
|---|---|---|
| ![](https://raw.githubusercontent.com/shellui-dev/shellui-native/main/assets/brand/shellui.svg) | [ShellUI](https://www.nuget.org/packages/ShellUI.CLI) | The Blazor component library this one mirrors |
| ![](https://raw.githubusercontent.com/shellui-dev/shellui-native/main/assets/brand/shellicons.svg) | [ShellIcons](https://www.nuget.org/packages/ShellIcons.Blazor) | Lucide icons for Blazor, MAUI and Avalonia |
| ![](https://raw.githubusercontent.com/shellui-dev/shellui-native/main/assets/brand/shelldocs.svg) | [ShellDocs](https://www.nuget.org/packages/ShellDocs.CLI) | The docs framework behind the ShellUI sites |
