using ShellUI.Native.Core.Models;
using ShellUI.Native.Templates.Templates;

namespace ShellUI.Native.Templates;

// Central registry of all available component templates.
//
// Template System v2: content is keyed by (componentName, NativePlatform). Each template class
// owns an IReadOnlyDictionary<NativePlatform, string> Contents, and the registry bundles
// (metadata, contents) so a name lookup is one hop — no per-component switch arms to maintain.
// Adding a platform to a component is one dictionary entry inside that template file.
public static class ComponentRegistry
{
    private static readonly Dictionary<string, (ComponentMetadata Meta, IReadOnlyDictionary<NativePlatform, string> Contents)> _templates = new()
    {
        // Core utilities
        { "shell", (ShellTemplate.Metadata, ShellTemplate.Contents) },
        { "icon", (IconTemplate.Metadata, IconTemplate.Contents) },
        { "theme-toggle", (ThemeToggleTemplate.Metadata, ThemeToggleTemplate.Contents) },

        // Form components
        { "button", (ButtonTemplate.Metadata, ButtonTemplate.Contents) },
        { "button-variants", (ButtonVariantsTemplate.Metadata, ButtonVariantsTemplate.Contents) },
        { "input", (InputTemplate.Metadata, InputTemplate.Contents) },
        { "label", (LabelTemplate.Metadata, LabelTemplate.Contents) },
        { "checkbox", (CheckboxTemplate.Metadata, CheckboxTemplate.Contents) },
        { "switch", (SwitchTemplate.Metadata, SwitchTemplate.Contents) },

        // Layout components
        { "card", (CardTemplate.Metadata, CardTemplate.Contents) },
        { "card-header", (CardHeaderTemplate.Metadata, CardHeaderTemplate.Contents) },
        { "card-content", (CardContentTemplate.Metadata, CardContentTemplate.Contents) },
        { "card-footer", (CardFooterTemplate.Metadata, CardFooterTemplate.Contents) },
        { "separator", (SeparatorTemplate.Metadata, SeparatorTemplate.Contents) },

        // Data display
        { "badge", (BadgeTemplate.Metadata, BadgeTemplate.Contents) },
        { "progress", (ProgressTemplate.Metadata, ProgressTemplate.Contents) },

        // Feedback
        { "alert", (AlertTemplate.Metadata, AlertTemplate.Contents) },

        // Overlay (modal/panel) - P1
        { "element-extensions", (ElementExtensionsTemplate.Metadata, ElementExtensionsTemplate.Contents) },
        { "dialog", (DialogTemplate.Metadata, DialogTemplate.Contents) },
        { "dialog-trigger", (DialogTriggerTemplate.Metadata, DialogTriggerTemplate.Contents) },
        { "dialog-content", (DialogContentTemplate.Metadata, DialogContentTemplate.Contents) },
        { "dialog-header", (DialogHeaderTemplate.Metadata, DialogHeaderTemplate.Contents) },
        { "dialog-footer", (DialogFooterTemplate.Metadata, DialogFooterTemplate.Contents) },
        { "dialog-title", (DialogTitleTemplate.Metadata, DialogTitleTemplate.Contents) },
        { "dialog-description", (DialogDescriptionTemplate.Metadata, DialogDescriptionTemplate.Contents) },
        { "dialog-close", (DialogCloseTemplate.Metadata, DialogCloseTemplate.Contents) },
        { "drawer", (DrawerTemplate.Metadata, DrawerTemplate.Contents) },
        { "drawer-trigger", (DrawerTriggerTemplate.Metadata, DrawerTriggerTemplate.Contents) },
        { "drawer-content", (DrawerContentTemplate.Metadata, DrawerContentTemplate.Contents) },
        { "sheet", (SheetTemplate.Metadata, SheetTemplate.Contents) },
        { "sheet-trigger", (SheetTriggerTemplate.Metadata, SheetTriggerTemplate.Contents) },
        { "sheet-content", (SheetContentTemplate.Metadata, SheetContentTemplate.Contents) },
        { "dropdown", (DropdownTemplate.Metadata, DropdownTemplate.Contents) },
        { "dropdown-trigger", (DropdownTriggerTemplate.Metadata, DropdownTriggerTemplate.Contents) },
        { "dropdown-content", (DropdownContentTemplate.Metadata, DropdownContentTemplate.Contents) },
        { "dropdown-item", (DropdownItemTemplate.Metadata, DropdownItemTemplate.Contents) },
        { "popover", (PopoverTemplate.Metadata, PopoverTemplate.Contents) },
        { "popover-trigger", (PopoverTriggerTemplate.Metadata, PopoverTriggerTemplate.Contents) },
        { "popover-content", (PopoverContentTemplate.Metadata, PopoverContentTemplate.Contents) },

        // Form P2
        { "textarea", (TextareaTemplate.Metadata, TextareaTemplate.Contents) },
        { "slider", (SliderTemplate.Metadata, SliderTemplate.Contents) },
        { "select", (SelectTemplate.Metadata, SelectTemplate.Contents) },
        { "radio-group", (RadioGroupTemplate.Metadata, RadioGroupTemplate.Contents) },
        { "radio-group-item", (RadioGroupItemTemplate.Metadata, RadioGroupItemTemplate.Contents) },
        { "date-picker", (DatePickerTemplate.Metadata, DatePickerTemplate.Contents) },
        { "time-picker", (TimePickerTemplate.Metadata, TimePickerTemplate.Contents) },

        // Navigation / Layout - P3
        { "collapsible", (CollapsibleTemplate.Metadata, CollapsibleTemplate.Contents) },
        { "collapsible-trigger", (CollapsibleTriggerTemplate.Metadata, CollapsibleTriggerTemplate.Contents) },
        { "collapsible-content", (CollapsibleContentTemplate.Metadata, CollapsibleContentTemplate.Contents) },
        { "accordion", (AccordionTemplate.Metadata, AccordionTemplate.Contents) },
        { "accordion-item", (AccordionItemTemplate.Metadata, AccordionItemTemplate.Contents) },
        { "accordion-trigger", (AccordionTriggerTemplate.Metadata, AccordionTriggerTemplate.Contents) },
        { "accordion-content", (AccordionContentTemplate.Metadata, AccordionContentTemplate.Contents) },
        { "tabs", (TabsTemplate.Metadata, TabsTemplate.Contents) },
        { "tabs-list", (TabsListTemplate.Metadata, TabsListTemplate.Contents) },
        { "tabs-trigger", (TabsTriggerTemplate.Metadata, TabsTriggerTemplate.Contents) },
        { "tabs-content", (TabsContentTemplate.Metadata, TabsContentTemplate.Contents) },
        { "breadcrumb", (BreadcrumbTemplate.Metadata, BreadcrumbTemplate.Contents) },
        { "breadcrumb-item", (BreadcrumbItemTemplate.Metadata, BreadcrumbItemTemplate.Contents) },
        { "skeleton", (SkeletonTemplate.Metadata, SkeletonTemplate.Contents) },
        { "scroll-area", (ScrollAreaTemplate.Metadata, ScrollAreaTemplate.Contents) },

        // Feedback & overlays / data display - P4-P5
        { "alert-dialog", (AlertDialogTemplate.Metadata, AlertDialogTemplate.Contents) },
        { "toast", (ToastTemplate.Metadata, ToastTemplate.Contents) },
        { "spinner", (SpinnerTemplate.Metadata, SpinnerTemplate.Contents) },
        { "avatar", (AvatarTemplate.Metadata, AvatarTemplate.Contents) },
        { "tooltip", (TooltipTemplate.Metadata, TooltipTemplate.Contents) },
        { "hover-card", (HoverCardTemplate.Metadata, HoverCardTemplate.Contents) },
        { "hover-card-trigger", (HoverCardTriggerTemplate.Metadata, HoverCardTriggerTemplate.Contents) },
        { "hover-card-content", (HoverCardContentTemplate.Metadata, HoverCardContentTemplate.Contents) },
        { "calendar", (CalendarTemplate.Metadata, CalendarTemplate.Contents) },
        { "toggle", (ToggleTemplate.Metadata, ToggleTemplate.Contents) },
        { "input-otp", (InputOtpTemplate.Metadata, InputOtpTemplate.Contents) },
        { "pagination", (PaginationTemplate.Metadata, PaginationTemplate.Contents) },
        { "empty-state", (EmptyStateTemplate.Metadata, EmptyStateTemplate.Contents) },
        { "callout", (CalloutTemplate.Metadata, CalloutTemplate.Contents) },
        { "combobox", (ComboboxTemplate.Metadata, ComboboxTemplate.Contents) },
        { "table", (TableTemplate.Metadata, TableTemplate.Contents) },
        { "context-menu", (ContextMenuTemplate.Metadata, ContextMenuTemplate.Contents) },
        { "carousel", (CarouselTemplate.Metadata, CarouselTemplate.Contents) },
        { "stepper", (StepperTemplate.Metadata, StepperTemplate.Contents) },
        { "toggle-group", (ToggleGroupTemplate.Metadata, ToggleGroupTemplate.Contents) },
        { "number-input", (NumberInputTemplate.Metadata, NumberInputTemplate.Contents) },
        { "tag-input", (TagInputTemplate.Metadata, TagInputTemplate.Contents) },
        { "kbd", (KbdTemplate.Metadata, KbdTemplate.Contents) },
        { "stat-card", (StatCardTemplate.Metadata, StatCardTemplate.Contents) },
        { "timeline", (TimelineTemplate.Metadata, TimelineTemplate.Contents) },
        { "wrap-layout", (WrapLayoutTemplate.Metadata, WrapLayoutTemplate.Contents) },
        { "copy-button", (CopyButtonTemplate.Metadata, CopyButtonTemplate.Contents) },
        { "aspect-ratio", (AspectRatioTemplate.Metadata, AspectRatioTemplate.Contents) },
        { "link-card", (LinkCardTemplate.Metadata, LinkCardTemplate.Contents) },
        { "tree-view", (TreeViewTemplate.Metadata, TreeViewTemplate.Contents) },
        { "multi-select", (MultiSelectTemplate.Metadata, MultiSelectTemplate.Contents) },
        { "resizable", (ResizableTemplate.Metadata, ResizableTemplate.Contents) },
        { "navbar", (NavbarTemplate.Metadata, NavbarTemplate.Contents) },
        { "bottom-nav", (BottomNavTemplate.Metadata, BottomNavTemplate.Contents) },
        { "edge-menu", (EdgeMenuTemplate.Metadata, EdgeMenuTemplate.Contents) },
        { "sidebar", (SidebarTemplate.Metadata, SidebarTemplate.Contents) },
        { "date-range-picker", (DateRangePickerTemplate.Metadata, DateRangePickerTemplate.Contents) },
        { "file-upload", (FileUploadTemplate.Metadata, FileUploadTemplate.Contents) },
        { "button-group", (ButtonGroupTemplate.Metadata, ButtonGroupTemplate.Contents) },
        { "input-group", (InputGroupTemplate.Metadata, InputGroupTemplate.Contents) },
        { "menubar", (MenubarTemplate.Metadata, MenubarTemplate.Contents) },
        { "command", (CommandTemplate.Metadata, CommandTemplate.Contents) },
        { "navigation-menu", (NavigationMenuTemplate.Metadata, NavigationMenuTemplate.Contents) },
        { "data-table", (DataTableTemplate.Metadata, DataTableTemplate.Contents) },
        { "chat", (ChatTemplate.Metadata, ChatTemplate.Contents) },
        { "form", (FormTemplate.Metadata, FormTemplate.Contents) },
        { "image-viewer", (ImageViewerTemplate.Metadata, ImageViewerTemplate.Contents) },
        { "prev-next-nav", (PrevNextNavTemplate.Metadata, PrevNextNavTemplate.Contents) },
        { "chart", (ChartTemplate.Metadata, ChartTemplate.Contents) },
        { "bar-chart", (BarChartTemplate.Metadata, BarChartTemplate.Contents) },
        { "line-chart", (LineChartTemplate.Metadata, LineChartTemplate.Contents) },
        { "area-chart", (AreaChartTemplate.Metadata, AreaChartTemplate.Contents) },
        { "pie-chart", (PieChartTemplate.Metadata, PieChartTemplate.Contents) },
        { "donut-chart", (DonutChartTemplate.Metadata, DonutChartTemplate.Contents) },
        { "radar-chart", (RadarChartTemplate.Metadata, RadarChartTemplate.Contents) },
        { "radial-chart", (RadialChartTemplate.Metadata, RadialChartTemplate.Contents) },

        // Blocks: numbered presets built from components, installed to Components/Blocks
        { "bottom-nav-01", (BottomNav01Template.Metadata, BottomNav01Template.Contents) },
        { "bottom-nav-02", (BottomNav02Template.Metadata, BottomNav02Template.Contents) },
        { "bottom-nav-03", (BottomNav03Template.Metadata, BottomNav03Template.Contents) },
        { "bottom-nav-04", (BottomNav04Template.Metadata, BottomNav04Template.Contents) },
        { "bottom-nav-05", (BottomNav05Template.Metadata, BottomNav05Template.Contents) },
        { "edge-menu-01", (EdgeMenu01Template.Metadata, EdgeMenu01Template.Contents) }
    };

