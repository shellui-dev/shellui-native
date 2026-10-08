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
        Dependencies = new List<string> { "shell", "drawer-trigger", "drawer-content" },
        Variants = new List<string> { "bottom", "top", "left", "right" },
        Tags = new List<string> { "overlay", "drawer", "panel" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"namespace YourProjectNamespace.Components.UI;

// Edge drawer (defaults to bottom, like vaul). Place it where it can fill the page:
//   <ui:Drawer x:Name=""MyDrawer""><ui:DrawerContent>...</ui:DrawerContent></ui:Drawer>
public partial class Drawer : ShellOverlayHost
{
    public static readonly BindableProperty SideProperty =
        BindableProperty.Create(nameof(Side), typeof(DrawerSide), typeof(Drawer), DrawerSide.Bottom);

    public DrawerSide Side
    {
        get => (DrawerSide)GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    protected override bool IsTrigger(Element child) => child is DrawerTrigger;
}

public enum DrawerSide { Left, Right, Top, Bottom }
",
        [NativePlatform.Avalonia] = @"using Avalonia;

namespace YourProjectNamespace.Components.UI;

// Edge drawer (defaults to bottom, like vaul). Declare it anywhere:
//   <ui:Drawer x:Name=""MyDrawer""><ui:DrawerContent>...</ui:DrawerContent></ui:Drawer>
public class Drawer : ShellOverlayHost
{
    public static readonly StyledProperty<DrawerSide> SideProperty =
        AvaloniaProperty.Register<Drawer, DrawerSide>(nameof(Side), DrawerSide.Bottom);

    public DrawerSide Side
    {
        get => GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }
}

public enum DrawerSide { Left, Right, Top, Bottom }
"
    };
}
