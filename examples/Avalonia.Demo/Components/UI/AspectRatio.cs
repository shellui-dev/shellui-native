using Avalonia;
using Avalonia.Controls;

namespace AvaloniaDemo.Components.UI;

// Keeps its content at a fixed width / height ratio: the height follows the width.
//   <ui:AspectRatio Ratio="1.7778" CornerRadius="8">
//       <Image Source="/Assets/cover.jpg" Stretch="UniformToFill" />
//   </ui:AspectRatio>
public class AspectRatio : Border
{
    // Width divided by height. 16:9 is 1.7778, 4:3 is 1.3333, a square is 1.
    public static readonly StyledProperty<double> RatioProperty =
        AvaloniaProperty.Register<AspectRatio, double>(nameof(Ratio), 16d / 9d);

    static AspectRatio()
    {
        AffectsMeasure<AspectRatio>(RatioProperty);
    }

    public double Ratio
    {
        get => GetValue(RatioProperty);
        set => SetValue(RatioProperty, value);
    }

    public AspectRatio()
    {
        // With a CornerRadius the content is clipped to the rounded shape.
        ClipToBounds = true;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Ratio <= 0) return base.MeasureOverride(availableSize);
        // Without a width to fill (e.g. in a horizontal stack) it takes the content's width.
        var width = double.IsInfinity(availableSize.Width) ? base.MeasureOverride(availableSize).Width : availableSize.Width;
        var size = new Size(width, width / Ratio);
        base.MeasureOverride(size);
        return size;
    }
}
