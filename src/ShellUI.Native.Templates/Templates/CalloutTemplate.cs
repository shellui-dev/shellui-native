using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class CalloutTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "callout",
        DisplayName = "Callout",
        Description = "Highlighted note tinted by variant (info, warning, danger, tip)",
        Category = ComponentCategory.Feedback,
        FilePath = "Callout.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "callout", "note", "admonition", "info" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Highlighted note — rounded-lg border px-4 py-3 on a faint tint of the variant color, with an
// icon, an optional title and text and/or any content.
//   <ui:Callout Variant=""Tip"" Title=""Pro tip"" Text=""Press Ctrl+K to search."" />
//   <ui:Callout Variant=""Warning"" Title=""Heads up""><Label Text=""..."" /></ui:Callout>
[ContentProperty(nameof(Body))]
public partial class Callout : ContentView
{
    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(CalloutVariant), typeof(Callout), CalloutVariant.Info,
            propertyChanged: (b, o, n) => ((Callout)b).Update());

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(Callout), string.Empty,
            propertyChanged: (b, o, n) => ((Callout)b).Update());

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Callout), string.Empty,
            propertyChanged: (b, o, n) => ((Callout)b).Update());

    // None uses the variant's own icon.
    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(IconName), typeof(Callout), IconName.None,
            propertyChanged: (b, o, n) => ((Callout)b).Update());

    public static readonly BindableProperty BodyProperty =
        BindableProperty.Create(nameof(Body), typeof(View), typeof(Callout), null,
            propertyChanged: (b, o, n) => ((Callout)b).Update());

    public CalloutVariant Variant
    {
        get => (CalloutVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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

    private readonly BoxView _tint;
    private readonly Icon _icon;
    private readonly Label _title;
    private readonly Label _text;
    private readonly ContentView _body;

    public Callout()
    {
        _tint = new BoxView { BackgroundColor = Colors.Transparent };
        _icon = new Icon { Size = 16, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(0, 2, 0, 0) };
        _title = new Label { FontSize = 14, FontAttributes = FontAttributes.Bold };
        _text = new Label { FontSize = 14, LineHeight = 1.4 };
        _text.Token(Label.TextColorProperty, ShellToken.Foreground);
        _body = new ContentView();

        var row = new Grid
        {
            Margin = new Thickness(16, 12),
            ColumnSpacing = 12,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }
        };
        row.Add(_icon, 0, 0);
        row.Add(new VerticalStackLayout { Spacing = 6, Children = { _title, _text, _body } }, 1, 0);

        var frame = new Border
        {
            Content = new Grid { Children = { _tint, row } },
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusLg },
            BackgroundColor = Colors.Transparent
        };
        frame.Token(Border.StrokeProperty, ShellToken.Border);
        Content = frame;
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
        _tint.Token(BoxView.ColorProperty, neutral ? ShellToken.Muted : accent);
        _tint.Opacity = neutral ? 0.5 : 0.08;

        _icon.Name = Icon == IconName.None ? icon : Icon;
        _icon.Token = accent;
        _title.Text = Title ?? string.Empty;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _title.Token(Label.TextColorProperty, neutral ? ShellToken.Foreground : accent);
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
",
        [NativePlatform.Avalonia] = @"using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace YourProjectNamespace.Components.UI;

// Highlighted note — rounded-lg border px-4 py-3 on a faint tint of the variant color, with an
// icon, an optional title and text and/or any content.
//   <ui:Callout Variant=""Tip"" Title=""Pro tip"" Text=""Press Ctrl+K to search."" />
//   <ui:Callout Variant=""Warning"" Title=""Heads up""><TextBlock Text=""..."" /></ui:Callout>
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
            ColumnDefinitions = new ColumnDefinitions(""Auto,*""),
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
"
    };
}
