# ShellUI Native Examples

This folder contains example projects demonstrating ShellUI Native components.

## Projects

| Project | Platform | Description |
|---------|----------|-------------|
| [MAUI.Demo](./MAUI.Demo/) | .NET MAUI | Complete MAUI app showcasing all components |
| [Avalonia.Demo](./Avalonia.Demo/) | Avalonia 12 | Avalonia components as they are ported (Phase 2) |
| WinUI.Demo | WinUI 3 | *(Coming Soon)* |
| WPF.Demo | WPF | *(Coming Soon)* |

## Running Examples

### MAUI Demo

```bash
cd examples/MAUI.Demo
dotnet restore
dotnet build

# Run on Windows
dotnet run -f net10.0-windows10.0.19041.0

# Run on Android
dotnet run -f net10.0-android

# Run on iOS (macOS only)
dotnet run -f net10.0-ios
```

### Avalonia Demo

```bash
dotnet run --project examples/Avalonia.Demo/AvaloniaDemo.csproj
```

Runs on Windows, macOS and Linux.

## Purpose

These examples serve as:

1. **Component Testing** - Verify components work correctly during development
2. **User Reference** - Show real-world usage patterns
3. **CI/CD Validation** - Automated builds ensure everything works
4. **Documentation** - Living examples of best practices
