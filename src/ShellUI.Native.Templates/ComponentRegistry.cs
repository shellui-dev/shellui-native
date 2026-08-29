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
        { "time-picker", (TimePickerTemplate.Metadata, TimePickerTemplate.Contents) }
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
