# ShellUI Native Architecture

## Overview

ShellUI Native follows a modular architecture designed to support multiple native platforms while maintaining a consistent developer experience and design system. It is the native counterpart to [ShellUI (Blazor)](https://shellui.dev/), inspired by [shadcn/ui](https://ui.shadcn.com/)'s copy-and-own approach.

## Key Difference from ShellUI Blazor: No Tailwind!

**ShellUI Native does NOT use Tailwind CSS.** Native platforms use XAML styles, not CSS.

| Aspect | [ShellUI Blazor](https://shellui.dev/) | ShellUI Native |
|--------|---------------|----------------|
| Styling | Tailwind CSS | XAML Styles/ResourceDictionary |
| Rendering | HTML/CSS in browser | Native platform controls |
| Performance | Web rendering | Native rendering (faster) |
| Bundle size | Includes CSS | Zero CSS overhead |

The design tokens (colors, spacing, typography) are **identical** to maintain visual consistency, but implemented natively for better performance.

## Project Structure

```
shellui-native/
├── Directory.Build.props          # Centralized versioning + package metadata
├── global.json                    # Pins .NET 10 SDK
├── ShellUI.Native.slnx            # XML-format solution (post .NET 9)
├── src/
│   ├── ShellUI.Native.Core/       # Shared models and abstractions
│   ├── ShellUI.Native.Templates/  # Component templates (MAUI today; per-platform planned)
│   ├── ShellUI.Native.CLI/        # Command-line tool
│   ├── ShellUI.Native.MAUI/       # MAUI reference implementation (optional)
│   └── ShellUI.Native.Avalonia/   # Avalonia reference implementation (planned, Phase 2)
├── tests/
│   └── ShellUI.Native.Tests/      # xUnit — registry invariants, template hygiene, ProjectDetector
├── examples/
│   └── MAUI.Demo/
└── docs/
```

## Package Dependencies

```
ShellUI.Native.CLI
    ├── ShellUI.Native.Core
    └── ShellUI.Native.Templates
            └── ShellUI.Native.Core
```

## Core Concepts

### 1. Component Templates

Components are stored as string templates in the Templates project. Each template includes:

- **Metadata** - Name, description, category, dependencies, file path
- **Content** - The actual C# code with namespace placeholders

```csharp
public class ButtonTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "button",
        DisplayName = "Button",
        Description = "Interactive button with variants",
        Category = ComponentCategory.Form,
        FilePath = "Button.cs",
        Dependencies = new List<string> { "button-variants" }
    };

    // Template System v2: content is keyed by target NativePlatform. Adding Avalonia
    // support to a component means adding one dict entry here — no registry edit needed.
    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;
// ... MAUI component code",
        // [NativePlatform.Avalonia] = @"..." // added in Phase 2
    };
}
```

**Registry lookup (Template System v2):** `ComponentRegistry.GetComponentContent(name, NativePlatform)`
returns the source for the target platform, or `null` if the component doesn't exist OR exists
but has no template for that platform. Callers use `SupportsPlatform(name, platform)` and
`GetSupportedPlatforms(name)` to distinguish those two failure modes. Landed on
`feat/template-system-v2` (2026-07-18).

### 2. Namespace Replacement

When components are installed, the placeholder `YourProjectNamespace` is replaced with the actual project namespace detected from the .csproj file.

### 3. Dependency Resolution

Components can declare dependencies on other components. The CLI automatically installs dependencies when a component is added.

### 4. Configuration File

The `shellui-native.json` file tracks:

- Target platform (MAUI, Avalonia, WinUI, or WPF — see `NativePlatform` in
  [ShellUINativeConfig.cs](../src/ShellUI.Native.Core/Models/ShellUINativeConfig.cs))
- Components path
- Installed components with versions
- Theme settings

## Platform Detection

The CLI detects project types by examining the .csproj file:

| Detection | Platform |
|-----------|----------|
| `UseMaui=true` or SDK contains "Maui" | MAUI |
| `PackageReference` starting with "Avalonia", or an `App.axaml` file present | Avalonia |
| `UseWinUI=true` | WinUI 3 |
| `UseWPF=true` or SDK contains "Wpf" | WPF (detection only — see [PLAN.md](./PLAN.md)) |

Detection order matters: MAUI is checked first (via SDK/`UseMaui`), then Avalonia (via
`PackageReference`/`App.axaml`, since Avalonia has no dedicated SDK), then WinUI/WPF. See
[`ProjectDetector.cs`](../src/ShellUI.Native.CLI/Services/ProjectDetector.cs).

**Current limitation:** detection recognizes Avalonia/WinUI/WPF projects, but
[`ComponentRegistry`](../src/ShellUI.Native.Templates/ComponentRegistry.cs) only has MAUI
templates today. `shellui-native add` prints a warning and installs MAUI code regardless of
detected platform until per-platform templates exist (see "Component Templates" in Core
Concepts above, and Template System v2 in [PLAN.md](./PLAN.md)).

## Theming System

Unlike [ShellUI Blazor](https://shellui.dev/) which uses CSS variables, ShellUI Native uses platform-native theming:

- **MAUI** - ResourceDictionary with Colors, Styles
- **Avalonia** - `Styles`/`ResourceDictionary` with `DynamicResource`, plus Avalonia's built-in
  Fluent/Simple theme variants for light/dark switching
- **WinUI** - XAML Resources and ThemeResources
- **WPF** - ResourceDictionary with DynamicResource (existing apps only, not an active target)

Design tokens are mapped from the ShellUI CSS variables to native equivalents:

| CSS Variable | Native Resource |
|--------------|-----------------|
| `--background` | `ShellUIBackground` |
| `--foreground` | `ShellUIForeground` |
| `--primary` | `ShellUIPrimary` |
| `--secondary` | `ShellUISecondary` |

## Component Pattern (MAUI)

MAUI components follow the ContentView pattern with BindableProperties:

```csharp
public partial class Button : ContentView
{
    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(ButtonVariant), 
            typeof(Button), ButtonVariant.Default, propertyChanged: OnVisualPropertyChanged);

    public ButtonVariant Variant
    {
        get => (ButtonVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Button button)
            button.UpdateVisualState();
    }
}
```

## Component Pattern (Avalonia, planned)

Avalonia components will follow the `StyledProperty`/`TemplatedControl` pattern instead of
MAUI's `BindableProperty`/`ContentView` — the same property-driven visual-state approach, just
Avalonia's equivalent APIs:

```csharp
public partial class Button : TemplatedControl
{
    public static readonly StyledProperty<ButtonVariant> VariantProperty =
        AvaloniaProperty.Register<Button, ButtonVariant>(nameof(Variant), ButtonVariant.Default);

    public ButtonVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    static Button()
    {
        VariantProperty.Changed.AddClassHandler<Button>((button, _) => button.UpdateVisualState());
    }
}
```

Not implemented yet — this illustrates the target shape once Template System v2 lands and
`ShellUI.Native.Avalonia` is scaffolded.

## Versioning Strategy

All packages share a single version defined in `Directory.Build.props`:

```xml
<ShellUINativeVersion>0.1.0</ShellUINativeVersion>
<ShellUINativeVersionSuffix>alpha.1</ShellUINativeVersionSuffix>
```

The build stamps it into the assemblies, and component metadata reads it from there at runtime,
so `shellui-native.json` records the CLI version each component was installed with.

**Releasing:** set the version in `Directory.Build.props`, add a `# ShellUI Native v<version>`
section to [RELEASE_NOTES.md](./RELEASE_NOTES.md), merge to `main`, then push a `v<version>` tag.
`.github/workflows/release.yml` checks that the tag matches the props version, runs the tests,
publishes `ShellUI.Native.CLI` to NuGet and creates the GitHub release (a prerelease when the
version has a `-`) with the notes as its body.
