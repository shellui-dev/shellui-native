using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class DateRangePickerTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "date-range-picker",
        DisplayName = "Date Range Picker",
        Description = "Picks a start and an end date from one calendar",
        Category = ComponentCategory.Form,
        FilePath = "DateRangePicker.cs",
        Dependencies = new List<string> { "shell", "icon", "calendar" },
        Tags = new List<string> { "date", "range", "picker", "calendar" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Date range picker — a DatePicker-style trigger (h-10 rounded-md border) that opens a Calendar.
// The first pick is the start, the second the end (picking an earlier day starts over); the
// panel closes when the range is complete. The × clears it.
// Usage: <ui:DateRangePicker StartDate=""{Binding From}"" EndDate=""{Binding To}"" />
public partial class DateRangePicker : ContentView, IShellPopup
{
    public static readonly BindableProperty StartDateProperty =
        BindableProperty.Create(nameof(StartDate), typeof(DateTime?), typeof(DateRangePicker), null, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((DateRangePicker)b).OnRangeChanged());

    public static readonly BindableProperty EndDateProperty =
        BindableProperty.Create(nameof(EndDate), typeof(DateTime?), typeof(DateRangePicker), null, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((DateRangePicker)b).OnRangeChanged());

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(DateRangePicker), ""Pick a date range"",
            propertyChanged: (b, o, n) => ((DateRangePicker)b).UpdateTrigger());

    // .NET date format string for the trigger text.
    public static readonly BindableProperty FormatProperty =
        BindableProperty.Create(nameof(Format), typeof(string), typeof(DateRangePicker), ""MMM d, yyyy"",
            propertyChanged: (b, o, n) => ((DateRangePicker)b).UpdateTrigger());

    // Shows the × that clears the range.
    public static readonly BindableProperty AllowClearProperty =
        BindableProperty.Create(nameof(AllowClear), typeof(bool), typeof(DateRangePicker), true,
            propertyChanged: (b, o, n) => ((DateRangePicker)b).UpdateTrigger());

    public static readonly BindableProperty MinimumDateProperty =
        BindableProperty.Create(nameof(MinimumDate), typeof(DateTime?), typeof(DateRangePicker), null,
            propertyChanged: (b, o, n) => ((DateRangePicker)b)._calendar.MinimumDate = (DateTime?)n);

    public static readonly BindableProperty MaximumDateProperty =
        BindableProperty.Create(nameof(MaximumDate), typeof(DateTime?), typeof(DateRangePicker), null,
            propertyChanged: (b, o, n) => ((DateRangePicker)b)._calendar.MaximumDate = (DateTime?)n);

    public DateTime? StartDate
    {
        get => (DateTime?)GetValue(StartDateProperty);
        set => SetValue(StartDateProperty, value);
    }

    public DateTime? EndDate
    {
        get => (DateTime?)GetValue(EndDateProperty);
        set => SetValue(EndDateProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string Format
    {
        get => (string)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public bool AllowClear
    {
        get => (bool)GetValue(AllowClearProperty);
        set => SetValue(AllowClearProperty, value);
    }

    public DateTime? MinimumDate
    {
        get => (DateTime?)GetValue(MinimumDateProperty);
        set => SetValue(MinimumDateProperty, value);
    }

    public DateTime? MaximumDate
    {
        get => (DateTime?)GetValue(MaximumDateProperty);
        set => SetValue(MaximumDateProperty, value);
    }

    public bool IsOpen { get; private set; }

    // Raised when StartDate or EndDate changes.
    public event EventHandler? RangeChanged;

    private readonly Border _trigger;
    private readonly Label _text;
    private readonly Grid _clear;
    private readonly Calendar _calendar;
    private readonly Border _panel;
    private ShellPopupHandle? _handle;
    private bool _clearTapped;

    public DateRangePicker()
    {
        _text = new Label { FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center, LineBreakMode = LineBreakMode.TailTruncation };

        var close = new Icon { Name = IconName.X, Size = 14, Token = ShellToken.MutedForeground, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        // 14px icon in a 24px tap target.
        _clear = new Grid { WidthRequest = 24, HeightRequest = 24, BackgroundColor = Colors.Transparent, VerticalOptions = LayoutOptions.Center, IsVisible = false, Children = { close } };
        SemanticProperties.SetDescription(_clear, ""Clear dates"");
        var clearPointer = new PointerGestureRecognizer();
        clearPointer.PointerEntered += (_, _) => close.Token = ShellToken.Foreground;
        clearPointer.PointerExited += (_, _) => close.Token = ShellToken.MutedForeground;
        _clear.GestureRecognizers.Add(clearPointer);
        var clearTap = new TapGestureRecognizer();
        clearTap.Tapped += (_, _) =>
        {
            if (!IsEnabled) return;
            _clearTapped = true;
            // Cleared on the next turn in case the tap never reaches the trigger.
            Dispatcher.Dispatch(() => _clearTapped = false);
            Clear();
        };
        _clear.GestureRecognizers.Add(clearTap);

        var row = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)
            }
        };
        row.Add(new Icon { Name = IconName.Calendar, Size = 16, Token = ShellToken.MutedForeground, VerticalOptions = LayoutOptions.Center }, 0, 0);
        row.Add(_text, 1, 0);
        row.Add(_clear, 2, 0);

        _trigger = new Border
        {
            Content = row,
            HeightRequest = 40,
            MinimumWidthRequest = 260,
            Padding = new Thickness(12, 0, 8, 0),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        _trigger.Token(Border.StrokeProperty, ShellToken.Input);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            // A tap on the × can reach the trigger too; it must not toggle the panel.
            if (_clearTapped) { _clearTapped = false; return; }
            if (!IsEnabled) return;
            ShellFocus.FocusPressed(this);
            SetOpen(!IsOpen);
        };
        _trigger.GestureRecognizers.Add(tap);
        ShellFocus.MakeFocusable(this, () => { if (IsEnabled) SetOpen(!IsOpen); });

        _calendar = new Calendar();
        _calendar.DateSelected += (_, date) => Pick(date);

        _panel = new Border
        {
            Content = _calendar,
            Padding = new Thickness(12),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            Shadow = ShellPopups.PanelShadow()
        };
        _panel.Token(VisualElement.BackgroundColorProperty, ShellToken.Popover);
        _panel.Token(Border.StrokeProperty, ShellToken.Border);

        Content = _trigger;
        HorizontalOptions = LayoutOptions.Start;
        UpdateTrigger();
    }

    public void SetOpen(bool open)
    {
        if (IsOpen == open) return;
        IsOpen = open;
        _trigger.Token(Border.StrokeProperty, open ? ShellToken.Ring : ShellToken.Input);
        if (open)
        {
            UpdateCalendar();
            _calendar.DisplayMonth = StartDate ?? DateTime.Today;
            ShellPopups.Opened(this);
            _handle = ShellPortal.ShowPopup(_trigger, _panel, new ShellPopupOptions { Owner = this, OnDismiss = Close });
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

    public void Clear()
    {
        StartDate = null;
        EndDate = null;
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
        RangeChanged?.Invoke(this, EventArgs.Empty);
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
            ? EndDate is { } end ? $""{start.ToString(Format, culture)} – {end.ToString(Format, culture)}"" : $""{start.ToString(Format, culture)} – …""
            : Placeholder;
        _text.Token(Label.TextColorProperty, hasValue ? ShellToken.Foreground : ShellToken.MutedForeground);
        _clear.IsVisible = AllowClear && hasValue;
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName)
            Opacity = IsEnabled ? 1.0 : 0.5;
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace YourProjectNamespace.Components.UI;

// Date range picker — a DatePicker-style trigger (h-10 rounded-md border) that opens a Calendar.
// The first pick is the start, the second the end (picking an earlier day starts over); the
// panel closes when the range is complete. The × clears it.
// Usage: <ui:DateRangePicker StartDate=""{Binding From}"" EndDate=""{Binding To}"" />
public class DateRangePicker : Border
{
    public static readonly StyledProperty<DateTime?> StartDateProperty =
        AvaloniaProperty.Register<DateRangePicker, DateTime?>(nameof(StartDate), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<DateTime?> EndDateProperty =
        AvaloniaProperty.Register<DateRangePicker, DateTime?>(nameof(EndDate), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> PlaceholderProperty =
        AvaloniaProperty.Register<DateRangePicker, string?>(nameof(Placeholder), ""Pick a date range"");

    // .NET date format string for the trigger text.
    public static readonly StyledProperty<string> FormatProperty =
        AvaloniaProperty.Register<DateRangePicker, string>(nameof(Format), ""MMM d, yyyy"");

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
    public event EventHandler? RangeChanged;

    public DateRangePicker()
    {
        _text = new TextBlock { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        Grid.SetColumn(_text, 1);

        var close = new Icon { Kind = IconName.X, Size = 14, Token = ShellToken.MutedForeground, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        // 14px icon in a 24px click target. Its clicks stop here, so they don't toggle the panel.
        _clear = new Border { Width = 24, Height = 24, Background = Brushes.Transparent, Child = close, VerticalAlignment = VerticalAlignment.Center, IsVisible = false };
        AutomationProperties.SetName(_clear, ""Clear dates"");
        _clear.PointerEntered += (_, _) => close.Token = ShellToken.Foreground;
        _clear.PointerExited += (_, _) => close.Token = ShellToken.MutedForeground;
        _clear.PointerPressed += (_, e) => e.Handled = true;
        _clear.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;
            if (IsEffectivelyEnabled) Clear();
            e.Handled = true;
        };
        Grid.SetColumn(_clear, 2);

        _trigger = new Border
        {
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions(""Auto,*,Auto""),
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
        _calendar.DateSelected += (_, date) => Pick(date);
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
        RangeChanged?.Invoke(this, EventArgs.Empty);
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
            ? EndDate is { } end ? $""{start.ToString(Format, culture)} – {end.ToString(Format, culture)}"" : $""{start.ToString(Format, culture)} – …""
            : Placeholder ?? string.Empty;
        _text.Token(TextBlock.ForegroundProperty, hasValue ? ShellToken.Foreground : ShellToken.MutedForeground);
        _clear.IsVisible = AllowClear && hasValue;
    }
}
"
    };
}
