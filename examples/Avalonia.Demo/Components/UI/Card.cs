using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AvaloniaDemo.Components.UI;

// Card container — rounded-lg border bg-card shadow-sm.
// The parts stack inside; XAML children go to Items.
// Usage: <ui:Card><ui:CardHeader Title="..." /><ui:CardContent>...</ui:CardContent><ui:CardFooter>...</ui:CardFooter></ui:Card>
public class Card : Border
{
    public static readonly StyledProperty<CardVariant> VariantProperty =
        AvaloniaProperty.Register<Card, CardVariant>(nameof(Variant));

    public static readonly StyledProperty<bool> IsPressableProperty =
        AvaloniaProperty.Register<Card, bool>(nameof(IsPressable));

    private const double Radius = ShellTheme.RadiusLg + 4;

    private readonly StackPanel _items = new();

    static Card()
    {
        VariantProperty.Changed.AddClassHandler<Card>((c, _) => c.UpdateShadow());
        IsPressableProperty.Changed.AddClassHandler<Card>((c, _) =>
            c.Cursor = c.IsPressable ? new Cursor(StandardCursorType.Hand) : null);
    }

    public CardVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public bool IsPressable
    {
        get => GetValue(IsPressableProperty);
        set => SetValue(IsPressableProperty, value);
    }

    public event EventHandler? Clicked;

    [Content]
    public Controls Items => _items.Children;

    public Card()
    {
        Child = _items;
        BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(Radius);
        this.Token(BackgroundProperty, ShellToken.Card).Token(BorderBrushProperty, ShellToken.Border);
        UpdateShadow();
    }

    private void UpdateShadow() => BoxShadow = new BoxShadows(Variant == CardVariant.Elevated
        ? new BoxShadow { OffsetY = 4, Blur = 12, Color = ShellTheme.Shadow(0.10) }
        : new BoxShadow { OffsetY = 1, Blur = 2, Color = ShellTheme.Shadow(0.05) });

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!IsPressable || e.InitialPressMouseButton != MouseButton.Left) return;
        Clicked?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }
}

public enum CardVariant { Default, Elevated }
