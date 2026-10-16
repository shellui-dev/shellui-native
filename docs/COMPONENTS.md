# Component Reference

Complete list of available ShellUI Native components. Examples assume
`xmlns:ui="clr-namespace:YourApp.Components.UI"` on the page.

## Theming

Every component takes its colors from **theme tokens** mirroring ShellUI's CSS variables
(`--background`, `--primary`, `--border`, …). The `shell` utility (installed by
`shellui-native init`) defines a light and a dark palette in `ShellTheme` and publishes the
active one as application resources, so switching theme repaints every component.

| Token | Role |
|-------|------|
| `Background` / `Foreground` | Page and dialog surface, body text |
| `Card` / `CardForeground` | Card surface |
| `Popover` / `PopoverForeground` | Dropdown, popover and select panels |
| `Primary` / `PrimaryForeground` | Default button, checked checkbox/switch/radio, progress fill |
| `Secondary`, `Muted`, `Accent` (+ `*Foreground`) | Secondary button, tab list and skeleton, hover backgrounds |
| `Destructive`, `Success`, `Warning`, `Info` (+ `*Foreground`) | Status variants (badge, alert, progress, labels) |
| `Border`, `Input`, `Ring` | Dividers, field borders, focus ring |
| `Overlay` | Dialog / drawer / sheet backdrop |
| `Chart1` … `Chart5` | Chart series, in order (ShellUI's `--chart-1` … `--chart-5`) |

**Use the tokens in your own XAML** — each token is published as a Color (`ShellUI<Token>`) and a
Brush (`ShellUI<Token>Brush`):

```xml
<ContentPage BackgroundColor="{DynamicResource ShellUIBackground}">
    <Label Text="Muted text" TextColor="{DynamicResource ShellUIMutedForeground}" />
    <Border Stroke="{DynamicResource ShellUIBorderBrush}" />
</ContentPage>
```

**Initialize early** so page-level `DynamicResource`s resolve on first load (components also do it
on first use):

```csharp
public App()
{
    InitializeComponent();
    Components.UI.ShellTheme.EnsureInitialized();
}
```

**Switch theme** with `ShellTheme.SetTheme(AppTheme.Dark)`, `ShellTheme.ToggleTheme()`, or the
`ThemeToggle` component. The theme follows the OS setting until you set one.

**Customize tokens** before the first page loads, then re-publish:

```csharp
ShellTheme.Light[ShellToken.Primary] = Color.FromArgb("#2563EB");
ShellTheme.Dark[ShellToken.Primary] = Color.FromArgb("#3B82F6");
ShellTheme.Apply();
```

In code, bind any Color/Brush property to a token with the `Token` extension:
`myBorder.Token(Border.StrokeProperty, ShellToken.Border);`

**Android system bars (opt-in).** Set `ShellTheme.SyncSystemBars = true` before
`EnsureInitialized()` to make the status and navigation bars follow the theme: the bars turn
transparent over the edge-to-edge page, the page layer draws a `Background`-colored strip behind
the status bar (so dialog backdrops and sheets cover it), and the bar icons flip with light/dark.
Also set `colorPrimary` / `colorPrimaryDark` in `Platforms/Android/Resources/values/colors.xml` to
your background so the splash-to-app transition doesn't flash the template's purple.

---

## Change events

Events that report a new value or selection (`Toggled`, `CheckedChanged`, `ValueChanged`,
`SelectedIndexChanged`, `SelectedValueChanged`, `PageChanged`, `DateChanged`, `TimeChanged`,
`ItemToggled` and the like) fire only for user input: a click, tap, swipe or key. Setting the
property from code or a binding updates the control without raising them, like Radix's
`onValueChange`, so filling a form from data or syncing a sidebar with the router doesn't look
like the user did it. To see every change, bind the property or watch it (`PropertyChanged` on
MAUI, `GetObservable` on Avalonia). The same goes for methods meant for code: `MultiSelect.Toggle`,
`TagInput.Add` / `Remove`, `DataTable.ClearSelection`, `DateRangePicker.Clear`, `Stepper.Next` /
`Previous`, `Carousel.Next` / `Previous` (and auto-play), `Accordion.Toggle`, `Collapsible.Toggle`.

These still report every change: text fields' `TextChanged` (the platform's own event),
`OpenChanged` / `IsOpenChanged` on overlays, `ThemeChanged`, `FileUpload.FilesChanged` (only the
file picker changes it) and `Sidebar.IsCollapsedChanged` (only code collapses a sidebar).

---

## Avalonia

Every component on this page has an Avalonia version, installed by the same CLI commands
(since `0.1.0-alpha.2`).

Overlays use Avalonia's `Popup` in the window's overlay layer: the content stays in the logical
tree (theme, DataContext and `FindParentOfType` work as declared), dropdowns and popovers get
Avalonia's placement and flip above the trigger when there is no room below, and a click outside
closes them. Escape closes the overlay on top. An open dialog, drawer or sheet keeps keyboard
focus inside it: focus moves to its first control, Tab and Shift+Tab wrap around, and on close
focus returns to the control that opened it.

Setup after `shellui-native init`: call `Components.UI.ShellTheme.EnsureInitialized();` in
`App.Initialize` after `AvaloniaXamlLoader.Load(this);`, and add
`xmlns:ui="using:YourApp.Components.UI"` to your XAML. Switch theme with
`ShellTheme.SetTheme(ThemeVariant.Dark)` or `ShellTheme.ToggleTheme()`.

Some names match Avalonia's own controls (`Button`, `Calendar`, `Carousel`, `ContextMenu`,
`TreeView`, …). In XAML the `ui:` prefix picks the ShellUI one. In C# that imports both
namespaces, qualify the name or add an alias: `using UI = YourApp.Components.UI;`.

Same names, properties and variants as MAUI, except:

| | MAUI | Avalonia |
|---|---|---|
| Icon | `<ui:Icon Name="Search" />` | `<ui:Icon Kind="Search" />` (every Avalonia control already has a `Name`) |
| Input length limit | `MaxLength`, default `int.MaxValue` | `MaxLength`, `0` (default) means no limit |
| Card children | XAML children | XAML children, collected in `Card.Items` |
| Block button | `HorizontalOptions="Fill"` | `HorizontalAlignment="Stretch"` |
| Focus ring | Platform focus visuals | shadcn's ring, for keyboard focus only |
| Part children | XAML children | XAML children, collected in `Items` (`Dialog`, `DialogContent`, `Dropdown`, `AccordionItem`, `TabsList`, …) |
| Dropdown / Popover | `ToggleAsync()` / `CloseAsync()` kept for older code | `Toggle()` / `Close()` only |
| Slider | Platform slider tinted with tokens | Custom-drawn shadcn slider; adds `Step` for the arrow keys; `ValueChanged` passes the new `double` |
| DatePicker `DateChanged` | `DateChangedEventArgs` | The new `DateTime` |
| Tabs keyboard | Tab and Enter | Also Left / Right to select the neighbouring tab |
| Skeleton `CornerRadius` | `double` | Border's `CornerRadius` (`CornerRadius="24"` still works) |
| ScrollArea scrollbar | Platform scrollbar | Fluent's thin scrollbar, hidden while idle |
| Tooltip | Shows on hover | Also shows on keyboard focus; `Placement` takes Avalonia's `PlacementMode` (`Top`, `Bottom`, `Left`, `Right`) |
| Toaster | Declared on a page | Declared in a window; `Toast.Show` without one uses the active window |
| HoverCard on touch | Tap toggles | Touch, pen and Enter / Space toggle; a mouse click acts on the wrapped control only |
| AlertDialog extra content | One `Body` view | Any XAML children, stacked between the description and the buttons |
| Avatar `Source` | `ImageSource` (`Source="profile.png"`) | `IImage`, an Avalonia resource path (`Source="/Assets/profile.png"`) |
| TableRow clicks | `Tapped` event on the row | Avalonia's own `Tapped` event (`TappedEventArgs`) |
| ContextMenu | Right-click, or long-press on touch | Also the Menu key / Shift+F10 while focus is inside the trigger; a right-click while it is open moves it; items are tab stops; children are collected in `Items` |
| Carousel | Swipe, arrows, dots | Drag with the mouse or touch; the arrows are tab stops and Left / Right move while one has focus; with `AspectRatio="0"` set `Height` |
| Stepper | Shows the active step's view | Every step stays in the tree and only the active one is visible; reached steps are tab stops (Enter / Space) |
| Pagination | Labels hidden on phones | `ShowLabels` defaults to true; every page and arrow is a tab stop (Enter / Space) |
| Charts | Drawn with `GraphicsView`; on touch a tap or a drag shows the tooltip; `HeightRequest` | Drawn in `Render`, in the control's own font; `Height`; `ChartSeries.Color` and the token colors are Avalonia `Color`s (`Color.Parse("#E76E50")`) |
| ImageViewer | `Source` is an `ImageSource`; `Aspect`; pinch zooms | `Source` is an `IImage` (a resource path such as `/Assets/photo.jpg`); `Stretch` instead of `Aspect`; the wheel and the + / - / 0 keys zoom; the thumbnail and the controls are tab stops |
| PrevNextNav | Tap a card | The cards are also tab stops (Enter / Space) |
| ChatInput | The send button sends | Also Enter sends and Shift+Enter starts a new line |
| DataTable | Tap a header to sort | Sortable headers are also tab stops (Enter / Space) and keep focus after sorting; a click on a row's checkbox does not raise `RowTapped` |
| Command | Enter picks the highlighted item (the first match) | Up / Down move the highlight and Enter picks it; `Value`, `Text` and the other strings are nullable; children are collected in `Groups` and `Items` |
| CommandPalette | Open it from code | Also `Hotkey`, a key gesture that toggles it anywhere in the window (Ctrl+K by default; `Hotkey="{x:Null}"` for none) |
| NavigationMenu | Tap an item to open its panel | Items are tab stops: Enter / Space / Down open a panel and Left / Right move between items; in a panel Up / Down move and Enter picks; children are collected in `Items` and `Links` |
| ButtonGroup | Children are the buttons | Children are collected in `Items` |
| InputGroup | `Keyboard` picks the on-screen keyboard | No `Keyboard`; a click on a control in the prefix or suffix is left to that control |
| Menubar | Tap a title to open its menu | Titles are tab stops: Enter / Space / Down open a menu and Left / Right move between titles; in a menu Up / Down move and Enter picks; children are collected in `Menus` and `Items` |
| Resizable | Drag a divider | Dividers are also tab stops: the arrow keys move them 16px; the pointer shows a resize cursor |
| Navbar | Links and the menu button react to taps | They are also tab stops (Enter / Space); children are collected in `Items` |
| BottomNav | Items react to taps (and Enter / Space on Windows); an item's XAML content is its `IconContent` | Items are also tab stops: Enter / Space pick one and Left / Right move to the next destination; set `IconContent` as a property element (`<ui:BottomNavItem.IconContent>`) |
| Sidebar | Width animates on collapse | Same, with items as tab stops; children are collected in `Groups` and `Items` |
| DateRangePicker | Tap the trigger | Also Enter / Space / Down on the focused trigger; Escape closes the panel |
| FileUpload | `Files` are `FileResult`; `FileTypes` is a `FilePickerFileType`; drop works on Windows only | `Files` are `IStorageFile`; `FileTypes` is a list of `FilePickerFileType` (e.g. `FilePickerFileTypes.ImageAll`); drop works wherever Avalonia supports it; Enter / Space open the picker |
| ToggleGroup | Items toggle on tap | Items are also tab stops (Enter / Space); children are collected in `Items` |
| NumberInput | Numeric keyboard | Up / Down step the value while the field has focus |
| TagInput | Enter, comma or semicolon adds a tag | Backspace in the empty field also removes the last tag |
| InputOtp | Hidden entry with the numeric keyboard | Invisible text box over the slots; `Focus()` focuses it |
| Combobox / MultiSelect | Type to filter, Enter takes the first match | Up / Down move a highlight and Enter takes it; the search field has focus as soon as the panel opens; `Value` is `string?` |
| TreeView | Rows are tab stops | Also Right to expand and Left to collapse |
| TimelineItem content | One `Body` view | One child control, also reachable as `Body` |
| CopyButton | `Clipboard.Default` | The window's clipboard; does nothing until the button is in a window |
| LinkCard `Url` | `Launcher.Default` | The window's `Launcher` |
| AspectRatio | `ContentView` | A `Border`, so `CornerRadius` rounds the content |
| WrapLayout | Custom layout with `Padding` | A `Panel` (no `Padding`); Avalonia's own `WrapPanel` covers the plain case, `LastChildFill` is what this adds |

