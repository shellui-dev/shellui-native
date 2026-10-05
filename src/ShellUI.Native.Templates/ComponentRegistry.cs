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
        { "wrap-layout", (WrapLayoutTemplate.Metadata, WrapLayoutTemplate.Contents) }
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
}
