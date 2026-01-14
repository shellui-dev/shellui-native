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
├── Directory.Build.props          # Centralized versioning
├── ShellUI.Native.sln
├── src/
│   ├── ShellUI.Native.Core/       # Shared models and abstractions
│   ├── ShellUI.Native.Templates/  # Component templates
│   ├── ShellUI.Native.CLI/        # Command-line tool
│   └── ShellUI.Native.MAUI/       # Reference implementation (optional)
├── samples/
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

    public static string Content => @"namespace YourProjectNamespace.Components.UI;
// ... component code";
}
```

### 2. Namespace Replacement

When components are installed, the placeholder `YourProjectNamespace` is replaced with the actual project namespace detected from the .csproj file.

### 3. Dependency Resolution

Components can declare dependencies on other components. The CLI automatically installs dependencies when a component is added.

### 4. Configuration File

The `shellui-native.json` file tracks:

- Target platform (MAUI, WinUI, WPF)
- Components path
- Installed components with versions
- Theme settings

## Platform Detection

The CLI detects project types by examining the .csproj file:

| Detection | Platform |
|-----------|----------|
| `UseMaui=true` or SDK contains "Maui" | MAUI |
| `UseWinUI=true` | WinUI 3 |
| `UseWPF=true` or SDK contains "Wpf" | WPF |

## Theming System

Unlike [ShellUI Blazor](https://shellui.dev/) which uses CSS variables, ShellUI Native uses platform-native theming:

- **MAUI** - ResourceDictionary with Colors, Styles
- **WinUI** - XAML Resources and ThemeResources
- **WPF** - ResourceDictionary with DynamicResource

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

## Versioning Strategy

All packages share a single version defined in `Directory.Build.props`:

```xml
<ShellUINativeVersion>0.1.0</ShellUINativeVersion>
```

Component metadata reads this version at runtime, ensuring consistency across all installed components.