---

## Icon

Stroke icons from [ShellIcons](../../../icons/shell-icons) (Lucide 0.475.0 — the set ShellUI
uses), drawn with MAUI shapes: no icon font or package, crisp at any size, theme-aware. The
template ships a curated set of ~110 icons with exact Lucide names (`trash-2` → `Trash2`).
To include more, add their names to `ICONS` in `scripts/generate-icons.py`, then run it and
`scripts/sync-templates.py`.

```bash
shellui-native add icon
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Name | IconName | None | Which icon (`Check`, `X`, `Plus`, `ChevronDown`, `Search`, `Settings`, `Trash2`, `Sun`, `Moon`, `Info`, `CircleAlert`, `CircleCheck`, `TriangleAlert`, `User`, `Bell`, … see `IconName`) |
| Size | double | 16 | Width and height in DIPs; the stroke scales with it |
| StrokeWidth | double | 2 | Stroke width on Lucide's 24×24 grid |
| Token | ShellToken | Foreground | Theme color |
| Color | Color? | null | Explicit color (overrides `Token`) |

```xml
<ui:Icon Name="Search" Size="16" Token="MutedForeground" />
```

---

## Form Components

### Button
Interactive button with variants, sizes, an optional icon and a loading state. Sizes to its
content (set `HorizontalOptions="Fill"` for a full-width button).

```bash
shellui-native add button
```

**Variants:** Default, Secondary, Outline, Destructive, Ghost, Link · **Sizes:** Sm (36), Default (40), Lg (44), Icon (40×40)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Text | string | "" | Button text |
| Variant | ButtonVariant | Default | Visual style |
| Size | ButtonSize | Default | Height / padding |
| Icon | IconName | None | Optional icon |
| IconPosition | IconPosition | Left | `Left` or `Right` of the text |
| IsLoading | bool | false | Shows a spinner and ignores clicks |
| IsEnabled | bool | true | Disabled buttons render at 50% opacity |

```xml
<ui:Button Text="Save" Clicked="OnSave" />
<ui:Button Text="New item" Icon="Plus" />
<ui:Button Text="Continue" Icon="ArrowRight" IconPosition="Right" Variant="Outline" />
<ui:Button Icon="Settings" Size="Icon" Variant="Ghost" />
```

**Events:** `Clicked`. On Windows buttons are keyboard tab stops (Enter/Space activate).

---

### Button Group
Buttons joined into one control: only the outer corners are rounded, and neighbouring borders
overlap so `Outline` buttons share one line between them. Each button keeps its own `Clicked`.

```bash
shellui-native add button-group
```

**Properties:** `Orientation` (Horizontal, Vertical) · its children are the buttons

```xml
<ui:ButtonGroup>
    <ui:Button Text="Day" Variant="Outline" />
    <ui:Button Text="Week" Variant="Outline" />
    <ui:Button Text="Month" Variant="Outline" />
</ui:ButtonGroup>
```

It needs the `Button` that ships with it. If `button` is already in your project from an older
CLI version, run `shellui-native update button` first.

---

### Toggle
Two-state button that stays pressed — transparent (or outlined) when off, `Accent` when on.

```bash
shellui-native add toggle
```

**Properties:** `IsPressed` (two-way), `Text`, `Icon`, `Variant` (Default, Outline),
`Size` (Sm 36, Default 40, Lg 44) · **Events:** `PressedChanged`

```xml
<ui:Toggle Icon="Bookmark" Variant="Outline" IsPressed="{Binding Saved}" />
<ui:Toggle Text="Notify me" Icon="Bell" Size="Sm" PressedChanged="OnToggled" />
```

---

### Toggle Group
A row of toggles that work as one control: pick one (the default) or several (`Multiple`).

```bash
shellui-native add toggle-group
```

**Properties (ToggleGroup):** `Value` (two-way; the pressed item, `null` when none), `Multiple`,
`Values` (two-way; the pressed items in `Multiple` mode), `Variant` (Default, Outline) ·
**Events:** `ValueChanged`, `ValuesChanged`

**Properties (ToggleGroupItem):** `Value`, `Text`, `Icon`

```xml
<ui:ToggleGroup Value="{Binding View}">
    <ui:ToggleGroupItem Value="day" Text="Day" />
    <ui:ToggleGroupItem Value="week" Text="Week" />
    <ui:ToggleGroupItem Value="month" Text="Month" />
</ui:ToggleGroup>
```

In single mode, tapping the pressed item clears the selection. To always keep one pressed, set
`Value` back in `ValueChanged` when it arrives as `null`.

---

### Input
Single-line text field — 40px, one themed border (the platform control's own frame is removed),
ring color + soft glow on focus, destructive border on error.

```bash
shellui-native add input
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Text | string | "" | Value (two-way) |
| Placeholder | string | "" | Placeholder text |
| IsPassword | bool | false | Mask input |
| HasError | bool | false | Error state |
| IsReadOnly | bool | false | Prevent editing |
| MaxLength | int | int.MaxValue | Maximum characters |
| Keyboard | Keyboard | Default | Soft keyboard type |

```xml
<ui:Input Placeholder="you@example.com" Keyboard="Email" Text="{Binding Email}" />
<ui:Input Placeholder="Password" IsPassword="True" />
<ui:Input Text="taken-name" HasError="True" />
```

**Events:** `TextChanged`, `Completed`

---

### Input Group
An input with something before or after the text, inside the same border: an icon, a short
text, or any view. Same height, focus ring and error state as `Input`.

```bash
shellui-native add input-group
```

**Properties:** `Text` (two-way), `Placeholder`, `IsPassword`, `HasError`, `Keyboard` (MAUI),
`PrefixIcon`, `SuffixIcon`, `PrefixText`, `SuffixText`, `Prefix`, `Suffix` (a view; replaces the
icon and text on that side) · **Events:** `TextChanged`, `Completed` · **Methods:** `Focus()`

```xml
<ui:InputGroup PrefixIcon="Search" Placeholder="Search..." />
<ui:InputGroup PrefixText="https://" SuffixText=".com" Placeholder="example" />
<ui:InputGroup Placeholder="you@example.com">
    <ui:InputGroup.Suffix>
        <ui:Badge Text="Required" Variant="Secondary" />
    </ui:InputGroup.Suffix>
</ui:InputGroup>
```

---

### Input OTP
One-time-code input: a row of 40px slots over one hidden text field, so paste, autofill and the
platform keyboard all work. The active slot shows the ring color and a blinking caret.

```bash
shellui-native add input-otp
```

**Properties:** `Length` (6), `Value` (two-way), `IsNumeric` (true — digits and numeric keyboard),
`HasError` · **Events:** `ValueChanged`, `Completed` (every slot filled)

