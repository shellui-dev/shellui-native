using Avalonia.Controls;

namespace AvaloniaDemo.Components.UI;

// Dropdown menu. The content floats below the trigger (above it when there is no room);
// clicking outside, picking an item, Escape or opening another menu closes it.
//   <ui:Dropdown>
//       <ui:DropdownTrigger><ui:Button Text="Options" Variant="Outline" /></ui:DropdownTrigger>
//       <ui:DropdownContent>
//           <ui:DropdownItem Text="Profile" Icon="User" Clicked="OnProfile" />
//       </ui:DropdownContent>
//   </ui:Dropdown>
public class Dropdown : ShellPopoverHost
{
    protected override bool IsContent(Control child) => child is DropdownContent;
}
