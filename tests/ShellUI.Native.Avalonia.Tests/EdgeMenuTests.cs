using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using AvaloniaDemo.Components.UI;
using Xunit;
using Button = AvaloniaDemo.Components.UI.Button;

namespace ShellUI.Native.Avalonia.Tests;

// The edge menu opens from its handle, reports picks once, and closes on a pick, a click outside
// or Escape; while closed it doesn't get in the page's way.
public class EdgeMenuTests
{
    private static (Window Window, EdgeMenu Menu, EdgeMenuItem Calendar, Button Behind) Show(EdgeMenuSide side)
    {
        var behind = new Button { Text = "Behind", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(16) };
        var calendar = new EdgeMenuItem { Icon = IconName.Calendar, Text = "Calendar", Value = "calendar" };
        var menu = new EdgeMenu { Side = side, SelectedValue = "notes" };
        menu.Items.Add(new EdgeMenuItem { Icon = IconName.FileText, Text = "Notes", Value = "notes" });
        menu.Items.Add(calendar);
        var window = Ui.Show(new Grid { Children = { behind, menu } }, width: 390, height: 600);
        return (window, menu, calendar, behind);
    }

    private static Control Handle(EdgeMenu menu) =>
        menu.GetVisualDescendants().OfType<Border>().First(b => b.Width == 28);

    [AvaloniaTheory]
    [InlineData(EdgeMenuSide.Right)]
    [InlineData(EdgeMenuSide.Left)]
    public void Opens_from_the_handle_and_reports_a_pick_once(EdgeMenuSide side)
    {
        var (window, menu, calendar, _) = Show(side);
        var picked = 0;
        var clicked = 0;
        menu.SelectedValueChanged += (_, _) => picked++;
        calendar.Clicked += (_, _) => clicked++;

        menu.SelectedValue = "calendar";
        menu.SelectedValue = "notes";
        Assert.Equal(0, picked);

        Ui.Click(window, Handle(menu));
        Assert.True(menu.IsOpen);
        Ui.Wait(300); // the rail slides in

        Ui.Click(window, calendar);
        Assert.Equal(1, picked);
        Assert.Equal(1, clicked);
        Assert.Equal("calendar", menu.SelectedValue);
        Assert.False(menu.IsOpen);
    }

    [AvaloniaFact]
    public void Closes_on_a_click_outside_or_Escape_and_lets_the_page_through_when_closed()
    {
        var (window, menu, _, behind) = Show(EdgeMenuSide.Right);
        var pressed = 0;
        behind.Clicked += (_, _) => pressed++;

        Ui.Click(window, behind);
        Assert.Equal(1, pressed);

        menu.IsOpen = true;
        Ui.Wait(300);
        Ui.Click(window, behind); // lands on the backdrop
        Assert.False(menu.IsOpen);
        Assert.Equal(1, pressed);

        menu.IsOpen = true;
        Ui.Wait(300);
        Ui.Press(window, Key.Escape);
        Assert.False(menu.IsOpen);
    }
}
