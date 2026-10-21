using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Dashboard metric — rounded-lg border bg-card p-5: a muted title with an optional icon, the
// value in text-2xl bold, a change pill tinted by trend, and an optional description.
// Usage: <ui:StatCard Title="Revenue" Value="$45,231" Change="+20.1%" Trend="Up" Icon="DollarSign"
//                     Description="from last month" />
// ValueTone tints the value ("Backup failed" in red); a long value wraps and the pill drops under it.
// IsLoading shows skeletons in place of the value and description; Size="Sm" fits six to a row.
public class StatCard : Border
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(Title));

    public static readonly StyledProperty<string?> ValueProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(Value));

    public static readonly StyledProperty<string?> ChangeProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(Change));

    public static readonly StyledProperty<StatTrend> TrendProperty =
        AvaloniaProperty.Register<StatCard, StatTrend>(nameof(Trend));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<StatCard, string?>(nameof(Description));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<StatCard, IconName>(nameof(Icon));

    public static readonly StyledProperty<StatTone> ValueToneProperty =
        AvaloniaProperty.Register<StatCard, StatTone>(nameof(ValueTone));

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<StatCard, bool>(nameof(IsLoading));

    public static readonly StyledProperty<StatCardSize> SizeProperty =
        AvaloniaProperty.Register<StatCard, StatCardSize>(nameof(Size));

    private readonly TextBlock _title;
    private readonly Icon _icon;
    private readonly TextBlock _value;
    private readonly Panel _pill;
    private readonly Border _pillTint;
    private readonly TextBlock _change;
    private readonly TextBlock _description;
    private readonly WrapPanel _valueRow;
    private readonly Skeleton _valueSkeleton;
    private readonly Skeleton _descriptionSkeleton;

    static StatCard()
    {
        TitleProperty.Changed.AddClassHandler<StatCard>((c, _) => c.Update());
        ValueProperty.Changed.AddClassHandler<StatCard>((c, _) => c.Update());
        ChangeProperty.Changed.AddClassHandler<StatCard>((c, _) => c.Update());
        TrendProperty.Changed.AddClassHandler<StatCard>((c, _) => c.Update());
        DescriptionProperty.Changed.AddClassHandler<StatCard>((c, _) => c.Update());
        IconProperty.Changed.AddClassHandler<StatCard>((c, _) => c.Update());
        ValueToneProperty.Changed.AddClassHandler<StatCard>((c, _) => c.Update());
        IsLoadingProperty.Changed.AddClassHandler<StatCard>((c, _) => c.Update());
        SizeProperty.Changed.AddClassHandler<StatCard>((c, _) => c.Update());
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string? Change
    {
        get => GetValue(ChangeProperty);
        set => SetValue(ChangeProperty, value);
    }

    public StatTrend Trend
    {
        get => GetValue(TrendProperty);
        set => SetValue(TrendProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public IconName Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public StatTone ValueTone
    {
        get => GetValue(ValueToneProperty);
        set => SetValue(ValueToneProperty, value);
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public StatCardSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public StatCard()
    {
        _title = new TextBlock { FontSize = 14, FontWeight = FontWeight.Medium, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        _title.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        _icon = new Icon { Size = 16, Token = ShellToken.MutedForeground, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(_icon, 1);
        var header = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), ColumnSpacing = 8, Children = { _title, _icon } };

        _value = new TextBlock { FontSize = 24, FontWeight = FontWeight.Bold, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };

        // The pill is the trend color at low strength behind text in the full color.
        _pillTint = new Border { CornerRadius = new CornerRadius(ShellTheme.RadiusMd) };
        _change = new TextBlock { FontSize = 12, FontWeight = FontWeight.Medium, Margin = new Thickness(6, 2), VerticalAlignment = VerticalAlignment.Center };
        _pill = new Panel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 2), Children = { _pillTint, _change } };
        // The pill wraps under a long value.
        _valueRow = new WrapPanel { ItemSpacing = 8, Margin = new Thickness(0, 8, 0, 0), Children = { _value, _pill } };
        _valueSkeleton = new Skeleton { Width = 96, Height = 28, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 10, 0, 2), IsVisible = false };
        _descriptionSkeleton = new Skeleton { Width = 128, Height = 12, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 6, 0, 0), IsVisible = false };

        _description = new TextBlock { FontSize = 12, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 0) };
        _description.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);

        Child = new StackPanel
        {
            Children =
            {
                header,
                _valueRow,
                _valueSkeleton,
                _description,
                _descriptionSkeleton
            }
        };
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusLg);
        this.Token(BackgroundProperty, ShellToken.Card).Token(BorderBrushProperty, ShellToken.Border);
        Update();
    }

    private void Update()
    {
        _title.Text = Title ?? string.Empty;
        _icon.Kind = Icon;
        _icon.IsVisible = Icon != IconName.None;
        _value.Text = Value ?? string.Empty;
        _value.Token(TextBlock.ForegroundProperty, ValueTone switch
        {
            StatTone.Success => ShellToken.Success,
            StatTone.Warning => ShellToken.Warning,
            StatTone.Destructive => ShellToken.Destructive,
            _ => ShellToken.CardForeground
        });

        // Sm — p-4, text-xl value, text-xs title.
        var small = Size == StatCardSize.Sm;
        Padding = small ? new Thickness(16) : new Thickness(20);
        _title.FontSize = small ? 12 : 14;
        _value.FontSize = small ? 20 : 24;
        _valueSkeleton.Height = small ? 24 : 28;

        _change.Text = Change ?? string.Empty;
        _pill.IsVisible = !string.IsNullOrEmpty(Change);
        var (tint, strength, text) = Trend switch
        {
            StatTrend.Up => (ShellToken.Success, 0.12, ShellToken.Success),
            StatTrend.Down => (ShellToken.Destructive, 0.12, ShellToken.Destructive),
            _ => (ShellToken.Muted, 1.0, ShellToken.MutedForeground)
        };
        _pillTint.Token(BackgroundProperty, tint);
        _pillTint.Opacity = strength;
        _change.Token(TextBlock.ForegroundProperty, text);

        _description.Text = Description ?? string.Empty;
        _description.IsVisible = !string.IsNullOrEmpty(Description) && !IsLoading;

        _valueRow.IsVisible = !IsLoading;
        _valueSkeleton.IsVisible = IsLoading;
        _descriptionSkeleton.IsVisible = IsLoading && !string.IsNullOrEmpty(Description);
    }
}

public enum StatTrend { Neutral, Up, Down }

public enum StatTone { Default, Success, Warning, Destructive }

public enum StatCardSize { Default, Sm }
