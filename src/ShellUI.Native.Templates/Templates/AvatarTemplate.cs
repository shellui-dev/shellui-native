using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class AvatarTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "avatar",
        DisplayName = "Avatar",
        Description = "Circular image with initials or icon fallback",
        Category = ComponentCategory.DataDisplay,
        FilePath = "Avatar.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "avatar", "image", "profile", "user" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Avatar — rounded-full image over a bg-muted fallback (initials, or a user icon when no
// Fallback is set). The fallback shows until the image loads, and stays if it fails.
// Usage: <ui:Avatar Source=""profile.png"" Fallback=""CN"" Size=""Lg"" />
public partial class Avatar : ContentView
{
    public static readonly BindableProperty SourceProperty =
        BindableProperty.Create(nameof(Source), typeof(ImageSource), typeof(Avatar), null,
            propertyChanged: (b, o, n) => ((Avatar)b)._image.Source = (ImageSource?)n);

    public static readonly BindableProperty FallbackProperty =
        BindableProperty.Create(nameof(Fallback), typeof(string), typeof(Avatar), string.Empty,
            propertyChanged: (b, o, n) => ((Avatar)b).UpdateFallback());

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(AvatarSize), typeof(Avatar), AvatarSize.Default,
            propertyChanged: (b, o, n) => ((Avatar)b).UpdateSize());

    public ImageSource? Source
    {
        get => (ImageSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public string Fallback
    {
        get => (string)GetValue(FallbackProperty);
        set => SetValue(FallbackProperty, value);
    }

    public AvatarSize Size
    {
        get => (AvatarSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private readonly Border _circle;
    private readonly Label _initials;
    private readonly Icon _userIcon;
    private readonly Image _image;

    public Avatar()
    {
        _initials = new Label
        {
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        _initials.Token(Label.TextColorProperty, ShellToken.Foreground);
        _userIcon = new Icon { Name = IconName.User, Token = ShellToken.MutedForeground };
        _image = new Image { Aspect = Aspect.AspectFill };

        _circle = new Border
        {
            StrokeThickness = 0,
            Content = new Grid { Children = { _initials, _userIcon, _image } }
        };
        _circle.Token(VisualElement.BackgroundColorProperty, ShellToken.Muted);

        Content = _circle;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        UpdateSize();
        UpdateFallback();
    }

    private void UpdateSize()
    {
        var size = Size switch
        {
            AvatarSize.Sm => 32,
            AvatarSize.Lg => 48,
            AvatarSize.Xl => 64,
            _ => 40
        };
        _circle.WidthRequest = size;
        _circle.HeightRequest = size;
        _circle.StrokeShape = new RoundRectangle { CornerRadius = size / 2f };
        _initials.FontSize = Math.Round(size * 0.36);
        _userIcon.Size = size / 2.0;
    }

    private void UpdateFallback()
    {
        var hasText = !string.IsNullOrWhiteSpace(Fallback);
        _initials.Text = Fallback ?? string.Empty;
        _initials.IsVisible = hasText;
        _userIcon.IsVisible = !hasText;
        SemanticProperties.SetDescription(this, hasText ? Fallback : ""Avatar"");
    }
}

public enum AvatarSize { Sm, Default, Lg, Xl }
"
    };
}
