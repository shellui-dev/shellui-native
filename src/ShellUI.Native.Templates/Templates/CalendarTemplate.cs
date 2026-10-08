using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Templates.Templates;

public static class CalendarTemplate
{
    public static ComponentMetadata Metadata => new()
    {
        Name = "calendar",
        DisplayName = "Calendar",
        Description = "Month calendar with day selection",
        Category = ComponentCategory.Form,
        FilePath = "Calendar.cs",
        Dependencies = new List<string> { "shell", "icon" },
        Tags = new List<string> { "calendar", "date", "picker" }
    };

    public static IReadOnlyDictionary<NativePlatform, string> Contents { get; } = new Dictionary<NativePlatform, string>
    {
        [NativePlatform.MAUI] = @"using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace YourProjectNamespace.Components.UI;

// Month calendar — prev/next header, weekday row, 6x7 day grid of 32px cells. Selected day is
// bg-primary, today is bg-accent, days outside the month are muted. Month names, weekday names
// and the first day of the week follow the current culture.
// Usage: <ui:Calendar SelectedDate=""{Binding Day}"" DateSelected=""OnDay"" />
public partial class Calendar : ContentView
{
    public static readonly BindableProperty SelectedDateProperty =
        BindableProperty.Create(nameof(SelectedDate), typeof(DateTime?), typeof(Calendar), null, BindingMode.TwoWay,
            propertyChanged: (b, o, n) => ((Calendar)b).OnSelectedDateChanged());

    public static readonly BindableProperty MinimumDateProperty =
        BindableProperty.Create(nameof(MinimumDate), typeof(DateTime?), typeof(Calendar), null,
            propertyChanged: (b, o, n) => ((Calendar)b).Refresh());

    public static readonly BindableProperty MaximumDateProperty =
        BindableProperty.Create(nameof(MaximumDate), typeof(DateTime?), typeof(Calendar), null,
            propertyChanged: (b, o, n) => ((Calendar)b).Refresh());

    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
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

    // First day of the month currently shown.
    public DateTime DisplayMonth
    {
        get => _month;
        set { _month = new DateTime(value.Year, value.Month, 1); Refresh(); }
    }

    public event EventHandler<DateTime>? DateSelected;

    private const double Cell = 32;

    private DateTime _month = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private readonly Label _title;
    private readonly List<(Border Box, Label Text)> _cells = new();
    private readonly DateTime[] _dates = new DateTime[42];
    private int _hovered = -1;

    public Calendar()
    {
        _title = new Label
        {
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        _title.Token(Label.TextColorProperty, ShellToken.Foreground);

        var header = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        header.Add(NavButton(IconName.ChevronLeft, -1, ""Previous month""), 0, 0);
        header.Add(_title, 1, 0);
        header.Add(NavButton(IconName.ChevronRight, 1, ""Next month""), 2, 0);

        var format = CultureInfo.CurrentCulture.DateTimeFormat;
        var first = (int)format.FirstDayOfWeek;
        var weekdays = new Grid();
        var days = new Grid { RowSpacing = 2 };
        for (var c = 0; c < 7; c++)
        {
            weekdays.ColumnDefinitions.Add(new ColumnDefinition(Cell));
            days.ColumnDefinitions.Add(new ColumnDefinition(Cell));
            var name = format.AbbreviatedDayNames[(first + c) % 7];
            var label = new Label
            {
                Text = name.Length > 2 ? name[..2] : name,
                FontSize = 12,
                HorizontalTextAlignment = TextAlignment.Center
            };
            label.Token(Label.TextColorProperty, ShellToken.MutedForeground);
            weekdays.Add(label, c, 0);
        }

        for (var i = 0; i < 42; i++)
        {
            var index = i;
            if (i % 7 == 0) days.RowDefinitions.Add(new RowDefinition(Cell));
            var text = new Label
            {
                FontSize = 13,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            var box = new Border
            {
                Content = text,
                WidthRequest = Cell,
                HeightRequest = Cell,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
                BackgroundColor = Colors.Transparent
            };
            var pointer = new PointerGestureRecognizer();
            pointer.PointerEntered += (_, _) => { _hovered = index; StyleCell(index); };
            pointer.PointerExited += (_, _) => { if (_hovered == index) _hovered = -1; StyleCell(index); };
            box.GestureRecognizers.Add(pointer);
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => Select(index);
            box.GestureRecognizers.Add(tap);
            _cells.Add((box, text));
            days.Add(box, i % 7, i / 7);
        }

        Content = new VerticalStackLayout { Spacing = 8, Children = { header, weekdays, days } };
        HorizontalOptions = LayoutOptions.Start;
        Refresh();
    }

    private View NavButton(IconName icon, int direction, string description)
    {
        var button = new Border
        {
            Content = new Icon { Name = icon, Size = 16, Token = ShellToken.MutedForeground },
            WidthRequest = 28,
            HeightRequest = 28,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = ShellTheme.RadiusMd },
            BackgroundColor = Colors.Transparent
        };
        button.Token(Border.StrokeProperty, ShellToken.Input);
        SemanticProperties.SetDescription(button, description);
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => button.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        pointer.PointerExited += (_, _) => { button.ClearValue(VisualElement.BackgroundColorProperty); button.BackgroundColor = Colors.Transparent; };
        button.GestureRecognizers.Add(pointer);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => DisplayMonth = _month.AddMonths(direction);
        button.GestureRecognizers.Add(tap);
        return button;
    }

    private bool IsSelectable(DateTime date) =>
        (MinimumDate is null || date >= MinimumDate.Value.Date) &&
        (MaximumDate is null || date <= MaximumDate.Value.Date);

    private void Select(int index)
    {
        var date = _dates[index];
        if (!IsSelectable(date)) return;
        SelectedDate = date;
        DateSelected?.Invoke(this, date);
    }

    private void OnSelectedDateChanged()
    {
        // Follow the selection when it lands in another month (e.g. set from code).
        if (SelectedDate is { } date && (date.Year != _month.Year || date.Month != _month.Month))
            _month = new DateTime(date.Year, date.Month, 1);
        Refresh();
    }

    private void Refresh()
    {
        _title.Text = _month.ToString(""MMMM yyyy"", CultureInfo.CurrentCulture);
        var first = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
        var offset = ((int)_month.DayOfWeek - first + 7) % 7;
        var start = _month.AddDays(-offset);
        for (var i = 0; i < 42; i++)
        {
            _dates[i] = start.AddDays(i);
            _cells[i].Text.Text = _dates[i].Day.ToString(CultureInfo.CurrentCulture);
            StyleCell(i);
        }
    }

    private void StyleCell(int index)
    {
        var (box, text) = _cells[index];
        var date = _dates[index];
        var inMonth = date.Month == _month.Month;
        var selectable = IsSelectable(date);
        var selected = SelectedDate?.Date == date;
        var today = date == DateTime.Today;

        if (selected)
            box.Token(VisualElement.BackgroundColorProperty, ShellToken.Primary);
        else if (today || (index == _hovered && selectable))
            box.Token(VisualElement.BackgroundColorProperty, ShellToken.Accent);
        else
        {
            box.ClearValue(VisualElement.BackgroundColorProperty);
            box.BackgroundColor = Colors.Transparent;
        }

        text.Token(Label.TextColorProperty,
            selected ? ShellToken.PrimaryForeground : inMonth ? ShellToken.Foreground : ShellToken.MutedForeground);
        box.Opacity = !selectable ? 0.3 : inMonth || selected ? 1 : 0.5;
    }
}
",
        [NativePlatform.Avalonia] = @"using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace YourProjectNamespace.Components.UI;

// Month calendar — prev/next header, weekday row, 6x7 day grid of 32px cells. Selected day is
// bg-primary, today is bg-accent, days outside the month are muted. Month names, weekday names
// and the first day of the week follow the current culture.
// Usage: <ui:Calendar SelectedDate=""{Binding Day}"" DateSelected=""OnDay"" />
public class Calendar : Border
{
    public static readonly StyledProperty<DateTime?> SelectedDateProperty =
        AvaloniaProperty.Register<Calendar, DateTime?>(nameof(SelectedDate), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<DateTime?> MinimumDateProperty =
        AvaloniaProperty.Register<Calendar, DateTime?>(nameof(MinimumDate));

    public static readonly StyledProperty<DateTime?> MaximumDateProperty =
        AvaloniaProperty.Register<Calendar, DateTime?>(nameof(MaximumDate));

    private const double Cell = 32;

    private DateTime _month = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private readonly TextBlock _title;
    private readonly List<(Border Box, TextBlock Text)> _cells = new();
    private readonly DateTime[] _dates = new DateTime[42];
    private int _hovered = -1;

    static Calendar()
    {
        SelectedDateProperty.Changed.AddClassHandler<Calendar>((c, _) => c.OnSelectedDateChanged());
        MinimumDateProperty.Changed.AddClassHandler<Calendar>((c, _) => c.Refresh());
        MaximumDateProperty.Changed.AddClassHandler<Calendar>((c, _) => c.Refresh());
    }

    public DateTime? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
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

    // First day of the month currently shown.
    public DateTime DisplayMonth
    {
        get => _month;
        set { _month = new DateTime(value.Year, value.Month, 1); Refresh(); }
    }

    public event EventHandler<DateTime>? DateSelected;

    public Calendar()
    {
        _title = new TextBlock { FontSize = 14, FontWeight = FontWeight.SemiBold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        _title.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);

        var next = NavButton(IconName.ChevronRight, 1, ""Next month"");
        Grid.SetColumn(_title, 1);
        Grid.SetColumn(next, 2);
        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions(""Auto,*,Auto""),
            Children = { NavButton(IconName.ChevronLeft, -1, ""Previous month""), _title, next }
        };

        var format = CultureInfo.CurrentCulture.DateTimeFormat;
        var first = (int)format.FirstDayOfWeek;
        var weekdays = new Grid();
        var days = new Grid { RowSpacing = 2 };
        for (var c = 0; c < 7; c++)
        {
            weekdays.ColumnDefinitions.Add(new ColumnDefinition(Cell, GridUnitType.Pixel));
            days.ColumnDefinitions.Add(new ColumnDefinition(Cell, GridUnitType.Pixel));
            var name = format.AbbreviatedDayNames[(first + c) % 7];
            var label = new TextBlock { Text = name.Length > 2 ? name[..2] : name, FontSize = 12, TextAlignment = TextAlignment.Center };
            label.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
            Grid.SetColumn(label, c);
            weekdays.Children.Add(label);
        }

        for (var i = 0; i < 42; i++)
        {
            var index = i;
            if (i % 7 == 0) days.RowDefinitions.Add(new RowDefinition(Cell, GridUnitType.Pixel));
            var text = new TextBlock { FontSize = 13, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            var box = new Border
            {
                Child = text,
                Width = Cell,
                Height = Cell,
                CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
                Background = Brushes.Transparent,
                Cursor = new Cursor(StandardCursorType.Hand)
            };
            box.PointerEntered += (_, _) => { _hovered = index; StyleCell(index); };
            box.PointerExited += (_, _) => { if (_hovered == index) _hovered = -1; StyleCell(index); };
            box.PointerReleased += (_, e) =>
            {
                Select(index);
                e.Handled = true;
            };
            Grid.SetColumn(box, i % 7);
            Grid.SetRow(box, i / 7);
            _cells.Add((box, text));
            days.Children.Add(box);
        }

        Child = new StackPanel { Spacing = 8, Children = { header, weekdays, days } };
        HorizontalAlignment = HorizontalAlignment.Left;
        Refresh();
    }

    private Control NavButton(IconName icon, int direction, string description)
    {
        var button = new Border
        {
            Child = new Icon { Kind = icon, Size = 16, Token = ShellToken.MutedForeground },
            Width = 28,
            Height = 28,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(ShellTheme.RadiusMd),
            Background = Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        button.Token(BorderBrushProperty, ShellToken.Input);
        AutomationProperties.SetName(button, description);
        button.PointerEntered += (_, _) => button.Token(BackgroundProperty, ShellToken.Accent);
        button.PointerExited += (_, _) => { button.ClearToken(BackgroundProperty); button.Background = Brushes.Transparent; };
        button.PointerReleased += (_, e) =>
        {
            DisplayMonth = _month.AddMonths(direction);
            e.Handled = true;
        };
        return button;
    }

    private bool IsSelectable(DateTime date) =>
        (MinimumDate is null || date >= MinimumDate.Value.Date) &&
        (MaximumDate is null || date <= MaximumDate.Value.Date);

    private void Select(int index)
    {
        var date = _dates[index];
        if (!IsSelectable(date)) return;
        SelectedDate = date;
        DateSelected?.Invoke(this, date);
    }

    private void OnSelectedDateChanged()
    {
        // Follow the selection when it lands in another month (e.g. set from code).
        if (SelectedDate is { } date && (date.Year != _month.Year || date.Month != _month.Month))
            _month = new DateTime(date.Year, date.Month, 1);
        Refresh();
    }

    private void Refresh()
    {
        if (_cells.Count == 0) return; // property set before the grid exists
        _title.Text = _month.ToString(""MMMM yyyy"", CultureInfo.CurrentCulture);
        var first = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
        var offset = ((int)_month.DayOfWeek - first + 7) % 7;
        var start = _month.AddDays(-offset);
        for (var i = 0; i < 42; i++)
        {
            _dates[i] = start.AddDays(i);
            _cells[i].Text.Text = _dates[i].Day.ToString(CultureInfo.CurrentCulture);
            StyleCell(i);
        }
    }

    private void StyleCell(int index)
    {
        var (box, text) = _cells[index];
        var date = _dates[index];
        var inMonth = date.Month == _month.Month;
        var selectable = IsSelectable(date);
        var selected = SelectedDate?.Date == date;
        var today = date == DateTime.Today;

        if (selected)
            box.Token(BackgroundProperty, ShellToken.Primary);
        else if (today || (index == _hovered && selectable))
            box.Token(BackgroundProperty, ShellToken.Accent);
        else
        {
            box.ClearToken(BackgroundProperty);
            box.Background = Brushes.Transparent;
        }

        text.Token(TextBlock.ForegroundProperty,
            selected ? ShellToken.PrimaryForeground : inMonth ? ShellToken.Foreground : ShellToken.MutedForeground);
        box.Opacity = !selectable ? 0.3 : inMonth || selected ? 1 : 0.5;
    }
}
"
    };
}