```xml
<ui:InputOtp Length="6" Value="{Binding Code}" Completed="OnCodeEntered" />
```

---

### Number Input
Number field with − and + buttons. The value is clamped to `Minimum` / `Maximum`, and the buttons
dim at the limits. Typing is committed on Enter or when the field loses focus; text that isn't a
number puts the last value back.

```bash
shellui-native add number-input
```

**Properties:** `Value` (`decimal?`, two-way; `null` when empty), `Minimum`, `Maximum`, `Step` (1),
`Placeholder` · **Events:** `ValueChanged`

```xml
<ui:NumberInput Value="{Binding Quantity}" Minimum="1" Maximum="10" WidthRequest="160" />
```

---

### Tag Input
Text field that turns entries into removable chips. Enter, a comma or a semicolon commits what was
typed (so pasting `a, b, c` adds three tags); the × on a chip removes it.

```bash
shellui-native add tag-input   # also installs wrap-layout
```

**Properties:** `Tags` (`IList<string>`, two-way), `Placeholder`, `MaxTags` (0 = no limit),
`AllowDuplicates` · **Events:** `TagsChanged` · **Methods:** `Add(tag)`, `Remove(tag)`

```xml
<ui:TagInput Tags="{Binding Labels}" Placeholder="Add a label..." MaxTags="6" />
```

---

### Form
Fields stacked with even spacing. Each `FormField` is a label, a control, an optional
description and an error message. Validation is yours: set a field's `Error` and the form
reports it.

```bash
shellui-native add form
```

**Properties (Form):** `Spacing` (24), `IsValid` (no field has an `Error`), `FirstInvalid`
(read-only) · **Methods:** `FormFields()`, `ClearErrors()`

**Properties (FormField):** `Label`, `Description`, `Error` (two-way; shown in the destructive
color, and the label turns red), `IsRequired` (a red asterisk), `HasError` (read-only) · its
child view is the control

```xml
<ui:Form x:Name="SignUp">
    <ui:FormField x:Name="EmailField" Label="Email" Description="We never share it." IsRequired="True">
        <ui:Input x:Name="Email" Placeholder="you@example.com" />
    </ui:FormField>
    <ui:Button Text="Create account" Clicked="OnSubmit" />
</ui:Form>
```

```csharp
EmailField.Error = Email.Text.Contains('@') ? "" : "Enter a valid email address.";
Email.HasError = EmailField.HasError;   // the red border is the control's own property
if (SignUp.IsValid) { /* submit */ }
```

---

### Textarea
Multi-line text field — min 80px, grows with content.

```bash
shellui-native add textarea
```

**Properties:** `Text` (two-way), `Placeholder`, `MaxLength`, `HasError` · **Events:** `TextChanged`

```xml
<ui:Textarea Placeholder="Type your message here." Text="{Binding Body}" />
```

---

### Checkbox
16×16 box with a check icon; the whole row (box + label) is the hit target.

```bash
shellui-native add checkbox
```

**Properties:** `IsChecked` (two-way), `Label`, `HasError`, `IsEnabled` · **Events:** `CheckedChanged`

```xml
<ui:Checkbox Label="Accept terms and conditions" IsChecked="{Binding Accepted}" />
```

---

### Switch
44×24 track with a 20px thumb that slides inside it.

```bash
shellui-native add switch
```

**Properties:** `IsToggled` (two-way), `Label`, `IsEnabled` · **Events:** `Toggled`

```xml
<ui:Switch Label="Airplane mode" IsToggled="{Binding AirplaneMode}" />
```

---

### RadioGroup
Single choice. The group drives its items.

```bash
shellui-native add radio-group
```

