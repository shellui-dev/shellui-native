# ShellUI Native — Development Plan

Living document tracking **branches**, **phases**, and **feature implementations** for
ShellUI Native. Companion to [PLAN.md](./PLAN.md) (long-term strategy) and
[COMPONENTS_ROADMAP.md](./COMPONENTS_ROADMAP.md) (prioritized component backlog).

Last revised: **2026-08-30** (Phase 1c in flight, P4 polish scoped).

---

## Branch Strategy

We use a phase-anchored branch model. `main` stays shippable; each phase lives on a
long-lived `feat/*` branch that is squash-merged into `main` when its exit criteria are hit.
Short-lived sub-branches (`feat/<phase>/<slice>`) cut off the phase branch and merge back
into it, not directly into `main`.

```
main   ← Phase 1a merged (2026-07-05), Phase 1b merged (2026-08-29 via PR #2)
 └─ feat/p3-navigation-layout         ← Phase 1c (active) — MAUI P3 tier (tabs, accordion, …)
 └─ feat/p4-overlay-portal            ← Phase 1d (queued after 1c) — portal rewrite + shadcn-style controls
 └─ feat/avalonia-implementation      ← Phase 2 (planned) — Avalonia templates + reference impl
 └─ feat/winui                        ← Phase 3 (conditional)
```

**Rules:**
- Never merge a phase branch to `main` without a green CI run and the exit criteria table checked.
- Never open a Phase 2 slice while Phase 1b (template system v2) is unmerged — it's the
  prerequisite.
- Docs (`docs/*.md`) may change on any branch; prefer conflict-free additions over edits to
  shared prose during parallel work.

---

## Current Snapshot

| Branch | Base | Status | Purpose |
|--------|------|--------|---------|
| `main` | — | Phase 1a + 1b merged | 44 platform-keyed components, `SupportsPlatform` registry, xUnit 199/199, sizing + token contracts locked |
| `feat/p3-navigation-layout` | `main` | **Next** | Phase 1c — MAUI P3 (tabs, accordion, collapsible, breadcrumb, scroll-area, skeleton) |

---

## Phase 1a — `feat/initial-foundation` (**merged 2026-07-05**)

The MAUI-only alpha foundation. Merged to `main` via PR #1.

### Delivered

**Infrastructure**
- [x] .NET 10 unified across `Core`, `Templates`, `CLI`, and `examples/MAUI.Demo`
- [x] [`global.json`](../global.json) pins SDK `10.0.100`
- [x] Solution migrated from `.sln` to [`.slnx`](../ShellUI.Native.slnx)
- [x] CI workflows on `dotnet-version: 10.0.x`
  ([ci.yml](../.github/workflows/ci.yml), [pr-check.yml](../.github/workflows/pr-check.yml),
  [release.yml](../.github/workflows/release.yml))
- [x] MAUI demo builds on Windows: `WindowsPackageType=None`, explicit
  `RuntimeIdentifier=win-x64`, explicit `Microsoft.Maui.Controls` +
  `Microsoft.Extensions.Logging.Debug` PackageReferences

**Platform model**
- [x] `NativePlatform.Avalonia` added to the enum
  ([NativePlatform.cs](../src/ShellUI.Native.Core/Models/NativePlatform.cs))
- [x] Avalonia project detection via `PackageReference` prefix or `App.axaml` presence
  ([ProjectDetector.cs](../src/ShellUI.Native.CLI/Services/ProjectDetector.cs))
- [x] Avalonia option in the `init` manual-selection prompt
  ([InitService.cs](../src/ShellUI.Native.CLI/Services/InitService.cs))
- [x] CLI description text reflects MAUI+Avalonia
  ([Program.cs](../src/ShellUI.Native.CLI/Program.cs))
- [x] MAUI-only warning in `ComponentInstaller` for non-MAUI target platforms
  ([ComponentInstaller.cs](../src/ShellUI.Native.CLI/Services/ComponentInstaller.cs))

**Components (see [COMPONENTS_ROADMAP.md](./COMPONENTS_ROADMAP.md) for full priority list)**
- [x] P0 complete: `shell`, `button`, `input`, `label`, `checkbox`, `switch`, `card` (+ header, content, footer), `separator`, `badge`, `progress`, `alert`
- [x] P1 complete: `dialog` (+ family), `drawer` (+ family), `sheet` (+ family), `dropdown` (+ family), `popover` (+ family)
- [x] P2 partial: `textarea`, `slider`, `select`, `radio-group` (+ item), `date-picker`, `time-picker`

