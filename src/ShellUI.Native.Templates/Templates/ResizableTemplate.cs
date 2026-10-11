using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class ResizableTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "resizable",
        DisplayName = "Resizable",
        Description = "Panels that share space and resize by dragging the divider",
        Category = ComponentCategory.Layout,
        FilePath = "Resizable.cs",
        Dependencies = new List<string> { "shell" },
        Tags = new List<string> { "resizable", "split", "panels", "layout" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Resizable panels — panels side by side (or stacked) with a hairline between them; drag the
// hairline to move space from one neighbor to the other. Give the group a size.
//   <ui:Resizable HeightRequest=""200"">
//       <ui:ResizablePanel Size=""1"" MinSize=""120""> ...any view... </ui:ResizablePanel>
//       <ui:ResizablePanel Size=""2""> ... </ui:ResizablePanel>
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
        SemanticProperties.SetDescription(handle, ""Resize"");

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
",
        [NativePlatform.Avalonia] = @"using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace YourProjectNamespace.Components.UI;

// Resizable panels — panels side by side (or stacked) with a hairline between them; drag the
// hairline to move space from one neighbor to the other. Each divider is a tab stop: the arrow
// keys move it. Give the group a size.
//   <ui:Resizable Height=""200"">
//       <ui:ResizablePanel Size=""1"" MinSize=""120""> ...any control... </ui:ResizablePanel>
//       <ui:ResizablePanel Size=""2""> ... </ui:ResizablePanel>
//   </ui:Resizable>
public class Resizable : Border
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Resizable, Orientation>(nameof(Orientation), Orientation.Horizontal);

    // The short bar on each divider. The divider can be dragged with or without it.
    public static readonly StyledProperty<bool> ShowHandleProperty =
        AvaloniaProperty.Register<Resizable, bool>(nameof(ShowHandle), true);

    // The divider is a hairline; its grab area reaches this far into the panels on each side.
    private const double GrabReach = 4;
    private const double KeyStep = 16;

    private readonly Grid _grid = new() { ClipToBounds = true };
    private double _startBefore;
    private double _startAfter;
    private Point? _dragFrom;

    static Resizable()
    {
        OrientationProperty.Changed.AddClassHandler<Resizable>((r, _) => r.Rebuild());
        ShowHandleProperty.Changed.AddClassHandler<Resizable>((r, _) => r.Rebuild());
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool ShowHandle
    {
        get => GetValue(ShowHandleProperty);
        set => SetValue(ShowHandleProperty, value);
    }

    // The ResizablePanel children.
    [Content]
    public Controls Panels { get; } = new();

    // Raised when a drag ends or a key moves a divider.
    public event EventHandler? Resized;

    public Resizable()
    {
        Child = _grid;
        Panels.CollectionChanged += (_, _) => Rebuild();
    }

    private bool IsHorizontal => Orientation == Orientation.Horizontal;
    private List<ResizablePanel> PanelList => Panels.OfType<ResizablePanel>().ToList();

    private void Rebuild()
    {
        _grid.Children.Clear();
        _grid.ColumnDefinitions.Clear();
        _grid.RowDefinitions.Clear();

        var panels = PanelList;
        for (var i = 0; i < panels.Count; i++)
        {
            var panel = panels[i];
            panel.SizeSet -= ApplySizes;
            panel.SizeSet += ApplySizes;
            if (i > 0) AddTrack(new GridLength(1), CreateHandle(i - 1));
            AddTrack(new GridLength(Math.Max(0.01, panel.Size), GridUnitType.Star), panel);
        }
    }

    private void AddTrack(GridLength length, Control control)
    {
        if (IsHorizontal)
        {
            _grid.ColumnDefinitions.Add(new ColumnDefinition(length));
            Grid.SetColumn(control, _grid.ColumnDefinitions.Count - 1);
            Grid.SetRow(control, 0);
        }
        else
        {
            _grid.RowDefinitions.Add(new RowDefinition(length));
            Grid.SetRow(control, _grid.RowDefinitions.Count - 1);
            Grid.SetColumn(control, 0);
        }
        _grid.Children.Add(control);
    }

    // Panel i sits on track 2i, the divider after it on track 2i + 1.
    private void ApplySizes()
    {
        var panels = PanelList;
        for (var i = 0; i < panels.Count; i++)
        {
            var length = new GridLength(Math.Max(0.01, panels[i].Size), GridUnitType.Star);
            if (IsHorizontal && 2 * i < _grid.ColumnDefinitions.Count) _grid.ColumnDefinitions[2 * i].Width = length;
            else if (!IsHorizontal && 2 * i < _grid.RowDefinitions.Count) _grid.RowDefinitions[2 * i].Height = length;
        }
    }

    private Control CreateHandle(int index)
    {
        var line = new Border();
        line.Token(BackgroundProperty, ShellToken.Border);
        if (IsHorizontal)
        {
            line.Width = 1;
            line.HorizontalAlignment = HorizontalAlignment.Center;
        }
        else
        {
            line.Height = 1;
            line.VerticalAlignment = VerticalAlignment.Center;
        }

        // A short rounded bar across the hairline; it also carries the focus ring.
        var grip = new Border
        {
            Width = IsHorizontal ? 5 : 28,
            Height = IsHorizontal ? 28 : 5,
            BorderThickness = new Thickness(ShowHandle ? 1 : 0),
            CornerRadius = new CornerRadius(2.5),
            // Not null: a border with no background paints no focus ring.
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        if (ShowHandle) grip.Token(BackgroundProperty, ShellToken.Border).Token(BorderBrushProperty, ShellToken.Ring);

        var handle = new Border
        {
            Child = new Panel { Children = { line, grip } },
            // Transparent, not null, so the whole grab area takes the pointer.
            Background = Brushes.Transparent,
            Margin = IsHorizontal ? new Thickness(-GrabReach, 0) : new Thickness(0, -GrabReach),
            ZIndex = 1,
            Cursor = new Cursor(IsHorizontal ? StandardCursorType.SizeWestEast : StandardCursorType.SizeNorthSouth)
        };
        AutomationProperties.SetName(handle, ""Resize"");
        ShellFocus.Ring(handle, grip);

        handle.PointerEntered += (_, _) => line.Token(BackgroundProperty, ShellToken.Ring);
        handle.PointerExited += (_, _) =>
        {
            if (_dragFrom is null) line.Token(BackgroundProperty, ShellToken.Border);
        };
        handle.PointerPressed += (_, e) =>
        {
            if (!e.GetCurrentPoint(handle).Properties.IsLeftButtonPressed || !Start(index)) return;
            _dragFrom = e.GetPosition(this);
            e.Pointer.Capture(handle);
            e.Handled = true;
        };
        handle.PointerMoved += (_, e) =>
        {
            if (_dragFrom is not { } from) return;
            var now = e.GetPosition(this);
            Move(index, IsHorizontal ? now.X - from.X : now.Y - from.Y);
            e.Handled = true;
        };
        handle.PointerReleased += (_, e) =>
        {
            if (_dragFrom is null) return;
            e.Pointer.Capture(null);
            e.Handled = true;
        };
        handle.PointerCaptureLost += (_, _) =>
        {
            if (_dragFrom is null) return;
            _dragFrom = null;
            if (!handle.IsPointerOver) line.Token(BackgroundProperty, ShellToken.Border);
            Resized?.Invoke(this, EventArgs.Empty);
        };
        handle.KeyDown += (_, e) =>
        {
            var back = IsHorizontal ? Key.Left : Key.Up;
            var forward = IsHorizontal ? Key.Right : Key.Down;
            if (e.Key != back && e.Key != forward) return;
            if (Start(index))
            {
                Move(index, e.Key == forward ? KeyStep : -KeyStep);
                Resized?.Invoke(this, EventArgs.Empty);
            }
            e.Handled = true;
        };
        return handle;
    }

    // Remembers the two neighbors' sizes at the start of a move.
    private bool Start(int index)
    {
        var panels = PanelList;
        if (index + 1 >= panels.Count) return false;
        _startBefore = IsHorizontal ? panels[index].Bounds.Width : panels[index].Bounds.Height;
        _startAfter = IsHorizontal ? panels[index + 1].Bounds.Width : panels[index + 1].Bounds.Height;
        return _startBefore + _startAfter > 0;
    }

    // Space only moves between the two neighbors, and neither goes under its minimum.
    private void Move(int index, double delta)
    {
        var panels = PanelList;
        if (index + 1 >= panels.Count) return;
        var before = panels[index];
        var after = panels[index + 1];
        var total = _startBefore + _startAfter;
        var low = Math.Min(before.MinSize, total / 2);
        var high = total - Math.Min(after.MinSize, total / 2);
        var size = Math.Clamp(_startBefore + delta, low, high);
        var weight = before.Size + after.Size;
        before.SetSize(weight * size / total);
        after.SetSize(weight - before.Size);
        ApplySizes();
    }
}

// One panel of a Resizable. Size is its share of the space (2 is twice as much as 1).
public class ResizablePanel : Border
{
    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<ResizablePanel, double>(nameof(Size), 1, defaultBindingMode: BindingMode.TwoWay);

    // The smallest the panel can be dragged to, in device-independent pixels.
    public static readonly StyledProperty<double> MinSizeProperty =
        AvaloniaProperty.Register<ResizablePanel, double>(nameof(MinSize), 48);

    private bool _dragging;

    static ResizablePanel()
    {
        SizeProperty.Changed.AddClassHandler<ResizablePanel>((p, _) =>
        {
            if (!p._dragging) p.SizeSet?.Invoke();
        });
    }

    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public double MinSize
    {
        get => GetValue(MinSizeProperty);
        set => SetValue(MinSizeProperty, value);
    }

    internal event Action? SizeSet;

    public ResizablePanel()
    {
        ClipToBounds = true;
    }

    // Set by the group during a drag; it applies the new sizes itself.
    internal void SetSize(double size)
    {
        _dragging = true;
        Size = size;
        _dragging = false;
    }
}
"
    };
}