**Properties (RadioGroup):** `Value` (two-way — the selected item's `Value`) · **Events:** `ValueChanged`
**Properties (RadioGroupItem):** `Value`, `Text`

```xml
<ui:RadioGroup Value="{Binding Density}">
    <ui:RadioGroupItem Value="default" Text="Default" />
    <ui:RadioGroupItem Value="comfortable" Text="Comfortable" />
    <ui:RadioGroupItem Value="compact" Text="Compact" />
</ui:RadioGroup>
```

---

### Select
Custom-drawn select: a 40px trigger with a chevrons icon and a floating list with a check on the
selected item. Looks the same on every platform.

```bash
shellui-native add select
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| ItemsSource | IList&lt;string&gt; | null | Options |
| SelectedIndex | int | -1 | Selected option (two-way) |
| SelectedItem | string? | — | Read-only selected option |
| Placeholder | string | "Select..." | Shown when nothing is selected |

```xml
<ui:Select Placeholder="Select a country" ItemsSource="{Binding Countries}"
           SelectedIndex="{Binding CountryIndex}" WidthRequest="280" HorizontalOptions="Start" />
```

**Events:** `SelectedIndexChanged`

---

### Combobox
A Select with a search field: the floating panel has a filter input above the option list, a check
on the selected option and a "no results" message. On desktop the search field takes focus when
the panel opens, and Enter picks the first match.

```bash
shellui-native add combobox
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| ItemsSource | IList&lt;string&gt; | null | Options |
| Value | string | "" | Selected option (two-way) |
| Placeholder | string | "Select..." | Shown when nothing is selected |
| SearchPlaceholder | string | "Search..." | Placeholder of the filter field |
| EmptyText | string | "No results found." | Shown when nothing matches |

```xml
<ui:Combobox Placeholder="Select framework..." ItemsSource="{Binding Frameworks}"
             Value="{Binding Framework}" WidthRequest="280" HorizontalOptions="Start" />
```

**Events:** `ValueChanged`

---

### Multi Select
Like Combobox, for several options: the trigger shows the picked options as removable chips, and
the floating panel has a search field and a checked list. Picking toggles an option and leaves
the panel open; click outside, press Escape or (on Android) go back to close it.

```bash
shellui-native add multi-select   # also installs wrap-layout
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| ItemsSource | IList&lt;string&gt; | null | Options |
| Values | IList&lt;string&gt; | null | Picked options (two-way) |
| Placeholder | string | "Select..." | Shown when nothing is picked |
| SearchPlaceholder | string | "Search..." | Placeholder of the filter field |
| EmptyText | string | "No results found." | Shown when nothing matches |

```xml
<ui:MultiSelect Placeholder="Select frameworks..." ItemsSource="{Binding Frameworks}"
                Values="{Binding Picked}" />
```

**Events:** `ValuesChanged` · **Methods:** `Toggle(option)`

---

### Slider
Platform slider tinted with the theme (primary range and thumb, secondary track).

```bash
shellui-native add slider
```

**Properties:** `Value` (two-way), `Minimum`, `Maximum` · **Events:** `ValueChanged`

---

### Calendar
Month grid with previous/next navigation. The selected day is primary, today is accented, and
month names, weekday names and the first day of the week follow the current culture.

```bash
shellui-native add calendar
```

**Properties:** `SelectedDate` (DateTime?, two-way), `DisplayMonth`, `MinimumDate`, `MaximumDate` · **Events:** `DateSelected`

```xml
<ui:Calendar SelectedDate="{Binding Day}" DateSelected="OnDay" />
```

### DatePicker
Custom-drawn: a 40px trigger (calendar icon + formatted date) that opens a `Calendar` floating
over the page. Looks the same on every platform.

```bash
shellui-native add date-picker   # also installs calendar
```

**Properties:** `Date` (two-way), `MinimumDate`, `MaximumDate`, `Format` (.NET date format, default `MMMM d, yyyy`) · **Events:** `DateChanged`

```xml
<ui:DatePicker Date="{Binding DueDate}" Format="MMM d, yyyy" />
```

### DateRangePicker
A DatePicker-style trigger that opens one calendar for a start and an end date. The first pick
is the start, the second the end; a pick before the start, or one made when the range is
already complete, starts over. The panel closes when the range is complete, and the × clears
it.

```bash
shellui-native add date-range-picker
```

**Properties:** `StartDate`, `EndDate` (two-way, nullable), `Placeholder` ("Pick a date
range"), `Format` ("MMM d, yyyy"), `AllowClear` (true), `MinimumDate`, `MaximumDate` ·
**Events:** `RangeChanged` · **Methods:** `SetOpen(bool)`, `Close()`, `Clear()`

```xml
<ui:DateRangePicker StartDate="{Binding From}" EndDate="{Binding To}" />
```

It uses `Calendar`'s `RangeStart` and `RangeEnd`, which tint a span of days; you can set them
on a `Calendar` of your own too.

---

### FileUpload
A dashed drop zone with an upload icon and a prompt. Tapping it opens the platform file picker;
the picked files are listed underneath, each with a × to remove it. On Windows, files can also
be dropped on it.

```bash
shellui-native add file-upload
```

**Properties:** `Text`, `Multiple` (false: a new pick replaces the file), `MaxFiles` (0 = no
limit; applies when `Multiple` is on), `FileTypes` (`FilePickerFileType`, e.g.
`FilePickerFileType.Images`; null offers every file), `ShowFiles` (true), `Files` (read-only) ·
**Events:** `FilesChanged` · **Methods:** `PickAsync()`, `Add(files)`, `Remove(file)`,
`Clear()`

```xml
<ui:FileUpload Multiple="True" MaxFiles="5" FilesChanged="OnFiles" />
```

```csharp
private async void OnFiles(object? sender, IReadOnlyList<FileResult> files)
{
    foreach (var file in files)
    {
        await using var stream = await file.OpenReadAsync();
        // upload or read the stream
    }
}
```

---

### TimePicker
Custom-drawn: a 40px trigger (clock icon + formatted time) that opens scrollable hour and minute
columns — plus AM/PM on a 12-hour clock — floating over the page. Picking a value updates `Time`
straight away; click outside or press Escape to close. Looks the same on every platform.

```bash
shellui-native add time-picker
```

**Properties:** `Time` (two-way), `MinuteStep` (default 5), `Is24Hour` (defaults to the current
culture's clock), `Format` (.NET time format; default `HH:mm` or `h:mm tt`) · **Events:** `TimeChanged`

```xml
<ui:TimePicker Time="{Binding StartsAt}" MinuteStep="15" />
```

---

### Label (ShellLabel)
Typography with size, weight and color variants.

```bash
shellui-native add label
```

**Properties:** `Text`, `Size` (Xs…Xxxl), `Weight` (Light…Bold), `Variant` (Default, Muted, Destructive, Success, Warning), `HorizontalTextAlignment` and `LineBreakMode` (MAUI; `WordWrap` by default). On Avalonia `ShellLabel` is a `TextBlock`, so `TextAlignment` and `TextWrapping` do the same.

```xml
<ui:ShellLabel Text="This username is already taken." Size="Sm" Variant="Destructive" />
```

---

### ThemeToggle
36×36 outline icon button that switches light/dark; the sun and moon cross-fade.

```bash
shellui-native add theme-toggle
```

**Events:** `ThemeChanged(bool isDark)` · **Methods:** `Toggle()`

---

## Layout Components

### Aspect Ratio
Keeps its content at a fixed width / height ratio: the height follows the width.

```bash
shellui-native add aspect-ratio
```

**Properties:** `Ratio` (width divided by height; default 16:9 = 1.7778)

```xml
<ui:AspectRatio Ratio="1.7778">
    <Image Source="cover.jpg" Aspect="AspectFill" />
</ui:AspectRatio>
```

### Wrap Layout
`flex flex-wrap gap-2`: children flow left to right and wrap onto new rows, centered vertically
within their row. Use it for button rows, chips and tag lists.

```bash
shellui-native add wrap-layout
```

**Properties:** `Spacing` (8, between children), `LineSpacing` (8, between rows), `LastChildFill`
(the last child takes the rest of its row), `MinimumLastChildWidth` (96)

```xml
<ui:WrapLayout>
    <ui:Button Text="One" />
    <ui:Button Text="Two" Variant="Outline" />
</ui:WrapLayout>
```

Prefer it to `FlexLayout` for simple wrapping: it measures each child once per layout pass.

### Resizable
Panels side by side (or stacked) with a hairline between them. Dragging the hairline moves
space from one neighbor to the other; neither goes under its `MinSize`. Groups nest, so a panel
can hold another `Resizable` with the other orientation. Give the group a size.

```bash
shellui-native add resizable
```

**Properties (Resizable):** `Orientation` (Horizontal), `ShowHandle` (true: a short bar on each
divider) · **Events:** `Resized` (a drag ended)

**Properties (ResizablePanel):** `Size` (two-way; its share of the space, 2 is twice as much as
1), `MinSize` (48) · its child view is the panel's content

```xml
<ui:Resizable HeightRequest="200">
    <ui:ResizablePanel Size="1" MinSize="120"> ... </ui:ResizablePanel>
    <ui:ResizablePanel Size="2"> ... </ui:ResizablePanel>
</ui:Resizable>
```

---

### Card
`rounded-xl border bg-card shadow-sm` container.

```bash
shellui-native add card   # also installs card-header, card-content, card-footer
```

**Properties (Card):** `Variant` (Default, Elevated), `IsPressable` · **Events:** `Clicked` (when pressable)
**CardHeader:** `Title`, `Description` · **CardContent:** any content · **CardFooter:** actions, right-aligned

```xml
<ui:Card WidthRequest="380" HorizontalOptions="Start">
    <ui:CardHeader Title="Create project" Description="Deploy your new project in one click." />
    <ui:CardContent><ui:Input Placeholder="Name of your project" /></ui:CardContent>
    <ui:CardFooter>
        <ui:Button Text="Cancel" Variant="Outline" />
        <ui:Button Text="Deploy" />
    </ui:CardFooter>
</ui:Card>
```

---

### Separator
1px divider in the `Border` token. **Properties:** `Orientation` (Horizontal, Vertical)

---

### Collapsible
Expand/collapse with a height animation. The trigger can wrap a Button or any view.

```bash
shellui-native add collapsible
```

**Properties:** `Open` (two-way; also `SetOpen(bool)`, `Toggle()`) · **Events:** `OpenChanged`

```xml
<ui:Collapsible>
    <Grid ColumnDefinitions="*,Auto">
        <Label Text="@peduarte starred 3 repositories" />
        <ui:CollapsibleTrigger Grid.Column="1">
            <ui:Button Icon="ChevronsUpDown" Size="Icon" Variant="Ghost" />
        </ui:CollapsibleTrigger>
    </Grid>
    <ui:CollapsibleContent>
        <Label Text="Revealed when open." />
    </ui:CollapsibleContent>
</ui:Collapsible>
```

---

### Accordion
Stacked sections with dividers and a chevron that rotates when open; content expands and
collapses its height.

```bash
shellui-native add accordion
```

**Properties (Accordion):** `Type` (Single, Multiple), `Value` (initially open item; comma-separated for Multiple)
**Events (Accordion):** `ItemToggled(value, isOpen)`
**AccordionItem:** `Value` · **AccordionTrigger:** `Text`, or any view as content

```xml
<ui:Accordion Type="Single" Value="item-1">
    <ui:AccordionItem Value="item-1">
        <ui:AccordionTrigger Text="Is it accessible?" />
        <ui:AccordionContent><Label Text="Yes." /></ui:AccordionContent>
    </ui:AccordionItem>
    <ui:AccordionItem Value="item-2">
        <ui:AccordionTrigger Text="Is it animated?" />
        <ui:AccordionContent><Label Text="Yes." /></ui:AccordionContent>
    </ui:AccordionItem>
</ui:Accordion>
```

---

### ScrollArea
`ScrollView` wrapper. Put it in a bordered `Border` for the shadcn look.

```xml
<Border Stroke="{DynamicResource ShellUIBorderBrush}" StrokeShape="RoundRectangle 6">
    <ui:ScrollArea HeightRequest="200"><VerticalStackLayout>...</VerticalStackLayout></ui:ScrollArea>
</Border>
```

---

## Navigation Components

### Navigation Menu
A row of top-level links for a site or app header. An item with links inside shows a chevron
and opens a panel of them, each with a title, an optional description and an optional icon. An
item without links is a plain link.

```bash
shellui-native add navigation-menu
```

**NavigationMenuItem:** `Title`, `IsOpen` (read-only), `IsEnabled` · **Events:** `Clicked` (an
item without links) · **Methods:** `SetOpen(bool)`, `Close()`

**NavigationMenuLink:** `Title`, `Description`, `Icon`, `IsEnabled` · **Events:** `Clicked`

```xml
<ui:NavigationMenu>
    <ui:NavigationMenuItem Title="Getting started">
        <ui:NavigationMenuLink Title="Introduction" Description="What ShellUI Native is." Clicked="OnLink" />
        <ui:NavigationMenuLink Title="Installation" Description="Add it to your app." Clicked="OnLink" />
    </ui:NavigationMenuItem>
    <ui:NavigationMenuItem Title="Docs" Clicked="OnDocs" />
</ui:NavigationMenu>
```

---

### Command
A search field over a list of grouped items. Typing filters the list by each item's `Text` and
`Keywords`, and Enter picks the highlighted item. `CommandPalette`, in the same file, shows the
list in a dialog near the top of the page.

```bash
shellui-native add command
```

**Properties (Command):** `Placeholder`, `EmptyText` ("No results found."), `MaxListHeight`
(300), `Query` (read-only) · **Events:** `ItemSelected` (the item) · **Methods:** `Focus()`,
`Reset()`, `Items()`

**CommandGroup:** `Heading` · **CommandItem:** `Text`, `Value`, `Icon`, `Shortcut` (a hint at
the end of the row; display only), `Keywords`, `IsEnabled` · **Events:** `Clicked` ·
**CommandSeparator:** a divider, hidden while the list is filtered

```xml
<ui:Command ItemSelected="OnCommand">
    <ui:CommandGroup Heading="Suggestions">
        <ui:CommandItem Text="Calendar" Icon="Calendar" Value="calendar" />
        <ui:CommandItem Text="Settings" Icon="Settings" Value="settings" Shortcut="Ctrl+," Keywords="preferences options" />
    </ui:CommandGroup>
</ui:Command>
```

**CommandPalette:** `Open` (two-way), `Placeholder`, `EmptyText`, `Command` (the list inside) ·
**Events:** `ItemSelected`, `OpenChanged` · **Methods:** `SetOpen(bool)`. Picking an item,
tapping the backdrop or pressing Escape closes it, and it opens with an empty search.

```xml
<ui:CommandPalette x:Name="Palette" ItemSelected="OnCommand">
    <ui:CommandGroup Heading="Pages"> ... </ui:CommandGroup>
</ui:CommandPalette>
```

```csharp
Palette.SetOpen(true);
```

On MAUI the name matches `Microsoft.Maui.Controls.Command` (the `ICommand` helper). In C# that
imports both namespaces, qualify one of them or add an alias:
`using UI = YourApp.Components.UI;`.

---

### Menubar
A row of menus, like a desktop app's File / Edit / View. Each menu opens a panel of items under
its title; clicking outside, picking an item or pressing Escape closes it, and opening one menu
closes the other.

```bash
shellui-native add menubar
```

**MenubarMenu:** `Title`, `IsOpen` (read-only) · **Events:** `Opened`, `Closed` · **Methods:**
`SetOpen(bool)`, `Close()`

**MenubarItem:** `Text`, `Icon`, `Shortcut` (a hint at the end of the row; display only),
`IsDestructive`, `IsEnabled` · **Events:** `Clicked` · **MenubarSeparator:** a divider

```xml
<ui:Menubar>
    <ui:MenubarMenu Title="File">
        <ui:MenubarItem Text="New tab" Shortcut="Ctrl+T" Clicked="OnNewTab" />
        <ui:MenubarSeparator />
        <ui:MenubarItem Text="Print" Icon="FileText" Clicked="OnPrint" />
    </ui:MenubarMenu>
    <ui:MenubarMenu Title="Edit"> ... </ui:MenubarMenu>
</ui:Menubar>
```

---

### Navbar
Top bar (56px, border below): a brand, links and actions at the end. Narrower than
`CollapseBelow`, the links move into a menu that a button opens under the bar; picking a link
closes it.

```bash
shellui-native add navbar
```

**Properties (Navbar):** `Title`, `Icon`, `Brand` (a view that replaces the icon and title),
`Actions` (a view at the end of the bar), `SelectedValue` (two-way), `CollapseBelow` (640; 0
never collapses), `IsMenuOpen` (two-way) · **Events:** `SelectedValueChanged`

**Properties (NavbarItem):** `Text`, `Value`, `Icon`, `IsEnabled` · **Events:** `Clicked`

```xml
<ui:Navbar Title="ShellUI" Icon="Sparkles" SelectedValue="{Binding Page}">
    <ui:NavbarItem Value="docs" Text="Docs" />
    <ui:NavbarItem Value="components" Text="Components" />
    <ui:Navbar.Actions>
        <ui:Button Icon="Search" Size="Icon" Variant="Ghost" />
    </ui:Navbar.Actions>
</ui:Navbar>
```

---

### BottomNav
Mobile tab bar pinned to the bottom of the screen, the phone take on a sidebar: icon tabs with
short labels, count badges and dots, and optionally a round action button in the middle. Put it
in the last row of the page. ShellUI for Blazor has no bottom nav, so this one is native only.

```bash
shellui-native add bottom-nav
```

**Variants:** `Default` (64px bar, border on top; a line slides to the active item), `Pill`
(80px; a pill grows behind the active icon, as in Material 3), `Floating` (an inset rounded bar
with a shadow; a filled pill slides behind the active icon, with the label under it, so long
labels stay readable), `Lifted` (the active icon rises into a ringed circle over the bar's top
edge, and the circle slides from item to item), `Expanding` (a compact floating capsule of
icons; the active one widens into a pill with its label). Switching items animates in about
200 ms, and a press scales the icon in slightly. A badge on an active pill or circle takes its
color as its ring.

**Properties (BottomNav):** `SelectedValue` (two-way), `Variant`, `Labels` (`Always`, `Active`:
only the active item shows its label and the others center their icon, `Never`; `Expanding`
always labels the active item only), `ActiveToken` (the active color in `Default`; `Foreground`
by default, e.g. `Primary` for a brand-colored bar), `ShowIndicator` (true), `BottomInset` (space
for the home indicator: the full-width bars fill it with their color, the floating ones sit
above it; by default it follows the safe area under the bar on phones, so only set it to
override), `ScrollSource` (a scroll view: scrolling it down slides the bar away, scrolling up
brings it back; put the bar over the content so the content can use the space), `IsHidden`
(two-way; hide or show it from code) · **Events:** `SelectedValueChanged`

**Properties (BottomNavItem):** `Text`, `Value`, `Icon`, `ActiveIcon` (shown while active),
`IconContent` (any view instead of the icon, such as an `Avatar`), `Badge` (a count, e.g. "3" or
"99+"), `Dot`, `Prominent` (a round action button; in a full-width bar it rises above the edge;
it raises `Clicked` and never becomes the selected item), `IsEnabled` · **Events:** `Clicked`
(on every press, also on the active item, e.g. to scroll to the top) · **Methods:** `Press()`

```xml
<ui:BottomNav SelectedValue="{Binding Page}" Variant="Floating" Labels="Active">
    <ui:BottomNavItem Value="home" Icon="House" Text="Home" />
    <ui:BottomNavItem Value="inbox" Icon="Inbox" Text="Inbox" Badge="3" />
    <ui:BottomNavItem Icon="Plus" Text="New" Prominent="True" Clicked="OnNew" />
    <ui:BottomNavItem Value="me" Text="Me" Dot="True">
        <ui:BottomNavItem.IconContent>
            <ui:Avatar Fallback="SS" Size="Sm" />
        </ui:BottomNavItem.IconContent>
    </ui:BottomNavItem>
</ui:BottomNav>
```

---

### Sidebar
App sidebar: a 256px column with a header, scrolling groups of items and a footer. Collapsed,
it is a 56px rail that shows the icons only. Put it in a column next to your content (or
inside a `Sheet` on phones).

```bash
shellui-native add sidebar
```

**Properties (Sidebar):** `IsCollapsed` (two-way), `SelectedValue` (two-way), `ExpandedWidth`
(256), `CollapsedWidth` (56), `Side` (Left, Right: the border is drawn on the inner edge),
`Header`, `Footer` · **Events:** `IsCollapsedChanged`, `SelectedValueChanged` · **Methods:**
`Toggle()`

**SidebarGroup:** `Label` · **SidebarItem:** `Text`, `Value`, `Icon`, `Badge`, `IsEnabled` ·
**Events:** `Clicked`

The header and footer are your own views: hide what doesn't fit the rail in
`IsCollapsedChanged`.

```xml
<Grid ColumnDefinitions="Auto,*">
    <ui:Sidebar x:Name="AppSidebar" SelectedValue="{Binding Page}">
        <ui:Sidebar.Header> ... </ui:Sidebar.Header>
        <ui:SidebarGroup Label="Workspace">
            <ui:SidebarItem Value="home" Text="Home" Icon="House" />
            <ui:SidebarItem Value="inbox" Text="Inbox" Icon="Inbox" Badge="12" />
        </ui:SidebarGroup>
    </ui:Sidebar>
    <ContentView Grid.Column="1"> ... </ContentView>
</Grid>
```

---

### Tabs
A muted pill-shaped list; the active tab is raised on the background color. Panels fade in on switch.

```bash
shellui-native add tabs
```

**Tabs:** `Value` (active tab, two-way) · **Events:** `ValueChanged(old, new)`
**TabsTrigger:** `Value`, `Text` · **TabsContent:** `Value`

```xml
<ui:Tabs Value="account">
    <ui:TabsList>
        <ui:TabsTrigger Value="account" Text="Account" />
        <ui:TabsTrigger Value="password" Text="Password" />
    </ui:TabsList>
    <ui:TabsContent Value="account">...</ui:TabsContent>
    <ui:TabsContent Value="password">...</ui:TabsContent>
</ui:Tabs>
```

---

### Breadcrumb
Trail with chevron separators; links turn foreground on hover.

**BreadcrumbItem:** `Text`, `IsCurrent` · **Events:** `Clicked` (not raised for the current item)

```xml
<ui:Breadcrumb>
    <ui:BreadcrumbItem Text="Home" Clicked="OnCrumb" />
    <ui:BreadcrumbItem Text="Components" Clicked="OnCrumb" />
    <ui:BreadcrumbItem Text="Breadcrumb" IsCurrent="True" />
</ui:Breadcrumb>
```

---

### Tree View
Tree of expandable rows with selection. Tapping a row selects it and, when it has children,
expands or collapses it. Nested `TreeViewItem`s are the children.

```bash
shellui-native add tree-view
```

**Properties (TreeView):** `SelectedValue` (two-way) · **Events:** `SelectedValueChanged`

**Properties (TreeViewItem):** `Text`, `Value` (defaults to `Text`), `Icon`, `IsExpanded` (two-way)

```xml
<ui:TreeView SelectedValue="{Binding Path}">
    <ui:TreeViewItem Text="src" Icon="Folder" IsExpanded="True">
        <ui:TreeViewItem Text="Components" Icon="Folder">
            <ui:TreeViewItem Text="Button.cs" Icon="File" />
        </ui:TreeViewItem>
        <ui:TreeViewItem Text="App.xaml" Icon="FileText" />
    </ui:TreeViewItem>
</ui:TreeView>
```

---

### Link Card
Tappable card with an optional icon, a title, a description and an arrow. Tapping raises
`Clicked` and opens `Url` in the system browser when one is set.

```bash
shellui-native add link-card
```

**Properties:** `Title`, `Description`, `Icon`, `Url` · **Events:** `Clicked`

```xml
<ui:LinkCard Title="Documentation" Description="Guides and the component reference."
             Icon="FileText" Url="https://example.com/docs" />
```

---

### Stepper
Step-by-step flow: numbered circles joined by lines (the active one filled, completed ones
checked), the active step's content, and Previous / Next / Confirm buttons. A step becomes
clickable once it has been reached.

```bash
shellui-native add stepper
```

**Properties (Stepper):** `CurrentStep` (two-way, 0-based), `ShowNavigation` (true),
`PreviousText`, `NextText`, `ConfirmText` · **Events:** `CurrentStepChanged`, `Confirmed` ·
**Methods:** `Next()`, `Previous()`

**Properties (StepperStep):** `Title`, `Description` · its child view is the step's content

```xml
<ui:Stepper Confirmed="OnDone">
    <ui:StepperStep Title="Account" Description="Your details">
        <ui:Input Placeholder="you@example.com" />
    </ui:StepperStep>
    <ui:StepperStep Title="Plan"> ... </ui:StepperStep>
    <ui:StepperStep Title="Review"> ... </ui:StepperStep>
</ui:Stepper>
```

---

### Pagination
Previous / page numbers / Next, with ellipses for skipped ranges. The current page is outlined;
Previous and Next disable at the ends.

```bash
shellui-native add pagination
```

**Properties:** `Page` (two-way, 1-based), `TotalPages`, `SiblingCount` (pages shown each side of
the current one, default 1), `ShowLabels` (text next to the chevrons) · **Events:** `PageChanged`

```xml
<ui:Pagination Page="{Binding Page}" TotalPages="20" PageChanged="OnPageChanged" />
```

---

### Prev Next Nav
Previous and next links for the foot of a page: two bordered cards side by side, each a small
"Previous" or "Next" over the page's name. A side with no label is left empty.

```bash
shellui-native add prev-next-nav
```

**Properties:** `PreviousLabel`, `NextLabel`, `PreviousText` ("Previous"), `NextText` ("Next"),
`IsEnabled` · **Events:** `PreviousClicked`, `NextClicked`

```xml
<ui:PrevNextNav PreviousLabel="Installation" NextLabel="Theming"
                PreviousClicked="OnPrevious" NextClicked="OnNext" />
```

---

## Data Display & Feedback

### Badge
Pill label. **Variants:** Default, Secondary, Outline, Destructive, Success, Warning, Info

```xml
<ui:Badge Text="New" Variant="Success" />
```

### Copy Button
Ghost icon button that copies `Text` to the clipboard; the icon turns into a check for two
seconds. `Label` adds text next to the icon.

```bash
shellui-native add copy-button
```

**Properties:** `Text` (what gets copied), `Label` · **Events:** `Copied` · **Methods:** `CopyAsync()`

```xml
<ui:CopyButton Text="shellui-native add button" />
<ui:CopyButton Text="{Binding ShareUrl}" Label="Copy link" />
```

### Kbd
Keyboard key hint. **Properties:** `Text`

```xml
<ui:Kbd Text="Ctrl" /> <ui:Kbd Text="K" />
```

### Stat Card
Dashboard metric: a muted title with an optional icon, the value in large bold text, a change
pill tinted by trend, and an optional description.

```bash
shellui-native add stat-card
```

**Properties:** `Title`, `Value`, `Change`, `Trend` (Neutral, Up, Down), `Description`, `Icon`

```xml
<ui:StatCard Title="Total revenue" Value="$45,231.89" Change="+20.1%" Trend="Up"
             Icon="DollarSign" Description="from last month" />
```

### Timeline
Vertical list of events joined by a hairline. Each item has a marker (a dot, or a circled icon),
a title, a time and details.

```bash
shellui-native add timeline
```

**Properties (TimelineItem):** `Title`, `Time`, `Text`, `Icon`, `IsActive` · a child view becomes
the item's body

```xml
<ui:Timeline>
    <ui:TimelineItem Title="Order placed" Time="09:12" Text="We received your order." />
    <ui:TimelineItem Title="Shipped" Time="14:40" Icon="Send" IsActive="True">
        <ui:Badge Text="In transit" Variant="Info" HorizontalOptions="Start" />
    </ui:TimelineItem>
</ui:Timeline>
```

### Progress
8px bar; the track is the fill color at 20%. Animates to new values.
**Properties:** `Value`, `Maximum`, `Variant` (Default, Success, Warning, Destructive), `ShowLabel`

### Skeleton
Pulsing placeholder in the `Muted` token (opacity 1 → 0.5 → 1 every 2s, only while on screen).
**Properties:** `CornerRadius` (default 6) plus `WidthRequest` / `HeightRequest`

### Table
Rows inside a rounded border: a header row in the muted foreground, body rows divided by a
hairline that tint on hover, and an optional caption.

```bash
shellui-native add table
```

**Properties (Table):** `Columns` (Grid column widths, e.g. `2*,*,*,100`), `Caption`, `Bordered`
(true), `MinimumContentWidth` (below this width the table scrolls sideways instead of squeezing)

**TableRow:** `IsSelected`, `Tapped` event · **TableHead:** `Text`, `HorizontalTextAlignment` ·
**TableCell:** `Text`, `HorizontalTextAlignment`, `IsBold`, or any child view

```xml
<ui:Table Columns="*,*,1.4*,90" Caption="A list of your recent invoices.">
    <ui:TableHeader>
        <ui:TableHead Text="Invoice" />
        <ui:TableHead Text="Status" />
        <ui:TableHead Text="Method" />
        <ui:TableHead Text="Amount" HorizontalTextAlignment="End" />
    </ui:TableHeader>
    <ui:TableRow Tapped="OnRowTapped">
        <ui:TableCell Text="INV001" IsBold="True" />
        <ui:TableCell><ui:Badge Text="Paid" Variant="Success" HorizontalOptions="Start" /></ui:TableCell>
        <ui:TableCell Text="Credit Card" />
        <ui:TableCell Text="$250.00" HorizontalTextAlignment="End" />
    </ui:TableRow>
</ui:Table>
```

Use star and fixed widths in `Columns`: every row is its own grid, so an `Auto` column would size
per row. For long lists, put a `TableHeader` above a `CollectionView` whose item template is a
`TableRow` with the same `Columns` set on the row itself.

### Data Table
A `Table` filled from a list of objects, with a filter field, sortable column headers, optional
row selection and pages. Each column names the item property it shows.

```bash
shellui-native add data-table
```

**Properties (DataTable):** `ItemsSource` (any list; an `ObservableCollection` refreshes the
table as it changes), `ShowFilter` (true), `FilterPlaceholder`, `Filter` (the filter text),
`ShowPagination` (true), `PageSize` (10), `ShowSelection` (false: adds a checkbox column),
`EmptyText` ("No results."), `MinimumContentWidth`, `SelectedItems`, `Page`, `PageCount`
(read-only) · **Events:** `SelectionChanged` (the selected items), `RowTapped` (the row's item)
· **Methods:** `Refresh()`, `GoToPage(int)`, `SortBy(column, descending)`, `ClearSelection()`

**Properties (DataTableColumn):** `Header`, `Property` (the item property to show), `Format` (a
.NET format string such as `C` or `MMM d, yyyy`), `Width` (`*`, `2*` or a fixed number),
`IsSortable` (true), `IsBold`, `HorizontalTextAlignment`, `Value` (a function that reads the
value; set in code, replaces `Property`)

```xml
<ui:DataTable ItemsSource="{Binding Invoices}" ShowSelection="True" PageSize="5">
    <ui:DataTableColumn Header="Invoice" Property="Number" IsBold="True" />
    <ui:DataTableColumn Header="Status" Property="Status" />
    <ui:DataTableColumn Header="Amount" Property="Amount" Format="C" Width="120" HorizontalTextAlignment="End" />
</ui:DataTable>
```

The filter matches every typed word against the text of any column. A sortable header cycles
ascending, descending and unsorted; numbers and dates sort by value. The header checkbox selects
every row that passes the filter, across pages. Cells show text only: for badges or buttons in
cells, build the rows yourself with `Table`.

`Property` is read by name at run time. In an app published with trimming or NativeAOT, set
`Value` in code instead, so the property is not trimmed away:

```csharp
InvoiceTable.Columns[0].Value = item => ((Invoice)item).Number;
```

---

### Chat
A conversation panel: an optional header, a scrolling list of messages that follows the newest
one, and an optional footer, usually a `ChatInput`. Give it a height.

```bash
shellui-native add chat
```

**Properties (Chat):** `Header`, `Footer` (views), `AutoScroll` (true: follows new messages and
a message that grows, unless the reader has scrolled up), `Messages` · **Methods:**
`ScrollToEnd()`

**Properties (ChatMessage):** `Role` (Assistant, User, System), `Text`, `Sender`, `Timestamp`,
`IsStreaming` (three bouncing dots while a reply is arriving), `AvatarText` (initials),
`Avatar` (a view; replaces the initials), `Actions` (a view under the bubble) · its child view
goes inside the bubble, under the text

**Properties (ChatInput):** `Text` (two-way), `Placeholder`, `IsLoading` (a spinner in the send
button; blocks sending), `Actions` (a view before the text) · **Events:** `Submitted` (the
trimmed text; the box is cleared) · **Methods:** `Submit()`, `Focus()`

```xml
<ui:Chat x:Name="Conversation" HeightRequest="420">
    <ui:ChatMessage Role="Assistant" Sender="ShellUI" AvatarText="S" Text="How can I help?" />
    <ui:ChatMessage Role="User" Text="Show me the button." />
    <ui:Chat.Footer>
        <ui:ChatInput Submitted="OnSend" />
    </ui:Chat.Footer>
</ui:Chat>
```

```csharp
private void OnSend(object? sender, string text)
{
    Conversation.Messages.Add(new ChatMessage { Role = ChatRole.User, Text = text });
    var reply = new ChatMessage { Role = ChatRole.Assistant, IsStreaming = true };
    Conversation.Messages.Add(reply);
    // later: reply.Text = "..."; reply.IsStreaming = false;
}
```

---

### Carousel
One slide at a time in a rounded frame. Swipe, use the round arrow buttons or tap a dot to move.
Each child view is a slide.

```bash
shellui-native add carousel
```

**Properties:** `Position` (two-way), `Loop` (true), `ShowArrows`, `ShowDots`, `AutoPlay`,
`AutoPlayInterval` (ms, default 3000), `AspectRatio` (width / height, default 16:9; `0` to size it
with `HeightRequest` instead) · **Events:** `PositionChanged` · **Methods:** `Next()`, `Previous()`

```xml
<ui:Carousel AspectRatio="2" AutoPlay="True">
    <Image Source="one.jpg" Aspect="AspectFill" />
    <Image Source="two.jpg" Aspect="AspectFill" />
</ui:Carousel>
```

### Image Viewer
A thumbnail that opens the image over the page on a dark backdrop, with zoom out, reset, zoom in
and close controls. A tap on the backdrop or Escape closes it. Give it a size: the thumbnail
fills it.

```bash
shellui-native add image-viewer
```

**Properties:** `Source`, `ThumbnailSource` (a smaller file for the page; `Source` then loads
only in the viewer), `Description` (for screen readers), `Aspect` (how the thumbnail fills its
box; AspectFill), `Open` (two-way), `Zoom` (read-only; 0.5 to 4, 1 fits the page) ·
**Events:** `OpenChanged` · **Methods:** `SetOpen(bool)`, `ZoomIn()`, `ZoomOut()`, `ResetZoom()`

```xml
<ui:ImageViewer Source="photo.jpg" Description="The harbour at dusk"
                WidthRequest="160" HeightRequest="120" />
```

Zooming scales the image around its centre; it does not pan.

---

### Empty State
Placeholder for an empty list or screen: icon in a muted tile, title, description and optional
actions, centered. `Bordered="True"` adds a dashed outline.

```bash
shellui-native add empty-state
```

**Properties:** `Icon`, `Title`, `Description`, `Bordered` · child views become the action row

```xml
<ui:EmptyState Icon="Folder" Title="No projects yet" Bordered="True"
               Description="Create your first project to get started.">
    <ui:Button Text="Create project" Icon="Plus" />
</ui:EmptyState>
```

### Avatar
Circular image over a muted fallback — initials, or a user icon when `Fallback` is empty. The
fallback shows until the image loads and stays if it fails.

```bash
shellui-native add avatar
```

**Properties:** `Source` (ImageSource), `Fallback` (initials), `Size` (Sm 32, Default 40, Lg 48, Xl 64)

```xml
<ui:Avatar Source="profile.png" Fallback="CN" />
<ui:Avatar Fallback="JD" Size="Lg" />
```

### Spinner
Rotating loader icon; spins only while on screen. **Properties:** `Size` (Sm 16, Default 24, Lg 32), `Token` (color, default Foreground), `IsRunning`

```xml
<ui:Spinner />
<ui:Spinner Size="Lg" Token="MutedForeground" />
```

### Toast
Sonner-style notifications that stack in a corner, slide in, pause while hovered and dismiss
themselves (4s default). Call the static API from anywhere — toasts float above the current page.
A `<ui:Toaster />` is optional: declare one anywhere on a page only to change position or count.

```bash
shellui-native add toast
```

```xml
<ui:Toaster Position="BottomRight" MaxVisible="3" />
```

```csharp
Toast.Show("Event has been created", "Sunday, December 03 at 9:00 AM");
Toast.Success("Profile saved");
Toast.Error("Upload failed", "The file is larger than 10 MB.");
Toast.Warning("Storage almost full");
Toast.Info("New version available");
var id = Toast.Show("Message archived", actionText: "Undo", action: Undo);
Toast.Dismiss(id);
```

**Toaster:** `Position` (BottomRight, BottomCenter, TopRight, TopCenter), `MaxVisible` (default 3)

### Callout
Highlighted note on a faint tint of the variant color, with an icon, an optional title, text
and/or any child content. **Variants:** Info, Warning, Danger, Tip, Default

```bash
shellui-native add callout
```

**Properties:** `Variant`, `Title`, `Text`, `Icon` (overrides the variant's icon) · child view becomes the body

```xml
<ui:Callout Variant="Tip" Title="Tip" Text="Press Escape to close the overlay on top." />
<ui:Callout Variant="Warning" Title="Heads up">
    <Label Text="Any content can go here." />
</ui:Callout>
```

### Alert
Bordered callout with an icon. **Properties:** `Title`, `Message`, `Variant` (Default, Destructive, Success, Warning, Info)

```xml
<ui:Alert Title="Heads up!" Message="You can add components to your app using the CLI." />
<ui:Alert Title="Error" Message="Your session has expired." Variant="Destructive" />
```

---

## Charts

Bar, line, area, pie, donut, radar and radial charts. They are drawn by the components
themselves (MAUI's `GraphicsView`, Avalonia's `Render`), so they need no charting package, and
they take their colors from the theme: `Chart1` to `Chart5`, in order. Every chart has a legend,
a tooltip under the pointer (or the finger) and a grow-in animation.

```bash
shellui-native add bar-chart line-chart area-chart pie-chart donut-chart radar-chart radial-chart
```

Each chart installs `chart`, the shared core. The chart colors are new theme tokens: a project
whose `shell` came from an older CLI version needs `shellui-native update shell` first.

### Data

A chart's children are its series; `Labels` names the categories (or the slices of a pie). In
XAML both are comma-separated lists.

**ChartSeries:** `Name` (shown in the legend and tooltip), `Values`, `Color` (replaces the
theme color for that series)

```xml
<ui:BarChart Labels="Jan, Feb, Mar, Apr, May, Jun" HeightRequest="260">
    <ui:ChartSeries Name="Desktop" Values="186, 305, 237, 73, 209, 214" />
    <ui:ChartSeries Name="Mobile" Values="80, 200, 120, 190, 130, 140" />
</ui:BarChart>
```

From code:

```csharp
Sales.Labels = new ChartLabels(months);
Sales.Series.Add(new ChartSeries { Name = "Desktop", Values = new ChartValues(desktopVisits) });
```

Adding or removing a series, or setting a series' `Values`, `Name` or `Color`, redraws the chart.
After changing numbers inside an existing `Values` list, call `Refresh()`.

**Every chart:** `Labels`, `ShowLegend` (true), `ShowTooltip` (true), `IsAnimated` (true),
`ValueFormat` (a .NET number format; `#,0.##`), `EmptyText` ("No data"), `Description` (for
screen readers) · **Methods:** `Refresh()`, `PlayAnimation()`

