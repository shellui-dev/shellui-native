using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class SheetTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "sheet",
        DisplayName = "Sheet",
        Description = "Bottom/top sheet overlay - use with SheetTrigger and SheetContent",
        Category = ComponentCategory.Overlay,
        FilePath = "Sheet.cs",
        Dependencies = new List<string> { "element-extensions", "sheet-trigger", "sheet-content" },
        Variants = new List<string> { "bottom", "top", "left", "right" },
        Tags = new List<string> { "overlay", "sheet", "panel" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

public partial class Sheet : AbsoluteLayout
{
    public static readonly BindableProperty OpenProperty =
        BindableProperty.Create(nameof(Open), typeof(bool), typeof(Sheet), false,
            propertyChanged: (b, o, n) => (b as Sheet)?.OnOpenChanged());

    public static readonly BindableProperty SideProperty =
        BindableProperty.Create(nameof(Side), typeof(SheetSide), typeof(Sheet), SheetSide.Right);

    public bool Open
    {
        get => (bool)GetValue(OpenProperty);
        set => SetValue(OpenProperty, value);
    }

    public SheetSide Side
    {
        get => (SheetSide)GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    public event EventHandler<bool>? OpenChanged;

    private readonly VerticalStackLayout _triggerContainer;
    private readonly Grid _overlayLayer;

    public Sheet()
    {
        // The host fills its area (page-root overlays fill the whole page). While closed it must
        // not swallow input meant for content underneath; CascadeInputTransparent = false keeps
        // an inline trigger inside it clickable. Toggled off while open so the backdrop catches taps.
        InputTransparent = true;
        CascadeInputTransparent = false;
        _triggerContainer = new VerticalStackLayout { Spacing = 0 };
        _overlayLayer = new Grid { IsVisible = false, ZIndex = 1000 };
        Children.Add(_triggerContainer);
        Children.Add(_overlayLayer);
        SetLayoutFlags(_triggerContainer, AbsoluteLayoutFlags.PositionProportional);
        SetLayoutBounds(_triggerContainer, new Rect(0, 0, 0, 0));
        SetLayoutFlags(_overlayLayer, AbsoluteLayoutFlags.All);
        SetLayoutBounds(_overlayLayer, new Rect(0, 0, 1, 1));
    }

    protected override void OnChildAdded(Element child)
    {
        base.OnChildAdded(child);
        if (child == _triggerContainer || child == _overlayLayer) return;
        Dispatcher.Dispatch(() =>
        {
            if (child is IView view)
            {
                Children.Remove(view);
                if (child is SheetTrigger st)
                    _triggerContainer.Children.Add(st);
                else if (child is SheetContent sc)
                    _overlayLayer.Children.Add(sc);
            }
        });
    }

    public void SetOpen(bool value)
    {
        if (Open != value) { Open = value; OnOpenChanged(); }
    }

    private void OnOpenChanged()
    {
        _overlayLayer.IsVisible = Open;
        InputTransparent = !Open;
        OpenChanged?.Invoke(this, Open);
    }
}

public enum SheetSide { Left, Right, Top, Bottom }
"
    };
}
