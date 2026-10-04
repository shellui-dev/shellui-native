namespace MAUI.Demo.Components.UI;

// Dropdown menu. The content floats in the page layer below the trigger (above it when there
// is no room); clicking outside, picking an item or opening another menu closes it.
//   <ui:Dropdown>
//       <ui:DropdownTrigger><ui:Button Text="Options" Variant="Outline" /></ui:DropdownTrigger>
//       <ui:DropdownContent>
//           <ui:DropdownItem Text="Profile" Icon="User" Clicked="OnProfile" />
//       </ui:DropdownContent>
//   </ui:Dropdown>
public partial class Dropdown : ShellPopoverHost
{
    protected override bool IsContent(Element child) => child is DropdownContent;

    // Kept for compatibility with earlier versions.
    public void ToggleAsync() => Toggle();
    public void CloseAsync() => Close();
}
