namespace MAUI.Demo.Components.UI;

// Popover — a panel that floats in the page layer next to its trigger; clicking outside closes it.
//   <ui:Popover>
//       <ui:PopoverTrigger><ui:Button Text="Info" Variant="Outline" /></ui:PopoverTrigger>
//       <ui:PopoverContent>...</ui:PopoverContent>
//   </ui:Popover>
public partial class Popover : ShellPopoverHost
{
    protected override bool IsContent(Element child) => child is PopoverContent;

    // Kept for compatibility with earlier versions.
    public void ToggleAsync() => Toggle();
    public void CloseAsync() => Close();
}
