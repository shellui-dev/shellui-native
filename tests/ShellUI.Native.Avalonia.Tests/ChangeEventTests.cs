using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using AvaloniaDemo.Components.UI;
using Xunit;
using Switch = AvaloniaDemo.Components.UI.Switch;
using Slider = AvaloniaDemo.Components.UI.Slider;

namespace ShellUI.Native.Avalonia.Tests;

// Change events report user input only: values set from code stay silent, a click or a key raises
// the event once.
public class ChangeEventTests
{
    [AvaloniaFact]
    public void Switch_reports_clicks_and_keys_not_code()
    {
        var sw = new Switch { Label = "Wi-Fi" };
        var window = Ui.Show(sw);
        var raised = 0;
        sw.Toggled += (_, _) => raised++;

        sw.IsToggled = true;
        Assert.Equal(0, raised);

        Ui.Click(window, sw);
        Assert.Equal(1, raised);
        Assert.False(sw.IsToggled);

        sw.Focus();
        Ui.Press(window, Key.Space);
        Assert.Equal(2, raised);
        Assert.True(sw.IsToggled);
    }

    [AvaloniaFact]
    public void Checkbox_reports_clicks_not_code()
    {
        var cb = new Checkbox { Label = "Accept" };
        var window = Ui.Show(cb);
        var raised = 0;
        cb.CheckedChanged += (_, _) => raised++;

        cb.IsChecked = true;
        Assert.Equal(0, raised);
        Ui.Click(window, cb);
        Assert.Equal(1, raised);
        Assert.False(cb.IsChecked);
    }

    [AvaloniaFact]
    public void Toggle_reports_clicks_not_code()
    {
        var toggle = new Toggle { Text = "Bold" };
        var window = Ui.Show(toggle);
        var raised = 0;
        toggle.PressedChanged += (_, _) => raised++;

        toggle.IsPressed = true;
        Assert.Equal(0, raised);
        Ui.Click(window, toggle);
        Assert.Equal(1, raised);
    }

    [AvaloniaFact]
    public void Slider_reports_drags_and_keys_not_code_or_new_limits()
    {
        var slider = new Slider { Width = 300, HorizontalAlignment = HorizontalAlignment.Left };
        var window = Ui.Show(slider);
        var raised = 0;
        slider.ValueChanged += (_, _) => raised++;

        slider.Value = 70;
        slider.Maximum = 60; // coerces Value to 60
        Assert.Equal(0, raised);

        var left = slider.TranslatePoint(new Point(10, slider.Bounds.Height / 2), window)!.Value;
        Ui.ClickAt(window, left);
        Assert.Equal(1, raised);
        Assert.True(slider.Value < 10);

        slider.Focus();
        Ui.Press(window, Key.Right);
        Assert.Equal(2, raised);
    }

    [AvaloniaFact]
    public void Tabs_report_trigger_clicks_and_arrow_keys_not_code()
    {
        var a = new TabsTrigger { Value = "a", Text = "Account" };
        var b = new TabsTrigger { Value = "b", Text = "Password" };
        var list = new TabsList();
        list.Items.Add(a);
        list.Items.Add(b);
        var tabs = new Tabs { Value = "a", Children = { list } };
        var window = Ui.Show(tabs);
        var raised = 0;
        tabs.ValueChanged += (_, _) => raised++;

        tabs.Value = "b";
        Assert.Equal(0, raised);

        Ui.Click(window, a);
        Assert.Equal(1, raised);
        Assert.Equal("a", tabs.Value);

        a.Focus();
        Ui.Press(window, Key.Right);
        Assert.Equal(2, raised);
        Assert.Equal("b", tabs.Value);
    }

