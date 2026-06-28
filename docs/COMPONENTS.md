# Component Reference

Complete list of available ShellUI Native components.

## Form Components

### Button
Interactive button with multiple variants and sizes.

```bash
shellui-native add button
```

**Variants:** Default, Destructive, Outline, Secondary, Ghost

**Sizes:** Sm, Default, Lg, Icon

**Properties:**
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Variant | ButtonVariant | Default | Visual style variant |
| Size | ButtonSize | Default | Button size |
| Text | string | "" | Button text |
| IsLoading | bool | false | Shows loading indicator |
| IsEnabled | bool | true | Enable/disable button |

**Usage:**
```xml
<ui:Button Variant="Primary" Size="Lg" Text="Click me!" Clicked="OnClick" />
```

**Events:** `Clicked`

---

### Input
Text input field with validation support.

```bash
shellui-native add input
```

**Properties:**
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Text | string | "" | Input value (two-way binding) |
| Placeholder | string | "" | Placeholder text |
| IsPassword | bool | false | Mask input as password |
| HasError | bool | false | Show error state |
| IsReadOnly | bool | false | Prevent editing |
| MaxLength | int | int.MaxValue | Maximum characters |

**Usage:**
```xml
<ui:Input Placeholder="Enter email" Text="{Binding Email}" />
<ui:Input Placeholder="Password" IsPassword="True" />
<ui:Input Placeholder="Error state" HasError="True" />
```

**Events:** `TextChanged`, `Completed`

---

### Checkbox
Checkbox input with label and validation states.

```bash
shellui-native add checkbox
```

**Properties:**
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| IsChecked | bool | false | Checked state (two-way binding) |
| Label | string | "" | Label text displayed next to checkbox |
| HasError | bool | false | Show error state |
| IsEnabled | bool | true | Enable/disable checkbox |

**Usage:**
```xml
<ui:Checkbox Label="Accept terms" IsChecked="{Binding Accepted}" />
<ui:Checkbox Label="Has error" HasError="True" />
```

**Events:** `CheckedChanged`

---

### Switch
Toggle switch component with label support.

```bash
shellui-native add switch
```

**Properties:**
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| IsToggled | bool | false | Toggled state (two-way binding) |
| Label | string | "" | Label text displayed next to switch |
| IsEnabled | bool | true | Enable/disable switch |

**Usage:**
```xml
<ui:Switch Label="Enable notifications" IsToggled="{Binding NotificationsEnabled}" />
```

**Events:** `Toggled`

---

### Label (ShellLabel)
Typography label with size, weight, and color variants.

```bash
shellui-native add label
```

**Properties:**
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Text | string | "" | Label text |
| Size | LabelSize | Default | Font size (Xs, Sm, Default, Lg, Xl, Xxl, Xxxl) |
| Weight | LabelWeight | Normal | Font weight (Light, Normal, Medium, Semibold, Bold) |
| Variant | LabelVariant | Default | Color variant (Default, Muted, Destructive, Success, Warning) |

**Usage:**
```xml
<ui:ShellLabel Text="Heading" Size="Xl" Weight="Bold" />
<ui:ShellLabel Text="Subtitle" Size="Sm" Variant="Muted" />
<ui:ShellLabel Text="Error!" Variant="Destructive" />
```

---

## Layout Components

### Card
Container for grouping related content with header, content, and footer.

```bash
shellui-native add card
```

**Variants:** Default, Bordered, Elevated

**Properties:**
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Variant | CardVariant | Default | Visual style variant |
| IsPressable | bool | false | Enable tap interactions |

**Usage:**
```xml
<ui:Card Variant="Elevated">
    <ui:CardHeader Title="Card Title" Description="Optional description" />
    <ui:CardContent>
        <Label Text="Card body content goes here" />
    </ui:CardContent>
    <ui:CardFooter>
        <ui:Button Text="Action" />
    </ui:CardFooter>
</ui:Card>
```

**Events:** `Clicked` (when IsPressable=true)

---

### Separator
Visual divider/separator line for layout.

```bash
shellui-native add separator
```

**Variants:** Horizontal, Vertical

**Properties:**
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Orientation | SeparatorOrientation | Horizontal | Direction of separator line |

**Usage:**
```xml
<ui:Separator Orientation="Horizontal" />
<ui:Separator Orientation="Vertical" />
```

---

### CardHeader
Header section for Card with title and description.

```bash
shellui-native add card-header
```

**Properties:**
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Title | string | "" | Header title text |
| Description | string | "" | Optional subtitle/description |

---

### CardContent
Main content section for Card.

```bash
shellui-native add card-content
```

**Properties:**
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| NoPadding | bool | false | Remove default padding |

---

### CardFooter
Footer section for Card, typically used for actions.

```bash
shellui-native add card-footer
```

**Properties:**
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Orientation | StackOrientation | Horizontal | Layout direction |
| Justify | FooterJustify | End | Content alignment (Start, Center, End, SpaceBetween) |

---

## Data Display

### Badge
Small status indicator with color variants.

```bash
shellui-native add badge
```

**Variants:** Default, Secondary, Destructive, Outline, Success, Warning

**Properties:**
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Text | string | "" | Badge text |
| Variant | BadgeVariant | Default | Color variant |

**Usage:**
```xml
<ui:Badge Text="New" Variant="Success" />
<ui:Badge Text="Deprecated" Variant="Destructive" />
<ui:Badge Text="v1.0" Variant="Outline" />
```

---