    // Metadata-only view for callers that don't need the per-platform payload.
    public static IReadOnlyDictionary<string, ComponentMetadata> Components { get; } =
        _templates.ToDictionary(kv => kv.Key, kv => kv.Value.Meta);

    // Returns the C# source for `name` targeted at `platform`, or null if the component
    // doesn't exist OR exists but has no template for that platform. Callers that need to
    // distinguish those two cases should use `Exists` and `SupportsPlatform` first.
    public static string? GetComponentContent(string name, NativePlatform platform)
    {
        if (!_templates.TryGetValue(name.ToLower(), out var entry))
            return null;
        return entry.Contents.TryGetValue(platform, out var content) ? content : null;
    }

    public static bool SupportsPlatform(string name, NativePlatform platform)
    {
        return _templates.TryGetValue(name.ToLower(), out var entry)
            && entry.Contents.ContainsKey(platform);
    }

    public static IReadOnlySet<NativePlatform> GetSupportedPlatforms(string name)
    {
        if (!_templates.TryGetValue(name.ToLower(), out var entry))
            return new HashSet<NativePlatform>();
        return entry.Contents.Keys.ToHashSet();
    }

    public static IEnumerable<ComponentMetadata> GetByCategory(ComponentCategory category)
    {
        return _templates.Values.Where(t => t.Meta.Category == category).Select(t => t.Meta);
    }

