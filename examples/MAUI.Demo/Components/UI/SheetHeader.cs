namespace MAUI.Demo.Components.UI;

// Title + description at the top of a sheet — flex flex-col space-y-2.
//   <ui:SheetContent>
//       <ui:SheetHeader>
//           <ui:SheetTitle Text="Edit profile" />
//           <ui:SheetDescription Text="Make changes to your profile here." />
//       </ui:SheetHeader>
//       ...
//       <ui:SheetFooter><ui:Button Text="Save changes" /></ui:SheetFooter>
//   </ui:SheetContent>
[ContentProperty(nameof(Children))]
public partial class SheetHeader : ContentView
{
    private readonly VerticalStackLayout _stack;

    public new IList<IView> Children => _stack.Children;

    public SheetHeader()
    {
        _stack = new VerticalStackLayout { Spacing = 8 };
        Content = _stack;
    }
}
