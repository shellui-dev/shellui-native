using System;
using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Date range picker — a DatePicker-style trigger (h-10 rounded-md border) that opens a Calendar.
// The first pick is the start, the second the end (picking an earlier day starts over); the
// panel closes when the range is complete. The × clears it.
// Usage: <ui:DateRangePicker StartDate="{Binding From}" EndDate="{Binding To}" />
public class DateRangePicker : Border
{
    public static readonly StyledProperty<DateTime?> StartDateProperty =
        AvaloniaProperty.Register<DateRangePicker, DateTime?>(nameof(StartDate), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<DateTime?> EndDateProperty =
        AvaloniaProperty.Register<DateRangePicker, DateTime?>(nameof(EndDate), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<DateRangePicker, string?>(nameof(Placeholder), "Pick a date range");

    // .NET date format string for the trigger text.
    public static readonly StyledProperty<string> FormatProperty =
        AvaloniaProperty.Register<DateRangePicker, string>(nameof(Format), "MMM d, yyyy");

    // Shows the × that clears the range.
    public static readonly StyledProperty<bool> AllowClearProperty =
        AvaloniaProperty.Register<DateRangePicker, bool>(nameof(AllowClear), true);

    public static readonly StyledProperty<DateTime?> MinimumDateProperty =
        AvaloniaProperty.Register<DateRangePicker, DateTime?>(nameof(MinimumDate));

    public static readonly StyledProperty<DateTime?> MaximumDateProperty =
        AvaloniaProperty.Register<DateRangePicker, DateTime?>(nameof(MaximumDate));

    private readonly Border _trigger;
    private readonly TextBlock _text;
    private readonly Border _clear;
    private readonly Calendar _calendar;
    private readonly ShellAnchoredPopup _popup;

    static DateRangePicker()
    {
        StartDateProperty.Changed.AddClassHandler<DateRangePicker>((p, _) => p.OnRangeChanged());
        EndDateProperty.Changed.AddClassHandler<DateRangePicker>((p, _) => p.OnRangeChanged());
        PlaceholderProperty.Changed.AddClassHandler<DateRangePicker>((p, _) => p.UpdateTrigger());
        FormatProperty.Changed.AddClassHandler<DateRangePicker>((p, _) => p.UpdateTrigger());
        AllowClearProperty.Changed.AddClassHandler<DateRangePicker>((p, _) => p.UpdateTrigger());
        MinimumDateProperty.Changed.AddClassHandler<DateRangePicker>((p, e) => p._calendar.MinimumDate = (DateTime?)e.NewValue);
        MaximumDateProperty.Changed.AddClassHandler<DateRangePicker>((p, e) => p._calendar.MaximumDate = (DateTime?)e.NewValue);
        IsEnabledProperty.Changed.AddClassHandler<DateRangePicker>((p, _) => p.Opacity = p.IsEnabled ? 1.0 : 0.5);
    }

    public DateTime? StartDate
    {
        get => GetValue(StartDateProperty);
        set => SetValue(StartDateProperty, value);
    }

    public DateTime? EndDate
    {
        get => GetValue(EndDateProperty);
        set => SetValue(EndDateProperty, value);
    }

    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string Format
    {
        get => GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public bool AllowClear
    {
        get => GetValue(AllowClearProperty);
        set => SetValue(AllowClearProperty, value);
    }

    public DateTime? MinimumDate
    {
        get => GetValue(MinimumDateProperty);
        set => SetValue(MinimumDateProperty, value);
    }

    public DateTime? MaximumDate
    {
        get => GetValue(MaximumDateProperty);
        set => SetValue(MaximumDateProperty, value);
    }

    public bool IsOpen => _popup.IsOpen;

    // Raised when StartDate or EndDate changes.
    // User input only: setting StartDate / EndDate, or calling Clear, from code doesn't raise it.
    public event EventHandler? RangeChanged;

    // Set while user input changes the value, so change events report user input only.
    private bool _byUser;

    public DateRangePicker()
    {
        _text = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        Grid.SetColumn(_text, 1);

        var close = new Icon { Kind = IconName.X, Size = 14, Token = ShellToken.MutedForeground, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        // 14px icon in a 24px click target. Its clicks stop here, so they don't toggle the panel.
        _clear = new Border { Width = 24, Height = 24, Background = Brushes.Transparent, Child = close, VerticalAlignment = VerticalAlignment.Center, IsVisible = false };
        AutomationProperties.SetName(_clear, "Clear dates");
        _clear.PointerEntered += (_, _) => close.Token = ShellToken.Foreground;
        _clear.PointerExited += (_, _) => close.Token = ShellToken.MutedForeground;
        _clear.PointerPressed += (_, e) => e.Handled = true;
        _clear.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;
            if (IsEffectivelyEnabled) ByUser(Clear);
            e.Handled = true;
        };
        Grid.SetColumn(_clear, 2);

        _trigger = new Border
        {
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
                ColumnSpacing = 8,
                Children = { new Icon { Kind = IconName.Calendar, Size = 16, Token = ShellToken.MutedForeground, VerticalAlignment = VerticalAlignment.Center }, _text, _clear }
            },
            Height = 40,
            MinWidth = 260,
            Padding = new Thickness(12, 0, 8, 0),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        _trigger.Token(BorderBrushProperty, ShellToken.Input);
        _trigger.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left || !IsEffectivelyEnabled) return;
            SetOpen(!IsOpen);
            e.Handled = true;
        };

        _calendar = new Calendar();
        _calendar.DateSelected += (_, date) => ByUser(() => Pick(date));
        var panel = new Border
        {
            Child = _calendar,
            Padding = new Thickness(12),
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
        UpdateTrigger();
    }

    public void SetOpen(bool open)
    {
        if (open == _popup.IsOpen) return;
        if (!open)
        {
            _popup.Close();
            return;
        }
        UpdateCalendar();
        _calendar.DisplayMonth = StartDate ?? DateTime.Today;
        _trigger.Token(BorderBrushProperty, ShellToken.Ring);
        _popup.Open();
    }

    public void Close() => SetOpen(false);

    private void ByUser(Action change)
    {
        var outer = _byUser;
        _byUser = true;
        try { change(); }
        finally { _byUser = outer; }
    }

    public void Clear()
    {
        StartDate = null;
        EndDate = null;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (IsOpen || !ReferenceEquals(e.Source, this) || e.Key is not (Key.Enter or Key.Space or Key.Down)) return;
        SetOpen(true);
        e.Handled = true;
    }

    // The first pick (or a pick with a complete range, or one before the start) begins a new
    // range; the next pick ends it.
    private void Pick(DateTime date)
    {
        if (StartDate is not { } start || EndDate.HasValue || date < start.Date)
        {
            EndDate = null;
            StartDate = date;
            return;
        }
        EndDate = date;
        SetOpen(false);
    }

    private void OnRangeChanged()
    {
        UpdateTrigger();
        UpdateCalendar();
        if (_byUser) RangeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateCalendar()
    {
        // The calendar marks the range itself; its own single selection stays empty.
        _calendar.SelectedDate = null;
        _calendar.RangeStart = StartDate;
        _calendar.RangeEnd = EndDate;
    }

    private void UpdateTrigger()
    {
        var culture = CultureInfo.CurrentCulture;
        var hasValue = StartDate.HasValue || EndDate.HasValue;
        _text.Text = StartDate is { } start
            ? EndDate is { } end ? $"{start.ToString(Format, culture)} – {end.ToString(Format, culture)}" : $"{start.ToString(Format, culture)} – …"
            : Placeholder ?? string.Empty;
        _text.Token(TextBlock.ForegroundProperty, hasValue ? ShellToken.Foreground : ShellToken.MutedForeground);
        _clear.IsVisible = AllowClear && hasValue;
    }
}