    [AvaloniaFact]
    public void Pagination_reports_page_clicks_not_code()
    {
        var pagination = new Pagination { TotalPages = 20, Page = 5 };
        var window = Ui.Show(pagination);
        var raised = 0;
        pagination.PageChanged += (_, _) => raised++;

        pagination.Page = 3;
        Assert.Equal(0, raised);
        Ui.Settle(); // the page items are rebuilt

        var page4 = pagination.GetVisualDescendants().OfType<Border>().First(b => AutomationProperties.GetName(b) == "Go to page 4");
        Ui.Click(window, page4);
        Assert.Equal(1, raised);
        Assert.Equal(4, pagination.Page);
    }

    [AvaloniaFact]
    public void Accordion_reports_trigger_clicks_not_code()
    {
        var accordion = new Accordion { Value = "a" };
        foreach (var value in new[] { "a", "b" })
        {
            var item = new AccordionItem { Value = value };
            item.Items.Add(new AccordionTrigger { Text = value });
            item.Items.Add(new AccordionContent { Child = new TextBlock { Text = value } });
            accordion.Children.Add(item);
        }
        var window = Ui.Show(accordion);
        var toggled = 0;
        var itemOpened = 0;
        accordion.ItemToggled += (_, _) => toggled++;
        foreach (var item in accordion.FindDescendantsOfType<AccordionItem>())
            item.OpenChanged += (_, _) => itemOpened++;

        accordion.Value = "b";
        Assert.Equal(0, toggled);
        Assert.Equal(0, itemOpened);

        // Single mode: opening "a" closes "b", and each change is reported.
        Ui.Click(window, accordion.FindDescendantsOfType<AccordionTrigger>().First());
        Assert.Equal(2, toggled);
        Assert.Equal(2, itemOpened);
        Assert.True(accordion.IsOpen("a"));
    }

    [AvaloniaFact]
    public void Collapsible_reports_trigger_clicks_not_code()
    {
        var trigger = new CollapsibleTrigger { Child = new TextBlock { Text = "Show more" } };
        var collapsible = new Collapsible { Children = { trigger, new CollapsibleContent { Child = new TextBlock { Text = "More" } } } };
        var window = Ui.Show(collapsible);
        var raised = 0;
        collapsible.OpenChanged += (_, _) => raised++;

        collapsible.Open = true;
        collapsible.Toggle();
        Assert.Equal(0, raised);

        Ui.Click(window, trigger);
        Assert.Equal(1, raised);
        Assert.True(collapsible.Open);
    }

    [AvaloniaFact]
    public void RadioGroup_reports_item_clicks_not_code()
    {
        var first = new RadioGroupItem { Value = "a", Text = "Default" };
        var second = new RadioGroupItem { Value = "b", Text = "Compact" };
        var group = new RadioGroup { Value = "a", Children = { first, second } };
        var window = Ui.Show(group);
        var raised = 0;
        group.ValueChanged += (_, _) => raised++;

        group.Value = "b";
        group.SetValue("a");
        Assert.Equal(0, raised);

        Ui.Click(window, second);
        Assert.Equal(1, raised);
        Assert.Equal("b", group.Value);
    }

    [AvaloniaFact]
    public void Select_reports_picked_items_not_code()
    {
        var select = new Select { Width = 240, ItemsSource = new[] { "Kenya", "Norway", "Zimbabwe" } };
        var window = Ui.Show(select);
        var raised = 0;
        select.SelectedIndexChanged += (_, _) => raised++;

        select.SelectedIndex = 1;
        Assert.Equal(0, raised);

        Ui.Click(window, select);
        Assert.True(select.IsOpen);
        Ui.Wait(250); // the list fades and scales in
        var zimbabwe = window.GetVisualDescendants().OfType<TextBlock>()
            .First(t => t.Text == "Zimbabwe" && t.IsEffectivelyVisible && !Ui.IsWithin(t, select));
        Ui.Click(window, zimbabwe);
        Assert.Equal(1, raised);
        Assert.Equal(2, select.SelectedIndex);
        Assert.False(select.IsOpen);
    }
}
