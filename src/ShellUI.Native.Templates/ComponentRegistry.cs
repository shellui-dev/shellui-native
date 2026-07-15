using ShellUI.Native.Core.Models;
using ShellUI.Native.Templates.Templates;

namespace ShellUI.Native.Templates;

// Central registry of all available component templates
public static class ComponentRegistry
{
    public static readonly Dictionary<string, ComponentMetadata> Components = new()
    {
        // Core utilities
        { "shell", ShellTemplate.Metadata },
        
        // Form components
        { "button", ButtonTemplate.Metadata },
        { "button-variants", ButtonVariantsTemplate.Metadata },
        { "input", InputTemplate.Metadata },
        { "label", LabelTemplate.Metadata },
        { "checkbox", CheckboxTemplate.Metadata },
        { "switch", SwitchTemplate.Metadata },
        
        // Layout components
        { "card", CardTemplate.Metadata },
        { "card-header", CardHeaderTemplate.Metadata },
        { "card-content", CardContentTemplate.Metadata },
        { "card-footer", CardFooterTemplate.Metadata },
        { "separator", SeparatorTemplate.Metadata },
        
        // Data display
        { "badge", BadgeTemplate.Metadata },
        { "progress", ProgressTemplate.Metadata },
        
        // Feedback
        { "alert", AlertTemplate.Metadata },
        
        // Overlay (modal/panel) - P1
        { "element-extensions", ElementExtensionsTemplate.Metadata },
        { "dialog", DialogTemplate.Metadata },
        { "dialog-trigger", DialogTriggerTemplate.Metadata },
        { "dialog-content", DialogContentTemplate.Metadata },
        { "dialog-header", DialogHeaderTemplate.Metadata },
        { "dialog-footer", DialogFooterTemplate.Metadata },
        { "dialog-title", DialogTitleTemplate.Metadata },
        { "dialog-description", DialogDescriptionTemplate.Metadata },
        { "dialog-close", DialogCloseTemplate.Metadata },
        { "drawer", DrawerTemplate.Metadata },
        { "drawer-trigger", DrawerTriggerTemplate.Metadata },
        { "drawer-content", DrawerContentTemplate.Metadata },
        { "sheet", SheetTemplate.Metadata },
        { "sheet-trigger", SheetTriggerTemplate.Metadata },
        { "sheet-content", SheetContentTemplate.Metadata },
        { "dropdown", DropdownTemplate.Metadata },
        { "dropdown-trigger", DropdownTriggerTemplate.Metadata },
        { "dropdown-content", DropdownContentTemplate.Metadata },
        { "dropdown-item", DropdownItemTemplate.Metadata },
        { "popover", PopoverTemplate.Metadata },
        { "popover-trigger", PopoverTriggerTemplate.Metadata },
        { "popover-content", PopoverContentTemplate.Metadata },
        
        // Form P2
        { "textarea", TextareaTemplate.Metadata },
        { "slider", SliderTemplate.Metadata },
        { "select", SelectTemplate.Metadata },
        { "radio-group", RadioGroupTemplate.Metadata },
        { "radio-group-item", RadioGroupItemTemplate.Metadata },
        { "date-picker", DatePickerTemplate.Metadata },
        { "time-picker", TimePickerTemplate.Metadata }
    };

    public static string? GetComponentContent(string componentName)
    {
        return componentName.ToLower() switch
        {
            "shell" => ShellTemplate.Content,
            "button" => ButtonTemplate.Content,
            "button-variants" => ButtonVariantsTemplate.Content,
            "input" => InputTemplate.Content,
            "label" => LabelTemplate.Content,
            "checkbox" => CheckboxTemplate.Content,
            "switch" => SwitchTemplate.Content,
            "card" => CardTemplate.Content,
            "card-header" => CardHeaderTemplate.Content,
            "card-content" => CardContentTemplate.Content,
            "card-footer" => CardFooterTemplate.Content,
            "separator" => SeparatorTemplate.Content,
            "badge" => BadgeTemplate.Content,
            "progress" => ProgressTemplate.Content,
            "alert" => AlertTemplate.Content,
            "element-extensions" => ElementExtensionsTemplate.Content,
            "dialog" => DialogTemplate.Content,
            "dialog-trigger" => DialogTriggerTemplate.Content,
            "dialog-content" => DialogContentTemplate.Content,
            "dialog-header" => DialogHeaderTemplate.Content,
            "dialog-footer" => DialogFooterTemplate.Content,
            "dialog-title" => DialogTitleTemplate.Content,
            "dialog-description" => DialogDescriptionTemplate.Content,
            "dialog-close" => DialogCloseTemplate.Content,
            "drawer" => DrawerTemplate.Content,
            "drawer-trigger" => DrawerTriggerTemplate.Content,
            "drawer-content" => DrawerContentTemplate.Content,
            "sheet" => SheetTemplate.Content,
            "sheet-trigger" => SheetTriggerTemplate.Content,
            "sheet-content" => SheetContentTemplate.Content,
            "dropdown" => DropdownTemplate.Content,
            "dropdown-trigger" => DropdownTriggerTemplate.Content,
            "dropdown-content" => DropdownContentTemplate.Content,
            "dropdown-item" => DropdownItemTemplate.Content,
            "popover" => PopoverTemplate.Content,
            "popover-trigger" => PopoverTriggerTemplate.Content,
            "popover-content" => PopoverContentTemplate.Content,
            "textarea" => TextareaTemplate.Content,
            "slider" => SliderTemplate.Content,
            "select" => SelectTemplate.Content,
            "radio-group" => RadioGroupTemplate.Content,
            "radio-group-item" => RadioGroupItemTemplate.Content,
            "date-picker" => DatePickerTemplate.Content,
            "time-picker" => TimePickerTemplate.Content,
            _ => null
        };
    }

    public static IEnumerable<ComponentMetadata> GetByCategory(ComponentCategory category)
    {
        return Components.Values.Where(c => c.Category == category);
    }

    public static IEnumerable<ComponentMetadata> SearchByTag(string tag)
    {
        return Components.Values.Where(c => c.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
    }

    public static ComponentMetadata? GetMetadata(string componentName)
    {
        return Components.TryGetValue(componentName.ToLower(), out var metadata) ? metadata : null;
    }

    public static bool Exists(string componentName)
    {
        return Components.ContainsKey(componentName.ToLower());
    }
}
