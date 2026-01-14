<div align="center">

```
███████╗██╗  ██╗███████╗██╗     ██╗     ██╗   ██╗██╗         ███╗   ██╗ █████╗ ████████╗██╗██╗   ██╗███████╗
██╔════╝██║  ██║██╔════╝██║     ██║     ██║   ██║██║         ████╗  ██║██╔══██╗╚══██╔══╝██║██║   ██║██╔════╝
███████╗███████║█████╗  ██║     ██║     ██║   ██║██║ ██████╗ ██╔██╗ ██║███████║   ██║   ██║██║   ██║█████╗  
╚════██║██╔══██║██╔══╝  ██║     ██║     ██║   ██║██║ ╚═════╝ ██║╚██╗██║██╔══██║   ██║   ██║╚██╗ ██╔╝██╔══╝  
███████║██║  ██║███████╗███████╗███████╗╚██████╔╝██║         ██║ ╚████║██║  ██║   ██║   ██║ ╚████╔╝ ███████╗
╚══════╝╚═╝  ╚═╝╚══════╝╚══════╝╚══════╝ ╚═════╝ ╚═╝         ╚═╝  ╚═══╝╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═══╝  ╚══════╝
```

**One design system, every platform**

[![NuGet](https://img.shields.io/nuget/v/ShellUI.Native.CLI.svg)](https://www.nuget.org/packages/ShellUI.Native.CLI)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](../LICENSE)

</div>

ShellUI Native is the native cross-platform extension of ShellUI, bringing the same design system and component philosophy to MAUI, WinUI, WPF, and other native platforms. Inspired by shadcn/ui's approach, ShellUI Native provides copy-and-own components for native desktop and mobile development.

## Features

- **Copy & Own** - Components are copied to your project, giving you full control
- **Platform Native** - Pure native controls, no web views, no CSS overhead
- **Lightweight & Fast** - No Tailwind/CSS processing, native styling baked in
- **Consistent Design** - Same design tokens as ShellUI Blazor
- **CLI-First** - Simple command-line interface for adding components
- **Composable** - Build complex UIs from simple components

## No Tailwind Required!

Unlike ShellUI Blazor, **ShellUI Native does not use Tailwind CSS**. Native platforms (MAUI/WinUI/WPF) use XAML styles and ResourceDictionaries, not CSS. The design tokens (colors, spacing, radii) are identical to ShellUI's Tailwind theme but implemented as native styles - making your app **lightweight and fast** with zero CSS processing overhead.

## Quick Start

```bash
# Install the CLI tool
dotnet tool install -g ShellUI.Native.CLI

# Initialize in your MAUI/WinUI/WPF project
shellui-native init --yes

# Add components
shellui-native add button input card

# List all available components
shellui-native list
```

## Supported Platforms

| Platform | Status | .NET Version |
|----------|--------|--------------|
| .NET MAUI | Active | .NET 8.0+ (including 9, 10, etc.) |
| WinUI 3 | Planned | .NET 8.0+ |
| WPF | Planned | .NET 8.0+ |

## Blazor MAUI Hybrid Projects

For **Blazor Hybrid** apps, you can use both libraries:

```
┌─────────────────────────────────────────┐
│           MAUI Native Shell             │  ← ShellUI Native
│  ┌───────────────────────────────────┐  │
│  │         BlazorWebView             │  │
│  │  ┌─────────────────────────────┐  │  │
│  │  │     Blazor Components       │  │  │  ← ShellUI (Blazor + Tailwind)
│  │  └─────────────────────────────┘  │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
```

- **ShellUI (Blazor)** - For components inside the BlazorWebView (HTML/CSS/Razor)
- **ShellUI Native** - For native MAUI controls outside the WebView

They're separate rendering contexts with no conflict. Install what you need based on your app architecture.

## Documentation

- [Getting Started](./QUICKSTART.md)
- [Component List](./COMPONENTS.md)
- [Theming](./THEMING.md)
- [Architecture](./ARCHITECTURE.md)

## Example Usage

```xml
<!-- MAUI XAML -->
<ContentPage xmlns:ui="clr-namespace:YourProject.Components.UI">
    <VerticalStackLayout>
        <ui:Button Variant="Primary" Size="Lg" Clicked="OnButtonClicked">
            <Label Text="Click me!" />
        </ui:Button>
        
        <ui:Card>
            <ui:CardHeader>
                <ui:CardTitle Text="Welcome" />
            </ui:CardHeader>
            <ui:CardContent>
                <ui:Input Placeholder="Enter your name" />
            </ui:CardContent>
        </ui:Card>
    </VerticalStackLayout>
</ContentPage>
```

## Project Structure

After initialization, your project will have:

```
YourProject/
├── Components/
│   └── UI/
│       ├── Button.cs
│       ├── Input.cs
│       ├── Card.cs
│       ├── Variants/
│       │   └── ButtonVariants.cs
│       └── Shell.cs
└── shellui-native.json
```

## Related Projects

- [ShellUI (Blazor)](https://github.com/shellui-dev/shellui) - Blazor component library
- [shadcn/ui](https://ui.shadcn.com/) - The original inspiration

## License

MIT License - see [LICENSE](../LICENSE) for details.
