# ShellUI Native Release Notes

# ShellUI Native v0.1.0-alpha.1 🧪

> The first public prerelease of ShellUI Native: shadcn-style, copy-and-own components for .NET MAUI, with the same design tokens as [ShellUI](https://shellui.dev/) for Blazor. It is a prerelease, so install with `--prerelease`. Report issues via [GitHub Issues](https://github.com/shellui-dev/shellui-native/issues).

## 📦 Install

```bash
dotnet tool install -g ShellUI.Native.CLI --prerelease

cd YourMauiApp
shellui-native init --yes
shellui-native add button input card
```

Requires the .NET 10 SDK and a .NET MAUI project. Components are plain C# files written to `Components/UI/`; edit them freely.

## ✨ Components

58 component families, 89 CLI targets. Parts such as `dialog-trigger` belong to a family; adding one installs the whole family.

| Category | Components |
|---|---|
| Form | `button`, `input`, `label`, `textarea`, `checkbox`, `switch`, `radio-group`, `select`, `combobox`, `multi-select`, `slider`, `date-picker`, `time-picker`, `calendar`, `input-otp`, `number-input`, `tag-input`, `toggle`, `toggle-group` |
| Layout | `card`, `separator`, `accordion`, `collapsible`, `scroll-area`, `aspect-ratio`, `wrap-layout` |
| Feedback | `alert`, `callout`, `toast`, `spinner`, `skeleton`, `progress` |
| Overlay | `dialog`, `alert-dialog`, `drawer`, `sheet`, `dropdown`, `popover`, `hover-card`, `tooltip`, `context-menu` |
| Navigation | `tabs`, `breadcrumb`, `pagination`, `stepper`, `tree-view`, `link-card` |
| Data display | `badge`, `avatar`, `table`, `empty-state`, `stat-card`, `timeline`, `carousel`, `kbd` |
| Utility | `icon`, `theme-toggle`, `copy-button` |

Compositional families (`dialog`, `drawer`, `sheet`, `dropdown`, `popover`, `hover-card`, `accordion`, `collapsible`, `tabs`, `breadcrumb`, `card`, `radio-group`) follow the shadcn Trigger/Content pattern and install their parts automatically.

## 🎨 Design system

- **Theme tokens:** `ShellTheme` holds light and dark palettes keyed like ShellUI's CSS variables (`Background`, `Primary`, `Border`, …) and publishes them as `ShellUI<Token>` / `ShellUI<Token>Brush` resources. Switching theme repaints every component; tokens can be overridden at startup.
- **Sizing:** every single-line form control is 40px high, matching shadcn's `h-10`.
- **Icons:** about 110 Lucide icons (from ShellIcons) drawn with MAUI shapes, with no icon font or package.

## 🪟 Overlays

- Dialogs, drawers, sheets, menus, selects, tooltips, hover cards and toasts float in a page-level layer (`ShellPortal`), so you can declare them next to their trigger, inside a `ScrollView`, and they are never clipped.
- Select, Combobox, Date Picker and Time Picker are custom-drawn, with no native picker chrome.
- Escape on Windows and the back button on Android close the overlay on top (`ShellDismiss`).
- Optional `ShellTheme.SyncSystemBars` makes the Android status and navigation bars follow the theme.

## 🧰 CLI

`shellui-native init`, `add`, `list`, `remove` and `update`. `init` writes the `shell` utility, `shellui-native.json` and a theme resource dictionary; `add` resolves dependencies and replaces the namespace with your project's.

## ⚠️ Known limitations

- **MAUI only.** Avalonia is the next phase; WinUI 3 is conditional. On those projects `init` and `add` say the components have no template for that platform yet.
- Tested on Windows and Android. iOS and Mac Catalyst build but have not been run on a device; Escape does not close overlays on Mac Catalyst yet.
- Very large pages on Windows can hit WinUI's layout-pass limit (`Layout cycle detected`). Split the page or keep sections hidden until needed; see [COMPONENTS.md](https://github.com/shellui-dev/shellui-native/blob/main/docs/COMPONENTS.md#long-pages-on-windows).
- The docs site (native.shellui.dev) is not live yet; see [COMPONENTS.md](https://github.com/shellui-dev/shellui-native/blob/main/docs/COMPONENTS.md) for usage.
