# ShellUI Native: Cross-Platform Component Library

## Overview

**ShellUI Native** is the native cross-platform extension of [ShellUI](https://shellui.dev/), bringing the same design system and component philosophy to MAUI, Avalonia, and other native platforms. Inspired by [shadcn/ui](https://ui.shadcn.com/)'s approach, ShellUI Native provides copy-and-own components for native desktop and mobile development.

**[ShellUI (Blazor)](https://shellui.dev/) Reference:** This plan builds upon the successful ShellUI Blazor implementation, adapting its patterns and architecture for native platforms while maintaining design consistency.

## Vision

> "One design system, every platform"

ShellUI Native brings the same beautiful, accessible, and customizable components to native platforms while maintaining the same developer experience and design consistency.

## Supported Platforms

> **2026-07-04 roadmap revision:** Avalonia UI replaces WPF as the Phase 2 target. WPF and WinUI
> are both Windows-only, and MAUI's desktop story (Mac Catalyst) still leaves Linux uncovered.
> Avalonia uses a WPF-like XAML/ResourceDictionary model but compiles to Windows, macOS, *and*
> Linux (plus experimental iOS/Android/WASM) from one codebase — implementing it once buys
> cross-desktop coverage that WPF alone never would. WPF project detection is kept (existing
> WPF apps still need to be recognized), but WPF is no longer an active component-authoring
> target. See [DEVELOPMENT_PLAN.md](./DEVELOPMENT_PLAN.md) for the branch/phase breakdown.

### Phase 1: MAUI (Primary Focus)
- **Target:** .NET MAUI (iOS, Android, Windows, macOS) on .NET 10
- **Components:** 50+ native controls
- **Status:** First prerelease `0.1.0-alpha.1` — 58 component families (89 CLI targets),
  P0–P5 complete, P6 partly; remaining work tracked in
  [COMPONENTS_ROADMAP.md](./COMPONENTS_ROADMAP.md).

### Phase 2: Avalonia UI
- **Target:** Windows, macOS, Linux (desktop-first; mobile/WASM heads are stretch goals)
- **Components:** 50+ controls, mirrored 1:1 with the MAUI set once that set stabilizes
- **Status:** Planned — starts after MAUI hits P0+P1 parity and Template System v2 lands.
  Supersedes the former WPF phase.
- **Why now and not later:** the template/pattern work only needs to be proven once (on MAUI);
  porting to Avalonia is mechanical translation of the same component contracts, and doing it
  before the template system calcifies around MAUI-only assumptions is cheaper than retrofitting
  it later. See "Template System v2" below — a prerequisite, not optional cleanup.

### Phase 3: WinUI 3 (Conditional)
- **Target:** Windows 11+ native apps (Fluent Design)
- **Components:** 40+ WinUI controls
- **Status:** Deprioritized — only pursued if there's a concrete need for Windows-11-specific
  Fluent styling that Avalonia's Fluent theme can't satisfy. Otherwise this phase is dropped
  and that effort folds into Avalonia.

### WPF (.NET 8+) — Detection Only
- **Target:** N/A — not an active build target
- **Status:** `ProjectDetector` still recognizes existing WPF projects (via SDK/`UseWPF`) so
  the CLI can report "not yet supported" instead of misbehaving, but no WPF component templates
  are planned. Users on WPF should be pointed at Avalonia once it ships.

## Architecture

### Package Structure

```
ShellUI.Native/
├── ShellUI.Native.Core/           # Shared abstractions
├── ShellUI.Native.MAUI/           # MAUI reference implementation (Phase 1)
├── ShellUI.Native.Avalonia/       # Avalonia implementation (Phase 2, planned)
├── ShellUI.Native.WinUI/          # WinUI 3 implementation (Phase 3, conditional)
├── ShellUI.Native.CLI/            # Native CLI tool
└── ShellUI.Native.Templates/      # Code templates (per-platform, see Template System v2)
```

Note: `ShellUI.Native.WPF/` has been removed from the planned package structure — see the
roadmap revision above.

### Template System v2 (prerequisite for Phase 2)

Today [`ShellUI.Native.Templates`](../src/ShellUI.Native.Templates) stores one hardcoded C#
string per component (e.g. [`ButtonTemplate.cs`](../src/ShellUI.Native.Templates/Templates/ButtonTemplate.cs)),
written directly against MAUI's `ContentView`/`BindableProperty` APIs, and `ComponentRegistry`
maps a component name straight to that single string. There is no platform dimension anywhere
in the lookup path. Before Avalonia templates can be added, the registry needs to key content
by `(componentName, NativePlatform)` — e.g. `ButtonTemplate.Maui` / `ButtonTemplate.Avalonia` —
so `ComponentRegistry.GetComponentContent` can select the right implementation for
`config.TargetPlatform`. Until this lands, `ComponentInstaller` warns and installs MAUI code
regardless of detected platform (see
[`ComponentInstaller.cs`](../src/ShellUI.Native.CLI/Services/ComponentInstaller.cs)).

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
dotnet tool install -g ShellUI.Native.CLI --prerelease
```

### Commands
```bash
shellui-native init     # Initialize project (creates shellui-native.json)
shellui-native add      # Add components (copies to Components/UI/)
shellui-native list     # List available components (89 targets)
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
      "version": "0.1.0-alpha.1",
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
dotnet tool install -g ShellUI.Native.CLI --prerelease

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
- **MAUI:** .NET 10.0 (unified across all libraries — see [global.json](../global.json))
- **Avalonia:** .NET 10.0 (Avalonia 12)
- **WinUI:** .NET 10.0 (WinUI 3) — conditional phase

### Platform Requirements
- **MAUI:** iOS 15+, Android API 21+, Windows 10 1809+, macOS 12+ (Mac Catalyst)
- **Avalonia:** Windows 10 1809+, macOS 10.14+, most current Linux distros (glibc 2.27+/GTK),
  with experimental iOS/Android/WASM heads
- **WinUI:** Windows 11 (recommended), Windows 10 1809+ — conditional phase
- **WPF:** Windows 7 SP1+ — existing apps only; not an active template target

### Dependencies
- **Minimal:** Only .NET runtime
- **Optional:** CommunityToolkit.Mvvm for advanced features

## Related Projects

- [ShellUI (Blazor)](https://shellui.dev/) - Blazor component library
- [shadcn/ui](https://ui.shadcn.com/) - The original inspiration

## License

MIT License