### Progress
Progress bar indicator with percentage support.

```bash
shellui-native add progress
```

**Variants:** Default, Success, Warning, Destructive

**Properties:**
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Value | double | 0.0 | Current progress value |
| Maximum | double | 100.0 | Maximum value |
| Variant | ProgressVariant | Default | Color variant |
| ShowLabel | bool | false | Display percentage label |

**Usage:**
```xml
<ui:Progress Value="75" Maximum="100" ShowLabel="True" />
<ui:Progress Value="50" Variant="Success" />
```

**Computed Properties:** `Percentage` - Calculated percentage (0-100)

---

### Skeleton
*(Coming Soon)* - Loading placeholder animation.

```bash
shellui-native add skeleton
```

---

### Progress
*(Coming Soon)* - Progress bar indicator.

```bash
shellui-native add progress
```

---

## Feedback

### Alert
Contextual feedback messages with variants.

```bash
shellui-native add alert
```

**Variants:** Default, Destructive, Success, Warning, Info

**Properties:**
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Title | string | "" | Alert title text |
| Message | string | "" | Alert message/body text |
| Variant | AlertVariant | Default | Color variant |

**Usage:**
```xml
<ui:Alert Title="Success!" Message="Operation completed successfully" Variant="Success" />
<ui:Alert Title="Error" Message="Something went wrong" Variant="Destructive" />
<ui:Alert Message="Info message" Variant="Info" />
```

---

## Overlay Components

Modal dialogs, drawers, sheets, dropdowns, and popovers. Use compositional pattern: parent + trigger + content.

**Tip:** Place `Dialog`, `Drawer`, or `Sheet` at the page root (e.g. last child of a Grid) with `HorizontalOptions="Fill"` and `VerticalOptions="Fill"` so the overlay covers the full screen.

### Dialog
Modal dialog overlay.

```bash
shellui-native add dialog
```

**Usage:**
```xml
<ui:Dialog x:Name="MyDialog" Open="{Binding IsOpen}" OpenChanged="OnDialogOpenChanged">
    <ui:DialogTrigger>
        <ui:Button Text="Open Dialog" />
    </ui:DialogTrigger>
    <ui:DialogContent>
        <ui:DialogHeader>
            <ui:DialogTitle Text="Title" />
            <ui:DialogDescription Text="Optional description" />
            <ui:DialogClose />
        </ui:DialogHeader>
        <Label Text="Modal body content" />
        <ui:DialogFooter>
            <ui:Button Text="Close" Clicked="OnCloseDialog" />
        </ui:DialogFooter>
    </ui:DialogContent>
</ui:Dialog>
```

### Drawer
Slide-out panel (Left, Right, Top, Bottom).

```bash
shellui-native add drawer
```

**Properties:** `Open`, `Side` (DrawerSide: Left, Right, Top, Bottom)

### Sheet
Bottom/top sheet panel.

```bash
shellui-native add sheet
```

**Properties:** `Open`, `Side` (SheetSide: Left, Right, Top, Bottom)

### Dropdown
Dropdown menu.

```bash
shellui-native add dropdown
```

**Usage:**
```xml
<ui:Dropdown>
    <ui:DropdownTrigger>
        <ui:Button Text="Menu" />
    </ui:DropdownTrigger>
    <ui:DropdownContent>
        <ui:DropdownItem Text="Option 1" Clicked="OnOption1" />
        <ui:DropdownItem Text="Option 2" Clicked="OnOption2" />
    </ui:DropdownContent>
</ui:Dropdown>
```

### Popover
Floating popover panel.

```bash
shellui-native add popover
```

**Usage:**
```xml
<ui:Popover>
    <ui:PopoverTrigger>
        <ui:Button Text="Info" />
    </ui:PopoverTrigger>
    <ui:PopoverContent>
        <Label Text="Popover content here" />
    </ui:PopoverContent>
</ui:Popover>
```

---

## Navigation

### Tabs
*(Coming Soon)* - Tabbed navigation interface.

```bash
shellui-native add tabs
```

---

## Adding Components

```bash
# Single component
shellui-native add button

# Multiple components
shellui-native add button input card

# Card with all sub-components
shellui-native add card card-header card-content card-footer

# With --force to overwrite existing
shellui-native add button --force
```

## Listing Components

```bash
# All available components
shellui-native list

# Only installed
shellui-native list --installed

# Only available (not installed)
shellui-native list --available
```

## Component Dependencies

When you add a component, its dependencies are automatically installed:

| Component | Auto-installs |
|-----------|---------------|
| button | button-variants |
| card | card-header, card-content, card-footer |
| dialog | element-extensions, dialog-trigger, dialog-content, dialog-header, dialog-footer, dialog-title, dialog-description, dialog-close |
| drawer | element-extensions, drawer-trigger, drawer-content |
| sheet | element-extensions, sheet-trigger, sheet-content |
| dropdown | element-extensions, dropdown-trigger, dropdown-content, dropdown-item |
| popover | element-extensions, popover-trigger, popover-content |

## Component Categories

Components are organized into the following categories:

- **Form Components**: `button`, `input`, `label`, `checkbox`, `switch`
- **Layout Components**: `card`, `card-header`, `card-content`, `card-footer`, `separator`
- **Data Display**: `badge`, `progress`
- **Feedback**: `alert`
- **Overlay**: `dialog`, `drawer`, `sheet`, `dropdown`, `popover` (+ their trigger/content sub-components)
- **Utility**: `shell`
