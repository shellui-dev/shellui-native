using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Dashboard metric — rounded-lg border bg-card p-5: a muted title with an optional icon, the
// value in text-2xl bold, a change pill tinted by trend, and an optional description.
// Usage: <ui:StatCard Title="Revenue" Value="$45,231" Change="+20.1%" Trend="Up" Icon="DollarSign"
//                     Description="from last month" />
// ValueTone tints the value ("Backup failed" in red); a long value wraps and the pill drops under it.
// IsLoading shows skeletons in place of the value and description; Size="Sm" fits six to a row.
public partial class StatCard : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(StatCard), string.Empty,
            propertyChanged: (b, o, n) => ((StatCard)b).Update());

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(StatCard), string.Empty,
            propertyChanged: (b, o, n) => ((StatCard)b).Update());

    public static readonly BindableProperty ChangeProperty =
        BindableProperty.Create(nameof(Change), typeof(string), typeof(StatCard), string.Empty,
            propertyChanged: (b, o, n) => ((StatCard)b).Update());

    public static readonly BindableProperty TrendProperty =
        BindableProperty.Create(nameof(Trend), typeof(StatTrend), typeof(StatCard), StatTrend.Neutral,
            propertyChanged: (b, o, n) => ((StatCard)b).Update());

    public static readonly BindableProperty DescriptionProperty =
        BindableProperty.Create(nameof(Description), typeof(string), typeof(StatCard), string.Empty,
            propertyChanged: (b, o, n) => ((StatCard)b).Update());

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(StatCard), IconName.None,
            propertyChanged: (b, o, n) => ((StatCard)b).Update());

    public static readonly BindableProperty ValueToneProperty =
        BindableProperty.Create(nameof(ValueTone), typeof(StatTone), typeof(StatCard), StatTone.Default,
            propertyChanged: (b, o, n) => ((StatCard)b).Update());

    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(StatCard), false,
            propertyChanged: (b, o, n) => ((StatCard)b).Update());

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(StatCardSize), typeof(StatCard), StatCardSize.Default,
            propertyChanged: (b, o, n) => ((StatCard)b).Update());

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Change
    {
        get => (string)GetValue(ChangeProperty);
        set => SetValue(ChangeProperty, value);
    }

    public StatTrend Trend
    {
        get => (StatTrend)GetValue(TrendProperty);
        set => SetValue(TrendProperty, value);
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

    public StatTone ValueTone
    {
        get => (StatTone)GetValue(ValueToneProperty);
        set => SetValue(ValueToneProperty, value);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public StatCardSize Size
    {
        get => (StatCardSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private readonly Border _card;
    private readonly FlexLayout _valueRow;
    private readonly Skeleton _valueSkeleton;
    private readonly Skeleton _descriptionSkeleton;
    private readonly Label _title;
    private readonly Icon _icon;
    private readonly Label _value;
    private readonly Border _pill;
    private readonly BoxView _pillTint;
    private readonly Label _change;
    private readonly Label _description;

    public StatCard()
    {
        _title = new Label { FontSize = 14, FontAttributes = FontAttributes.Bold, VerticalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.TailTruncation };
        _title.Token(Label.TextColorProperty, ShellToken.MutedForeground);
        _icon = new Icon { Size = 16, Token = ShellToken.MutedForeground, VerticalOptions = LayoutOptions.Center };
        var header = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        header.Add(_title, 0, 0);
        header.Add(_icon, 1, 0);

        // FlexLayout has no gap: the value keeps 8 on its right, so the pill wraps under a long value.
        _value = new Label { FontSize = 24, FontAttributes = FontAttributes.Bold, VerticalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(0, 0, 8, 0) };

        // The pill is the trend color at low strength behind text in the full color.
        _pillTint = new BoxView { BackgroundColor = Colors.Transparent };
        _change = new Label { FontSize = 12, FontAttributes = FontAttributes.Bold, Margin = new Thickness(6, 2), VerticalTextAlignment = TextAlignment.Center };
        _pill = new Border
        {
            Content = new Grid { Children = { _pillTint, _change } },
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 2),
            BackgroundColor = Colors.Transparent
        };
        _valueRow = new FlexLayout
        {
            Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
            AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Center,
            Margin = new Thickness(0, 8, 0, 0),
            Children = { _value, _pill }
        };
        _valueSkeleton = new Skeleton { WidthRequest = 96, HeightRequest = 28, HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(0, 10, 0, 2), IsVisible = false };
        _descriptionSkeleton = new Skeleton { WidthRequest = 128, HeightRequest = 12, HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(0, 6, 0, 0), IsVisible = false };

        _description = new Label { FontSize = 12 };
        _description.Token(Label.TextColorProperty, ShellToken.MutedForeground);

        _card = new Border
        {
            Content = new VerticalStackLayout
            {
                Spacing = 0,
                Children =
                {
                    header,
                    _valueRow,
                    _valueSkeleton,
                    _description,
                    _descriptionSkeleton
                }
            },
            Padding = new Thickness(20),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusLg }
        };
        _card.Token(VisualElement.BackgroundColorProperty, ShellToken.Card);
        _card.Token(Border.StrokeProperty, ShellToken.Border);
        Content = _card;
        Update();
    }

    private void Update()
    {
        _title.Text = Title ?? string.Empty;
        _icon.Name = Icon;
        _icon.IsVisible = Icon != IconName.None;
        _value.Text = Value ?? string.Empty;
        _value.Token(Label.TextColorProperty, ValueTone switch
        {
            StatTone.Success => ShellToken.Success,
            StatTone.Warning => ShellToken.Warning,
            StatTone.Destructive => ShellToken.Destructive,
            _ => ShellToken.CardForeground
        });

        // Sm — p-4, text-xl value, text-xs title.
        var small = Size == StatCardSize.Sm;
        _card.Padding = small ? new Thickness(16) : new Thickness(20);
        _title.FontSize = small ? 12 : 14;
        _value.FontSize = small ? 20 : 24;
        _valueSkeleton.HeightRequest = small ? 24 : 28;

        _change.Text = Change ?? string.Empty;
        _pill.IsVisible = !string.IsNullOrEmpty(Change);
        var (tint, strength, text) = Trend switch
        {
            StatTrend.Up => (ShellToken.Success, 0.12, ShellToken.Success),
            StatTrend.Down => (ShellToken.Destructive, 0.12, ShellToken.Destructive),
            _ => (ShellToken.Muted, 1.0, ShellToken.MutedForeground)
        };
        _pillTint.Token(BoxView.ColorProperty, tint);
        _pillTint.Opacity = strength;
        _change.Token(Label.TextColorProperty, text);

        _description.Text = Description ?? string.Empty;
        _description.IsVisible = !string.IsNullOrEmpty(Description) && !IsLoading;
        _description.Margin = new Thickness(0, 4, 0, 0);

        _valueRow.IsVisible = !IsLoading;
        _valueSkeleton.IsVisible = IsLoading;
        _descriptionSkeleton.IsVisible = IsLoading && !string.IsNullOrEmpty(Description);
    }
}

public enum StatTrend { Neutral, Up, Down }

public enum StatTone { Default, Success, Warning, Destructive }

public enum StatCardSize { Default, Sm }
