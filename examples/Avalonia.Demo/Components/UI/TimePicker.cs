using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace AvaloniaDemo.Components.UI;

// Time picker — shadcn-style trigger (clock icon + formatted time, h-10 rounded-md border) that
// opens scrollable hour / minute (/ AM-PM) columns floating below it. A click outside or Escape
// closes it. Usage: <ui:TimePicker Time="{Binding StartsAt}" MinuteStep="15" />
public class TimePicker : Border
{
    public static readonly StyledProperty<TimeSpan> TimeProperty =
        AvaloniaProperty.Register<TimePicker, TimeSpan>(nameof(Time), DateTime.Now.TimeOfDay, defaultBindingMode: BindingMode.TwoWay);

    // Minutes between entries in the minute column.
    public static readonly StyledProperty<int> MinuteStepProperty =
        AvaloniaProperty.Register<TimePicker, int>(nameof(MinuteStep), 5);

    // Defaults to the current culture's clock.
    public static readonly StyledProperty<bool> Is24HourProperty =
        AvaloniaProperty.Register<TimePicker, bool>(nameof(Is24Hour), CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains('H'));

    // .NET time format string for the trigger text. Null uses "HH:mm" or "h:mm tt".
    public static readonly StyledProperty<string?> FormatProperty =
        AvaloniaProperty.Register<TimePicker, string?>(nameof(Format));

    private const double CellHeight = 32;
    private const double CellSpacing = 2;
    private const double ColumnHeight = 7 * CellHeight + 6 * CellSpacing;

    private sealed class Cell(int value, Border box, TextBlock text)
    {
        public int Value { get; } = value;
        public Border Box { get; } = box;
        public TextBlock Text { get; } = text;
        public bool Selected { get; set; }
    }

    private readonly Border _trigger;
    private readonly TextBlock _text;
    private readonly StackPanel _columns = new() { Orientation = Orientation.Horizontal, Spacing = 4 };
    private readonly List<Cell> _hours = new();
    private readonly List<Cell> _minutes = new();
    private readonly List<Cell> _periods = new();
    private readonly ShellAnchoredPopup _popup;
    private ScrollViewer? _hourScroll;
    private ScrollViewer? _minuteScroll;

    static TimePicker()
    {
        TimeProperty.Changed.AddClassHandler<TimePicker>((p, _) =>
        {
            p.UpdateText();
            p.UpdateSelection();
            p.TimeChanged?.Invoke(p, EventArgs.Empty);
        });
        MinuteStepProperty.Changed.AddClassHandler<TimePicker>((p, _) => p.BuildColumns());
        Is24HourProperty.Changed.AddClassHandler<TimePicker>((p, _) => { p.BuildColumns(); p.UpdateText(); });
        FormatProperty.Changed.AddClassHandler<TimePicker>((p, _) => p.UpdateText());
        IsEnabledProperty.Changed.AddClassHandler<TimePicker>((p, _) => p.Opacity = p.IsEnabled ? 1.0 : 0.5);
    }

