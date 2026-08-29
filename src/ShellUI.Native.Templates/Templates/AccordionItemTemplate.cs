using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// AccordionItem - one expandable row. Its IsOpen is derived from the parent Accordion.
public static class AccordionItemTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "accordion-item",
        DisplayName = "Accordion Item",
        Description = "Single row of an Accordion - contains AccordionTrigger + AccordionContent",
        Category = ComponentCategory.Layout,
        FilePath = "AccordionItem.cs",
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "layout", "accordion", "item" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

public partial class AccordionItem : VerticalStackLayout
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(AccordionItem), string.Empty);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // Item-local view of parent's open set. Trigger/Content subscribe here so they don't
    // have to know about the Accordion above.
    public bool IsOpen { get; private set; }
    public event EventHandler<bool>? OpenChanged;

    private Accordion? _parent;

    public AccordionItem()
    {
        Spacing = 0;
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (_parent != null)
            _parent.ItemToggled -= OnParentToggled;

        _parent = this.FindParentOfType<Accordion>();
        if (_parent != null)
        {
            _parent.ItemToggled += OnParentToggled;
            var open = _parent.IsOpen(Value);
            if (open != IsOpen)
            {
                IsOpen = open;
                OpenChanged?.Invoke(this, IsOpen);
            }
        }
    }

    private void OnParentToggled(object? sender, (string Value, bool IsOpen) e)
    {
        if (e.Value != Value) return;
        if (IsOpen == e.IsOpen) return;
        IsOpen = e.IsOpen;
        OpenChanged?.Invoke(this, IsOpen);
    }

    public void Toggle() => _parent?.Toggle(Value);
}
"
    };
}
