# Quick Start Guide

Get started with ShellUI Native in under 5 minutes.

## Prerequisites

- .NET 10.0 SDK (see [global.json](../global.json))
- A MAUI project today; Avalonia support is landing in Phase 2 (see [DEVELOPMENT_PLAN.md](./DEVELOPMENT_PLAN.md))

## Installation

### 1. Install the CLI Tool

```bash
dotnet tool install -g ShellUI.Native.CLI
```

### 2. Navigate to Your Project

```bash
cd YourProject
```

### 3. Initialize ShellUI Native

```bash
shellui-native init --yes
```

This will:
- Detect your project type (MAUI, Avalonia, WinUI, or WPF — see [ARCHITECTURE.md § Platform Detection](./ARCHITECTURE.md#platform-detection))
- Create the `Components/UI/` folder
- Install the `Shell.cs` utility
- Create `shellui-native.json` configuration
- Generate theme resources (MAUI)

> If the CLI detects Avalonia/WinUI/WPF, `shellui-native add` will print a warning and still
> install MAUI-flavored code, since per-platform templates are the Phase 1b / Phase 2 workstream.

### 4. Add Components

```bash
# Add a single component
shellui-native add button

# Add multiple components
shellui-native add button input card

# Add with comma separation
shellui-native add button,input,card
```

### 5. Use Components in Your App

```xml
<!-- Add namespace to your XAML -->
<ContentPage xmlns:ui="clr-namespace:YourProject.Components.UI">
    
    <!-- Use the Button component -->
    <ui:Button Variant="Primary" 
               Size="Lg" 
               Text="Click me!" 
               Clicked="OnButtonClicked" />
    
</ContentPage>
```

## Common Commands

| Command | Description |
|---------|-------------|
| `shellui-native init` | Initialize project |
| `shellui-native add <name>` | Add component(s) |
| `shellui-native list` | List all components |
| `shellui-native list --installed` | List installed components |
| `shellui-native remove <name>` | Remove component(s) |
| `shellui-native update --all` | Update all components |

## Project Structure

After initialization, your project will have:

```
YourProject/
├── Components/
│   └── UI/
│       ├── Button.cs           # Added components
│       ├── Variants/
│       │   └── ButtonVariants.cs
│       └── Shell.cs            # Utility class
├── Resources/
│   └── Styles/
│       └── ShellUITheme.xaml   # Theme resources (MAUI)
└── shellui-native.json         # Configuration
```

## Next Steps

- Browse all components: `shellui-native list`
- Read the [Architecture](./ARCHITECTURE.md) guide
- Check the [Component documentation](./COMPONENTS.md)
