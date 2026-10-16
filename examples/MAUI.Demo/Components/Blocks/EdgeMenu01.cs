using MAUI.Demo.Components.UI;

namespace MAUI.Demo.Components.Blocks;

// edge-menu-01 — a page with a dock of tools on its right edge: a solid rail that dims the page
// while open. Put your view in Page, change the tiles to your tools and handle ToolPicked.
public partial class EdgeMenu01 : ContentView
{
    public EdgeMenu Menu { get; }

    // The page under the menu.
    public View? Page
    {
        get => _page.Content as View;
        set => _page.Content = value;
    }

    // The picked tile's Value.
    public event EventHandler<string>? ToolPicked;

    private readonly ContentView _page = new();

    public EdgeMenu01()
    {
        Menu = new EdgeMenu { Variant = EdgeMenuVariant.Solid, Dim = true, Position = 0.45 };
        foreach (var (icon, text) in new[] { (IconName.Settings, "Settings"), (IconName.FileText, "Notes"), (IconName.Calendar, "Calendar"), (IconName.Image, "Images"), (IconName.Camera, "Camera") })
        {
            var value = text.ToLowerInvariant();
            var tile = new EdgeMenuItem { Icon = icon, Text = text, Value = value };
            tile.Clicked += (_, _) => ToolPicked?.Invoke(this, value);
            Menu.Items.Add(tile);
        }
        Content = new Grid { Children = { _page, Menu } };
        this.Token(BackgroundColorProperty, ShellToken.Background);
        Page = Placeholder("Notes");
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
