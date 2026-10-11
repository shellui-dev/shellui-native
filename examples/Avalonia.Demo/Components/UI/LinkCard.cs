using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Clickable card — rounded-lg border bg-card p-4, hover:bg-accent: an optional icon, a title, a
// muted description and an arrow. A click (or Enter / Space) raises Clicked and opens Url when
// one is set.
// Usage: <ui:LinkCard Title="Documentation" Description="Guides and API reference."
//                     Icon="FileText" Url="https://shellui.dev/docs" />
public class LinkCard : Border, IShellFocusable
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<LinkCard, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<LinkCard, string?>(nameof(Description));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<LinkCard, IconName>(nameof(Icon));

    // Opened in the system browser (or the app registered for the link) when the card is clicked.
    public static readonly StyledProperty<string?> UrlProperty =
        AvaloniaProperty.Register<LinkCard, string?>(nameof(Url));

    private readonly Icon _icon;
    private readonly TextBlock _title;
    private readonly TextBlock _description;

    static LinkCard()
    {
        TitleProperty.Changed.AddClassHandler<LinkCard>((c, _) => c.Update());
        DescriptionProperty.Changed.AddClassHandler<LinkCard>((c, _) => c.Update());
        IconProperty.Changed.AddClassHandler<LinkCard>((c, _) => c.Update());
        IsEnabledProperty.Changed.AddClassHandler<LinkCard>((c, _) => c.Opacity = c.IsEnabled ? 1.0 : 0.5);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
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

    public string? Url
    {
        get => GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }

    public event EventHandler? Clicked;

    public LinkCard()
    {
        _icon = new Icon { Size = 20, Token = ShellToken.MutedForeground, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 1, 0, 0) };
        _title = new TextBlock { FontSize = 15, FontWeight = FontWeight.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis };
        _title.Token(TextBlock.ForegroundProperty, ShellToken.CardForeground);
        _description = new TextBlock { FontSize = 14, TextWrapping = TextWrapping.Wrap };
        _description.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        var text = new StackPanel { Spacing = 4, Children = { _title, _description } };
        Grid.SetColumn(text, 1);
        var arrow = new Icon { Kind = IconName.ArrowUpRight, Size = 16, Token = ShellToken.MutedForeground, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 2, 0, 0) };
        Grid.SetColumn(arrow, 2);

        Child = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"), ColumnSpacing = 12, Children = { _icon, text, arrow } };
        Padding = new Thickness(16);
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(ShellTheme.RadiusLg);
        Cursor = new Cursor(StandardCursorType.Hand);
        this.Token(BackgroundProperty, ShellToken.Card).Token(BorderBrushProperty, ShellToken.Border);
        ShellFocus.Ring(this, this);
        Update();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (IsEffectivelyEnabled) this.Token(BackgroundProperty, ShellToken.Accent);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        this.Token(BackgroundProperty, ShellToken.Card);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left || !new Rect(Bounds.Size).Contains(e.GetPosition(this))) return;
        Press();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is not (Key.Enter or Key.Space)) return;
        Press();
        e.Handled = true;
    }

    // Clicks in code, as a pointer click or Enter/Space would.
    public async void Press()
    {
        if (!IsEffectivelyEnabled) return;
        Clicked?.Invoke(this, EventArgs.Empty);
        if (string.IsNullOrWhiteSpace(Url) || !Uri.TryCreate(Url, UriKind.Absolute, out var uri)) return;
        if (TopLevel.GetTopLevel(this) is not { } top) return;
        try
        {
            await top.Launcher.LaunchUriAsync(uri);
        }
        catch (Exception)
        {
            // Nothing on the device can open this link.
        }
    }

    private void Update()
    {
        _icon.Kind = Icon;
        _icon.IsVisible = Icon != IconName.None;
        _title.Text = Title ?? string.Empty;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _description.Text = Description ?? string.Empty;
        _description.IsVisible = !string.IsNullOrEmpty(Description);
        AutomationProperties.SetName(this, Title ?? string.Empty);
    }
}
