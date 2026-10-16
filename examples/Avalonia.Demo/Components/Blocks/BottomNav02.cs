using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

using AvaloniaDemo.Components.UI;

namespace AvaloniaDemo.Components.Blocks;

// bottom-nav-02 — an app shell with a Material 3 tab bar: a pill grows behind the active icon.
// Use it as a page's content (Content = new BottomNav02();), put your views in Pages, one per
// tab Value, and change the items to your own.
public class BottomNav02 : UserControl
{
    // One view per tab, keyed by the item's Value. Replace the placeholders with your pages.
    public Dictionary<string, Control> Pages { get; } = new()
    {
        ["home"] = Placeholder("Home"),
        ["search"] = Placeholder("Search"),
        ["saved"] = Placeholder("Saved"),
        ["settings"] = Placeholder("Settings"),
    };

    public BottomNav Nav { get; }

    private readonly ContentControl _page = new();

    public BottomNav02()
    {
        Nav = new BottomNav { Variant = BottomNavVariant.Pill, SelectedValue = "home" };
        Nav.Items.Add(new BottomNavItem { Value = "home", Icon = IconName.House, Text = "Home" });
        Nav.Items.Add(new BottomNavItem { Value = "search", Icon = IconName.Search, Text = "Search" });
        Nav.Items.Add(new BottomNavItem { Value = "saved", Icon = IconName.Heart, Text = "Saved" });
        Nav.Items.Add(new BottomNavItem { Value = "settings", Icon = IconName.Settings, Text = "Settings" });
        Nav.SelectedValueChanged += (_, value) => Show(value);
        Content = new Grid { RowDefinitions = new RowDefinitions("*,Auto"), Children = { _page, Nav } };
        Grid.SetRow(Nav, 1);
        this.Token(BackgroundProperty, ShellToken.Background);
        Show(Nav.SelectedValue);
    }

    // Shows the page for a tab Value; call it after setting Nav.SelectedValue from code.
    public void Show(string? value)
    {
        _page.Content = value != null && Pages.TryGetValue(value, out var page) ? page : null;
    }

    private static Control Placeholder(string title)
    {
        var heading = new TextBlock { Text = title, FontSize = 22, FontWeight = FontWeight.Bold };
        heading.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        var stack = new StackPanel { Margin = new Thickness(16, 16, 16, 120), Spacing = 12, Children = { heading } };
        for (var i = 0; i < 12; i++)
            stack.Children.Add(new Border { Height = 56, CornerRadius = new CornerRadius(12) }.Token(Border.BackgroundProperty, ShellToken.Muted));
        return new ScrollViewer { Content = stack };
    }
}
