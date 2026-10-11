using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Vertical timeline — markers joined by a hairline, each with a title, a time and details.
//   <ui:Timeline>
//       <ui:TimelineItem Title="Order placed" Time="09:12" Text="We received your order." />
//       <ui:TimelineItem Title="Shipped" Time="14:40" Icon="Send" IsActive="True">
//           <TextBlock Text="Any control can go here." />
//       </ui:TimelineItem>
//   </ui:Timeline>
public class Timeline : Border
{
    private readonly StackPanel _stack = new();

    [Content]
    public Controls Items => _stack.Children;

    public Timeline()
    {
        _stack.Children.CollectionChanged += (_, _) => Refresh();
        Child = _stack;
    }

    // The timeline tells each item if it is first or last, so the line starts and stops at a marker.
    private void Refresh()
    {
        var items = _stack.Children.OfType<TimelineItem>().ToList();
        for (var i = 0; i < items.Count; i++)
            items[i].Place(isFirst: i == 0, isLast: i == items.Count - 1);
    }
}

// One entry of a Timeline. Without an Icon the marker is a dot (ringed when active); with one it
// is a 28px circle around the icon. A child control goes under the text.
public class TimelineItem : Border
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<TimelineItem, string?>(nameof(Title));

    public static readonly StyledProperty<string?> TimeProperty =
        AvaloniaProperty.Register<TimelineItem, string?>(nameof(Time));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<TimelineItem, string?>(nameof(Text));

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<TimelineItem, bool>(nameof(IsActive));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<TimelineItem, IconName>(nameof(Icon));

    public static readonly StyledProperty<Control?> BodyProperty =
        AvaloniaProperty.Register<TimelineItem, Control?>(nameof(Body));

    private const double MarkerSize = 28;

    private readonly Border _line;
    private readonly Border _ring;
    private readonly Border _dot;
    private readonly Border _badge;
    private readonly Icon _icon;
    private readonly TextBlock _title;
    private readonly TextBlock _time;
    private readonly TextBlock _text;
    private readonly Border _body;
    private readonly StackPanel _details;

    static TimelineItem()
    {
        TitleProperty.Changed.AddClassHandler<TimelineItem>((i, _) => i.Update());
        TimeProperty.Changed.AddClassHandler<TimelineItem>((i, _) => i.Update());
        TextProperty.Changed.AddClassHandler<TimelineItem>((i, _) => i.Update());
        IsActiveProperty.Changed.AddClassHandler<TimelineItem>((i, _) => i.Update());
        IconProperty.Changed.AddClassHandler<TimelineItem>((i, _) => i.Update());
        BodyProperty.Changed.AddClassHandler<TimelineItem>((i, _) => i.Update());
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Time
    {
        get => GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public IconName Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    [Content]
    public Control? Body
    {
        get => GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public TimelineItem()
    {
        _line = new Border { Width = 1, HorizontalAlignment = HorizontalAlignment.Center };
        _line.Token(BackgroundProperty, ShellToken.Border);

        // ring-4 ring-primary/20 around the active dot.
        _ring = new Border { Width = 18, Height = 18, CornerRadius = new CornerRadius(9), Opacity = 0.2, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        _ring.Token(BackgroundProperty, ShellToken.Primary);
        _dot = new Border { Width = 10, Height = 10, CornerRadius = new CornerRadius(5), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        _icon = new Icon { Size = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        _badge = new Border
        {
            Child = _icon,
            Width = MarkerSize,
            Height = MarkerSize,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(MarkerSize / 2)
        };
        _badge.Token(BackgroundProperty, ShellToken.Background);
        var marker = new Panel
        {
            Width = MarkerSize,
            Height = MarkerSize,
            VerticalAlignment = VerticalAlignment.Top,
            Children = { _ring, _dot, _badge }
        };

        _title = new TextBlock { FontSize = 14, FontWeight = FontWeight.Medium, VerticalAlignment = VerticalAlignment.Center };
        _title.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        _time = new TextBlock { FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
        _time.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        _text = new TextBlock { FontSize = 14, TextWrapping = TextWrapping.Wrap };
        _text.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        _body = new Border();
        _details = new StackPanel
        {
            Spacing = 2,
            Children =
            {
                new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, MinHeight = MarkerSize, Children = { _title, _time } },
                _text,
                _body
            }
        };
        Grid.SetColumn(_details, 1);

        Child = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("28,*"),
            ColumnSpacing = 12,
            Children = { _line, marker, _details }
        };
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
            _line.VerticalAlignment = VerticalAlignment.Top;
            _line.Height = MarkerSize / 2;
            _line.Margin = new Thickness(0);
        }
        else
        {
            _line.VerticalAlignment = VerticalAlignment.Stretch;
            _line.Height = double.NaN;
            _line.Margin = new Thickness(0, isFirst ? MarkerSize / 2 : 0, 0, 0);
        }
        _details.Margin = new Thickness(0, 0, 0, isLast ? 0 : 24);
    }

    private void Update()
    {
        var hasIcon = Icon != IconName.None;
        _badge.IsVisible = hasIcon;
        _dot.IsVisible = !hasIcon;
        _ring.IsVisible = !hasIcon && IsActive;
        _icon.Kind = Icon;
        _icon.Token = IsActive ? ShellToken.Primary : ShellToken.MutedForeground;
        _badge.Token(BorderBrushProperty, IsActive ? ShellToken.Primary : ShellToken.Border);
        _dot.Token(BackgroundProperty, IsActive ? ShellToken.Primary : ShellToken.MutedForeground);

        _title.Text = Title ?? string.Empty;
        _time.Text = Time ?? string.Empty;
        _time.IsVisible = !string.IsNullOrEmpty(Time);
        _text.Text = Text ?? string.Empty;
        _text.IsVisible = !string.IsNullOrEmpty(Text);
        _body.Child = Body;
        _body.IsVisible = Body != null;
    }
}
