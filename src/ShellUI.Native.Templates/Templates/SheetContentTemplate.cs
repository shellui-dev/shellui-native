using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class SheetContentTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "sheet-content",
        DisplayName = "Sheet Content",
        Description = "Sheet panel content",
        Category = ComponentCategory.Overlay,
        FilePath = "SheetContent.cs",
        Dependencies = new List<string> { "element-extensions" },
        Tags = new List<string> { "overlay", "sheet", "content" }
    };

    public static string Content => @"namespace YourProjectNamespace.Components.UI;

[ContentProperty(nameof(Children))]
public partial class SheetContent : ContentView
{
    private readonly Grid _root;
    private readonly Border _backdrop;
    private readonly Border _panel;
    private readonly VerticalStackLayout _body;

    public IList<IView> Children => _body.Children;

    public SheetContent()
    {
        _backdrop = new Border
        {
            BackgroundColor = Color.FromArgb(""#80000000""),
            StrokeThickness = 0,
            ZIndex = 0
        };
        var tapBackdrop = new TapGestureRecognizer();
        tapBackdrop.Tapped += (s, e) => FindParentOfType<Sheet>()?.SetOpen(false);
        _backdrop.GestureRecognizers.Add(tapBackdrop);

        _body = new VerticalStackLayout { Spacing = 16, Padding = new Thickness(24) };
        _panel = new Border
        {
            Content = _body,
            BackgroundColor = Color.FromArgb(""#FFFFFF""),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            ZIndex = 1
        };

        _root = new Grid { Children = { _backdrop, _panel } };
        Content = _root;
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        var sheet = FindParentOfType<Sheet>();
        if (sheet != null)
        {
            _panel.VerticalOptions = sheet.Side switch
            {
                SheetSide.Top => LayoutOptions.Start,
                SheetSide.Bottom => LayoutOptions.End,
                _ => LayoutOptions.Fill
            };
            _panel.HorizontalOptions = sheet.Side switch
            {
                SheetSide.Left => LayoutOptions.Start,
                SheetSide.Right => LayoutOptions.End,
                _ => LayoutOptions.Fill
            };
            _panel.WidthRequest = sheet.Side is SheetSide.Left or SheetSide.Right ? 300 : -1;
            _panel.HeightRequest = sheet.Side is SheetSide.Top or SheetSide.Bottom ? 400 : -1;
        }
    }
}
";
}
