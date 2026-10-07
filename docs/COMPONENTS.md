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

**Properties:** `Text`, `Size` (Xs…Xxxl), `Weight` (Light…Bold), `Variant` (Default, Muted, Destructive, Success, Warning)

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

**Properties:** `Open` (also `SetOpen(bool)`) · **Events:** `OpenChanged` · Drawer/Sheet: `Side` (Left, Right, Top, Bottom)

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
