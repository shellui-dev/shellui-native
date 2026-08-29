using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

// CollapsibleContent - inline panel that appears/disappears with the parent Collapsible's
// Open state. Fades over 150ms rather than animating height: cross-platform height animation
// in MAUI is unreliable (needs measured child height), fade + visibility toggle is not.
public static class CollapsibleContentTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "collapsible-content",
        DisplayName = "Collapsible Content",
        Description = "Body of a Collapsible - shown when the parent Collapsible is Open. Place inside Collapsible",
        Category = ComponentCategory.Layout,
        FilePath = "CollapsibleContent.cs",
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "layout", "collapsible", "content" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

[ContentProperty(nameof(Content))]
public partial class CollapsibleContent : ContentView
{
    private Collapsible? _parent;

    public CollapsibleContent()
    {
        IsVisible = false;
        Opacity = 0;
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        // Detach from any previous parent to avoid duplicate handlers if the view is reparented.
        if (_parent != null)
            _parent.OpenChanged -= OnParentOpenChanged;

        _parent = this.FindParentOfType<Collapsible>();
        if (_parent != null)
        {
            _parent.OpenChanged += OnParentOpenChanged;
            ApplyState(_parent.Open, animate: false);
        }
    }

    private void OnParentOpenChanged(object? sender, bool open) => _ = AnimateAsync(open);

    private async Task AnimateAsync(bool open)
    {
        if (open)
        {
            IsVisible = true;
            await this.FadeTo(1, 150, Easing.CubicOut);
        }
        else
        {
            await this.FadeTo(0, 150, Easing.CubicIn);
            IsVisible = false;
        }
    }

    private void ApplyState(bool open, bool animate)
    {
        if (animate) { _ = AnimateAsync(open); return; }
        IsVisible = open;
        Opacity = open ? 1 : 0;
    }
}
"
    };
}