    public static IEnumerable<ComponentMetadata> SearchByTag(string tag)
    {
        return _templates.Values
            .Where(t => t.Meta.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            .Select(t => t.Meta);
    }

    public static ComponentMetadata? GetMetadata(string componentName)
    {
        return _templates.TryGetValue(componentName.ToLower(), out var entry) ? entry.Meta : null;
    }

    public static bool Exists(string componentName)
    {
        return _templates.ContainsKey(componentName.ToLower());
    }

    // Up to `max` registered names close to an unknown `name`, closest first: a typo
    // ("buton"), or a shared word ("chart-04", "chart-bar").
    public static IReadOnlyList<string> Suggest(string name, int max = 3)
    {
        var key = name.ToLower();
        var limit = Math.Max(2, key.Length / 3);
        var words = key.Split('-', StringSplitOptions.RemoveEmptyEntries).Where(w => w.Length >= 3).ToList();
        return _templates.Keys
            .Select(candidate => (candidate, distance: Distance(key, candidate)))
            .Where(c => c.distance <= limit || words.Any(c.candidate.Split('-').Contains))
            .OrderBy(c => c.distance)
            .ThenBy(c => c.candidate, StringComparer.Ordinal)
            .Select(c => c.candidate)
            .Take(max)
            .ToList();
    }

    // Levenshtein edit distance.
    private static int Distance(string a, string b)
    {
        var row = Enumerable.Range(0, b.Length + 1).ToArray();
        for (var i = 1; i <= a.Length; i++)
        {
            var diagonal = row[0];
            row[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var above = row[j];
                row[j] = Math.Min(Math.Min(row[j] + 1, row[j - 1] + 1), diagonal + (a[i - 1] == b[j - 1] ? 0 : 1));
                diagonal = above;
            }
        }
        return row[b.Length];
    }

    /* The family a compositional part belongs to (`dialog-trigger` → `dialog`), or null.
       Parts find their parent type at runtime and parents depend on their parts, so a part
       only builds when its family is installed. */
    public static string? GetFamily(string componentName)
    {
        var name = componentName.ToLower();
        return _templates
            .Where(t => name.StartsWith(t.Key + "-") && t.Value.Meta.Dependencies.Contains(name))
            .Select(t => t.Key)
            .OrderByDescending(k => k.Length)
            .FirstOrDefault();
    }
}