**Bar, line and area charts also have:** `ShowGrid`, `ShowXAxis`, `ShowYAxis` (all true),
`Minimum`, `Maximum` (the ends of the value axis; by default they fit the data and include zero)

Give a chart a height (`HeightRequest`, or `Height` on Avalonia).

### Bar Chart
One bar per series for each category. **Properties:** `IsStacked`, `IsHorizontal` (bars lie
down; negative values go the other way), `CornerRadius` (4), `MaxBarSize` (56)

```xml
<ui:BarChart Labels="Jan, Feb, Mar" IsStacked="True" HeightRequest="260"> ... </ui:BarChart>
```

### Line Chart
One line per series, from edge to edge of the plot. **Properties:** `IsSmooth` (true; false
joins the points with straight segments), `ShowDots`, `StrokeWidth` (2)

### Area Chart
A line per series with the space under it filled. **Properties:** `IsSmooth` (true),
`IsStacked`, `FillOpacity` (0.4), `StrokeWidth` (2)

### Pie Chart and Donut Chart
The first series' values as slices, clockwise from twelve o'clock, named by `Labels` and colored
from the chart colors in order. The tooltip shows a slice's value and its share. `DonutChart` is
a pie with a hole, and its own component (`shellui-native add donut-chart`, which brings `pie-chart`). **Properties:** `InnerRadius` (0 for a pie, 0.6 for a donut), `Gap` (degrees
between slices; 1), `ShowLabels` (the value outside each slice), `CenterValue`, `CenterLabel`
(text in a donut's hole)

```xml
<ui:DonutChart Labels="Chrome, Safari, Firefox" CenterValue="662" CenterLabel="Visitors" HeightRequest="260">
    <ui:ChartSeries Values="275, 200, 187" />
</ui:DonutChart>
```

### Radar Chart
One spoke per category around a ringed grid, and a filled shape per series. It needs at least
three categories. **Properties:** `Maximum` (the value at the outer ring), `FillOpacity` (0.5),
`ShowDots`

### Radial Chart
The first series' values as concentric rings on a muted track, each filling clockwise towards
`Maximum`; the first value is the outer ring. **Properties:** `Maximum` (a full ring; by default
the largest value), `InnerRadius` (0.35), `IsRounded` (true), `CenterValue`, `CenterLabel`

```xml
<ui:RadialChart Labels="Move, Exercise, Stand" Maximum="100" CenterValue="72%" CenterLabel="Move" HeightRequest="260">
    <ui:ChartSeries Values="72, 48, 90" />
</ui:RadialChart>
```

### Colors
The chart colors are theme tokens, so they follow light and dark mode and can be replaced like
any other token, before the first page loads:

```csharp
ShellTheme.Light[ShellToken.Chart1] = Color.FromArgb("#E76E50");
ShellTheme.Dark[ShellToken.Chart1] = Color.FromArgb("#2662D9");
```

Past the fifth series (or slice) the colors repeat, fainter. A series' own `Color` wins over the
theme.

### What the charts don't do
Zooming and panning, mixing bars and lines in one chart, click events on a bar or slice, and very
large data sets (every point is drawn). For those, a charting library such as LiveCharts sits
fine next to ShellUI Native: color it from `ShellTheme.Get(ShellToken.Chart1)`.

To draw a chart of your own, derive from `ChartBase` and draw in `DrawChart` with the
`IChartCanvas` it hands you; the legend, tooltip and animation come with the base class.

---

## Overlay Components

### Dialog, Drawer, Sheet
Modal overlays with a dimmed backdrop (tap it to close) and open/close animations: the dialog
fades and zooms in, the drawer slides up with a grab handle, the sheet slides in from the side.
Dialog and sheet have a close (X) button.

**Declare them anywhere** — next to the button that opens them is fine. When opened, the content
is shown in a page-level layer above everything else (see *How overlays float* below).

```bash
shellui-native add dialog
shellui-native add drawer
shellui-native add sheet
```

**Properties:** `Open` (also `SetOpen(bool)`) · **Events:** `OpenChanged` · Drawer/Sheet: `Side` (Left, Right, Top, Bottom) · `SheetContent.MaxPanelWidth`: how wide a left or right sheet gets (384 by default, shadcn's `sm:max-w-sm`; never more than 75% of the window)

```xml
<ui:Dialog x:Name="EditDialog">
    <ui:DialogTrigger><ui:Button Text="Edit profile" Variant="Outline" /></ui:DialogTrigger>
    <ui:DialogContent>
        <ui:DialogHeader>
            <ui:DialogTitle Text="Edit profile" />
            <ui:DialogDescription Text="Make changes to your profile here." />
        </ui:DialogHeader>
        <ui:Input Text="Pedro Duarte" />
        <ui:DialogFooter>
            <ui:DialogClose><ui:Button Text="Cancel" Variant="Outline" /></ui:DialogClose>
            <ui:Button Text="Save changes" Clicked="OnSave" />
        </ui:DialogFooter>
    </ui:DialogContent>
</ui:Dialog>
```

`DialogTrigger` / `DrawerTrigger` / `SheetTrigger` and `DialogClose` wrap a Button or any view.
The trigger is optional: open from code with `EditDialog.SetOpen(true)`.

### Alert Dialog
A confirmation that requires a choice: no close button, and the backdrop doesn't dismiss it.
Declare it anywhere, like Dialog.

```bash
shellui-native add alert-dialog
```

**Properties:** `Title`, `Description`, `ConfirmText` ("Continue"), `CancelText` ("Cancel"; empty hides it), `ConfirmVariant` (ButtonVariant), optional extra content inside the tag
**Events:** `Confirmed`, `Cancelled` · **Methods:** `Task<bool> ShowAsync()`, `SetOpen(bool)`

```xml
<ui:AlertDialog x:Name="DeleteDialog"
                Title="Are you absolutely sure?"
                Description="This action cannot be undone."
                ConfirmText="Delete account"
                ConfirmVariant="Destructive" />
```

```csharp
if (await DeleteDialog.ShowAsync())
    await DeleteAccountAsync();
```

`AlertDialogTrigger` wraps a Button to open it from XAML instead.

### Dropdown, Popover
Panels that float next to their trigger — below it, or above when there is no room, and always
inside the window. Clicking outside, or opening another dropdown, popover or select, closes them.

```bash
shellui-native add dropdown
shellui-native add popover
```

**Properties:** `IsOpen` (also `SetOpen`, `Toggle`, `Close`) · **Events:** `IsOpenChanged`
**DropdownItem:** `Text`, `Icon` · **Events:** `Clicked` (the menu closes first)

```xml
<ui:Dropdown>
    <ui:DropdownTrigger>
        <ui:Button Text="Open menu" Variant="Outline" Icon="ChevronDown" IconPosition="Right" />
    </ui:DropdownTrigger>
    <ui:DropdownContent>
        <ui:DropdownItem Text="Profile" Icon="User" Clicked="OnProfile" />
        <ui:DropdownItem Text="Settings" Icon="Settings" Clicked="OnSettings" />
    </ui:DropdownContent>
</ui:Dropdown>

<ui:Popover>
    <ui:PopoverTrigger><ui:Button Text="Open popover" Variant="Outline" /></ui:PopoverTrigger>
    <ui:PopoverContent>
        <Label Text="Dimensions" />
        <ui:Input Placeholder="Width" />
    </ui:PopoverContent>
</ui:Popover>
```

### Context Menu
Right-click the trigger (long-press on touch) to open a menu at the pointer. Clicking outside,
picking an item or pressing Escape closes it.

```bash
shellui-native add context-menu
```

**ContextMenuItem:** `Text`, `Icon`, `Shortcut` (hint text, display only), `IsDestructive`,
`IsEnabled` · **Events:** `Clicked` · **ContextMenuSeparator** divides groups ·
`ContextMenu.OpenAt(point)` opens it from code

```xml
<ui:ContextMenu>
    <ui:ContextMenuTrigger>
        <Border HeightRequest="120"> ... </Border>
    </ui:ContextMenuTrigger>
    <ui:ContextMenuContent>
        <ui:ContextMenuItem Text="Copy" Icon="Copy" Shortcut="Ctrl+C" Clicked="OnCopy" />
        <ui:ContextMenuSeparator />
        <ui:ContextMenuItem Text="Delete" Icon="Trash2" IsDestructive="True" Clicked="OnDelete" />
    </ui:ContextMenuContent>
</ui:ContextMenu>
```

On touch devices the menu opens under the trigger rather than at the finger.

### Tooltip
Small label shown above (or below) a view after the pointer rests on it. Pointer devices only.

```bash
shellui-native add tooltip
```

**Properties:** `Text`, `Placement` (Top, Bottom), `Delay` (ms, default 400)

```xml
<ui:Tooltip Text="Add to library">
    <ui:Button Icon="Plus" Size="Icon" Variant="Outline" />
</ui:Tooltip>
```

### Hover Card
Rich content that floats next to its trigger while the pointer is over the trigger or the card.

```bash
shellui-native add hover-card
```

**Properties:** `OpenDelay` (300 ms), `CloseDelay` (200 ms), `IsOpen`

On touch devices (no pointer hover) tapping the trigger toggles the card, and tapping outside
closes it.

```xml
<ui:HoverCard>
    <ui:HoverCardTrigger><ui:Button Text="@shellui" Variant="Link" /></ui:HoverCardTrigger>
    <ui:HoverCardContent>
        <Label Text="Beautifully designed components for .NET." />
    </ui:HoverCardContent>
</ui:HoverCard>
```

### Closing overlays from the keyboard
Open dialogs, drawers, sheets and popups (select, combobox, dropdown, popover, date and time
pickers) are tracked newest-last in `ShellDismiss`. **Escape** on Windows and the **back button**
on Android close the one on top; an Alert Dialog treats that as Cancel. With nothing open the key
keeps its normal behavior. Call `ShellDismiss.DismissTop()` to do the same from your own code.

### How overlays float
`ShellPortal` (in `Shell.cs`) keeps one layer above each page's content. As the page appears, its
content is wrapped in a `Grid`, once, and the layer is that Grid's last child. Dialogs, drawers, sheets, menus, selects, tooltips, hover cards and toasts are all
placed in that layer, so they are never clipped by a `ScrollView` and always draw on top. Content
moved there keeps a link to its component, so bindings and lookups keep working.

The layer is edge-to-edge: backdrops dim the whole window, drawers and sheets run under the
system bars, and their content (plus popups and toasts) is kept clear of the bars and notch with
`ShellPortal.GetSafeInsets`. If you build your own overlay chrome, call
`ShellPortal.EdgeToEdge(...)` on its layouts so MAUI doesn't inset them a second time.

---

## Long pages on Windows

WinUI gives a window 250 layout passes to settle, and a very large page can run out and crash at
startup with `Layout cycle detected`. The demo hit this when every component was on one page:
WinUI's trace showed layout still reaching new elements when the passes ran out, with `Border`
outline updates each asking for another pass. An ordinary screen is nowhere near the limit; a
"kitchen sink" page is.

If you hit it, lay out less at once: split the page, or keep sections collapsed
(`IsVisible="False"`) until they are needed, since hidden content is not laid out. The demo does
this, showing one category of components at a time.

---

## Adding Components

```bash
shellui-native add button              # single
shellui-native add button input card   # several
shellui-native add button --force      # overwrite an installed copy
```

## Listing Components

```bash
shellui-native list
shellui-native list --installed
shellui-native list --available
```

## Component Dependencies

Dependencies install automatically. Almost every component depends on `shell` (theme tokens and
core helpers); components that draw icons also depend on `icon`.

Parts of a compositional family (`dialog-trigger`, `accordion-item`, `card-header`, …) find their
parent at runtime, so they only build next to it. Adding a part on its own installs the whole
family: `shellui-native add dialog-trigger` installs `dialog` and all its parts.

| Component | Auto-installs |
|-----------|---------------|
| button | shell, icon, button-variants |
| card | shell, card-header, card-content, card-footer |
| select, combobox, time-picker, checkbox, alert, callout, breadcrumb-item, theme-toggle, avatar, spinner, toggle, pagination, empty-state | shell, icon |
| input-otp | shell |
| alert-dialog | shell, element-extensions, button |
| toast | shell, icon, button |
| dialog | shell, dialog-trigger, dialog-content, dialog-header, dialog-footer, dialog-title, dialog-description, dialog-close |
| drawer / sheet | shell, *-trigger, *-content (content also installs icon for the close button) |
| dropdown / popover | shell, *-trigger, *-content (+ dropdown-item) |
| table | shell |
| carousel | shell, icon |
| context-menu | shell, icon, element-extensions |
| stepper | shell, icon, button |
| toggle-group | shell, icon, element-extensions |
| number-input, stat-card, timeline | shell, icon |
| tag-input | shell, icon, wrap-layout |
| kbd | shell |
| wrap-layout | — |
| multi-select | shell, icon, wrap-layout |
| tree-view | shell, icon, element-extensions |
| copy-button, link-card | shell, icon |
| aspect-ratio | — |
| hover-card | shell, hover-card-trigger, hover-card-content |
| date-picker | shell, icon, calendar |
| tooltip | shell |
| collapsible / accordion / tabs | element-extensions + their sub-components |
| breadcrumb | breadcrumb-item |

**Upgrading an existing project:** components now require the new `Shell.cs` (theme tokens). If
your project was initialized earlier, refresh it once with `shellui-native add shell --force`.
