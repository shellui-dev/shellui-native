<picture>
  <source media="(prefers-color-scheme: dark)" srcset="https://raw.githubusercontent.com/shellui-dev/shellui-native/main/assets/icon-dark.png" />
  <img src="https://raw.githubusercontent.com/shellui-dev/shellui-native/main/assets/icon.png" alt="" width="64" />
</picture>

# ShellUI Native CLI

Command-line interface for ShellUI Native component library.

## Installation

```bash
dotnet tool install -g ShellUI.Native.CLI
```

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
# Navigate to your MAUI/WinUI/WPF project
cd YourProject

# Initialize ShellUI Native
shellui-native init --yes

# Add components
shellui-native add button input card
```

## More Information

See the full documentation at [native.shellui.dev](https://native.shellui.dev)
