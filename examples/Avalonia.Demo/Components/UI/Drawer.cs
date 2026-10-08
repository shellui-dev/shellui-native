using Avalonia;

namespace AvaloniaDemo.Components.UI;

// Edge drawer (defaults to bottom, like vaul). Declare it anywhere:
//   <ui:Drawer x:Name="MyDrawer"><ui:DrawerContent>...</ui:DrawerContent></ui:Drawer>
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