**Template hygiene**
- [x] All 18 `RoundRectangle`-using templates now include `using Microsoft.Maui.Controls.Shapes;`
  in their `Content` string (previously produced non-compiling code on install)
- [x] All 14 compositional-container `Children` properties (Card, CardFooter, Dialog family,
  Drawer/Sheet/Popover/DropdownContent, RadioGroup — in both templates and demo copies) now
  use `public new IList<IView> Children`, silencing CS0108 shadow warnings against
  `TemplatedView.Children`
- [x] `Switch.IsEnabledProperty` and `Checkbox.IsEnabledProperty` marked `static new` — they
  are intentional shadows of `VisualElement.IsEnabledProperty` so the components can observe
  their own enable-state changes via `propertyChanged` handlers
- [x] `Switch` now uses `_ = TranslateToAsync(...)` instead of the obsolete `TranslateTo`
- [x] **Cold multi-TFM build**: `dotnet build ShellUI.Native.slnx -c Release` → **0 warnings,
  0 errors** across net10.0-android / ios / maccatalyst / windows10.0.19041.0

**Tests**
- [x] xUnit test project at [`tests/ShellUI.Native.Tests`](../tests/ShellUI.Native.Tests) —
  147 tests covering:
  - Every registered component has non-empty `Content`
  - Every template using `RoundRectangle` imports `Microsoft.Maui.Controls.Shapes`
    (regression lock for the 2026-07-04 bug)
  - Every template has a `YourProjectNamespace` placeholder (namespace replacement contract)
  - Every metadata `Name` matches its registry key
  - Every declared dependency is itself registered
  - `ProjectDetector.DetectPlatform` maps `.csproj` XML → `NativePlatform` correctly for
    MAUI / Avalonia (both PackageReference + `App.axaml` paths) / WinUI / WPF / Unknown, and
    prefers MAUI when a project declares both `UseMaui=true` and an `Avalonia` PackageReference
- [x] `ProjectDetector.DetectPlatform` promoted to `internal` + `InternalsVisibleTo` in
  [ShellUI.Native.CLI.csproj](../src/ShellUI.Native.CLI/ShellUI.Native.CLI.csproj) so tests
  hit the pure XML→enum core without cwd manipulation
- [x] CI runs `dotnet test` and uploads TRX + coverage results
  ([ci.yml](../.github/workflows/ci.yml))

### In progress / remaining before merge to `main`

- [x] `Element.FindParentOfType<T>()` is implemented in
  [`ElementExtensionsTemplate`](../src/ShellUI.Native.Templates/Templates/ElementExtensionsTemplate.cs)
  and auto-installed as a dependency by every overlay component (Dialog, Drawer, Sheet,
  Dropdown, Popover). Verified.
- [x] Windows CI head builds `MAUI.Demo` end-to-end via the `build-examples` job
  ([ci.yml](../.github/workflows/ci.yml)) — runs `dotnet workload install maui` then
  `dotnet build … --framework net10.0-windows10.0.19041.0`.
- [x] [COMPONENTS.md](./COMPONENTS.md) reconciled with the registry — removed stale
  "Coming Soon" entries, added docs for `textarea`, `slider`, `select`, `radio-group`,
  `date-picker`, `time-picker`, updated category summary.
- [x] [QUICKSTART.md](./QUICKSTART.md) — prerequisites now say .NET 10 SDK; platform
  detection line mentions Avalonia and links the "MAUI-only warning" behavior.
- [ ] Manually run `shellui-native init --yes` + `add button input card dialog` against a
  fresh `dotnet new maui` project to confirm the end-to-end install path works — needs a
  human on a workstation with the MAUI workload installed.
- [ ] Manually launch `MAUI.Demo` at least once to verify components render as expected
  (compilation is verified by CI, rendering is not).

### Exit criteria to merge → `main`

1. `dotnet build ShellUI.Native.slnx` green
2. `dotnet build examples/MAUI.Demo/MAUI.Demo.csproj --framework net10.0-windows10.0.19041.0` green in CI
3. `shellui-native init --yes` + `shellui-native add button input card dialog` works end-to-end on a fresh MAUI project
4. `COMPONENTS.md` is honest about what's shipped

---

## Phase 1b — `feat/template-system-v2` (**merged 2026-08-29 via [PR #2](https://github.com/shellui-dev/shellui-native/pull/2)**)

