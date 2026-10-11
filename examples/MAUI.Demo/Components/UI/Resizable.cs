using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Resizable panels — panels side by side (or stacked) with a hairline between them; drag the
// hairline to move space from one neighbor to the other. Give the group a size.
//   <ui:Resizable HeightRequest="200">
//       <ui:ResizablePanel Size="1" MinSize="120"> ...any view... </ui:ResizablePanel>
//       <ui:ResizablePanel Size="2"> ... </ui:ResizablePanel>
//   </ui:Resizable>
[ContentProperty(nameof(Panels))]
public partial class Resizable : ContentView
{
    public static readonly BindableProperty OrientationProperty =
        BindableProperty.Create(nameof(Orientation), typeof(StackOrientation), typeof(Resizable), StackOrientation.Horizontal,
            propertyChanged: (b, o, n) => ((Resizable)b).Rebuild());

    // The short bar on each divider. The divider can be dragged with or without it.
    public static readonly BindableProperty ShowHandleProperty =
        BindableProperty.Create(nameof(ShowHandle), typeof(bool), typeof(Resizable), true,
            propertyChanged: (b, o, n) => ((Resizable)b).Rebuild());

    public StackOrientation Orientation
    {
        get => (StackOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool ShowHandle
    {
        get => (bool)GetValue(ShowHandleProperty);
        set => SetValue(ShowHandleProperty, value);
    }

    public IList<ResizablePanel> Panels => _panels;

    // Raised when a drag ends.
    public event EventHandler? Resized;

    // The divider is a hairline; its grab area reaches this far into the panels on each side.
    private const double GrabReach = 4;

    private readonly ObservableCollection<ResizablePanel> _panels = new();
    private readonly Grid _grid;
    private double _startBefore;
    private double _startAfter;

    public Resizable()
    {
        _grid = new Grid { IsClippedToBounds = true };
        Content = _grid;
        _panels.CollectionChanged += (_, _) => Rebuild();
    }

    private bool IsHorizontal => Orientation == StackOrientation.Horizontal;

    private void Rebuild()
    {
        _grid.Children.Clear();
        _grid.ColumnDefinitions.Clear();
        _grid.RowDefinitions.Clear();

        for (var i = 0; i < _panels.Count; i++)
        {
            var panel = _panels[i];
            panel.SizeSet -= ApplySizes;
            panel.SizeSet += ApplySizes;
            if (i > 0) AddTrack(new GridLength(1), CreateHandle(i - 1));
            AddTrack(new GridLength(Math.Max(0.01, panel.Size), GridUnitType.Star), panel);
        }
    }

    private void AddTrack(GridLength length, View view)
    {
        if (IsHorizontal)
        {
            _grid.ColumnDefinitions.Add(new ColumnDefinition(length));
            _grid.Add(view, _grid.ColumnDefinitions.Count - 1, 0);
        }
        else
        {
            _grid.RowDefinitions.Add(new RowDefinition(length));
            _grid.Add(view, 0, _grid.RowDefinitions.Count - 1);
        }
    }

    // Panel i sits on track 2i, the divider after it on track 2i + 1.
    private void ApplySizes()
    {
        for (var i = 0; i < _panels.Count; i++)
        {
            var length = new GridLength(Math.Max(0.01, _panels[i].Size), GridUnitType.Star);
            if (IsHorizontal && 2 * i < _grid.ColumnDefinitions.Count) _grid.ColumnDefinitions[2 * i].Width = length;
            else if (!IsHorizontal && 2 * i < _grid.RowDefinitions.Count) _grid.RowDefinitions[2 * i].Height = length;
        }
    }

    private View CreateHandle(int index)
    {
        var line = new BoxView { BackgroundColor = Colors.Transparent };
        line.Token(BoxView.ColorProperty, ShellToken.Border);
        if (IsHorizontal)
        {
            line.WidthRequest = 1;
            line.HorizontalOptions = LayoutOptions.Center;
        }
        else
        {
            line.HeightRequest = 1;
            line.VerticalOptions = LayoutOptions.Center;
        }

        var handle = new Grid
        {
            BackgroundColor = Colors.Transparent,
            Margin = IsHorizontal ? new Thickness(-GrabReach, 0) : new Thickness(0, -GrabReach),
            ZIndex = 1,
            Children = { line }
        };
        if (ShowHandle)
        {
            // A short rounded bar across the hairline.
            var grip = new Border
            {
                WidthRequest = IsHorizontal ? 5 : 28,
                HeightRequest = IsHorizontal ? 28 : 5,
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 2.5 },
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            grip.Token(VisualElement.BackgroundColorProperty, ShellToken.Border);
            grip.Token(Border.StrokeProperty, ShellToken.Ring);
            handle.Children.Add(grip);
        }
        SemanticProperties.SetDescription(handle, "Resize");

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => line.Token(BoxView.ColorProperty, ShellToken.Ring);
        pointer.PointerExited += (_, _) => line.Token(BoxView.ColorProperty, ShellToken.Border);
        handle.GestureRecognizers.Add(pointer);

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += (_, e) => OnPan(index, e);
        handle.GestureRecognizers.Add(pan);
        return handle;
    }

    private void OnPan(int index, PanUpdatedEventArgs e)
    {
        if (index + 1 >= _panels.Count) return;
        var before = _panels[index];
        var after = _panels[index + 1];
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _startBefore = IsHorizontal ? before.Width : before.Height;
                _startAfter = IsHorizontal ? after.Width : after.Height;
                break;
            case GestureStatus.Running:
                var total = _startBefore + _startAfter;
                if (total <= 0) return;
                // Space only moves between the two neighbors, and neither goes under its minimum.
                var low = Math.Min(before.MinSize, total / 2);
                var high = total - Math.Min(after.MinSize, total / 2);
                var size = Math.Clamp(_startBefore + (IsHorizontal ? e.TotalX : e.TotalY), low, high);
                var weight = before.Size + after.Size;
                before.SetSize(weight * size / total);
                after.SetSize(weight - before.Size);
                ApplySizes();
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                Resized?.Invoke(this, EventArgs.Empty);
                break;
        }
    }
}

// One panel of a Resizable. Size is its share of the space (2 is twice as much as 1).
public partial class ResizablePanel : ContentView
{
    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(double), typeof(ResizablePanel), 1d, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((ResizablePanel)b).OnSizeSet());

    // The smallest the panel can be dragged to, in device-independent pixels.
    public static readonly BindableProperty MinSizeProperty =
        BindableProperty.Create(nameof(MinSize), typeof(double), typeof(ResizablePanel), 48d);

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public double MinSize
    {
        get => (double)GetValue(MinSizeProperty);
        set => SetValue(MinSizeProperty, value);
    }

    internal event Action? SizeSet;

    private bool _dragging;

    public ResizablePanel()
    {
        IsClippedToBounds = true;
    }

    // Set by the group during a drag; it applies the new sizes itself.
    internal void SetSize(double size)
    {
        _dragging = true;
        Size = size;
        _dragging = false;
    }

    private void OnSizeSet()
    {
        if (!_dragging) SizeSet?.Invoke();
    }
}
