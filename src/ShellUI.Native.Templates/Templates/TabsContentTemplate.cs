using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class TabsContentTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "tabs-content",
        DisplayName = "Tabs Content",
        Description = "Body of one tab - shown when parent Tabs.Value matches this Content's Value",
        Category = ComponentCategory.Navigation,
        FilePath = "TabsContent.cs",
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "navigation", "tabs", "content" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

[ContentProperty(nameof(Content))]
public partial class TabsContent : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(TabsContent), string.Empty,
            propertyChanged: (b, o, n) => (b as TabsContent)?.RefreshVisibility());

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private Tabs? _parent;

    public TabsContent()
    {
        IsVisible = false;
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (_parent != null)
            _parent.ValueChanged -= OnParentValueChanged;

        _parent = this.FindParentOfType<Tabs>();
        if (_parent != null)
        {
            _parent.ValueChanged += OnParentValueChanged;
            RefreshVisibility();
        }
    }

    private void OnParentValueChanged(object? sender, (string Old, string New) e) => RefreshVisibility();

    private void RefreshVisibility()
    {
        IsVisible = _parent != null && _parent.Value == Value;
    }
}
"
    };
}
