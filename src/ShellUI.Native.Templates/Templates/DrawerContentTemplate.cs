using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DrawerContentTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "drawer-content",
        DisplayName = "Drawer Content",
        Description = "Drawer panel content - slides in from side",
        Category = ComponentCategory.Overlay,
        FilePath = "DrawerContent.cs",
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "overlay", "drawer", "content" }
    };

    public static string Content => @"using Microsoft.Maui.Controls.Shapes;
namespace YourProjectNamespace.Components.UI;

[ContentProperty(nameof(Children))]
public partial class DrawerContent : ContentView
{
    private readonly Grid _root;
    private readonly Border _backdrop;
    private readonly Border _panel;
    private readonly VerticalStackLayout _body;

    public new IList<IView> Children => _body.Children;

    public DrawerContent()
    {
        _backdrop = new Border
        {
            BackgroundColor = Color.FromArgb(""#80000000""),
            StrokeThickness = 0,
            ZIndex = 0
        };
        var tapBackdrop = new TapGestureRecognizer();
        tapBackdrop.Tapped += (s, e) => FindParentOfType<Drawer>()?.SetOpen(false);
        _backdrop.GestureRecognizers.Add(tapBackdrop);

        _body = new VerticalStackLayout { Spacing = 16, Padding = new Thickness(16) };
        _panel = new Border
        {
            Content = _body,
            BackgroundColor = Color.FromArgb(""#FFFFFF""),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 0 },
            ZIndex = 1
        };

        _root = new Grid { Children = { _backdrop, _panel } };
        Content = _root;
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        var drawer = FindParentOfType<Drawer>();
        if (drawer != null)
        {
            _panel.VerticalOptions = drawer.Side switch
            {
                DrawerSide.Top => LayoutOptions.Start,
                DrawerSide.Bottom => LayoutOptions.End,
                _ => LayoutOptions.Fill
            };
            _panel.HorizontalOptions = drawer.Side switch
            {
                DrawerSide.Left => LayoutOptions.Start,
                DrawerSide.Right => LayoutOptions.End,
                _ => LayoutOptions.Fill
            };
            _panel.WidthRequest = drawer.Side is DrawerSide.Left or DrawerSide.Right ? 280 : -1;
            _panel.HeightRequest = drawer.Side is DrawerSide.Top or DrawerSide.Bottom ? 300 : -1;
            if (drawer.Side == DrawerSide.Top || drawer.Side == DrawerSide.Bottom)
                _panel.StrokeShape = new RoundRectangle { CornerRadius = drawer.Side == DrawerSide.Bottom ? 12 : 0 };
        }
    }
}
";
}
