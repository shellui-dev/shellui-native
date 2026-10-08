using Avalonia.Controls;

namespace AvaloniaDemo.Components.UI;

// Popover — a panel that floats next to its trigger; clicking outside or Escape closes it.
//   <ui:Popover>
//       <ui:PopoverTrigger><ui:Button Text="Info" Variant="Outline" /></ui:PopoverTrigger>
//       <ui:PopoverContent>...</ui:PopoverContent>
//   </ui:Popover>
public class Popover : ShellPopoverHost
{
    protected override bool IsContent(Control child) => child is PopoverContent;
}
