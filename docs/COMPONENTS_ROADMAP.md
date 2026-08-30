# ShellUI Native Components Roadmap

Prioritized list of components to create for ShellUI Native, aligned with [ShellUI Components](https://github.com/shellui/shell-ui) patterns. All components should follow **compositional patterns** using Dependencies (parent + sub-components) instead of monolithic ChildContent.

Last revised: **2026-08-29** (post PR #2 merge — Phase 1b live on `main`).

**Status at a glance:** P0 ✅ done · P1 ✅ done · P2 ✅ done · **P3 ⏭️ next (`feat/p3-navigation-layout`)** · P4–P7 backlog · Avalonia (Phase 2) unblocked, sequenced after P3 to avoid a moving target.

---

## Form Sizing Contract

Every single-line form control renders at **40px** high (matches shadcn `h-10` / default `ButtonSize.Default`). Multi-line controls use `MinimumHeightRequest`. Horizontal padding is 12px; vertical padding is 0 when `HeightRequest` is fixed (the fixed height owns the vertical rhythm), 8px when it isn't.

| Component | Height | Padding | Notes |
|-----------|--------|---------|-------|
| `Button` (Default) | 40 (from `ButtonStyle.Height`) | `(16, 10)` | Variant-driven: Sm=36, Lg=44, Icon=40×40 |
| `Input` | 40 | `(12, 0)` | Fixed 2026-08-29 — was unset + `(12, 8)`, rendered inconsistent across platforms |
| `Select` | 40 (on native `Picker`) | native | Native chrome — outer `Border` wrap removed 2026-08-30 (overflowed on Windows) |
| `DatePicker` | 40 (on native `DatePicker`) | native | Native chrome — outer `Border` wrap removed 2026-08-30 |
| `TimePicker` | 40 (on native `TimePicker`) | native | Native chrome — outer `Border` wrap removed 2026-08-30 |
| `Textarea` | `MinimumHeightRequest=80` | `(12, 8)` | Multi-line — grows with content |
| `Checkbox` | 20×20 (box) | — | Icon-shaped, not a field |
| `RadioGroupItem` | 20×20 (dot) | — | Icon-shaped, not a field |

**Rule for new form controls:** if it visually sits in a form row next to `Input`, it MUST be 40px tall. If it's a compositional container that hosts its own field (e.g. `Combobox`, `InputOTP`), the inner field carries the 40. Don't rely on platform default heights — MAUI's `Entry` / `Picker` defaults vary wildly across Android / iOS / Windows.

**Menu / list rows** (`DropdownItem`, `SelectItem` when added, `ContextMenuOption`) use `MinimumHeightRequest = 40` plus symmetric `Padding = (12, 12)` — they need a real touch target (44px iOS / 48dp Android guidance), not just enough space to draw the text.

**Icon-shaped controls** (`Checkbox` box 20×20, `RadioGroupItem` dot 20×20, `Switch` track 44×24) keep their exact pixel dimensions — those numbers are the design, not a fill. Their outer row inherits its hit area from the surrounding `HorizontalStackLayout`, which currently follows the icon height. If a future accessibility pass needs 44px touch targets for these, expand the container's `MinimumHeightRequest`, not the icon size.

---

## Design Token Contract

Templates hardcode ARGB strings today (no shared token file until Phase 2 lands the Avalonia `ResourceDictionary`). While we're still copy-pasting them, they MUST agree — otherwise "primary" reads as two different blues across the demo.

| Token | Value | Used by |
|-------|-------|---------|
| `Primary` | `#2563EB` | Button (Default), Checkbox (fill+border when checked), RadioGroupItem (fill+border when checked, fixed 2026-08-29), Switch (track when on), Progress (Default fill), Input (focus border, fixed 2026-08-29) |
| `Destructive` | `#EF4444` | Button (Destructive), Badge (Destructive), Progress (Destructive), Input (error border), Alert title (Destructive) |
| `Border` | `#E5E7EB` | Input (idle), Card (Default/Bordered), Checkbox (unchecked ring), RadioGroupItem (unchecked ring), Separator, Alert (Default) |
| `Muted background` | `#F3F4F6` | Badge (Secondary bg), Alert (Default bg) |
| `Foreground` | `#1F2937` | Input text, Label (Default), Checkbox label, Switch label, Alert title (Default) |
| `Muted foreground` | `#6B7280` | Progress label, Label (Muted variant) |
| `Placeholder` | `#9CA3AF` | Input placeholder |
| `Success` | `#22C55E` | Badge/Alert/Progress Success |
| `Warning` | `#F59E0B` | Badge/Alert/Progress Warning |

**Rule:** never introduce a new hex for a role that already has a token above. If you need a token that isn't listed, add it here first, then use it — that keeps Phase 2's `ResourceDictionary` extraction mechanical (grep the hex, replace with `{DynamicResource ShellUIPrimary}` in Avalonia XAML).

---

## Architectural Pattern: Dependencies over ChildComponent

**ShellUI Blazor approach:** Parent components use `CascadingValue` to provide themselves; child components use `[CascadingParameter]` to consume the parent. This enables:

- **Compositional API**: `Dialog` + `DialogTrigger` + `DialogContent` instead of a single `Dialog` with internal slots
- **Flexible layout**: Children can be placed anywhere in the tree
- **State sharing**: Parent holds state (Open, Value); Trigger/Content react to it

**ShellUI Native (MAUI) equivalent:** Use `Dependencies` in `ComponentMetadata` so `shellui-native add dialog` auto-installs `dialog-trigger`, `dialog-content`, etc. Children find parent via visual tree: `this.FindParentOfType<Dialog>()` or similar.

### Pattern Structure (from ShellUI)

| Parent | Sub-components (Dependencies) | Usage |
|--------|------------------------------|-------|
| Dialog | DialogTrigger, DialogContent, DialogHeader, DialogFooter, DialogTitle, DialogDescription, DialogClose | Modal overlay |
| Drawer | DrawerTrigger, DrawerContent | Slide-out panel |
| Sheet | SheetTrigger, SheetContent | Bottom/top sheet |
| Dropdown | DropdownTrigger, DropdownContent, DropdownItem | Dropdown menu |
| Popover | PopoverTrigger, PopoverContent | Popover overlay |
| Tabs | TabsList, TabsTrigger, TabsContent | Tabbed navigation |
| Accordion | AccordionItem, AccordionTrigger, AccordionContent | Expandable sections |
| Collapsible | CollapsibleTrigger, CollapsibleContent | Expand/collapse |
| Select | SelectTrigger, SelectContent, SelectItem | Select dropdown |
| HoverCard | HoverCardTrigger, HoverCardContent | Hover popover |
| ContextMenu | ContextMenuTrigger, ContextMenuContent, ContextMenuOption | Context menu |
| Card | CardHeader, CardContent, CardFooter | *(Already implemented)* |

---

## Priority Tiers

### P0 — Critical (Core UX) ✅ Mostly done
| Component | Status | Dependencies | ShellUI Ref |
|----------|--------|--------------|-------------|
| shell | ✅ Done | — | Shell |
| button | ✅ Done | button-variants | Button |
| button-variants | ✅ Done | — | ButtonVariants |
| input | ✅ Done (height fix 2026-08-29) | — | Input |
| label | ✅ Done | — | Label |
| checkbox | ✅ Done | — | Checkbox |
| switch | ✅ Done | — | Switch |
| card | ✅ Done | card-header, card-content, card-footer | Card |
| card-header | ✅ Done | — | CardHeader |
| card-content | ✅ Done | — | CardContent |
| card-footer | ✅ Done | — | CardFooter |
| separator | ✅ Done | — | Separator |
| badge | ✅ Done | — | Badge |
| progress | ✅ Done | — | Progress |
| alert | ✅ Done | — | Alert |

---

### P1 — High (Modal / Overlay patterns) ✅ Done
*Compositional: Parent + Trigger + Content. Needed for most app flows.*

| Component | Status | Dependencies | ShellUI Ref | Notes |
|-----------|--------|--------------|-------------|-------|
| **dialog** | ✅ Done | dialog-trigger, dialog-content, dialog-header, dialog-footer, dialog-title, dialog-description, dialog-close | Dialog | Modal overlay, center screen |
| **drawer** | ✅ Done | drawer-trigger, drawer-content | Drawer | Slide-out from side (Bottom/Left/Right/Top) |
| **sheet** | ✅ Done | sheet-trigger, sheet-content | Sheet | Similar to Drawer, bottom sheet variant |
| **dropdown** | ✅ Done | dropdown-trigger, dropdown-content, dropdown-item | Dropdown | Menu dropdown. Item hit target raised to 40px 2026-08-29 |
| **popover** | ✅ Done | popover-trigger, popover-content | Popover | Floating content popover |

---

### P2 — High (Form completeness) ✅ Done
*Essential form controls for full form coverage.*

| Component | Status | Dependencies | ShellUI Ref | Notes |
|-----------|--------|--------------|-------------|-------|
| **textarea** | ✅ Done | — | Textarea | Multi-line text input |
| **select** | ✅ Done | — | Select | Native `Picker` wrapper at 40px baseline. Trigger/Content split deferred until custom-dropdown variant is needed |
| **slider** | ✅ Done | — | Slider | Range input |
| **radio-group** | ✅ Done | radio-group-item | RadioGroup | Radio button group. Item checked-border fixed 2026-08-29 |
| **date-picker** | ✅ Done | — | DatePicker | Date selection |
| **time-picker** | ✅ Done | — | TimePicker | Time selection |

---

### P3 — Medium (Navigation & Layout) ⏭️ Next — `feat/p3-navigation-layout`
*See [DEVELOPMENT_PLAN.md § Phase 1c](./DEVELOPMENT_PLAN.md) for scope, order, and exit criteria.*

| Component | Priority | Dependencies | ShellUI Ref | Notes |
|-----------|----------|--------------|-------------|-------|
| **collapsible** | P3.1 (build first — primitive) | collapsible-trigger, collapsible-content, element-extensions | Collapsible | Single expand/collapse — the `IsOpen` + animation primitive `accordion-item` composes on top of. |
| **accordion** | P3.2 | accordion-item, accordion-trigger, accordion-content, element-extensions | Accordion | Multiple sections. `Type=Single` (radio-style) or `Multiple`. |
| **tabs** | P3.3 | tabs-list, tabs-trigger, tabs-content, element-extensions | Tabs | Tab bar + one visible panel. Trigger MUST render at the 40px baseline (row-level control). |
| **breadcrumb** | P3.4 | breadcrumb-item | Breadcrumb | Nav trail with separator between items. |
| **skeleton** | P3.5 | — | Skeleton | Animated grey block, sized by parent — use for perceived-perf on lists. |
| **scroll-area** | P3.6 | — | ScrollArea | `ScrollView` wrapper. Cross-platform scrollbar styling is thin — consider whether this is worth the wrapper vs documenting native `ScrollView`. |

---

### P4 — Medium (Feedback & Overlays)
| Component | Priority | Dependencies | ShellUI Ref | Notes |
|-----------|----------|--------------|-------------|-------|
| **tooltip** | P4.1 | — | Tooltip | Hover tooltip (simpler than Popover) |
| **toast** | P4.2 | — | Toast | Transient notification |
| **loading** | P4.3 | — | Loading | Loading spinner/state |
| **alert-dialog** | P4.4 | — | AlertDialog | Confirm/cancel dialog |
| **hover-card** | P4.5 | hover-card-trigger, hover-card-content | HoverCard | Hover-triggered popover |

---

### P5 — Lower (Data Display)
| Component | Priority | Dependencies | ShellUI Ref | Notes |
|-----------|----------|--------------|-------------|-------|
| **avatar** | P5.1 | — | Avatar | User avatar image/initials |
| **table** | P5.2 | table-header, table-body, table-row, table-cell, table-head | Table | Data table |
| **empty-state** | P5.3 | — | EmptyState | Empty list/state message |
| **callout** | P5.4 | — | Callout | Info/warning callout block |
| **pagination** | P5.5 | — | Pagination | Page navigation |

---

### P6 — Lower (Advanced)
| Component | Priority | Dependencies | ShellUI Ref | Notes |
|-----------|----------|--------------|-------------|-------|
| **context-menu** | P6.1 | context-menu-trigger, context-menu-content, context-menu-option | ContextMenu | Right-click / long-press menu |
| **carousel** | P6.2 | carousel-content, carousel-item, carousel-previous, carousel-next, carousel-dots | Carousel | Image/content carousel |
| **stepper** | P6.3 | stepper-list, stepper-step, stepper-content | Stepper | Step wizard |
| **resizable** | P6.4 | — | Resizable | Resizable panels |
| **navbar** | P6.5 | nav-trigger, nav-content, nav-list, nav-item | Navbar | Collapsible nav bar |
| **sidebar** | P6.6 | — | Sidebar | App sidebar layout |

---

### P7 — Optional (Charts, Pickers, etc.)
| Component | Priority | Notes |
|-----------|----------|-------|
| chart (line, bar, pie, area) | P7 | Consider external chart lib integration |
| combobox | P7 | Searchable select |
| file-upload | P7 | File picker |
| input-otp | P7 | OTP input |
| date-range-picker | P7 | Date range selection |
| theme-toggle | P7 | Already in demo; consider as component |
| copy-button | P7 | Copy to clipboard |
| toggle | P7 | Toggle button (vs Switch) |

---

## Existing Components: Alignment Checklist

Components that should be reviewed for consistency with the compositional pattern:

| Component | Current | Target | Action |
|-----------|---------|--------|--------|
| **Card** | ✅ Uses Dependencies (card-header, card-content, card-footer) | Keep | Card uses [ContentProperty("Children")] so XAML `<Card><CardHeader/><CardContent/><CardFooter/></Card>` works. Sub-components are layout slots (compositional). |
| **Button** | ✅ Uses Dependencies (button-variants) | Keep | Single component + variants; no Trigger/Content pattern. Correct. |
| **Alert** | No sub-components | Keep | Standalone feedback; no composition needed. |

**No breaking changes needed** for current components. New components (Dialog, Drawer, etc.) will follow the Trigger/Content pattern from the start.

---

## Implementation Notes

### Parent-finding in MAUI
For Trigger/Content components to talk to their parent:
```csharp
// Extension method (add to shared utilities)
public static T? FindParentOfType<T>(this Element element) where T : Element
{
    var parent = element.Parent;
    while (parent != null)
    {
        if (parent is T match) return match;
        parent = parent.Parent;
    }
    return null;
}
```

### DRY / SOLID
- **Single template per component** — one `.cs` file per logical component
- **Shared enums/variants** — `ButtonVariants`, `AlertVariant`, etc. in `Variants/` folder
- **Design tokens** — use `ShellTheme` or shared color constants
- **Composition over configuration** — prefer Trigger+Content over `IsModal`/`RenderMode` flags
- **Minimal surface** — only expose properties that map to real use cases

### Clean API (per ShellUI)
```xml
<!-- Good: compositional -->
<ui:Dialog IsOpen="{Binding IsOpen}" OpenChanged="OnOpenChanged">
    <ui:DialogTrigger>
        <ui:Button Text="Open" />
    </ui:DialogTrigger>
    <ui:DialogContent>
        <ui:DialogHeader>
            <ui:DialogTitle Text="Title" />
        </ui:DialogHeader>
        <Label Text="Body content" />
        <ui:DialogFooter>
            <ui:Button Text="Close" Clicked="Close" />
        </ui:DialogFooter>
    </ui:DialogContent>
</ui:Dialog>
```

---

## Order of Implementation

**Shipped on `main`** (Phase 1a + 1b — see [DEVELOPMENT_PLAN.md](./DEVELOPMENT_PLAN.md)):
P0 (15 components) → P1 (14 components across dialog / drawer / sheet / dropdown / popover
families) → P2 (6 components including textarea, select, slider, radio-group, date-picker,
time-picker). Total 44 templates, all platform-keyed via Template System v2.

**Up next — `feat/p3-navigation-layout`** (Phase 1c). Build the primitive before the
composite:

1. **collapsible** + collapsible-trigger, collapsible-content — establishes the `IsOpen` +
   animation pattern the rest of the tier reuses
2. **accordion** + accordion-item, accordion-trigger, accordion-content — accordion-item
   is collapsible with sibling coordination via `FindParentOfType<Accordion>()`
3. **tabs** + tabs-list, tabs-trigger, tabs-content — same show/hide pattern as accordion
   but always exactly one visible
4. **breadcrumb** + breadcrumb-item — standalone, no shared state
5. **skeleton** — standalone, animation primitive worth locking early so P5 (avatar,
   table, empty-state) can compose it
6. **scroll-area** — decide during implementation whether it's worth the wrapper vs
   documenting native `ScrollView`

**After Phase 1c → Phase 2 (Avalonia).** Do not open Phase 2 until Phase 1c merges — the
whole point of finishing P3 on MAUI first is to freeze the vocabulary before it doubles.

**Then Phase 1d / P4+ (feedback, data display, advanced).** Sequenced after Avalonia has
Avalonia-parity on P0/P1 at least, so new MAUI components don't get too far ahead again.
