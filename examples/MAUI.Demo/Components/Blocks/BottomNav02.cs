using MAUI.Demo.Components.UI;

namespace MAUI.Demo.Components.Blocks;

// bottom-nav-02 — an app shell with a Material 3 tab bar: a pill grows behind the active icon.
// Use it as a page's content (Content = new BottomNav02();), put your views in Pages, one per
// tab Value, and change the items to your own.
public partial class BottomNav02 : ContentView
{
    // One view per tab, keyed by the item's Value. Replace the placeholders with your pages.
    public Dictionary<string, View> Pages { get; } = new()
    {
        ["home"] = Placeholder("Home"),
        ["search"] = Placeholder("Search"),
        ["saved"] = Placeholder("Saved"),
        ["settings"] = Placeholder("Settings"),
    };

    public BottomNav Nav { get; }

    private readonly ContentView _page = new();

    public BottomNav02()
    {
        Nav = new BottomNav { Variant = BottomNavVariant.Pill, SelectedValue = "home" };
        Nav.Items.Add(new BottomNavItem { Value = "home", Icon = IconName.House, Text = "Home" });
        Nav.Items.Add(new BottomNavItem { Value = "search", Icon = IconName.Search, Text = "Search" });
        Nav.Items.Add(new BottomNavItem { Value = "saved", Icon = IconName.Heart, Text = "Saved" });
        Nav.Items.Add(new BottomNavItem { Value = "settings", Icon = IconName.Settings, Text = "Settings" });
        Nav.SelectedValueChanged += (_, value) => Show(value);
        Content = new Grid { RowDefinitions = new RowDefinitionCollection(new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto)), Children = { _page, Nav } };
        Grid.SetRow(Nav, 1);
        this.Token(BackgroundColorProperty, ShellToken.Background);
        Show(Nav.SelectedValue);
    }

    // Shows the page for a tab Value; call it after setting Nav.SelectedValue from code.
    public void Show(string? value)
    {
        _page.Content = value != null && Pages.TryGetValue(value, out var page) ? page : null;
    }

    private static View Placeholder(string title)
    {
        var heading = new Label { Text = title, FontSize = 22, FontAttributes = FontAttributes.Bold };
        heading.Token(Label.TextColorProperty, ShellToken.Foreground);
        var stack = new VerticalStackLayout { Padding = new Thickness(16, 16, 16, 120), Spacing = 12, Children = { heading } };
        for (var i = 0; i < 12; i++)
        {
            var row = new BoxView { HeightRequest = 56, CornerRadius = 12 };
            row.Token(BoxView.ColorProperty, ShellToken.Muted);
            stack.Children.Add(row);
        }
        return new ScrollView { Content = stack };
    }
}
