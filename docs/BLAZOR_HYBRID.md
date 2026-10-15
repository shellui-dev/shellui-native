# Blazor MAUI Hybrid Guide

How to use [ShellUI](https://shellui.dev/) libraries in Blazor Hybrid applications.

## Understanding Hybrid Architecture

A Blazor Hybrid app has two distinct rendering contexts:

```
┌──────────────────────────────────────────────────────┐
│                  MAUI Application                    │
│  ┌────────────────────────────────────────────────┐  │
│  │              Native MAUI Layer                 │  │
│  │  • Native navigation (Shell, tabs, flyout)     │  │  ← ShellUI Native
│  │  • Native dialogs, alerts, action sheets       │  │
│  │  • Platform-specific UI outside WebView        │  │
│  │                                                │  │
│  │  ┌────────────────────────────────────────┐    │  │
│  │  │           BlazorWebView                │    │  │
│  │  │  ┌──────────────────────────────────┐  │    │  │
│  │  │  │      Blazor Components           │  │    │  │  ← ShellUI (Blazor)
│  │  │  │  • HTML/CSS/Razor rendering      │  │    │  │
│  │  │  │  • Tailwind CSS styling          │  │    │  │
│  │  │  │  • Web-based UI inside WebView   │  │    │  │
│  │  │  └──────────────────────────────────┘  │    │  │
│  │  └────────────────────────────────────────┘    │  │
│  └────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────┘
```

## Which Library to Use?

### Use [ShellUI (Blazor)](https://shellui.dev/) When:
- Building UI inside the `BlazorWebView`
- Working with Razor components
- Need Tailwind CSS styling
- Building the main content of your hybrid app

### Use ShellUI Native When:
- Building native MAUI UI outside the WebView
- Need native navigation (Shell, NavigationPage)
- Want truly native dialogs and alerts
- Building platform-specific features

## Installation

### Option 1: Blazor-Heavy Hybrid (Most Common)
If your app is primarily Blazor with minimal native UI:

```bash
# Just install ShellUI Blazor (https://github.com/shellui-dev/shellui)
dotnet tool install -g ShellUI.CLI
cd YourHybridApp
shellui init --yes
shellui add button input card
```

### Option 2: Both Libraries
If you need both Blazor and native components:

```bash
# Install ShellUI Blazor (for BlazorWebView content)
dotnet tool install -g ShellUI.CLI
shellui init --yes
shellui add button input card

# Install ShellUI Native (for native MAUI controls)
dotnet tool install -g ShellUI.Native.CLI --prerelease
shellui-native init --yes
shellui-native add button dialog
```

Run `shellui init` first. Both libraries have a `Button` (and many other components with the same
names), so they can't share a folder and namespace: when `shellui-native init` finds `shellui.json`,
it puts the native components in `Components/Native/`, namespace `YourApp.Components.Native`, and
leaves `Components/UI/` to ShellUI Blazor.

If ShellUI Native was set up first, run `shellui-native init --force` after `shellui init`, reinstall
your native components with `shellui-native add <names> --force`, and delete the old native `.cs`
files from `Components/UI/`. The folder is the `ComponentsPath` setting in `shellui-native.json`;
the namespace follows it.

## File Structure

With both libraries installed:

```
YourHybridApp/
├── Components/
│   ├── UI/                      # ShellUI Blazor (YourApp.Components.UI)
│   │   ├── Button.razor
│   │   └── ...
│   ├── Native/                  # ShellUI Native (YourApp.Components.Native)
│   │   ├── Button.cs
│   │   ├── Variants/
│   │   │   └── ButtonVariants.cs
│   │   └── Shell.cs
│   ├── Layout/                  # Your Blazor layout (MainLayout.razor)
│   └── Pages/                   # Your Blazor pages
│       └── Home.razor
├── wwwroot/
│   ├── app.css                  # Tailwind output
│   └── input.css                # Tailwind input
├── shellui.json                 # Blazor config
├── shellui-native.json          # Native config
└── MainPage.xaml                # Contains BlazorWebView
```

## Example: Hybrid App with Both

### MainPage.xaml (Native MAUI)
```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:components="clr-namespace:YourApp.Components"
             xmlns:ui="clr-namespace:YourApp.Components.Native"
             x:Class="YourApp.MainPage">
    <Grid RowDefinitions="Auto,*">
        <!-- Native header -->
        <ui:Button Grid.Row="0" Text="Native Button" />
        
        <!-- Blazor content -->
        <BlazorWebView Grid.Row="1" HostPage="wwwroot/index.html">
            <BlazorWebView.RootComponents>
                <RootComponent Selector="#app" ComponentType="{x:Type components:Routes}" />
            </BlazorWebView.RootComponents>
        </BlazorWebView>
    </Grid>
</ContentPage>
```

### Home.razor (Blazor inside WebView)
```razor
@using YourApp.Components.UI

<div class="p-4">
    <!-- This uses ShellUI Blazor components -->
    <Button>Blazor Button</Button>
    
    <Card>
        <CardHeader>
            <CardTitle>Welcome</CardTitle>
        </CardHeader>
        <CardContent>
            <Input Placeholder="Enter text" />
        </CardContent>
    </Card>
</div>
```

## Recommendations

1. **Start with Blazor** - Most hybrid apps are Blazor-heavy. Start with [ShellUI Blazor](https://shellui.dev/) only.

2. **Add Native as Needed** - Only add ShellUI Native when you need truly native controls.

3. **Keep Separate** - The two libraries have their own folders and namespaces (`Components/UI/` for Blazor, `Components/Native/` for native) and configs (`shellui.json` vs `shellui-native.json`).

4. **Consistent Design** - Both libraries share the same design tokens, so your app will look consistent even when mixing native and web components.

## Performance Notes

- **Blazor components** run inside a WebView (HTML/CSS rendering)
- **Native components** run as true platform controls (faster, no web overhead)

For maximum performance in areas outside the WebView, use ShellUI Native.
