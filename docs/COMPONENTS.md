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

**Usage:**
```xml
<ui:Button Variant="Primary" Size="Lg" Text="Click me!" Clicked="OnClick" />
```

---

### Input
*(Coming Soon)*

Text input field with validation support.

```bash
shellui-native add input
```

---

### Label
*(Coming Soon)*

Text label for form fields.

```bash
shellui-native add label
```

---

### Checkbox
*(Coming Soon)*

Checkbox input for boolean values.

```bash
shellui-native add checkbox
```

---

### Switch
*(Coming Soon)*

Toggle switch for on/off states.

```bash
shellui-native add switch
```

---

## Layout Components

### Card
*(Coming Soon)*

Container for grouping related content.

```bash
shellui-native add card
```

**Sub-components:** CardHeader, CardTitle, CardDescription, CardContent, CardFooter

---

### Separator
*(Coming Soon)*

Visual divider between content.

```bash
shellui-native add separator
```

---

## Data Display

### Badge
*(Coming Soon)*

Small label for status or counts.

```bash
shellui-native add badge
```

---

### Skeleton
*(Coming Soon)*

Loading placeholder animation.

```bash
shellui-native add skeleton
```

---

### Progress
*(Coming Soon)*

Progress bar indicator.

```bash
shellui-native add progress
```

---

## Feedback

### Alert
*(Coming Soon)*

Contextual feedback messages.

```bash
shellui-native add alert
```

---

## Navigation

### Tabs
*(Coming Soon)*

Tabbed navigation interface.

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

# With --force to overwrite
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
