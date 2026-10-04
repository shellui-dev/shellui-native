using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace MAUI.Demo.Components.UI;

// Date picker — shadcn-style trigger (calendar icon + formatted date, h-10 rounded-md border)
// that opens a Calendar floating in the page layer. Custom-drawn, so it looks the same on every
// platform. Usage: <ui:DatePicker Date="{Binding DueDate}" Format="MMM d, yyyy" />
public partial class DatePicker : ContentView, IShellPopup
{
    public static readonly BindableProperty DateProperty =
        BindableProperty.Create(nameof(Date), typeof(DateTime), typeof(DatePicker),
            DateTime.Today, BindingMode.TwoWay, propertyChanged: OnDateChanged);

    public static readonly BindableProperty MinimumDateProperty =
        BindableProperty.Create(nameof(MinimumDate), typeof(DateTime?), typeof(DatePicker), null,
            propertyChanged: (b, o, n) => ((DatePicker)b)._calendar.MinimumDate = (DateTime?)n);

    public static readonly BindableProperty MaximumDateProperty =
        BindableProperty.Create(nameof(MaximumDate), typeof(DateTime?), typeof(DatePicker), null,
            propertyChanged: (b, o, n) => ((DatePicker)b)._calendar.MaximumDate = (DateTime?)n);

    // .NET date format string for the trigger text.
    public static readonly BindableProperty FormatProperty =
        BindableProperty.Create(nameof(Format), typeof(string), typeof(DatePicker), "MMMM d, yyyy",
            propertyChanged: (b, o, n) => ((DatePicker)b).UpdateText());

    public DateTime Date
    {
        get => (DateTime)GetValue(DateProperty);
        set => SetValue(DateProperty, value);
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

    public string Format
    {
        get => (string)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public bool IsOpen { get; private set; }

    public event EventHandler<DateChangedEventArgs>? DateChanged;

    private readonly Border _trigger;
    private readonly Label _text;
    private readonly Calendar _calendar;
    private readonly Border _panel;
    private ShellPopupHandle? _handle;

    public DatePicker()
    {
        _text = new Label { FontSize = 14, VerticalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center };
        _text.Token(Label.TextColorProperty, ShellToken.Foreground);

        _trigger = new Border
        {
            Content = new HorizontalStackLayout
            {
                Spacing = 8,
                Children = { new Icon { Name = IconName.Calendar, Size = 16, Token = ShellToken.MutedForeground }, _text }
            },
            HeightRequest = 40,
            MinimumWidthRequest = 200,
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

        _calendar = new Calendar { SelectedDate = Date };
        _calendar.DateSelected += (_, date) =>
        {
            Date = date;
            SetOpen(false);
        };

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
        UpdateText();
    }

    public void SetOpen(bool open)
    {
        if (IsOpen == open) return;
        IsOpen = open;
        _trigger.Token(Border.StrokeProperty, open ? ShellToken.Ring : ShellToken.Input);
        if (open)
        {
            _calendar.SelectedDate = Date;
            _calendar.DisplayMonth = Date;
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

    private static void OnDateChanged(BindableObject b, object o, object n)
    {
        var picker = (DatePicker)b;
        picker.UpdateText();
        picker._calendar.SelectedDate = (DateTime)n;
        picker.DateChanged?.Invoke(picker, new DateChangedEventArgs((DateTime)o, (DateTime)n));
    }

    private void UpdateText() => _text.Text = Date.ToString(Format, CultureInfo.CurrentCulture);
}
