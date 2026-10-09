using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Breadcrumb link — text-sm text-muted-foreground hover:text-foreground; the current page is
// text-foreground and not clickable. A chevron separator follows every item but the last.
public class BreadcrumbItem : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<BreadcrumbItem, string?>(nameof(Text));

    public static readonly StyledProperty<bool> IsCurrentProperty =
        AvaloniaProperty.Register<BreadcrumbItem, bool>(nameof(IsCurrent));

    private readonly Border _link;
    private readonly TextBlock _text;
    private readonly Icon _separator;
    private bool _hovered;

    static BreadcrumbItem()
    {
        TextProperty.Changed.AddClassHandler<BreadcrumbItem>((i, _) => i.UpdateVisualState());
        IsCurrentProperty.Changed.AddClassHandler<BreadcrumbItem>((i, _) => i.UpdateVisualState());
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsCurrent
    {
        get => GetValue(IsCurrentProperty);
        set => SetValue(IsCurrentProperty, value);
    }

    public event EventHandler? Clicked;

    public BreadcrumbItem()
    {
        _text = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
        // Takes the hover, click and focus ring, so the separator stays inert.
        _link = new Border { Child = _text, CornerRadius = new CornerRadius(ShellTheme.RadiusSm), Background = Brushes.Transparent };
        _link.PointerEntered += (_, _) => { _hovered = true; UpdateVisualState(); };
        _link.PointerExited += (_, _) => { _hovered = false; UpdateVisualState(); };
        _link.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left || IsCurrent) return;
            Clicked?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        };

        _separator = new Icon { Kind = IconName.ChevronRight, Size = 14, Token = ShellToken.MutedForeground, Margin = new Thickness(6, 0), VerticalAlignment = VerticalAlignment.Center };
        Child = new StackPanel { Orientation = Orientation.Horizontal, Children = { _link, _separator } };
        ShellFocus.Ring(this, _link);
        UpdateVisualState();
    }

    public void SetSeparatorVisible(bool visible) => _separator.IsVisible = visible;

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is not (Key.Enter or Key.Space) || IsCurrent) return;
        Clicked?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    private void UpdateVisualState()
    {
        _text.Text = Text ?? string.Empty;
        _text.Token(TextBlock.ForegroundProperty, IsCurrent || _hovered ? ShellToken.Foreground : ShellToken.MutedForeground);
        // The current page is a label, not a link: no tab stop and no hand cursor.
        Focusable = !IsCurrent;
        _link.Cursor = IsCurrent ? Cursor.Default : new Cursor(StandardCursorType.Hand);
    }
}
