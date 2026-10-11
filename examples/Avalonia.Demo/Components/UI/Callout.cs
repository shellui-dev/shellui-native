using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Highlighted note — rounded-lg border px-4 py-3 on a faint tint of the variant color, with an
// icon, an optional title and text and/or any content.
//   <ui:Callout Variant="Tip" Title="Pro tip" Text="Press Ctrl+K to search." />
//   <ui:Callout Variant="Warning" Title="Heads up"><TextBlock Text="..." /></ui:Callout>
public class Callout : Border
{
    public static readonly StyledProperty<CalloutVariant> VariantProperty =
        AvaloniaProperty.Register<Callout, CalloutVariant>(nameof(Variant), CalloutVariant.Info);

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Callout, string?>(nameof(Title));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Callout, string?>(nameof(Text));

    // None uses the variant's own icon.
    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<Callout, IconName>(nameof(Icon), IconName.None);

    public static readonly StyledProperty<Control?> BodyProperty =
        AvaloniaProperty.Register<Callout, Control?>(nameof(Body));

    private readonly Border _tint;
    private readonly Icon _icon;
    private readonly TextBlock _title;
    private readonly TextBlock _text;
    private readonly ContentControl _body = new();

    static Callout()
    {
        VariantProperty.Changed.AddClassHandler<Callout>((c, _) => c.Update());
        TitleProperty.Changed.AddClassHandler<Callout>((c, _) => c.Update());
        TextProperty.Changed.AddClassHandler<Callout>((c, _) => c.Update());
        IconProperty.Changed.AddClassHandler<Callout>((c, _) => c.Update());
        BodyProperty.Changed.AddClassHandler<Callout>((c, _) => c.Update());
    }

    public CalloutVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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

    public Callout()
    {
        _tint = new Border { CornerRadius = new CornerRadius(ShellTheme.RadiusLg - 1) };
        _icon = new Icon { Size = 16, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 2, 0, 0) };
        _title = new TextBlock { FontSize = 14, FontWeight = FontWeight.SemiBold, TextWrapping = TextWrapping.Wrap };
        _text = new TextBlock { FontSize = 14, LineHeight = 20, TextWrapping = TextWrapping.Wrap };
        _text.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);

        var text = new StackPanel { Spacing = 6, Children = { _title, _text, _body } };
        Grid.SetColumn(text, 1);
        var row = new Grid
        {
            Margin = new Thickness(16, 12),
            ColumnSpacing = 12,
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            Children = { _icon, text }
        };

        Child = new Panel { Children = { _tint, row } };
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusLg);
        this.Token(BorderBrushProperty, ShellToken.Border);
        Update();
    }

    private void Update()
    {
        var (icon, accent) = Variant switch
        {
            CalloutVariant.Warning => (IconName.TriangleAlert, ShellToken.Warning),
            CalloutVariant.Danger => (IconName.CircleAlert, ShellToken.Destructive),
            CalloutVariant.Tip => (IconName.Lightbulb, ShellToken.Success),
            CalloutVariant.Default => (IconName.Info, ShellToken.MutedForeground),
            _ => (IconName.Info, ShellToken.Info)
        };
        var neutral = Variant == CalloutVariant.Default;

        // bg-<color>/8 for the tinted variants, bg-muted/50 for the neutral one.
        _tint.Token(BackgroundProperty, neutral ? ShellToken.Muted : accent);
        _tint.Opacity = neutral ? 0.5 : 0.08;

        _icon.Kind = Icon == IconName.None ? icon : Icon;
        _icon.Token = accent;
        _title.Text = Title ?? string.Empty;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _title.Token(TextBlock.ForegroundProperty, neutral ? ShellToken.Foreground : accent);
        _text.Text = Text ?? string.Empty;
        _text.IsVisible = !string.IsNullOrEmpty(Text);
        _body.Content = Body;
        _body.IsVisible = Body != null;
    }
}

public enum CalloutVariant
{
    Info,
    Warning,
    Danger,
    Tip,
    Default
}
