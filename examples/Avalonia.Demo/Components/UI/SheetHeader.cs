using Avalonia.Controls;

namespace AvaloniaDemo.Components.UI;

// Title + description at the top of a sheet — flex flex-col space-y-2.
//   <ui:SheetContent>
//       <ui:SheetHeader>
//           <ui:SheetTitle Text="Edit profile" />
//           <ui:SheetDescription Text="Make changes to your profile here." />
//       </ui:SheetHeader>
//       ...
//       <ui:SheetFooter><ui:Button Text="Save changes" /></ui:SheetFooter>
//   </ui:SheetContent>
public class SheetHeader : StackPanel
{
    public SheetHeader()
    {
        Spacing = 8;
    }
}
