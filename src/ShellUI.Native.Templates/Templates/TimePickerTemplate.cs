using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class TimePickerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "time-picker",
        DisplayName = "Time Picker",
        Description = "Time selection picker",
        Category = ComponentCategory.Form,
        FilePath = "TimePicker.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "form", "time", "picker" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Time picker — shadcn-style trigger (clock icon + formatted time, h-10 rounded-md border) that
// opens scrollable hour / minute (/ AM-PM) columns floating in the page layer. Custom-drawn, so
// it looks the same on every platform.
// Usage: <ui:TimePicker Time=""{Binding StartsAt}"" MinuteStep=""15"" />
public partial class TimePicker : ContentView, IShellPopup
{
    public static readonly BindableProperty TimeProperty =
        BindableProperty.Create(nameof(Time), typeof(TimeSpan), typeof(TimePicker),
            DateTime.Now.TimeOfDay, BindingMode.TwoWay, propertyChanged: OnTimeChanged);

    // Minutes between entries in the minute column.
    public static readonly BindableProperty MinuteStepProperty =
        BindableProperty.Create(nameof(MinuteStep), typeof(int), typeof(TimePicker), 5,
            propertyChanged: (b, o, n) => ((TimePicker)b).BuildColumns());

    // Defaults to the current culture's clock.
    public static readonly BindableProperty Is24HourProperty =
        BindableProperty.Create(nameof(Is24Hour), typeof(bool), typeof(TimePicker),
            CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains('H'),
            propertyChanged: (b, o, n) => { ((TimePicker)b).BuildColumns(); ((TimePicker)b).UpdateText(); });

    // .NET time format string for the trigger text. Null uses ""HH:mm"" or ""h:mm tt"".
    public static readonly BindableProperty FormatProperty =
        BindableProperty.Create(nameof(Format), typeof(string), typeof(TimePicker), null,
            propertyChanged: (b, o, n) => ((TimePicker)b).UpdateText());

