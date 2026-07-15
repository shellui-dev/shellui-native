# ShellUI Native Components Roadmap

Prioritized list of components to create for ShellUI Native, aligned with [ShellUI Components](https://github.com/shellui/shell-ui) patterns. All components should follow **compositional patterns** using Dependencies (parent + sub-components) instead of monolithic ChildContent.

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
| input | ✅ Done | — | Input |
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

### P1 — High (Modal / Overlay patterns)
*Compositional: Parent + Trigger + Content. Needed for most app flows.*

| Component | Priority | Dependencies | ShellUI Ref | Notes |
|-----------|----------|--------------|-------------|-------|
| **dialog** | P1.1 | dialog-trigger, dialog-content, dialog-header, dialog-footer, dialog-title, dialog-description, dialog-close | Dialog | Modal overlay, center screen |
| **drawer** | P1.2 | drawer-trigger, drawer-content | Drawer | Slide-out from side (Bottom/Left/Right/Top) |
| **sheet** | P1.3 | sheet-trigger, sheet-content | Sheet | Similar to Drawer, bottom sheet variant |
| **dropdown** | P1.4 | dropdown-trigger, dropdown-content, dropdown-item | Dropdown | Menu dropdown |
| **popover** | P1.5 | popover-trigger, popover-content | Popover | Floating content popover |

---

### P2 — High (Form completeness)
*Essential form controls for full form coverage.*

| Component | Priority | Dependencies | ShellUI Ref | Notes |
|-----------|----------|--------------|-------------|-------|
| **textarea** | P2.1 | — | Textarea | Multi-line text input |
| **select** | P2.2 | select-trigger, select-content, select-item | Select | Native-style picker or custom dropdown |
| **slider** | P2.3 | — | Slider | Range input |
| **radio-group** | P2.4 | radio-group-item | RadioGroup | Radio button group |
| **date-picker** | P2.5 | — | DatePicker | Date selection |
| **time-picker** | P2.6 | — | TimePicker | Time selection |

---

### P3 — Medium (Navigation & Layout)
| Component | Priority | Dependencies | ShellUI Ref | Notes |
|-----------|----------|--------------|-------------|-------|
| **tabs** | P3.1 | tabs-list, tabs-trigger, tabs-content | Tabs | Tabbed navigation |
| **accordion** | P3.2 | accordion-item, accordion-trigger, accordion-content | Accordion | Expandable sections |
| **collapsible** | P3.3 | collapsible-trigger, collapsible-content | Collapsible | Single expand/collapse |
| **breadcrumb** | P3.4 | breadcrumb-item | Breadcrumb | Breadcrumb nav |
| **scroll-area** | P3.5 | — | ScrollArea | Scrollable container |
| **skeleton** | P3.6 | — | Skeleton | Loading placeholder |

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

## Order of Implementation (Recommended)

1. **dialog** + dialog-trigger, dialog-content, dialog-header, dialog-footer, dialog-title, dialog-close
2. **drawer** + drawer-trigger, drawer-content
3. **tabs** + tabs-list, tabs-trigger, tabs-content
4. **textarea**
5. **select** + select-trigger, select-content, select-item
6. **dropdown** + dropdown-trigger, dropdown-content, dropdown-item
7. **slider**
8. **skeleton**
9. **accordion** + accordion-item, accordion-trigger, accordion-content
10. **popover** + popover-trigger, popover-content
