using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class LinkCardTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "link-card",
        DisplayName = "Link Card",
        Description = "Tappable card with icon, title and description",
        Category = ComponentCategory.Navigation,
        FilePath = "LinkCard.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "link", "card", "navigation" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Tappable card — rounded-lg border bg-card p-4, hover:bg-accent: an optional icon, a title, a
// muted description and an arrow. Tapping raises Clicked and opens Url when one is set.
// Usage: <ui:LinkCard Title=""Documentation"" Description=""Guides and API reference.""
//                     Icon=""FileText"" Url=""https://shellui.dev/docs"" />
public partial class LinkCard : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(LinkCard), string.Empty,
            propertyChanged: (b, o, n) => ((LinkCard)b).Update());

    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(LinkCard), string.Empty,
            propertyChanged: (b, o, n) => ((LinkCard)b).Update());

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(LinkCard), IconName.None,
            propertyChanged: (b, o, n) => ((LinkCard)b).Update());

    // Opened in the system browser (or the app registered for the link) when the card is tapped.
    public static readonly BindableProperty UrlProperty =
        BindableProperty.Create(nameof(Url), typeof(string), typeof(LinkCard), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public IconName Icon
    {
        get => (IconName)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Url
    {
        get => (string)GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }

    public event EventHandler? Clicked;

    private readonly Border _card;
    private readonly Icon _icon;
    private readonly Label _title;
    private readonly Label _description;

    public LinkCard()
    {
        _icon = new Icon { Size = 20, Token = ShellToken.MutedForeground, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(0, 1, 0, 0) };
        _title = new Label { FontSize = 15, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.TailTruncation };
        _title.Token(Label.TextColorProperty, ShellToken.CardForeground);
        _description = new Label { FontSize = 14 };
        _description.Token(Label.TextColorProperty, ShellToken.MutedForeground);
        var arrow = new Icon { Name = IconName.ArrowUpRight, Size = 16, Token = ShellToken.MutedForeground, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(0, 2, 0, 0) };

        var row = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)
            }
        };
        row.Add(_icon, 0, 0);
        row.Add(new VerticalStackLayout { Spacing = 4, Children = { _title, _description } }, 1, 0);
        row.Add(arrow, 2, 0);

        _card = new Border
        {
            Content = row,
            Padding = new Thickness(16),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusLg }
        };
        _card.Token(VisualElement.BackgroundColorProperty, ShellToken.Card);
        _card.Token(Border.StrokeProperty, ShellToken.Border);

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => _card.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        pointer.PointerExited += (_, _) => _card.Token(VisualElement.BackgroundColorProperty, ShellToken.Card);
        _card.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { ShellFocus.FocusPressed(this); Press(); };
        _card.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, Press);

        Content = _card;
        Update();
    }

    private async void Press()
    {
        if (!IsEnabled) return;
        Clicked?.Invoke(this, EventArgs.Empty);
        if (string.IsNullOrWhiteSpace(Url) || !Uri.TryCreate(Url, UriKind.Absolute, out var uri)) return;
        try
        {
            await Launcher.Default.OpenAsync(uri);
        }
        catch (Exception)
        {
            // Nothing on the device can open this link.
        }
    }

    private void Update()
    {
        _icon.Name = Icon;
        _icon.IsVisible = Icon != IconName.None;
        _title.Text = Title ?? string.Empty;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _description.Text = Description ?? string.Empty;
        _description.IsVisible = !string.IsNullOrEmpty(Description);
    }
}
"
    };
}