Focused refactor: the template layer became platform-keyed so adding Avalonia is a per-template
dict entry rather than a whole parallel registry. Prerequisite for Phase 2, now unblocked.

### Problem (context)

Before this branch, [`ComponentRegistry.GetComponentContent`](../src/ShellUI.Native.Templates/ComponentRegistry.cs)
returned a single MAUI-flavored C# string per component name. There was no platform dimension in
the lookup path. Adding Avalonia templates would have meant either duplicating the registry or
string-mangling MAUI templates at install time.

### Design

Per-template `Contents` dictionary owned by the template class; registry stores
`(Metadata, Contents)` bundles so lookup is one hop — no per-component switch arms to maintain
when a new platform is added.

```csharp
public static class ButtonTemplate
{
    public static ComponentMetadata Metadata => new() { ... };
    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new()
    {
        [NativePlatform.MAUI] = @"..."
        // [NativePlatform.Avalonia] = @"..."  ← added in Phase 2, one line, no registry edit
    };
}
```

### Deliverables

- [x] `ComponentRegistry.GetComponentContent(name, NativePlatform)` signature
- [x] Every template migrated from `public static string Content =>` to
  `public static IReadOnlyDictionary<NativePlatform, string> Contents { get; }` (all 44 templates)
- [x] Registry stores `(Meta, Contents)` bundles instead of just metadata; adds
  `SupportsPlatform(name, platform)` and `GetSupportedPlatforms(name)`
- [x] `ComponentInstaller` uses `config.TargetPlatform`; checks `SupportsPlatform` before
  installing and prints a clear error listing supported platforms if the combo isn't ready
- [x] `InitService.InstallShellUtilityAsync` and `ComponentManager.UpdateComponents` updated
  to pass the target platform through
- [x] New `UnsupportedPlatformException` typed exception in
  [Core.Models](../src/ShellUI.Native.Core/Models/UnsupportedPlatformException.cs) with
  `ComponentName`, `RequestedPlatform`, `SupportedPlatforms` for programmatic callers
- [x] Blanket "MAUI-only" warning removed from `ComponentInstaller` — replaced with per-component
  platform check (silent when the platform IS supported)

### Verification

- [x] `dotnet build src/` → 0 warnings, 0 errors
- [x] `dotnet test` → **199/199 passing** (up from 147 — 52 new/updated tests):
  - `TemplateContentTests` parameterized on `NativePlatform.MAUI`
  - `Every_component_reports_MAUI_as_a_supported_platform` — regression lock
  - `GetComponentContent_returns_null_for_known_component_but_unsupported_platform` — verifies
    the null return contract for the Avalonia case that will flip once Phase 2 adds content
  - `SupportsPlatform_reports_MAUI_for_all_components_and_no_others_yet` — will fail-loud on
    Phase 2 when Avalonia content is added (intended trigger to update the test)
  - `UnsupportedPlatformExceptionTests` — message formatting + property exposure

### Follow-ups landed on this branch

