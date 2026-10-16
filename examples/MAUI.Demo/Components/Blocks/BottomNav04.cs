using MAUI.Demo.Components.UI;

namespace MAUI.Demo.Components.Blocks;

// bottom-nav-04 — an app shell with a compact capsule of icons; the active one widens into a pill
// with its label.
// Use it as a page's content (Content = new BottomNav04();), put your views in Pages, one per
// tab Value, and change the items to your own.
public partial class BottomNav04 : ContentView
{
    // One view per tab, keyed by the item's Value. Replace the placeholders with your pages.
    public Dictionary<string, View> Pages { get; } = new()
    {
        ["home"] = Placeholder("Home"),
        ["search"] = Placeholder("Search"),
        ["saved"] = Placeholder("Saved"),
        ["profile"] = Placeholder("Profile"),
    };

    public BottomNav Nav { get; }

    private readonly ContentView _page = new();

    public BottomNav04()
    {
        Nav = new BottomNav { Variant = BottomNavVariant.Expanding, SelectedValue = "home" };
        Nav.Items.Add(new BottomNavItem { Value = "home", Icon = IconName.House, Text = "Home" });
        Nav.Items.Add(new BottomNavItem { Value = "search", Icon = IconName.Search, Text = "Search" });
        Nav.Items.Add(new BottomNavItem { Value = "saved", Icon = IconName.Heart, Text = "Saved" });
        Nav.Items.Add(new BottomNavItem { Value = "profile", Icon = IconName.User, Text = "Profile" });
        Nav.SelectedValueChanged += (_, value) => Show(value);
        // The bar floats over the page, so the page can use the space it leaves when it hides.
        Content = new Grid { Children = { _page, Nav } };
        this.Token(BackgroundColorProperty, ShellToken.Background);
        Show(Nav.SelectedValue);
    }

    // Shows the page for a tab Value; call it after setting Nav.SelectedValue from code.
    public void Show(string? value)
    {
        _page.Content = value != null && Pages.TryGetValue(value, out var page) ? page : null;
        Nav.ScrollSource = _page.Content as ScrollView;
        Nav.IsHidden = false;
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
