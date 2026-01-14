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
*(Coming Soon)* - Contextual feedback messages.

```bash
shellui-native add alert
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
