using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

using AvaloniaDemo.Components.UI;

namespace AvaloniaDemo.Components.Blocks;

// bottom-nav-01 — an app shell with a tab bar along the bottom, a raised Pay button in the middle
// and the user's avatar on the last tab, like a wallet app.
// Use it as a page's content (Content = new BottomNav01();), put your views in Pages, one per
// tab Value, and change the items to your own. Handle ActionClicked for the round button.
public class BottomNav01 : UserControl
{
    // One view per tab, keyed by the item's Value. Replace the placeholders with your pages.
    public Dictionary<string, Control> Pages { get; } = new()
    {
        ["home"] = Placeholder("Home"),
        ["cards"] = Placeholder("Cards"),
        ["alerts"] = Placeholder("Alerts"),
        ["me"] = Placeholder("Me"),
    };

    public BottomNav Nav { get; }

    // The round button in the middle (an action, never the selected tab).
    public event EventHandler? ActionClicked;

    private readonly ContentControl _page = new();

    public BottomNav01()
    {
        var action = new BottomNavItem { Icon = IconName.Plus, Text = "Pay", Prominent = true };
        action.Clicked += (_, _) => ActionClicked?.Invoke(this, EventArgs.Empty);
        Nav = new BottomNav { ActiveToken = ShellToken.Primary, SelectedValue = "home" };
        Nav.Items.Add(new BottomNavItem { Value = "home", Icon = IconName.House, Text = "Home" });
        Nav.Items.Add(new BottomNavItem { Value = "cards", Icon = IconName.CreditCard, Text = "Cards" });
        Nav.Items.Add(action);
        Nav.Items.Add(new BottomNavItem { Value = "alerts", Icon = IconName.Bell, Text = "Alerts", Badge = "3" });
        Nav.Items.Add(new BottomNavItem { Value = "me", IconContent = new Avatar { Size = AvatarSize.Sm, Fallback = "ME" }, Text = "Me", Dot = true });
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
