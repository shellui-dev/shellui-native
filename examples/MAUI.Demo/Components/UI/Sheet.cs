namespace MAUI.Demo.Components.UI;

// Side sheet (defaults to right). Place it where it can fill the page:
//   <ui:Sheet x:Name="Settings"><ui:SheetContent>...</ui:SheetContent></ui:Sheet>
public partial class Sheet : ShellOverlayHost
{
    public static readonly BindableProperty SideProperty =
        BindableProperty.Create(nameof(Side), typeof(SheetSide), typeof(Sheet), SheetSide.Right);

    public SheetSide Side
    {
        get => (SheetSide)GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    protected override bool IsTrigger(Element child) => child is SheetTrigger;
}

public enum SheetSide { Left, Right, Top, Bottom }
