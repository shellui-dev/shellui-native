using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Menu row — rounded-sm px-2 py-1.5 text-sm, hover:bg-accent, optional leading icon.
public class DropdownItem : Border
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<DropdownItem, string?>(nameof(Text));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<DropdownItem, IconName>(nameof(Icon));

    private readonly Icon _icon;
    private readonly TextBlock _label;

    static DropdownItem()
    {
        TextProperty.Changed.AddClassHandler<DropdownItem>((i, e) => i._label.Text = (string?)e.NewValue ?? string.Empty);
        IconProperty.Changed.AddClassHandler<DropdownItem>((i, _) => i.UpdateIcon());
        IsEnabledProperty.Changed.AddClassHandler<DropdownItem>((i, _) => i.Opacity = i.IsEnabled ? 1.0 : 0.5);
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

    public event EventHandler? Clicked;

    public DropdownItem()
    {
        _icon = new Icon { Size = 16, IsVisible = false, Token = ShellToken.PopoverForeground };
        _label = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
        _label.Token(TextBlock.ForegroundProperty, ShellToken.PopoverForeground);

        Child = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { _icon, _label } };
        Height = 32;
        Padding = new Thickness(8, 0);
        CornerRadius = new CornerRadius(ShellTheme.RadiusSm);
        Background = Brushes.Transparent;
        Cursor = new Cursor(StandardCursorType.Hand);
        ShellFocus.Ring(this, this);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        this.Token(BackgroundProperty, ShellToken.Accent);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        this.ClearToken(BackgroundProperty);
        Background = Brushes.Transparent;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left) return;
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

    // Picks the item in code, as a click or Enter/Space would.
    public void Press()
    {
        if (!IsEffectivelyEnabled) return;
        this.FindParentOfType<Dropdown>()?.Close();
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateIcon()
    {
        _icon.Kind = Icon;
        _icon.IsVisible = Icon != IconName.None;
    }
}
