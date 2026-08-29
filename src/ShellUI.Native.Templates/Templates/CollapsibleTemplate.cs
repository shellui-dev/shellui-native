using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// Collapsible - single expand/collapse primitive. Compositional parent for
// CollapsibleTrigger + CollapsibleContent. Also the reusable IsOpen/animation
// primitive that AccordionItem builds on.
public static class CollapsibleTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "collapsible",
        DisplayName = "Collapsible",
        Description = "Single expand/collapse container - use with CollapsibleTrigger and CollapsibleContent",
        Category = ComponentCategory.Layout,
        FilePath = "Collapsible.cs",
        Dependencies = new List<string> { "element-extensions", "collapsible-trigger", "collapsible-content" },
        Tags = new List<string> { "layout", "collapsible", "disclosure", "expand" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Compositional expand/collapse. Usage:
//   <Collapsible Open=""{Binding IsOpen}"">
//       <CollapsibleTrigger><Label Text=""Toggle"" /></CollapsibleTrigger>
//       <CollapsibleContent><Label Text=""Hidden until open"" /></CollapsibleContent>
//   </Collapsible>
public partial class Collapsible : VerticalStackLayout
{
    public static readonly BindableProperty OpenProperty =
        BindableProperty.Create(nameof(Open), typeof(bool), typeof(Collapsible), false,
            propertyChanged: (b, o, n) => (b as Collapsible)?.OnOpenChanged());

    public bool Open
    {
        get => (bool)GetValue(OpenProperty);
        set => SetValue(OpenProperty, value);
    }

    // Raised whenever Open flips. CollapsibleContent subscribes to drive its visibility;
    // consumers may also subscribe for side effects (analytics, lazy loading, etc.).
    public event EventHandler<bool>? OpenChanged;

    public Collapsible()
    {
        Spacing = 0;
    }

    public void SetOpen(bool value)
    {
        if (Open != value) Open = value;
    }

    public void Toggle() => SetOpen(!Open);

    private void OnOpenChanged() => OpenChanged?.Invoke(this, Open);
}
"
    };
}
