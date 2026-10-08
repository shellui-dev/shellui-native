using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace AvaloniaDemo.Components.UI;

// Date picker — shadcn-style trigger (calendar icon + formatted date, h-10 rounded-md border)
// that opens a Calendar floating below it. A click outside or Escape closes it.
// Usage: <ui:DatePicker Date="{Binding DueDate}" Format="MMM d, yyyy" />
public class DatePicker : Border
{
    public static readonly StyledProperty<DateTime> DateProperty =
        AvaloniaProperty.Register<DatePicker, DateTime>(nameof(Date), DateTime.Today, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<DateTime?> MinimumDateProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(MinimumDate));

    public static readonly StyledProperty<DateTime?> MaximumDateProperty =
        AvaloniaProperty.Register<DatePicker, DateTime?>(nameof(MaximumDate));

    // .NET date format string for the trigger text.
    public static readonly StyledProperty<string> FormatProperty =
        AvaloniaProperty.Register<DatePicker, string>(nameof(Format), "MMMM d, yyyy");

    private readonly Border _trigger;
    private readonly TextBlock _text;
    private readonly Calendar _calendar;
    private readonly ShellAnchoredPopup _popup;

    static DatePicker()
    {
        DateProperty.Changed.AddClassHandler<DatePicker>((p, e) =>
        {
            p.UpdateText();
            p._calendar.SelectedDate = (DateTime)e.NewValue!;
            p.DateChanged?.Invoke(p, (DateTime)e.NewValue!);
        });
        MinimumDateProperty.Changed.AddClassHandler<DatePicker>((p, e) => p._calendar.MinimumDate = (DateTime?)e.NewValue);
        MaximumDateProperty.Changed.AddClassHandler<DatePicker>((p, e) => p._calendar.MaximumDate = (DateTime?)e.NewValue);
        FormatProperty.Changed.AddClassHandler<DatePicker>((p, _) => p.UpdateText());
        IsEnabledProperty.Changed.AddClassHandler<DatePicker>((p, _) => p.Opacity = p.IsEnabled ? 1.0 : 0.5);
    }

    public DateTime Date
    {
        get => GetValue(DateProperty);
        set => SetValue(DateProperty, value);
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

    public string Format
    {
        get => GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public bool IsOpen => _popup.IsOpen;

    // The new date.
    public event EventHandler<DateTime>? DateChanged;

    public DatePicker()
    {
        _text = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
        _text.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);

        _trigger = new Border
        {
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children = { new Icon { Kind = IconName.Calendar, Size = 16, Token = ShellToken.MutedForeground }, _text }
            },
            Height = 40,
            MinWidth = 200,
            Padding = new Thickness(12, 0),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        _trigger.Token(BorderBrushProperty, ShellToken.Input);

        _calendar = new Calendar { SelectedDate = Date };
        _calendar.DateSelected += (_, date) =>
        {
            Date = date;
            SetOpen(false);
        };
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
        _calendar.SelectedDate = Date;
        _calendar.DisplayMonth = Date;
        _trigger.Token(BorderBrushProperty, ShellToken.Ring);
        _popup.Open();
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

    private void UpdateText() => _text.Text = Date.ToString(Format, CultureInfo.CurrentCulture);
}
