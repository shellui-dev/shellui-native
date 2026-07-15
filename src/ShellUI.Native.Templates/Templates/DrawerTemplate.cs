using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DrawerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "drawer",
        DisplayName = "Drawer",
        Description = "Slide-out panel (drawer) - use with DrawerTrigger and DrawerContent",
        Category = ComponentCategory.Overlay,
        FilePath = "Drawer.cs",
        Dependencies = new List<string> { "element-extensions", "drawer-trigger", "drawer-content" },
        Variants = new List<string> { "bottom", "top", "left", "right" },
        Tags = new List<string> { "overlay", "drawer", "panel" }
    };

    public static string Content => @"namespace YourProjectNamespace.Components.UI;

public partial class Drawer : AbsoluteLayout
{
    public static readonly BindableProperty OpenProperty =
        BindableProperty.Create(nameof(Open), typeof(bool), typeof(Drawer), false,
            propertyChanged: (b, o, n) => (b as Drawer)?.OnOpenChanged());

    public static readonly BindableProperty SideProperty =
        BindableProperty.Create(nameof(Side), typeof(DrawerSide), typeof(Drawer), DrawerSide.Bottom);

    public bool Open
    {
        get => (bool)GetValue(OpenProperty);
        set => SetValue(OpenProperty, value);
    }

    public DrawerSide Side
    {
        get => (DrawerSide)GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    public event EventHandler<bool>? OpenChanged;

    private readonly VerticalStackLayout _triggerContainer;
    private readonly Grid _overlayLayer;

    public Drawer()
    {
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
                if (child is DrawerTrigger dt)
                    _triggerContainer.Children.Add(dt);
                else if (child is DrawerContent dc)
                    _overlayLayer.Children.Add(dc);
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
        OpenChanged?.Invoke(this, Open);
    }
}

public enum DrawerSide { Left, Right, Top, Bottom }
";
}
