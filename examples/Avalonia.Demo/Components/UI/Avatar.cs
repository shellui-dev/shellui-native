using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Avatar — rounded-full image over a bg-muted fallback (initials, or a user icon when no
// Fallback is set). The fallback shows wherever the image doesn't cover it.
// Usage: <ui:Avatar Source="/Assets/profile.png" Fallback="SS" Size="Lg" />
public class Avatar : Border
{
    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<Avatar, IImage?>(nameof(Source));

    public static readonly StyledProperty<string?> FallbackProperty =
        AvaloniaProperty.Register<Avatar, string?>(nameof(Fallback));

    public static readonly StyledProperty<AvatarSize> SizeProperty =
        AvaloniaProperty.Register<Avatar, AvatarSize>(nameof(Size), AvatarSize.Default);

    private readonly TextBlock _initials;
    private readonly Icon _userIcon;
    private readonly Image _image;

    static Avatar()
    {
        SourceProperty.Changed.AddClassHandler<Avatar>((a, _) =>
        {
            a._image.Source = a.Source;
            a._image.IsVisible = a.Source != null;
        });
        FallbackProperty.Changed.AddClassHandler<Avatar>((a, _) => a.UpdateFallback());
        SizeProperty.Changed.AddClassHandler<Avatar>((a, _) => a.UpdateSize());
    }

    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public string? Fallback
    {
        get => GetValue(FallbackProperty);
        set => SetValue(FallbackProperty, value);
    }

    public AvatarSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public Avatar()
    {
        _initials = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _initials.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
        _userIcon = new Icon { Kind = IconName.User, Token = ShellToken.MutedForeground, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        _image = new Image { Stretch = Stretch.UniformToFill, IsVisible = false };

        Child = new Panel { Children = { _initials, _userIcon, _image } };
        // Clips the image to the circle.
        ClipToBounds = true;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Center;
        this.Token(BackgroundProperty, ShellToken.Muted);
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
        Width = size;
        Height = size;
        CornerRadius = new CornerRadius(size / 2.0);
        _initials.FontSize = Math.Round(size * 0.36);
        _userIcon.Size = size / 2.0;
    }

    private void UpdateFallback()
    {
        var hasText = !string.IsNullOrWhiteSpace(Fallback);
        _initials.Text = Fallback ?? string.Empty;
        _initials.IsVisible = hasText;
        _userIcon.IsVisible = !hasText;
        AutomationProperties.SetName(this, hasText ? Fallback : "Avatar");
    }
}

public enum AvatarSize { Sm, Default, Lg, Xl }