- [x] **Input height fix (2026-08-29):** [`InputTemplate`](../src/ShellUI.Native.Templates/Templates/InputTemplate.cs)
  now sets `HeightRequest = 40` and `Padding = (12, 0)` on its `Border`, matching
  `Select` / `DatePicker` / `TimePicker`. Previously the `Border` had no explicit height
  and used `Padding = (12, 8)`, so the composite grew to whatever platform-default height
  `Entry` picked (Windows ~44, Android ~48+ with material padding) — inputs rendered
  visibly taller than the pickers sitting next to them in the same form row. Rule captured
  in [COMPONENTS_ROADMAP.md § Form Sizing Contract](./COMPONENTS_ROADMAP.md#form-sizing-contract).
  Tests still green (199/199).

- [x] **P0/P1 sweep (2026-08-29):** full re-read of every P0 + P1 template looking for the
  same class of drift Input had. Three real bugs found and fixed:
  1. **Primary-blue token drift.** [`InputTemplate`](../src/ShellUI.Native.Templates/Templates/InputTemplate.cs)
     focus border and [`RadioGroupItemTemplate`](../src/ShellUI.Native.Templates/Templates/RadioGroupItemTemplate.cs)
     checked indicator used `#3B82F6` (blue-500) while `Button` / `Checkbox` / `Switch` /
     `Progress` all use `#2563EB` (blue-600) for the same "primary" role. Standardized on
     `#2563EB` and locked the mapping in
     [COMPONENTS_ROADMAP.md § Design Token Contract](./COMPONENTS_ROADMAP.md#design-token-contract).
  2. **`DropdownItem` hit target too small.** Was `Padding = (8, 10)` with no minimum
     height → ~34px rows, below iOS 44px / Android 48dp touch guidance. Now
     `Padding = (12, 12)` + `MinimumHeightRequest = 40`. Rule added to Form Sizing Contract
     under "Menu / list rows".
  3. **`RadioGroupItem` checked-border missing.** Only the fill flipped color when a radio
     became selected; the ring stayed grey. `Checkbox` had already handled this. Fixed to
     match — both stroke and background flip to `#2563EB` when checked.

  Everything else surveyed (Badge, Progress, Alert, Card + variants, Label, Separator,
  Dialog/Drawer/Sheet/Popover content boxes, Shell) was sized/tokenized correctly. Build
  clean, 199/199 tests still passing.

### Exit criteria to merge → `main`

1. All existing MAUI templates install and compile identically (verified: `TemplateContentTests`
   parameterized over all 44 components pass)
2. `Registry.GetComponentContent("button", Avalonia)` returns `null` today (documented behavior);
   will return non-null once Phase 2 adds Avalonia content
3. `ComponentInstaller` fails-loud on unsupported (name, platform) combos with a clear error
   listing supported platforms
4. Form controls that share a row (`Input`, `Select`, `DatePicker`, `TimePicker`) render at
   the same 40px height on Windows/Android/iOS heads of `MAUI.Demo` — visual check, not
   yet automated

---

## Phase 1c — `feat/p3-navigation-layout` (**in review**)

Finish MAUI's component vocabulary through P3 before opening the cross-platform front.
Rationale: Phase 2 (Avalonia) turns every new MAUI component into two components to
maintain. Locking the P3 tier on MAUI first keeps Phase 2 a mechanical port instead of
chasing a moving target.

Small, additive branch — no infrastructure changes, no registry surgery. Each new component
is one template file + one `ComponentRegistry` entry + `TemplateContentTests` parameters
picking it up automatically.

### Scope — P3 (Navigation & Layout, from [COMPONENTS_ROADMAP.md](./COMPONENTS_ROADMAP.md))

| # | Component | Sub-components | Notes |
|---|-----------|----------------|-------|
| P3.1 | **tabs** | `tabs-list`, `tabs-trigger`, `tabs-content` | Compositional (Trigger/Content pattern). Tab bar + one visible panel at a time. |
| P3.2 | **accordion** | `accordion-item`, `accordion-trigger`, `accordion-content` | Multiple expandable sections; `Type="single"` vs `"multiple"` behavior. |
| P3.3 | **collapsible** | `collapsible-trigger`, `collapsible-content` | Single expand/collapse — the primitive `accordion-item` composes on top of. |
| P3.4 | **breadcrumb** | `breadcrumb-item` | Nav trail. Icon-aware separator between items. |
| P3.5 | **scroll-area** | — | `ScrollView` wrapper with consistent scrollbar styling. |
| P3.6 | **skeleton** | — | Loading placeholder — animated grey block, sized by parent. |

### Implementation order

Build the primitive before the composite: **collapsible → accordion → tabs → breadcrumb →
skeleton → scroll-area**. Collapsible's animation + `IsOpen` state is the accordion-item
core; accordion-item generalizes it; tabs reuses the same show/hide pattern with mutual
exclusion. Breadcrumb and skeleton are standalone and can slot in wherever.

### Constraints (locked by Phase 1b)

- Every new template MUST populate `Contents[NativePlatform.MAUI]` (registry lookup contract).
- Every new form-adjacent control MUST follow the
  [Form Sizing Contract](./COMPONENTS_ROADMAP.md#form-sizing-contract) — 40px baseline for
  row-level triggers (`tabs-trigger`, `accordion-trigger`, `breadcrumb-item`).
- Every new color usage MUST reuse a token from the
  [Design Token Contract](./COMPONENTS_ROADMAP.md#design-token-contract). New role → add
  the token to the table first, then use it.
- Compositional families use `FindParentOfType<T>()` from `element-extensions` (already
  installed as a dependency by every overlay component — add it as a dependency for the
  new families too).

### Deliverables

- [x] 6 new template families + their sub-components
- [x] `ComponentRegistry` entries for all of them
- [x] `MAUI.Demo` gains a demo section for each family (Tabs / Accordion / Collapsible /
  Breadcrumb / ScrollArea / Skeleton)
- [x] `TemplateContentTests` picks the new components up automatically (parameterized
  over the whole registry — nothing to add manually)
- [x] [COMPONENTS.md](./COMPONENTS.md) updated with usage snippets for each

Fix-forwards that landed here (despite the "no P0–P2 refactoring" rule below) because they
blocked the demo from rendering at all on Windows: overlays moved to page-root in
`MAUI.Demo`, outer `Border` removed from Select/DatePicker/TimePicker, and .NET 10 MAUI
nullable `Date`/`Time` + non-generic `ItemsSource` adaptations. Proper fixes → Phase 1d.

### Exit criteria to merge → `main`

1. `dotnet build ShellUI.Native.slnx` — 0/0
2. `dotnet test` — new baseline count reflects 9 new components (should land ~10 tests up)
3. Every new component has a demo section in `MAUI.Demo` that renders on the Windows head
4. No token drift: `grep -E '"#[0-9A-Fa-f]{6}"' src/ShellUI.Native.Templates/Templates/` in
   the diff introduces zero hexes not already in the Design Token Contract table

### What this phase deliberately does NOT do

- No Avalonia work (that's Phase 2, and starting it now would double the maintenance
  surface while P3 is in flight).
- No P4+ components (tooltip, toast, alert-dialog, hover-card) — they need overlay
  primitives that already exist, but they're feedback/overlay rather than nav/layout;
  scoping them into a separate `feat/p4-feedback` branch keeps PRs reviewable.
- No refactoring of existing P0/P1/P2 components. Sizing + token contracts are locked;
  any drift found in review gets a fix-forward in the P4 branch, not here.

---

## Phase 1d — `feat/p4-overlay-portal` (queued after Phase 1c)

Surface polish pass driven by live testing on Windows 2026-08-30. Two distinct problems
that both need architectural fixes rather than sizing tweaks.

### Problem 1: Dialog / Drawer / Sheet require a page-root parent

Today these components extend `AbsoluteLayout` with an inner `_overlayLayer` set to
proportional-fill (0,0,1,1). When placed inside a `VerticalStackLayout` (the natural
place a consumer would drop them next to a form), the AbsoluteLayout sizes to children
while the overlay layer wants to fill the parent — circular sizing produces a giant
empty inline box. The Phase 1c demo pushed them to page-root as a workaround, but that's
a footgun consumers WILL hit.

**Fix:** rewrite as `ContentView`s that walk up to `ContentPage.Content` on attach, wrap
it in a `Grid` once (tracked via attached property), inject the overlay layer as the top
child of that grid. Trigger renders inline; content teleports to page root. Portal-style,
matches shadcn's `Dialog`/`Sheet` behavior in React.

### Problem 2: Select / DatePicker / TimePicker use native chrome

Same fix as Phase 1c's short-term patch (remove the outer `Border` to stop overflow) —
functional but not shadcn-quality. Windows' native `Picker`/`CalendarDatePicker`/`TimePicker`
draw their own chrome that we can't override cleanly cross-platform.

**Fix:** replace each with a `Popover`-based custom control that renders its own trigger
(bounded, our styling) and opens a `PopoverContent` with the item list / calendar /
hour-minute wheels. Native pickers drop out entirely. Matches shadcn `Select` /
`DatePicker` / `TimePicker` — inspiration: `nativewind` + `shadcn/ui`.

### Deliverables

- [x] Portal helper — landed 2026-10-02 as `ShellPortal` in `Shell.cs` (page layer, logical owner, anchored popups with flip + click-outside)
- [x] Dialog / Drawer / Sheet / AlertDialog show their content in the portal layer — declare them anywhere
- [x] Custom `Select` — trigger + floating list (landed early, 2026-09-27, with the theme-token pass)
- [x] Custom `DatePicker` — trigger + floating `calendar` component (month navigation + day grid)
- [x] Custom `TimePicker` — trigger + floating hour / minute / AM-PM columns (2026-10-04); native picker dropped
- [x] `MAUI.Demo` no longer needs the page-root `Grid` workaround — its root is a plain `ScrollView` and overlays sit next to their triggers
- [x] Click-outside closes Dropdown / Popover / Select / DatePicker
- [x] `tooltip` and `hover-card` (unblocked by the portal)
- [x] Android pass (2026-10-04, Pixel 7 / API 34 emulator): native underline and padding stripped
  from text fields; Select list sized and placed from real layout; `BoxView`s no longer pick up
  the stock dark background; page layer set up as the page appears (first overlay no longer
  resets the scroll position); overlays edge-to-edge with their content kept inside the safe
  area; Hover Card opens on tap; opt-in `ShellTheme.SyncSystemBars`
- [x] `toggle`, `input-otp`, `pagination`, `empty-state`
- [x] Escape (Windows) and the Android back button close the overlay on top — `ShellDismiss` (2026-10-04; Mac Catalyst still open)
- [x] `callout`, `combobox`

Already in place from the 2026-09-27 pass: theme tokens (`ShellTheme`), overlay open/close
animations, closed overlays no longer block input, triggers that wrap a Button
(`ShellTriggerView`), floating Dropdown/Popover/Select via `ShellAnchorLayout`, and
Date/Time pickers inside a themed border with the native frame stripped.

### Exit criteria

1. `MAUI.Demo` Dialog / Drawer / Sheet sections can move back into the `ScrollView` and still render + open correctly
2. Select / DatePicker / TimePicker render with our own border + rounded corners on Windows (no native chrome bleed)
3. `dotnet test` still green (visual tests remain manual until we automate them)
4. Every color still reuses the [Design Token Contract](./COMPONENTS_ROADMAP.md#design-token-contract)

---

## Phase 2 — `feat/avalonia-implementation` (queued after Phase 1d)

Cross-desktop (Windows + macOS + Linux) from one XAML codebase.

### Deliverables

- [ ] New `src/ShellUI.Native.Avalonia/` reference project (Avalonia 11.x)
- [ ] Avalonia design-token `ResourceDictionary` mirroring [`StyleTemplates.cs`](../src/ShellUI.Native.Templates/StyleTemplates.cs)
- [ ] Add `AvaloniaContent` to each of the ~40 templates, using the
  `TemplatedControl`/`StyledProperty` pattern documented in
  [ARCHITECTURE.md § Component Pattern (Avalonia)](./ARCHITECTURE.md)
- [ ] `examples/Avalonia.Demo/` reference app mirroring the MAUI demo
- [ ] CI job that builds the Avalonia demo on all three OSes (`ubuntu-latest`, `windows-latest`, `macos-latest`)

### Order of implementation

Same P0 → P7 order as MAUI, per [COMPONENTS_ROADMAP.md](./COMPONENTS_ROADMAP.md). Do not start
component `N` until MAUI has landed `N`.

### Exit criteria

1. Every P0+P1 MAUI component has a working Avalonia counterpart
2. `shellui-native init` in an Avalonia project installs Avalonia code (verified by grepping
   installed files for `TemplatedControl`, not `ContentView`)
3. Avalonia demo builds & runs on Linux (the platform Phase 2 exists to unlock)

---

## Phase 3 — `feat/winui` (conditional)

Only pursued if a concrete requirement surfaces for Windows 11 Fluent-specific styling that
Avalonia's Fluent theme cannot satisfy. Default: **skip**, redirect that effort into
Avalonia's Windows head.

If pursued, mirror Phase 2's structure: new project, `WinUIContent` per template, dedicated
demo, CI job.

---

## Cross-cutting workstreams

These aren't phases but ongoing concerns that touch every branch:

| Concern | Owner branch | Notes |
|---------|--------------|-------|
| **Docs** | Whatever branch touches the feature | Prefer additions over prose rewrites on active parallel branches |
| **CI** | The branch introducing the new build target | Every new platform gets its own job, not a matrix |
| **Versioning** | `main` after every merge | Bump [Directory.Build.props](../Directory.Build.props) via [prepare-release.ps1](../prepare-release.ps1) |
| **Compositional pattern** | Component-authoring branches | Enforce Dialog+Trigger+Content style (see [COMPONENTS_ROADMAP.md § Architectural Pattern](./COMPONENTS_ROADMAP.md#architectural-pattern-dependencies-over-childcomponent)) |
| **Template hygiene** | Component-authoring branches | Every `RoundRectangle`/`Border`/`Path` usage must ship its `using` in the template `Content` string |

---

## How to add a new branch to this plan

1. Cut off the current phase branch (not `main`) unless you're starting a new phase.
2. Add a row to the Current Snapshot table above.
3. If it's a new phase, add a Phase section with **Deliverables** and **Exit criteria**.
4. Link the branch to the priority tier it delivers (P0–P7) in
   [COMPONENTS_ROADMAP.md](./COMPONENTS_ROADMAP.md).
5. Update this doc's `Last revised` date.
