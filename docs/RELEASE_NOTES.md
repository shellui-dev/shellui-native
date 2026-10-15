# ShellUI Native Release Notes

# ShellUI Native v0.2.0-beta.1 🧪

> The first beta: 18 new components, among them charts, a data table and a command palette, each for .NET MAUI and Avalonia. Change events now report user input only, so read **Upgrading** before you update an existing app. Still a prerelease, so install with `--prerelease`. Report issues via [GitHub Issues](https://github.com/shellui-dev/shellui-native/issues).

## 📦 Install or update

```bash
dotnet tool update -g ShellUI.Native.CLI --prerelease

cd YourApp
shellui-native init --yes
shellui-native add button input card
```

Requires the .NET 10 SDK and a .NET MAUI or Avalonia 12 project. `init` detects which one it is.

## ✨ New components

For MAUI and Avalonia. 81 component families, 112 CLI targets in total.

| Component | What it is |
|---|---|
| `button-group` | Buttons joined into one control. |
| `input-group` | An input with an icon, text or view before or after it. |
| `menubar` | A row of menus that open under their titles. |
| `command` | A searchable list of grouped commands, inline or as a command palette. |
| `navigation-menu` | A row of top-level links that open panels of links. |
| `data-table` | A table filled from a list, with filter, sorting, selection and pages. |
| `chat` | A message list with bubbles, a typing indicator and a message box. |
| `form` | Fields with a label, description and error message. |
| `image-viewer` | A thumbnail that opens the image over the page, with zoom. |
| `prev-next-nav` | Previous and next page links for the foot of a page. |
| `bar-chart`, `line-chart`, `area-chart`, `pie-chart`, `donut-chart`, `radar-chart`, `radial-chart` | Charts drawn by the components themselves, with no charting package: grouped, stacked or horizontal bars, smooth or straight lines, stacked areas, pies and donuts, radar shapes and progress rings. Each brings the shared `chart` core (series, axes, legend, tooltip, grow-in animation). The names are the same as in ShellUI for Blazor. |

## ⚠️ Change events report user input only

`Toggled`, `CheckedChanged`, `ValueChanged`, `SelectedIndexChanged`, `SelectedValueChanged`, `PageChanged`, `DateChanged`, `TimeChanged`, `ItemToggled` and the other value and selection events now fire only when the user changes the value (a click, tap, swipe or key), like Radix's `onValueChange`. Setting the property from code or a binding updates the control without raising them, so filling a form from data or syncing a sidebar with the router no longer looks like user input. To see every change, bind the property or watch it (`PropertyChanged` on MAUI, `GetObservable` on Avalonia).

Methods meant for code are silent too: `MultiSelect.Toggle`, `TagInput.Add` / `Remove`, `DataTable.ClearSelection`, `DateRangePicker.Clear`, `Stepper.Next` / `Previous`, `Carousel.Next` / `Previous` and its auto-play, `Accordion.Toggle`, `Collapsible.Toggle`.

Unchanged, still reporting every change: text fields' `TextChanged`, overlays' `OpenChanged` / `IsOpenChanged`, `ThemeChanged`, `FileUpload.FilesChanged` and `Sidebar.IsCollapsedChanged`.

## 🧰 CLI

- In a project that also uses ShellUI for Blazor (it has `shellui.json`), `init` puts the native components in `Components/Native/` with the namespace `YourApp.Components.Native`, so the two sets don't clash. The namespace now follows the `ComponentsPath` setting; the default `Components/UI` gives the same code as before. See [BLAZOR_HYBRID.md](https://github.com/shellui-dev/shellui-native/blob/main/docs/BLAZOR_HYBRID.md).
- An unknown name gets suggestions: `shellui-native add donut` answers "Did you mean 'donut-chart'?".
- `update shell` and `update --all` include the `shell` utility. `init` used to write `Shell.cs` without recording it, so `update` skipped it; projects set up by an older version are picked up from the file.

## 🎨 Theme

- Five chart colors, `Chart1` to `Chart5`, in both themes, converted from ShellUI's `--chart-1` to `--chart-5`.

## 🛠️ Fixes and additions

- `shell` builds in a MAUI Blazor Hybrid app; the app's `Components.Layout` namespace used to hide MAUI's `Layout` type (CS0118).
- `SheetContent.MaxPanelWidth` sets how wide a left or right sheet gets (384 by default, never more than 75% of the window).
- MAUI `ShellLabel` gains `HorizontalTextAlignment` and `LineBreakMode` (Avalonia's is a `TextBlock` and already has them).

## ⬆️ Upgrading from alpha.2

Installing the new tool doesn't change the components already in your project. `update` rewrites a component's file, so reapply any edits you made to it.

- Before adding a chart, run `shellui-native update shell` (it holds the new chart colors and the Blazor Hybrid fix).
- Before adding `button-group`, run `shellui-native update button`.
- Your existing components keep their old event behaviour until you update them. To move everything at once, run `shellui-native update --all`; check handlers that relied on code-set values raising events.
- Coming from alpha.1, also run `shellui-native update calendar` before adding `date-range-picker`.

## ⚠️ Known limitations

- The components were checked on Windows, for MAUI and Avalonia. CI builds the Avalonia demo on Linux, Windows and macOS, but it hasn't been run on macOS or Linux, or with touch. The new components build for Android, iOS and Mac Catalyst but haven't been run there.
- On Windows, MAUI buttons and sidebar items can't be pressed through UI Automation (Narrator, test tools), [#25](https://github.com/shellui-dev/shellui-native/issues/25).
- `file-upload`: on MAUI, dropping files works on Windows only. The platform file dialog and file drop have not been tested beyond the build on either platform.
- iOS and Mac Catalyst build but have not been run on a device; Escape does not close overlays on Mac Catalyst yet.
- Very large MAUI pages on Windows can hit WinUI's layout-pass limit (`Layout cycle detected`). Split the page or keep sections hidden until needed; see [COMPONENTS.md](https://github.com/shellui-dev/shellui-native/blob/main/docs/COMPONENTS.md#long-pages-on-windows).
- The docs site (native.shellui.dev) is not live yet; see [COMPONENTS.md](https://github.com/shellui-dev/shellui-native/blob/main/docs/COMPONENTS.md) for usage.

---

# ShellUI Native v0.1.0-alpha.2 🧪

> Avalonia support and five new components. Every component now has a .NET MAUI and an Avalonia version, installed by the same commands. Still a prerelease, so install with `--prerelease`. Report issues via [GitHub Issues](https://github.com/shellui-dev/shellui-native/issues).

## 📦 Install or update

```bash
dotnet tool update -g ShellUI.Native.CLI --prerelease

cd YourApp
shellui-native init --yes
shellui-native add button input card
```

Requires the .NET 10 SDK and a .NET MAUI or Avalonia 12 project. `init` detects which one it is.

## 🟣 Avalonia

- All 63 component families (94 CLI targets) have an Avalonia version, with the same names, properties and variants as MAUI. The few differences are listed in [COMPONENTS.md](https://github.com/shellui-dev/shellui-native/blob/main/docs/COMPONENTS.md#avalonia).
- Setup after `init`: call `Components.UI.ShellTheme.EnsureInitialized();` in `App.Initialize`, after `AvaloniaXamlLoader.Load(this);`, and add `xmlns:ui="using:YourApp.Components.UI"` to your XAML. `init` prints these steps.
- Overlays use Avalonia's `Popup`: menus and popovers flip above the trigger when there is no room below, a click outside or Escape closes them, and an open dialog, drawer or sheet keeps keyboard focus inside it.
- Keyboard: interactive parts are tab stops with shadcn's focus ring, and Enter / Space activate them.
- In an initialized project, `shellui-native list` shows the components for that project's platform.

## ✨ New components

For MAUI and Avalonia.

| Component | What it is |
|---|---|
| `resizable` | Panels side by side or stacked, with a divider to drag. Each panel has a share of the space and a minimum size; groups nest. |
| `navbar` | A top bar with a brand, links and actions. Below 640px wide the links move into a menu under the bar. |
| `sidebar` | An app sidebar with groups and items that collapses to an icon rail. |
| `date-range-picker` | One calendar for a start and an end date. `Calendar` gains `RangeStart` and `RangeEnd`, which highlight a span of days. |
| `file-upload` | A drop zone that opens the platform file picker and lists the picked files. |

## 🧰 CLI

- `init` derives a valid namespace when the project file has no `RootNamespace`.

## ⬆️ Upgrading from alpha.1

Components already in your project are not changed by installing the new tool. `date-range-picker` needs the new `Calendar`: if `calendar` is already in your project, run `shellui-native update calendar` before adding it. `update` rewrites the file, so reapply any edits you made to it.

## ⚠️ Known limitations

- Avalonia was checked on Windows. CI builds the Avalonia demo on Linux, Windows and macOS, but the components have not been run on macOS or Linux, or with touch.
- The five new components were checked on Windows (MAUI and Avalonia). On Android, iOS and Mac Catalyst they build but have not been run.
- `file-upload`: on MAUI, dropping files works on Windows only. The platform file dialog and file drop have not been tested beyond the build on either platform.
- iOS and Mac Catalyst build but have not been run on a device; Escape does not close overlays on Mac Catalyst yet.
- Very large MAUI pages on Windows can hit WinUI's layout-pass limit (`Layout cycle detected`). Split the page or keep sections hidden until needed; see [COMPONENTS.md](https://github.com/shellui-dev/shellui-native/blob/main/docs/COMPONENTS.md#long-pages-on-windows).
- The docs site (native.shellui.dev) is not live yet; see [COMPONENTS.md](https://github.com/shellui-dev/shellui-native/blob/main/docs/COMPONENTS.md) for usage.

---

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

`shellui-native init`, `add`, `list`, `remove` and `update`. `init` writes the `shell` utility, `shellui-native.json` and a theme resource dictionary; `add` resolves dependencies and replaces the namespace with your project's. In a terminal, `init` and `list` show the ShellUI Native mark, and work in progress uses ShellUI's logo and snake loaders.

## ⚠️ Known limitations

- **MAUI only.** Avalonia is the next phase; WinUI 3 is conditional. On those projects `init` and `add` say the components have no template for that platform yet.
- Tested on Windows and Android. iOS and Mac Catalyst build but have not been run on a device; Escape does not close overlays on Mac Catalyst yet.
- Very large pages on Windows can hit WinUI's layout-pass limit (`Layout cycle detected`). Split the page or keep sections hidden until needed; see [COMPONENTS.md](https://github.com/shellui-dev/shellui-native/blob/main/docs/COMPONENTS.md#long-pages-on-windows).
- The docs site (native.shellui.dev) is not live yet; see [COMPONENTS.md](https://github.com/shellui-dev/shellui-native/blob/main/docs/COMPONENTS.md) for usage.
