using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class WrapLayoutTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "wrap-layout",
        DisplayName = "Wrap Layout",
        Description = "Children flow left to right and wrap onto new rows",
        Category = ComponentCategory.Layout,
        FilePath = "WrapLayout.cs",
        Dependencies = new List<string>(),
        Tags = new List<string> { "wrap", "flow", "flex", "layout" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using Microsoft.Maui.Layouts;

namespace YourProjectNamespace.Components.UI;

// flex flex-wrap gap-2 — children flow left to right and wrap onto new rows, centered vertically
// within their row. Use it for button rows, chips and tag lists.
//   <ui:WrapLayout Spacing=""8"" LineSpacing=""8"">
//       <ui:Button Text=""One"" /> <ui:Button Text=""Two"" /> ...
//   </ui:WrapLayout>
// Unlike FlexLayout it measures children once per layout pass, which matters on Windows: a long
// page with many FlexLayouts can run WinUI into its layout-cycle limit.
public partial class WrapLayout : Layout
{
    // Gap between children in a row.
    public static readonly BindableProperty SpacingProperty =
        BindableProperty.Create(nameof(Spacing), typeof(double), typeof(WrapLayout), 8d,
            propertyChanged: (b, o, n) => ((WrapLayout)b).InvalidateMeasure());

    // Gap between rows.
    public static readonly BindableProperty LineSpacingProperty =
        BindableProperty.Create(nameof(LineSpacing), typeof(double), typeof(WrapLayout), 8d,
            propertyChanged: (b, o, n) => ((WrapLayout)b).InvalidateMeasure());

    // The last child takes what is left of its row (or a row of its own when less than
    // MinimumLastChildWidth remains) — e.g. the text field after a run of chips.
    public static readonly BindableProperty LastChildFillProperty =
        BindableProperty.Create(nameof(LastChildFill), typeof(bool), typeof(WrapLayout), false,
            propertyChanged: (b, o, n) => ((WrapLayout)b).InvalidateMeasure());

    public static readonly BindableProperty MinimumLastChildWidthProperty =
        BindableProperty.Create(nameof(MinimumLastChildWidth), typeof(double), typeof(WrapLayout), 96d,
            propertyChanged: (b, o, n) => ((WrapLayout)b).InvalidateMeasure());

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public double LineSpacing
    {
        get => (double)GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }

    public bool LastChildFill
    {
        get => (bool)GetValue(LastChildFillProperty);
        set => SetValue(LastChildFillProperty, value);
    }

    public double MinimumLastChildWidth
    {
        get => (double)GetValue(MinimumLastChildWidthProperty);
        set => SetValue(MinimumLastChildWidthProperty, value);
    }

    protected override ILayoutManager CreateLayoutManager() => new Manager(this);

    private sealed class Manager(WrapLayout layout) : ILayoutManager
    {
        public Size Measure(double widthConstraint, double heightConstraint)
        {
            var padding = layout.Padding;
            var bounded = !double.IsInfinity(widthConstraint);
            var available = bounded ? Math.Max(0, widthConstraint - padding.HorizontalThickness) : double.PositiveInfinity;
            var content = Flow(available, measure: true, origin: null);
            return new Size(content.Width + padding.HorizontalThickness, content.Height + padding.VerticalThickness);
        }

        public Size ArrangeChildren(Rect bounds)
        {
            var padding = layout.Padding;
            var available = Math.Max(0, bounds.Width - padding.HorizontalThickness);
            // Sizes come from the measure pass: measuring again here would restart layout.
            Flow(available, measure: false, origin: new Point(bounds.X + padding.Left, bounds.Y + padding.Top));
            return bounds.Size;
        }

        private Size Flow(double available, bool measure, Point? origin)
        {
            var bounded = !double.IsInfinity(available);
            var row = new List<(IView View, double X, double Width, double Height)>();
            double x = 0, y = 0, rowHeight = 0, widest = 0;

            var last = -1;
            for (var i = layout.Count - 1; i >= 0 && last < 0; i--)
                if (layout[i].Visibility != Visibility.Collapsed) last = i;

            void EndRow()
            {
                if (origin is { } at)
                {
                    foreach (var (view, left, width, height) in row)
                        view.Arrange(new Rect(at.X + left, at.Y + y + (rowHeight - height) / 2, width, height));
                }
                row.Clear();
                y += rowHeight + layout.LineSpacing;
                x = 0;
                rowHeight = 0;
            }

            for (var i = 0; i <= last; i++)
            {
                var child = layout[i];
                if (child.Visibility == Visibility.Collapsed) continue;

                var start = row.Count > 0 ? x + layout.Spacing : 0;
                double width, height;
                if (layout.LastChildFill && i == last)
                {
                    var rest = bounded ? available - start : layout.MinimumLastChildWidth;
                    if (bounded && rest < layout.MinimumLastChildWidth && row.Count > 0)
                    {
                        EndRow();
                        start = 0;
                        rest = available;
                    }
                    width = Math.Max(0, rest);
                    height = measure ? child.Measure(width, double.PositiveInfinity).Height : child.DesiredSize.Height;
                }
                else
                {
                    var size = measure ? child.Measure(available, double.PositiveInfinity) : child.DesiredSize;
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
}
"
    };
}