    public TimeSpan Time
    {
        get => GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public int MinuteStep
    {
        get => GetValue(MinuteStepProperty);
        set => SetValue(MinuteStepProperty, value);
    }

    public bool Is24Hour
    {
        get => GetValue(Is24HourProperty);
        set => SetValue(Is24HourProperty, value);
    }

    public string? Format
    {
        get => GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public bool IsOpen => _popup.IsOpen;

    public event EventHandler? TimeChanged;

    public TimePicker()
    {
        _text = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
        _text.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);

        _trigger = new Border
        {
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children = { new Icon { Kind = IconName.Clock, Size = 16, Token = ShellToken.MutedForeground }, _text }
            },
            Height = 40,
            MinWidth = 130,
            Padding = new Thickness(12, 0),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        _trigger.Token(BorderBrushProperty, ShellToken.Input);

        var panel = new Border
        {
            Child = _columns,
            Padding = new Thickness(8),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            BoxShadow = ShellPopups.PanelShadow()
        };
        panel.Token(BackgroundProperty, ShellToken.Popover).Token(BorderBrushProperty, ShellToken.Border);

        _popup = new ShellAnchoredPopup(_trigger);
        _popup.Popup.Child = panel;
        _popup.Closed += (_, _) => _trigger.Token(BorderBrushProperty, ShellToken.Input);

        Child = new Panel { Children = { _trigger, _popup.Popup } };
        HorizontalAlignment = HorizontalAlignment.Left;
        ShellFocus.Ring(this, _trigger);
        BuildColumns();
        UpdateText();
    }

    public void SetOpen(bool open)
    {
        if (open == _popup.IsOpen) return;
        if (!open)
        {
            _popup.Close();
            return;
        }
        UpdateSelection();
        _trigger.Token(BorderBrushProperty, ShellToken.Ring);
        _popup.Open();
        // Once the columns are laid out, bring the current hour and minute into view.
        Dispatcher.UIThread.Post(ScrollToSelection, DispatcherPriority.Background);
    }

    public void Close() => SetOpen(false);

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left || !IsEffectivelyEnabled) return;
        SetOpen(!IsOpen);
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is not (Key.Enter or Key.Space or Key.Down)) return;
        SetOpen(true);
        e.Handled = true;
    }

    private void UpdateText()
    {
        var format = string.IsNullOrEmpty(Format) ? (Is24Hour ? "HH:mm" : "h:mm tt") : Format;
        _text.Text = DateTime.Today.Add(new TimeSpan(Time.Hours, Time.Minutes, 0)).ToString(format, CultureInfo.CurrentCulture);
    }

    private void BuildColumns()
    {
        _columns.Children.Clear();
        _hours.Clear();
        _minutes.Clear();
        _periods.Clear();

        var hours = Is24Hour ? Enumerable.Range(0, 24) : new[] { 12 }.Concat(Enumerable.Range(1, 11));
        _hourScroll = AddColumn(_hours, hours, h => h.ToString(Is24Hour ? "00" : "0", CultureInfo.CurrentCulture), SetHour);

        var step = Math.Clamp(MinuteStep, 1, 30);
        _minuteScroll = AddColumn(_minutes, Enumerable.Range(0, (59 / step) + 1).Select(i => i * step), m => m.ToString("00", CultureInfo.CurrentCulture), SetMinute);

        if (!Is24Hour)
        {
            var format = CultureInfo.CurrentCulture.DateTimeFormat;
            var am = string.IsNullOrEmpty(format.AMDesignator) ? "AM" : format.AMDesignator;
            var pm = string.IsNullOrEmpty(format.PMDesignator) ? "PM" : format.PMDesignator;
            AddColumn(_periods, new[] { 0, 1 }, p => p == 0 ? am : pm, SetPeriod);
        }
        UpdateSelection();
    }

    private ScrollViewer AddColumn(List<Cell> cells, IEnumerable<int> values, Func<int, string> text, Action<int> select)
    {
        if (_columns.Children.Count > 0)
        {
            var divider = new Border { Width = 1 };
            divider.Token(BackgroundProperty, ShellToken.Border);
            _columns.Children.Add(divider);
        }

        var stack = new StackPanel { Spacing = CellSpacing };
        foreach (var value in values)
        {
            var label = new TextBlock { Text = text(value), FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            var box = new Border
            {
                Child = label,
                Width = 48,
                Height = CellHeight,
                CornerRadius = new CornerRadius(ShellTheme.RadiusSm),
                Background = Brushes.Transparent,
                Cursor = new Cursor(StandardCursorType.Hand)
            };
            var cell = new Cell(value, box, label);
            box.PointerEntered += (_, _) => { if (!cell.Selected) box.Token(BackgroundProperty, ShellToken.Accent); };
            box.PointerExited += (_, _) => Paint(cell);
            box.PointerReleased += (_, e) =>
            {
                select(value);
                e.Handled = true;
            };
            cells.Add(cell);
            stack.Children.Add(box);
        }

        var scroll = new ScrollViewer
        {
            Content = stack,
            MaxHeight = ColumnHeight,
            VerticalAlignment = VerticalAlignment.Top,
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        _columns.Children.Add(scroll);
        return scroll;
    }

    private bool IsPm => Time.Hours >= 12;

    private void SetHour(int hour) =>
        Time = new TimeSpan(Is24Hour ? hour : (hour % 12) + (IsPm ? 12 : 0), Time.Minutes, 0);

    private void SetMinute(int minute) => Time = new TimeSpan(Time.Hours, minute, 0);

    private void SetPeriod(int period) => Time = new TimeSpan((Time.Hours % 12) + (period == 1 ? 12 : 0), Time.Minutes, 0);

    private void UpdateSelection()
    {
        var hour = Is24Hour ? Time.Hours : (Time.Hours % 12 == 0 ? 12 : Time.Hours % 12);
        foreach (var cell in _hours) { cell.Selected = cell.Value == hour; Paint(cell); }
        foreach (var cell in _minutes) { cell.Selected = cell.Value == Time.Minutes; Paint(cell); }
        foreach (var cell in _periods) { cell.Selected = cell.Value == (IsPm ? 1 : 0); Paint(cell); }
    }

    private static void Paint(Cell cell)
    {
        if (cell.Selected)
        {
            cell.Box.Token(BackgroundProperty, ShellToken.Primary);
            cell.Text.Token(TextBlock.ForegroundProperty, ShellToken.PrimaryForeground);
        }
        else
        {
            cell.Box.ClearToken(BackgroundProperty);
            cell.Box.Background = Brushes.Transparent;
            cell.Text.Token(TextBlock.ForegroundProperty, ShellToken.PopoverForeground);
        }
    }

    private void ScrollToSelection()
    {
        if (!IsOpen) return;
        Center(_hourScroll, _hours);
        Center(_minuteScroll, _minutes);

        static void Center(ScrollViewer? scroll, List<Cell> cells)
        {
            var index = cells.FindIndex(c => c.Selected);
            if (scroll is null || index < 0) return;
            var y = index * (CellHeight + CellSpacing) - (ColumnHeight - CellHeight) / 2;
            scroll.Offset = new Vector(0, Math.Max(0, y));
        }
    }
}
