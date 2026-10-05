using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class TimelineTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "timeline",
        DisplayName = "Timeline",
        Description = "Vertical list of events joined by a line",
        Category = ComponentCategory.DataDisplay,
        FilePath = "Timeline.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "timeline", "history", "activity", "steps" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Vertical timeline — markers joined by a hairline, each with a title, a time and details.
//   <ui:Timeline>
//       <ui:TimelineItem Title=""Order placed"" Time=""09:12"" Text=""We received your order."" />
//       <ui:TimelineItem Title=""Shipped"" Time=""14:40"" Icon=""Send"" IsActive=""True"">
//           <Label Text=""Any view can go here."" />
//       </ui:TimelineItem>
//   </ui:Timeline>
[ContentProperty(nameof(Items))]
public partial class Timeline : ContentView
{
    private readonly VerticalStackLayout _stack;

    public IList<IView> Items => _stack.Children;

    public Timeline()
    {
        _stack = new VerticalStackLayout { Spacing = 0 };
        _stack.ChildAdded += (_, _) => Refresh();
        _stack.ChildRemoved += (_, _) => Refresh();
        Content = _stack;
        Loaded += (_, _) => Refresh();
    }

    // Items don't know their place: the timeline tells each one whether it is first or last,
    // so the line starts at the first marker and stops at the last.
    private void Refresh()
    {
        var items = _stack.Children.OfType<TimelineItem>().ToList();
        for (var i = 0; i < items.Count; i++)
            items[i].Place(isFirst: i == 0, isLast: i == items.Count - 1);
    }
}

// One entry of a Timeline. Without an Icon the marker is a dot (ringed when active); with one it
// is a 28px circle around the icon.
[ContentProperty(nameof(Body))]
public partial class TimelineItem : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(TimelineItem), string.Empty,
            propertyChanged: (b, o, n) => ((TimelineItem)b).Update());

    public static readonly BindableProperty TimeProperty =
        BindableProperty.Create(nameof(Time), typeof(string), typeof(TimelineItem), string.Empty,
            propertyChanged: (b, o, n) => ((TimelineItem)b).Update());

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(TimelineItem), string.Empty,
            propertyChanged: (b, o, n) => ((TimelineItem)b).Update());

    public static readonly BindableProperty IsActiveProperty =
        BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(TimelineItem), false,
            propertyChanged: (b, o, n) => ((TimelineItem)b).Update());

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(TimelineItem), IconName.None,
            propertyChanged: (b, o, n) => ((TimelineItem)b).Update());

    public static readonly BindableProperty BodyProperty =
        BindableProperty.Create(nameof(Body), typeof(View), typeof(TimelineItem), null,
            propertyChanged: (b, o, n) => ((TimelineItem)b).Update());

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Time
    {
        get => (string)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public IconName Icon
    {
        get => (IconName)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    private const double MarkerSize = 28;

    private readonly BoxView _line;
    private readonly BoxView _ring;
    private readonly Border _dot;
    private readonly Border _badge;
    private readonly Icon _icon;
    private readonly Label _title;
    private readonly Label _time;
    private readonly Label _text;
    private readonly ContentView _body;
    private readonly VerticalStackLayout _details;

    public TimelineItem()
    {
        _line = new BoxView { WidthRequest = 1, BackgroundColor = Colors.Transparent, HorizontalOptions = LayoutOptions.Center };
        _line.Token(BoxView.ColorProperty, ShellToken.Border);

        // ring-4 ring-primary/20 around the active dot.
        _ring = new BoxView { WidthRequest = 18, HeightRequest = 18, CornerRadius = 9, BackgroundColor = Colors.Transparent, Opacity = 0.2, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        _ring.Token(BoxView.ColorProperty, ShellToken.Primary);
        _dot = new Border
        {
            WidthRequest = 10,
            HeightRequest = 10,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 5 },
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        _icon = new Icon { Size = 14 };
        _badge = new Border
        {
            Content = _icon,
            WidthRequest = MarkerSize,
            HeightRequest = MarkerSize,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = MarkerSize / 2 }
        };
        _badge.Token(VisualElement.BackgroundColorProperty, ShellToken.Background);
        var marker = new Grid
        {
            WidthRequest = MarkerSize,
            HeightRequest = MarkerSize,
            VerticalOptions = LayoutOptions.Start,
            Children = { _ring, _dot, _badge }
        };

        _title = new Label { FontSize = 14, FontAttributes = FontAttributes.Bold, VerticalTextAlignment = TextAlignment.Center };
        _title.Token(Label.TextColorProperty, ShellToken.Foreground);
        _time = new Label { FontSize = 12, VerticalTextAlignment = TextAlignment.Center };
        _time.Token(Label.TextColorProperty, ShellToken.MutedForeground);
        _text = new Label { FontSize = 14 };
        _text.Token(Label.TextColorProperty, ShellToken.MutedForeground);
        _body = new ContentView();
        _details = new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                new HorizontalStackLayout { Spacing = 8, MinimumHeightRequest = MarkerSize, Children = { _title, _time } },
                _text,
                _body
            }
        };

        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(MarkerSize), new ColumnDefinition(GridLength.Star) }
        };
        grid.Add(_line, 0, 0);
        grid.Add(marker, 0, 0);
        grid.Add(_details, 1, 0);
        Content = grid;
        Place(isFirst: true, isLast: true);
        Update();
    }

    internal void Place(bool isFirst, bool isLast)
    {
        // The line runs through the marker's center: from it down (first), up to it (last), or
        // the full height (in between). A lone item has no line.
        _line.IsVisible = !(isFirst && isLast);
        if (isLast)
        {
            _line.VerticalOptions = LayoutOptions.Start;
            _line.HeightRequest = MarkerSize / 2;
            _line.Margin = new Thickness(0);
        }
        else
        {
            _line.VerticalOptions = LayoutOptions.Fill;
            _line.HeightRequest = -1;
            _line.Margin = new Thickness(0, isFirst ? MarkerSize / 2 : 0, 0, 0);
        }
        _details.Padding = new Thickness(0, 0, 0, isLast ? 0 : 24);
    }

    private void Update()
    {
        var hasIcon = Icon != IconName.None;
        _badge.IsVisible = hasIcon;
        _dot.IsVisible = !hasIcon;
        _ring.IsVisible = !hasIcon && IsActive;
        _icon.Name = Icon;
        _icon.Token = IsActive ? ShellToken.Primary : ShellToken.MutedForeground;
        _badge.Token(Border.StrokeProperty, IsActive ? ShellToken.Primary : ShellToken.Border);
        _dot.Token(VisualElement.BackgroundColorProperty, IsActive ? ShellToken.Primary : ShellToken.MutedForeground);

        _title.Text = Title ?? string.Empty;
        _time.Text = Time ?? string.Empty;
        _time.IsVisible = !string.IsNullOrEmpty(Time);
        _text.Text = Text ?? string.Empty;
        _text.IsVisible = !string.IsNullOrEmpty(Text);
        _body.Content = Body;
        _body.IsVisible = Body != null;
    }
}
"
    };
}
