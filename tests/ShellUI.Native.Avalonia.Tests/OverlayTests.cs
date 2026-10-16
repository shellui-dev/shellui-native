using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using AvaloniaDemo.Components.UI;
using AvaloniaDemo.Components.UI.Variants;
using Xunit;
using Button = AvaloniaDemo.Components.UI.Button;

namespace ShellUI.Native.Avalonia.Tests;

// Dialogs keep focus inside and hand it back; floating panels close on a click outside or Escape.
public class OverlayTests
{
    [AvaloniaFact]
    public void Dialog_traps_tab_and_returns_focus_on_escape()
    {
        var open = new Button { Text = "Open" };
        var first = new Button { Text = "Cancel" };
        var second = new Button { Text = "Continue" };
        var content = new DialogContent();
        content.Items.Add(first);
        content.Items.Add(second);
        var dialog = new Dialog();
        dialog.Items.Add(new DialogTrigger { Child = open });
        dialog.Items.Add(content);
        var window = Ui.Show(new StackPanel { Children = { new Button { Text = "Before" }, dialog } });

        Ui.Click(window, open);
        Assert.True(dialog.Open);
        Ui.Wait(250);
        Assert.True(Ui.IsWithin(Ui.Focused(window), content));

        // More presses than there are tab stops: focus wraps and never reaches the page behind.
        for (var i = 0; i < 6; i++)
        {
            Ui.Press(window, Key.Tab);
            Assert.True(Ui.IsWithin(Ui.Focused(window), content), $"Tab {i + 1} left the dialog");
        }
        Ui.Press(window, Key.Tab, RawInputModifiers.Shift);
        Assert.True(Ui.IsWithin(Ui.Focused(window), content));

        Ui.Press(window, Key.Escape);
        Assert.False(dialog.Open);
        Assert.True(Ui.IsWithin(Ui.Focused(window), open));
    }

    [AvaloniaFact]
    public void Dropdown_closes_on_a_click_outside_without_clicking_through()
    {
        var picked = 0;
        var behind = 0;
        var item = new DropdownItem { Text = "Profile" };
        item.Clicked += (_, _) => picked++;
        var menu = new DropdownContent();
        menu.Items.Add(item);
        var dropdown = new Dropdown();
        dropdown.Items.Add(new DropdownTrigger { Child = new Button { Text = "Options" } });
        dropdown.Items.Add(menu);
        var below = new Button { Text = "Below", Margin = new Thickness(0, 300, 0, 0) };
        below.Clicked += (_, _) => behind++;
        var window = Ui.Show(new StackPanel { HorizontalAlignment = HorizontalAlignment.Left, Children = { dropdown, below } });

        Ui.Click(window, dropdown);
        Assert.True(dropdown.IsOpen);
        Ui.Wait(250);

        // The click that dismisses does nothing else.
        Ui.Click(window, below);
        Assert.False(dropdown.IsOpen);
        Assert.Equal(0, behind);

        Ui.Click(window, dropdown);
        Ui.Wait(250);
        Ui.Click(window, item);
        Assert.Equal(1, picked);
        Assert.False(dropdown.IsOpen);

        Ui.Click(window, dropdown);
        Ui.Wait(250);
        Ui.Press(window, Key.Escape);
        Assert.False(dropdown.IsOpen);
    }

    [AvaloniaFact]
    public void HoverCard_stays_open_while_hovered_and_closes_on_leave()
    {
        var link = new Button { Text = "@shewart", Variant = ButtonVariant.Link };
        var card = new HoverCard();
        card.Items.Add(new HoverCardTrigger { Child = link });
        card.Items.Add(new HoverCardContent { Child = new TextBlock { Text = "Shewart Shepherd" } });
        var window = Ui.Show(card);

        Ui.Hover(window, link);
        Ui.Wait(450); // OpenDelay is 300 ms
        Assert.True(card.IsOpen);

        // Nothing covers the trigger, so the pointer stays on it and the card doesn't flicker shut.
        Assert.DoesNotContain(window.GetVisualDescendants(), v => v.GetType().Name == "LightDismissOverlayLayer" && v.IsVisible);
        Ui.Hover(window, link);
        Ui.Wait(400);
        Assert.True(card.IsOpen);

        window.MouseMove(new Point(850, 650), RawInputModifiers.None);
        Ui.Wait(400); // CloseDelay is 200 ms
        Assert.False(card.IsOpen);
    }
}
