using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;

namespace AvaloniaDemo.Components.UI;

// flex flex-wrap gap-2 — children flow left to right and wrap onto new rows, centered vertically
// within their row. Use it for button rows, chips and tag lists.
//   <ui:WrapLayout Spacing="8" LineSpacing="8">
//       <ui:Button Text="One" /> <ui:Button Text="Two" /> ...
//   </ui:WrapLayout>
// Unlike WrapPanel, the last child can take the rest of its row (LastChildFill).
public class WrapLayout : Panel
{
    // Gap between children in a row.
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<WrapLayout, double>(nameof(Spacing), 8);

    // Gap between rows.
    public static readonly StyledProperty<double> LineSpacingProperty =
        AvaloniaProperty.Register<WrapLayout, double>(nameof(LineSpacing), 8);

    // The last child takes what is left of its row (or a row of its own when less than
    // MinimumLastChildWidth remains) — e.g. the text field after a run of chips.
    public static readonly StyledProperty<bool> LastChildFillProperty =
        AvaloniaProperty.Register<WrapLayout, bool>(nameof(LastChildFill));

    public static readonly StyledProperty<double> MinimumLastChildWidthProperty =
        AvaloniaProperty.Register<WrapLayout, double>(nameof(MinimumLastChildWidth), 96);

    static WrapLayout()
    {
        AffectsMeasure<WrapLayout>(SpacingProperty, LineSpacingProperty, LastChildFillProperty, MinimumLastChildWidthProperty);
    }

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public double LineSpacing
    {
        get => GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }

    public bool LastChildFill
    {
        get => GetValue(LastChildFillProperty);
        set => SetValue(LastChildFillProperty, value);
    }

    public double MinimumLastChildWidth
    {
        get => GetValue(MinimumLastChildWidthProperty);
        set => SetValue(MinimumLastChildWidthProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize) => Flow(availableSize.Width, arrange: false);

    protected override Size ArrangeOverride(Size finalSize)
    {
        Flow(finalSize.Width, arrange: true);
        return finalSize;
    }

    private Size Flow(double available, bool arrange)
    {
        var bounded = !double.IsInfinity(available);
        var row = new List<(Control Child, double X, double Width, double Height)>();
        double x = 0, y = 0, rowHeight = 0, widest = 0;

        var last = -1;
        for (var i = Children.Count - 1; i >= 0 && last < 0; i--)
            if (Children[i].IsVisible) last = i;

        void EndRow()
        {
            if (arrange)
            {
                foreach (var (child, left, width, height) in row)
                    child.Arrange(new Rect(left, y + (rowHeight - height) / 2, width, height));
            }
            row.Clear();
            y += rowHeight + LineSpacing;
            x = 0;
            rowHeight = 0;
        }

        for (var i = 0; i <= last; i++)
        {
            var child = Children[i];
            if (!child.IsVisible) continue;

            var start = row.Count > 0 ? x + Spacing : 0;
            double width, height;
            if (LastChildFill && i == last)
            {
                var rest = bounded ? available - start : MinimumLastChildWidth;
                if (bounded && rest < MinimumLastChildWidth && row.Count > 0)
                {
                    EndRow();
                    start = 0;
                    rest = available;
                }
                width = Math.Max(0, rest);
                if (!arrange) child.Measure(new Size(width, double.PositiveInfinity));
                height = child.DesiredSize.Height;
            }
            else
            {
                if (!arrange) child.Measure(new Size(available, double.PositiveInfinity));
                var size = child.DesiredSize;
                width = bounded ? Math.Min(size.Width, available) : size.Width;
                height = size.Height;
                // Half a pixel of slack so rounding can't wrap differently between passes.
                if (bounded && row.Count > 0 && start + width > available + 0.5)
                {
                    EndRow();
                    start = 0;
                }
            }

            row.Add((child, start, width, height));
            x = start + width;
            rowHeight = Math.Max(rowHeight, height);
            widest = Math.Max(widest, x);
        }

        var total = y + rowHeight;
        if (row.Count > 0) EndRow();
        return new Size(widest, last < 0 ? 0 : total);
    }
}
