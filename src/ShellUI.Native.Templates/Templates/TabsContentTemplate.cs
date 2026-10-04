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
        Dependencies = new List<string>(),
        Tags = new List<string> { "navigation", "tabs", "content" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Panel shown while its Value is the active tab; fades in on switch.
[ContentProperty(nameof(Content))]
public partial class TabsContent : ContentView
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(TabsContent), string.Empty);

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public TabsContent()
    {
        IsVisible = false;
    }

    internal void SetActive(bool active, bool animate)
    {
        if (IsVisible == active) return;
        IsVisible = active;
        if (active && animate)
        {
            Opacity = 0;
            _ = this.FadeToAsync(1, 150, Easing.CubicOut);
        }
        else
        {
            Opacity = 1;
        }
    }
}
"
    };
}
