namespace MAUI.Demo.Components.UI;

// Edge drawer (defaults to bottom, like vaul). Place it where it can fill the page:
//   <ui:Drawer x:Name="MyDrawer"><ui:DrawerContent>...</ui:DrawerContent></ui:Drawer>
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
