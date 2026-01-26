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
        { "alert", AlertTemplate.Metadata }
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
