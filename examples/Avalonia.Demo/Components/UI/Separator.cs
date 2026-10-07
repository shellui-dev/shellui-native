using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AvaloniaDemo.Components.UI;

// 1px divider in the Border token.
public class Separator : Border
{
    public static readonly StyledProperty<SeparatorOrientation> OrientationProperty =
        AvaloniaProperty.Register<Separator, SeparatorOrientation>(nameof(Orientation));

    static Separator()
    {
        OrientationProperty.Changed.AddClassHandler<Separator>((s, _) => s.UpdateVisualState());
    }

    public SeparatorOrientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public Separator()
    {
        this.Token(BackgroundProperty, ShellToken.Border);
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        var horizontal = Orientation == SeparatorOrientation.Horizontal;
        Height = horizontal ? 1 : double.NaN;
        Width = horizontal ? double.NaN : 1;
        HorizontalAlignment = horizontal ? HorizontalAlignment.Stretch : HorizontalAlignment.Center;
        VerticalAlignment = horizontal ? VerticalAlignment.Center : VerticalAlignment.Stretch;
    }
}

public enum SeparatorOrientation
{
    Horizontal,
    Vertical
}
