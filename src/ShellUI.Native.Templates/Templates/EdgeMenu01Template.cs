using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class EdgeMenu01Template
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "edge-menu-01",
        DisplayName = "Edge Menu 01",
        Description = "A page with a solid dock of tools on its right edge",
        Category = ComponentCategory.Block,
        FilePath = "../Blocks/EdgeMenu01.cs",
        Dependencies = new List<string> { "shell", "icon", "edge-menu" },
        Tags = new List<string> { "block", "edge-menu", "dock", "tools", "mobile" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using YourProjectNamespace.Components.UI;

namespace YourProjectNamespace.Components.Blocks;

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
        foreach (var (icon, text) in new[] { (IconName.Settings, ""Settings""), (IconName.FileText, ""Notes""), (IconName.Calendar, ""Calendar""), (IconName.Image, ""Images""), (IconName.Camera, ""Camera"") })
        {
            var value = text.ToLowerInvariant();
            var tile = new EdgeMenuItem { Icon = icon, Text = text, Value = value };
            tile.Clicked += (_, _) => ToolPicked?.Invoke(this, value);
            Menu.Items.Add(tile);
        }
        Content = new Grid { Children = { _page, Menu } };
        this.Token(BackgroundColorProperty, ShellToken.Background);
        Page = Placeholder(""Notes"");
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
",
        [NativePlatform.Avalonia] = @"using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

using YourProjectNamespace.Components.UI;

namespace YourProjectNamespace.Components.Blocks;

// edge-menu-01 — a page with a dock of tools on its right edge: a solid rail that dims the page
// while open. Put your view in Page, change the tiles to your tools and handle ToolPicked.
public class EdgeMenu01 : UserControl
{
    public EdgeMenu Menu { get; }

    // The page under the menu.
    public Control? Page
    {
        get => _page.Content as Control;
        set => _page.Content = value;
    }

    // The picked tile's Value.
    public event EventHandler<string>? ToolPicked;

    private readonly ContentControl _page = new();

    public EdgeMenu01()
    {
        Menu = new EdgeMenu { Variant = EdgeMenuVariant.Solid, Dim = true, Position = 0.45 };
        foreach (var (icon, text) in new[] { (IconName.Settings, ""Settings""), (IconName.FileText, ""Notes""), (IconName.Calendar, ""Calendar""), (IconName.Image, ""Images""), (IconName.Camera, ""Camera"") })
        {
            var value = text.ToLowerInvariant();
            var tile = new EdgeMenuItem { Icon = icon, Text = text, Value = value };
            tile.Clicked += (_, _) => ToolPicked?.Invoke(this, value);
            Menu.Items.Add(tile);
        }
        Content = new Grid { Children = { _page, Menu } };
        this.Token(BackgroundProperty, ShellToken.Background);
        Page = Placeholder(""Notes"");
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
"
    };
}
