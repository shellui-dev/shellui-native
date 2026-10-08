using Avalonia;

namespace AvaloniaDemo.Components.UI;

// Side sheet (defaults to right). Declare it anywhere:
//   <ui:Sheet x:Name="Settings"><ui:SheetContent>...</ui:SheetContent></ui:Sheet>
public class Sheet : ShellOverlayHost
{
    public static readonly StyledProperty<SheetSide> SideProperty =
        AvaloniaProperty.Register<Sheet, SheetSide>(nameof(Side), SheetSide.Right);

    public SheetSide Side
    {
        get => GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }
}

public enum SheetSide { Left, Right, Top, Bottom }
