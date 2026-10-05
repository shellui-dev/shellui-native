# ShellUI Native Components Roadmap

Prioritized list of components to create for ShellUI Native, aligned with [ShellUI Components](https://github.com/shellui/shell-ui) patterns. All components should follow **compositional patterns** using Dependencies (parent + sub-components) instead of monolithic ChildContent.

Last revised: **2026-09-27** (theme tokens, icons and component polish on `feat/p3-navigation-layout`).

**Status at a glance:** P0 ✅ done · P1 ✅ done · P2 ✅ done · **P3 ⏭️ next (`feat/p3-navigation-layout`)** · P4–P7 backlog · Avalonia (Phase 2) unblocked, sequenced after P3 to avoid a moving target.

---

## Form Sizing Contract

Every single-line form control renders at **40px** high (matches shadcn `h-10` / default `ButtonSize.Default`). Multi-line controls use `MinimumHeightRequest`. Horizontal padding is 12px; vertical padding is 0 when `HeightRequest` is fixed (the fixed height owns the vertical rhythm), 8px when it isn't.

| Component | Height | Padding | Notes |
|-----------|--------|---------|-------|
| `Button` (Default) | 40 (from `ButtonStyle.Height`) | `(16, 0)` | Sm=36 `(12,0)`, Lg=44 `(32,0)`, Icon=40×40. Sizes to content (`HorizontalOptions=Start`) |
| `Input` | 40 | `(12, 0)` | Native `Entry` frame stripped (`ShellPlatform.StripNativeChrome`) — one border only |
| `Select` | 40 | `(12, 0)` | Custom-drawn trigger + floating list (rows 32) since 2026-09-27 |
| `DatePicker` | 40 | `(12, 0)` | Custom-drawn trigger + floating `Calendar` (32px day cells) since 2026-10-02 |
| `TimePicker` | 40 | `(12, 0)` | Custom-drawn trigger + floating hour / minute / AM-PM columns (32px cells) since 2026-10-04 |
| `Textarea` | `MinimumHeightRequest=80` | `(12, 8)` | Multi-line — grows with content |
| `TabsList` | 40 | `(4)` | Triggers fill the remaining 32 |
| `Checkbox` / `RadioGroupItem` | 16×16 | — | Icon-shaped, not a field (shadcn h-4 w-4) |
| `Switch` | 44×24 track, 20 thumb | inset 2 | Thumb travels exactly `44 - 2*2 - 20 = 20` |

**Rule for new form controls:** if it visually sits in a form row next to `Input`, it MUST be 40px tall. If it's a compositional container that hosts its own field (e.g. `Combobox`, `InputOTP`), the inner field carries the 40. Don't rely on platform default heights — MAUI's `Entry` / `Picker` defaults vary wildly across Android / iOS / Windows. When wrapping a platform control in a ShellUI `Border`, call `ShellPlatform.StripNativeChrome` so it doesn't draw a second frame inside ours.

**Menu / list rows** (`DropdownItem`, `Select` items) are 32 tall with `Padding = (8, 0)` inside a panel with 4px padding — shadcn's `px-2 py-1.5` rows.

**Row labels** that sit in a fixed-height row set both `VerticalOptions = Center` and `VerticalTextAlignment = Center`; without the latter a label that gets stretched to the row height draws its text at the top.

**Icon-shaped controls** keep their exact pixel dimensions — those numbers are the design, not a fill. Their outer row is the hit target.

---

## Design Token Contract

Since 2026-09-27 colors live in one place: `ShellTheme` in the `shell` template (`Shell.cs`). It
holds a `Light` and a `Dark` palette keyed by `ShellToken` (ShellUI's CSS variables:
`Background`, `Foreground`, `Card`, `Popover`, `Primary`, `Secondary`, `Muted`, `Accent`,
`Destructive`, `Border`, `Input`, `Ring`, plus `Success` / `Warning` / `Info` / `Overlay`), and
publishes the active palette as app resources (`ShellUIPrimary` Color + `ShellUIPrimaryBrush`
Brush, …) that it swaps on theme change. Values follow ShellUI's default neutral theme.

Components bind with `element.Token(property, ShellToken.X)` (a `SetDynamicResource` wrapper), so a
theme switch or a runtime override repaints everything. The `init` XAML
(`StyleTemplates.ThemeResourceDictionary`) mirrors the same palettes for design-time use.

**Rules:**
- Never write a hex color in a component template — `TemplateContentTests.MAUI_components_use_theme_tokens_not_hardcoded_colors` fails the build. Shadows may use `Colors.Black` with an opacity.
- Need a new role? Add it to `ShellToken`, both palettes and `StyleTemplates` (checked by `StyleTemplatesTests`), then use it.
- Avalonia (Phase 2) maps the same keys to `{DynamicResource ShellUIPrimary}` in its own dictionaries.

---

## Contributor Workflow: demo first, templates generated

Components are written and run as normal C# in `examples/MAUI.Demo/Components/UI/`, then copied
into the CLI templates by a script — the demo always tests exactly what `shellui-native add` installs.

1. Edit or add `examples/MAUI.Demo/Components/UI/<Name>.cs` and exercise it in `MainPage.xaml`.
2. New component? Add it to `NEW` in `scripts/sync-templates.py` (registry key, display name, category, tags).
3. `python scripts/sync-templates.py` — rewrites each `<Name>Template.cs` content string (namespace
   placeholder restored, quotes escaped) and recomputes `shell` / `icon` / `element-extensions` / `button`
   dependencies from the code. Running it twice writes nothing.
4. Register new templates in `ComponentRegistry`, then `dotnet test`.

Icons: `python scripts/generate-icons.py` regenerates `Icon.cs` from the ShellIcons catalog
(`../../icons/shell-icons`); edit its `ICONS` list to change the curated set.

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
| **tooltip** ✅ | P4.1 | shell | Tooltip | Done 2026-10-02 (floats in the page layer) |
| **toast** ✅ | P4.2 | shell, icon, button | Sonner | Done 2026-09-27 — `Toaster` host + static `Toast.Show/Success/Error/...` |
| **spinner** ✅ | P4.3 | shell, icon | Loading | Done 2026-09-27 as `spinner` (Loading's spinner variant) |
| **alert-dialog** ✅ | P4.4 | shell, element-extensions, button | AlertDialog | Done 2026-09-27 — `ShowAsync()` returns the choice |
| **hover-card** ✅ | P4.5 | shell, hover-card-trigger, hover-card-content | HoverCard | Done 2026-10-02 |

---

### P5 — Lower (Data Display)
| Component | Priority | Dependencies | ShellUI Ref | Notes |
|-----------|----------|--------------|-------------|-------|
| **avatar** ✅ | P5.1 | shell, icon | Avatar | Done 2026-09-27 |
| **table** ✅ | P5.2 | shell | Table | Done 2026-10-05 — one template (Table, TableHeader, TableRow, TableHead, TableCell); hover, selection, caption, sideways scroll |
| **empty-state** ✅ | P5.3 | shell, icon | EmptyState | Done 2026-10-04 — icon tile, title, description, action row, optional dashed border |
| **callout** ✅ | P5.4 | shell, icon | Callout | Done 2026-10-04 — Info / Warning / Danger / Tip / Default, tinted background |
| **pagination** ✅ | P5.5 | shell, icon | Pagination | Done 2026-10-04 — previous/next, sibling window, ellipses |
| **toggle** ✅ | — | shell, icon | Toggle | Done 2026-10-04 — pressed-state button (Default / Outline) |

---

### P6 — Lower (Advanced)
| Component | Priority | Dependencies | ShellUI Ref | Notes |
|-----------|----------|--------------|-------------|-------|
| **context-menu** ✅ | P6.1 | shell, icon, element-extensions | ContextMenu | Done 2026-10-05 — one template; opens at the pointer on right-click, under the trigger on long-press |
| **carousel** ✅ | P6.2 | shell, icon | Carousel | Done 2026-10-05 — one template; swipe, arrows, dots, loop, auto-play |
| **stepper** ✅ | P6.3 | shell, icon, button | Stepper | Done 2026-10-05 — one template (Stepper + StepperStep) with built-in navigation |
| **resizable** | P6.4 | — | Resizable | Resizable panels |
| **navbar** | P6.5 | nav-trigger, nav-content, nav-list, nav-item | Navbar | Collapsible nav bar |
| **sidebar** | P6.6 | — | Sidebar | App sidebar layout |

---

### P7 — Optional (Charts, Pickers, etc.)
| Component | Priority | Notes |
|-----------|----------|-------|
| chart (line, bar, pie, area) | P7 | Consider external chart lib integration |
| combobox ✅ | P7 | Done 2026-10-04 — Select with a filter field, Enter picks the first match |
| file-upload | P7 | File picker |
| input-otp ✅ | P7 | Done 2026-10-04 — slots over one hidden field (paste / autofill / numeric keyboard) |
| date-range-picker | P7 | Date range selection |
| toggle-group ✅ | — | Done 2026-10-05 — single or multiple selection, Default / Outline |
| number-input ✅ | — | Done 2026-10-05 — − / + buttons, min / max clamp, decimal value |
| tag-input ✅ | — | Done 2026-10-05 — chips with remove, Enter / comma / semicolon to commit |
| kbd ✅ | — | Done 2026-10-05 |
| stat-card ✅ | — | Done 2026-10-05 — title, value, trend pill, description, icon |
| timeline ✅ | — | Done 2026-10-05 — dot or icon markers joined by a line |
| multi-select ✅ | — | Done 2026-10-05 — searchable, chips in the trigger, panel stays open while picking |
| tree-view ✅ | — | Done 2026-10-05 — nested items, expand / collapse, selection |
| copy-button ✅ | P7 | Done 2026-10-05 — clipboard copy with a check confirmation |
| link-card ✅ | — | Done 2026-10-05 — tappable card that can open a URL |
| aspect-ratio ✅ | — | Done 2026-10-05 |
| wrap-layout ✅ | — | Done 2026-10-05 — wrapping layout used by tag-input and the demo's button rows |
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
- **Design tokens** — bind colors with `.Token(property, ShellToken.X)`; never hardcode a hex (see Design Token Contract)
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
