# Quick Start Guide

Get started with ShellUI Native in under 5 minutes.

## Prerequisites

- .NET 10.0 SDK (see [global.json](../global.json))
- A .NET MAUI project (`dotnet new maui`); Avalonia support is the next phase (see [DEVELOPMENT_PLAN.md](./DEVELOPMENT_PLAN.md))

## Installation

### 1. Install the CLI Tool

ShellUI Native is in prerelease, so include `--prerelease`:

```bash
dotnet tool install -g ShellUI.Native.CLI --prerelease
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

> Templates exist for MAUI only so far. In an Avalonia, WinUI or WPF project, `init` skips the
> `Shell.cs` utility and `add` reports that the component has no template for that platform.

### 4. Add Components

```bash
# Add a single component
shellui-native add button

# Add multiple components
shellui-native add button input card

# Add with comma separation
shellui-native add button,input,card
```

### 5. Load the Theme

Publish the theme tokens before the first page loads, in `App.xaml.cs`:

```csharp
public App()
{
    InitializeComponent();
    Components.UI.ShellTheme.EnsureInitialized();
}
```

Components also do this on first use, but calling it early lets your own
`{DynamicResource ShellUI*}` references resolve on the first page. See
[COMPONENTS.md § Theming](./COMPONENTS.md#theming) for switching and customizing the theme.

### 6. Use Components in Your App

```xml
<!-- Add namespace to your XAML -->
<ContentPage xmlns:ui="clr-namespace:YourProject.Components.UI"
             BackgroundColor="{DynamicResource ShellUIBackground}">

    <!-- Use the Button component -->
    <ui:Button Text="Click me!"
               Size="Lg"
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