    public TimeSpan Time
    {
        get => (TimeSpan)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public int MinuteStep
    {
        get => (int)GetValue(MinuteStepProperty);
        set => SetValue(MinuteStepProperty, value);
    }

    public bool Is24Hour
    {
        get => (bool)GetValue(Is24HourProperty);
        set => SetValue(Is24HourProperty, value);
    }

    public string? Format
    {
        get => (string?)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public bool IsOpen { get; private set; }

    public event EventHandler? TimeChanged;

    private const double CellHeight = 32;
    private const double CellSpacing = 2;
    private const double ColumnHeight = 7 * CellHeight + 6 * CellSpacing;

    private sealed class Cell(int value, Border box, Label text)
    {
        public int Value { get; } = value;
        public Border Box { get; } = box;
        public Label Text { get; } = text;
        public bool Selected { get; set; }
    }

    private readonly Border _trigger;
    private readonly Label _text;
    private readonly Border _panel;
    private readonly HorizontalStackLayout _columns;
    private readonly List<Cell> _hours = new();
    private readonly List<Cell> _minutes = new();
    private readonly List<Cell> _periods = new();
    private ScrollView? _hourScroll;
    private ScrollView? _minuteScroll;
    private ShellPopupHandle? _handle;

    public TimePicker()
    {
        _text = new Label { FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center };
        _text.Token(Label.TextColorProperty, ShellToken.Foreground);

        _trigger = new Border
        {
            Content = new HorizontalStackLayout
            {
                Spacing = 8,
                Children = { new Icon { Name = IconName.Clock, Size = 16, Token = ShellToken.MutedForeground }, _text }
            },
            HeightRequest = 40,
            MinimumWidthRequest = 130,
            Padding = new Thickness(12, 0),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        _trigger.Token(Border.StrokeProperty, ShellToken.Input);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { if (!IsEnabled) return; ShellFocus.FocusPressed(this); SetOpen(!IsOpen); };
        _trigger.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, () => { if (IsEnabled) SetOpen(!IsOpen); });

        _columns = new HorizontalStackLayout { Spacing = 4 };
        _panel = new Border
        {
            Content = _columns,
            Padding = new Thickness(8),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            Shadow = ShellPopups.PanelShadow()
        };
        _panel.Token(VisualElement.BackgroundColorProperty, ShellToken.Popover);
        _panel.Token(Border.StrokeProperty, ShellToken.Border);
        // Taps between cells must not fall through to the click-outside catcher.
        _panel.GestureRecognizers.Add(new TapGestureRecognizer());

        Content = _trigger;
        HorizontalOptions = LayoutOptions.Start;
        BuildColumns();
        UpdateText();
    }

    public void SetOpen(bool open)
    {
        if (IsOpen == open) return;
        IsOpen = open;
        _trigger.Token(Border.StrokeProperty, open ? ShellToken.Ring : ShellToken.Input);
        if (open)
        {
            UpdateSelection();
            ShellPopups.Opened(this);
            _handle = ShellPortal.ShowPopup(_trigger, _panel, new ShellPopupOptions { Owner = this, OnDismiss = Close });
            // Once the columns are laid out, bring the current hour and minute into view.
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(80), ScrollToSelection);
        }
        else
        {
            ShellPopups.Closed(this);
            var handle = _handle;
            _handle = null;
            if (handle != null) _ = handle.CloseAsync();
        }
    }

    public void Close() => SetOpen(false);

    private static void OnTimeChanged(BindableObject b, object o, object n)
    {
        var picker = (TimePicker)b;
        picker.UpdateText();
        picker.UpdateSelection();
        picker.TimeChanged?.Invoke(picker, EventArgs.Empty);
    }

    private void UpdateText()
    {
        var format = string.IsNullOrEmpty(Format) ? (Is24Hour ? ""HH:mm"" : ""h:mm tt"") : Format;
        _text.Text = DateTime.Today.Add(new TimeSpan(Time.Hours, Time.Minutes, 0)).ToString(format, CultureInfo.CurrentCulture);
    }

    private void BuildColumns()
    {
        _columns.Children.Clear();
        _hours.Clear();
        _minutes.Clear();
        _periods.Clear();

        var hours = Is24Hour ? Enumerable.Range(0, 24) : new[] { 12 }.Concat(Enumerable.Range(1, 11));
        _hourScroll = AddColumn(_hours, hours, h => h.ToString(Is24Hour ? ""00"" : ""0""), SetHour);

        var step = Math.Clamp(MinuteStep, 1, 30);
        _minuteScroll = AddColumn(_minutes, Enumerable.Range(0, (59 / step) + 1).Select(i => i * step), m => m.ToString(""00""), SetMinute);

        if (!Is24Hour)
        {
            var format = CultureInfo.CurrentCulture.DateTimeFormat;
            var am = string.IsNullOrEmpty(format.AMDesignator) ? ""AM"" : format.AMDesignator;
            var pm = string.IsNullOrEmpty(format.PMDesignator) ? ""PM"" : format.PMDesignator;
            AddColumn(_periods, new[] { 0, 1 }, p => p == 0 ? am : pm, SetPeriod);
        }
        UpdateSelection();
    }

    private ScrollView AddColumn(List<Cell> cells, IEnumerable<int> values, Func<int, string> text, Action<int> select)
    {
        if (_columns.Children.Count > 0)
        {
            var divider = new BoxView { WidthRequest = 1, BackgroundColor = Colors.Transparent };
            divider.Token(BoxView.ColorProperty, ShellToken.Border);
            _columns.Children.Add(divider);
        }

        var stack = new VerticalStackLayout { Spacing = CellSpacing };
        foreach (var value in values)
        {
            var label = new Label
            {
                Text = text(value),
                FontSize = 14,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            var box = new Border
            {
                Content = label,
                WidthRequest = 48,
                HeightRequest = CellHeight,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusSm },
                BackgroundColor = Colors.Transparent
            };
            var cell = new Cell(value, box, label);
            var pointer = new PointerGestureRecognizer();
            pointer.PointerEntered += (_, _) => { if (!cell.Selected) box.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent); };
            pointer.PointerExited += (_, _) => Paint(cell);
            box.GestureRecognizers.Add(pointer);
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => select(value);
            box.GestureRecognizers.Add(tap);
            cells.Add(cell);
            stack.Children.Add(box);
        }

        // Explicit height: a ScrollView sizes to its content on some platforms and stretches to
        // the available space on others.
        var scroll = new ScrollView
        {
            Content = stack,
            HeightRequest = Math.Min(ColumnHeight, cells.Count * (CellHeight + CellSpacing) - CellSpacing),
            VerticalOptions = LayoutOptions.Start,
            VerticalScrollBarVisibility = ScrollBarVisibility.Never
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
            cell.Box.Token(VisualElement.BackgroundColorProperty, ShellToken.Primary);
            cell.Text.Token(Label.TextColorProperty, ShellToken.PrimaryForeground);
        }
        else
        {
            cell.Box.ClearValue(VisualElement.BackgroundColorProperty);
            cell.Box.BackgroundColor = Colors.Transparent;
            cell.Text.Token(Label.TextColorProperty, ShellToken.PopoverForeground);
        }
    }

    private void ScrollToSelection()
    {
        if (!IsOpen) return;
        Center(_hourScroll, _hours);
        Center(_minuteScroll, _minutes);

        static void Center(ScrollView? scroll, List<Cell> cells)
        {
            var index = cells.FindIndex(c => c.Selected);
            if (scroll is null || index < 0) return;
            var y = index * (CellHeight + CellSpacing) - (scroll.HeightRequest - CellHeight) / 2;
            _ = scroll.ScrollToAsync(0, Math.Max(0, y), false);
        }
    }
}
"
    };
}
