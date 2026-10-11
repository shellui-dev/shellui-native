using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Two-state button — h-10 px-3 rounded-md text-sm font-medium; hover:bg-muted, and
// bg-accent text-accent-foreground while pressed. Variant Outline adds border-input.
// Usage: <ui:Toggle Icon="Star" IsPressed="{Binding IsFavorite}" />  <ui:Toggle Text="Bold" Variant="Outline" />
public class Toggle : Border, IShellFocusable
{
    public static readonly StyledProperty<bool> IsPressedProperty =
        AvaloniaProperty.Register<Toggle, bool>(nameof(IsPressed), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Toggle, string?>(nameof(Text));

    public static readonly StyledProperty<IconName> IconProperty =
        AvaloniaProperty.Register<Toggle, IconName>(nameof(Icon), IconName.None);

    public static readonly StyledProperty<ToggleVariant> VariantProperty =
        AvaloniaProperty.Register<Toggle, ToggleVariant>(nameof(Variant));

    public static readonly StyledProperty<ToggleSize> SizeProperty =
        AvaloniaProperty.Register<Toggle, ToggleSize>(nameof(Size), ToggleSize.Default);

    private readonly Icon _icon;
    private readonly TextBlock _label;

    static Toggle()
    {
        IsPressedProperty.Changed.AddClassHandler<Toggle>((t, _) =>
        {
            t.UpdateVisualState();
            t.PressedChanged?.Invoke(t, t.IsPressed);
        });
        TextProperty.Changed.AddClassHandler<Toggle>((t, _) => t.UpdateVisualState());
        IconProperty.Changed.AddClassHandler<Toggle>((t, _) => t.UpdateVisualState());
        VariantProperty.Changed.AddClassHandler<Toggle>((t, _) => t.UpdateVisualState());
        SizeProperty.Changed.AddClassHandler<Toggle>((t, _) => t.UpdateVisualState());
        IsEnabledProperty.Changed.AddClassHandler<Toggle>((t, _) => t.UpdateVisualState());
    }

    public bool IsPressed
    {
        get => GetValue(IsPressedProperty);
        set => SetValue(IsPressedProperty, value);
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

    public ToggleVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public ToggleSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public event EventHandler<bool>? PressedChanged;

    public Toggle()
    {
        _icon = new Icon { Size = 16, IsVisible = false, VerticalAlignment = VerticalAlignment.Center };
        _label = new TextBlock { FontSize = 14, FontWeight = FontWeight.Medium, VerticalAlignment = VerticalAlignment.Center };
        Child = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { _icon, _label }
        };
        CornerRadius = new CornerRadius(ShellTheme.RadiusMd);
        HorizontalAlignment = HorizontalAlignment.Left;
        Cursor = new Cursor(StandardCursorType.Hand);
        ShellFocus.Ring(this, this);
        UpdateVisualState();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        UpdateVisualState();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        UpdateVisualState();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left || !IsEffectivelyEnabled) return;
        IsPressed = !IsPressed;
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is not (Key.Enter or Key.Space) || !IsEffectivelyEnabled) return;
        IsPressed = !IsPressed;
        e.Handled = true;
    }

    private void UpdateVisualState()
    {
        var (height, padding) = Size switch
        {
            ToggleSize.Sm => (36.0, 10.0),
            ToggleSize.Lg => (44.0, 20.0),
            _ => (40.0, 12.0)
        };
        Height = height;
        MinWidth = height;
        Padding = new Thickness(padding, 0);
        BorderThickness = new Thickness(Variant == ToggleVariant.Outline ? 1 : 0);
        this.Token(BorderBrushProperty, ShellToken.Input);

        _label.Text = Text ?? string.Empty;
        _label.IsVisible = !string.IsNullOrEmpty(Text);
        _icon.Kind = Icon;
        _icon.IsVisible = Icon != IconName.None;
        AutomationProperties.SetName(this, string.IsNullOrEmpty(Text) ? Icon.ToString() : Text);

        var hovered = IsPointerOver && IsEffectivelyEnabled;
        var foreground = IsPressed ? ShellToken.AccentForeground
            : hovered ? ShellToken.MutedForeground
            : ShellToken.Foreground;
        _label.Token(TextBlock.ForegroundProperty, foreground);
        _icon.Token = foreground;

        if (IsPressed) this.Token(BackgroundProperty, ShellToken.Accent);
        else if (hovered) this.Token(BackgroundProperty, ShellToken.Muted);
        else
        {
            // Transparent, not null, so the whole toggle stays hit-testable.
            this.ClearToken(BackgroundProperty);
            Background = Brushes.Transparent;
        }
        Opacity = IsEnabled ? 1.0 : 0.5;
    }
}

public enum ToggleVariant { Default, Outline }
public enum ToggleSize { Sm, Default, Lg }
