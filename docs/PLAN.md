# ShellUI Native: Cross-Platform Component Library

## Overview

**ShellUI Native** is the native cross-platform extension of [ShellUI](https://shellui.dev/), bringing the same design system and component philosophy to MAUI, WinUI, WPF, and other native platforms. Inspired by [shadcn/ui](https://ui.shadcn.com/)'s approach, ShellUI Native provides copy-and-own components for native desktop and mobile development.

**[ShellUI (Blazor)](https://shellui.dev/) Reference:** This plan builds upon the successful ShellUI Blazor implementation, adapting its patterns and architecture for native platforms while maintaining design consistency.

## Vision

> "One design system, every platform"

ShellUI Native brings the same beautiful, accessible, and customizable components to native platforms while maintaining the same developer experience and design consistency.

## Supported Platforms

### Phase 1: MAUI (Primary Focus)
- **Target:** .NET MAUI (iOS, Android, Windows, macOS)
- **Components:** 50+ native controls
- **Status:** In Development

### Phase 2: WinUI 3
- **Target:** Windows 11+ native apps
- **Components:** 40+ WinUI controls
- **Status:** Planned Q2 2026

### Phase 3: WPF (.NET 8+)
- **Target:** Modern WPF applications
- **Components:** 35+ WPF controls
- **Status:** Planned Q3 2026

## Architecture

### Package Structure

```
ShellUI.Native/
├── ShellUI.Native.Core/           # Shared abstractions
├── ShellUI.Native.MAUI/           # MAUI implementation
├── ShellUI.Native.WinUI/          # WinUI 3 implementation
├── ShellUI.Native.WPF/            # WPF implementation
├── ShellUI.Native.CLI/            # Native CLI tool
└── ShellUI.Native.Templates/      # Code templates
```

### Core Design Principles

1. **Platform Agnostic APIs** - Same component interfaces across platforms
2. **Native Performance** - No web views, pure native controls
3. **Theme Consistency** - Same CSS variables mapped to Platform theming
4. **Copy & Own** - Components copied to your project
5. **Composable** - Build complex UIs from simple components

## Component Library

### Core Components (All Platforms)

#### Form Components
- Button (Primary, Secondary, Outline, Ghost, Link)
- Input (Text, Password, Email, Number)
- TextArea
- Checkbox
- RadioGroup / RadioButton
- Select / ComboBox
- Slider
- Switch / Toggle
- Label

#### Layout Components
- Card (Header, Content, Footer, Title, Description)
- Dialog / Modal
- Sheet (Bottom/Top/Left/Right)
- Tooltip
- Popover
- Separator
- ScrollArea
- Resizable
- Collapsible / Accordion

#### Data Display
- Table / DataGrid
- Badge
- Avatar
- Skeleton
- Progress
- EmptyState

#### Navigation
- Tabs
- Breadcrumb
- Pagination
- NavigationMenu
- Sidebar

#### Feedback
- Alert / Toast
- Loading / Spinner
- Progress Indicators

## CLI Tool

### Installation
```bash
dotnet tool install -g ShellUI.Native.CLI
```

### Commands
```bash
shellui-native init     # Initialize project (creates shellui-native.json)
shellui-native add      # Add components (copies to Components/UI/)
shellui-native list     # List available components (70+ components)
shellui-native remove   # Remove components
shellui-native update   # Update components to latest version
```

### Configuration
```json
{
  "schema": "https://native.shellui.dev/schema.json",
  "style": "default",
  "componentsPath": "Components/UI",
  "targetPlatform": "MAUI",
  "installedComponents": [
    {
      "name": "button",
      "version": "0.1.0",
      "platform": "MAUI",
      "installedAt": "2026-01-11T...",
      "isCustomized": false
    }
  ]
}
```

## Getting Started

### For MAUI Projects
```bash
# Install CLI
dotnet tool install -g ShellUI.Native.CLI

# Create new MAUI project
dotnet new maui -n MyApp
cd MyApp

# Initialize
shellui-native init --yes

# Add components
shellui-native add button input card dialog

# Build & run
dotnet build
dotnet run
```

### Usage in XAML
```xml
<ContentPage xmlns:ui="clr-namespace:YourProject.Components.UI">
    <VerticalStackLayout>
        <ui:Button Variant="Primary"
                   Size="Lg"
                   Clicked="OnButtonClicked">
            <Label Text="Click me!" />
        </ui:Button>

        <ui:Card>
            <ui:CardHeader>
                <ui:CardTitle Text="Welcome to ShellUI Native" />
            </ui:CardHeader>
            <ui:CardContent>
                <ui:Input Placeholder="Enter your name" />
            </ui:CardContent>
        </ui:Card>
    </VerticalStackLayout>
</ContentPage>
```

## Technical Specifications

### .NET Version Support
- **MAUI:** .NET 8.0+
- **WinUI:** .NET 8.0+ (WinUI 3)
- **WPF:** .NET 8.0+

### Platform Requirements
- **MAUI:** iOS 14+, Android API 21+, Windows 10 1903+
- **WinUI:** Windows 11 (recommended), Windows 10 1903+
- **WPF:** Windows 7 SP1+

### Dependencies
- **Minimal:** Only .NET runtime
- **Optional:** CommunityToolkit.Mvvm for advanced features

## Related Projects

- [ShellUI (Blazor)](https://github.com/shellui-dev/shellui) - Blazor component library
- [shadcn/ui](https://ui.shadcn.com/) - The original inspiration

## License

MIT License
