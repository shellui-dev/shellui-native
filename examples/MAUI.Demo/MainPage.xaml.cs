using MAUI.Demo.Components.UI;

namespace MAUI.Demo;

public partial class MainPage : ContentPage
{
    private int _clickCount;

    public MainPage()
    {
        InitializeComponent();
        CountrySelect.ItemsSource = new List<string> { "United States", "Canada", "United Kingdom", "Germany", "Japan" };
        FrameworkCombobox.ItemsSource = new List<string>
        {
            ".NET MAUI", "Avalonia", "Blazor", "Uno Platform", "WinUI", "WPF", "Flutter", "React Native", "SwiftUI", "Jetpack Compose"
        };

        foreach (var tag in Enumerable.Range(1, 30).Reverse().Select(i => $"v1.2.0-beta.{i}"))
        {
            TagList.Children.Add(new Separator());
            TagList.Children.Add(new Label { Text = tag, FontSize = 14 }.Token(Label.TextColorProperty, ShellToken.Foreground));
        }
    }

    private void Report(string message) => StatusLabel.Text = message;

    private void OnButtonClicked(object? sender, EventArgs e)
    {
        _clickCount++;
        var name = sender is Components.UI.Button { Text.Length: > 0 } btn ? btn.Text : $"{(sender as Components.UI.Button)?.Icon} icon";
        Report($"Clicked: {name} (total {_clickCount})");
    }

    private async void OnLoadingClicked(object? sender, EventArgs e)
    {
        LoadingBtn.IsLoading = true;
        LoadingBtn.Text = "Please wait";
        Report("Loading…");
        await Task.Delay(2000);
        LoadingBtn.IsLoading = false;
        LoadingBtn.Text = "Click to load";
        Report("Loading complete");
    }

    private void OnSelectChanged(object? sender, EventArgs e) => Report($"Select: {CountrySelect.SelectedItem}");
    private void OnCheckChanged(object? sender, bool isChecked) => Report($"Checkbox: {(isChecked ? "checked" : "unchecked")}");
    private void OnSwitchToggled(object? sender, bool isOn) => Report($"Switch: {(isOn ? "on" : "off")}");
    private void OnRadioChanged(object? sender, string value) => Report($"Radio: {value}");

    private void OnSliderChanged(object? sender, ValueChangedEventArgs e)
    {
        ProgressDemo.Value = e.NewValue;
        Report($"Slider: {e.NewValue:F0}");
    }

    private void OnDialogSave(object? sender, EventArgs e) { DemoDialog.SetOpen(false); Report("Dialog: saved"); }
    private void OnDrawerSubmit(object? sender, EventArgs e) { DemoDrawer.SetOpen(false); Report("Drawer: submitted"); }
    private void OnSheetSave(object? sender, EventArgs e) { DemoSheet.SetOpen(false); Report("Sheet: saved"); }

    private async void OnAlertDialogClicked(object? sender, EventArgs e)
    {
        var confirmed = await DeleteDialog.ShowAsync();
        Report($"Alert dialog: {(confirmed ? "confirmed" : "cancelled")}");
        if (confirmed) Toast.Success("Account deleted", "This was only a demo.");
    }

    private void OnToastDefault(object? sender, EventArgs e) =>
        Toast.Show("Event has been created", "Sunday, December 03, 2023 at 9:00 AM");
    private void OnToastSuccess(object? sender, EventArgs e) => Toast.Success("Profile saved");
    private void OnToastError(object? sender, EventArgs e) => Toast.Error("Upload failed", "The file is larger than 10 MB.");
    private void OnToastWarning(object? sender, EventArgs e) => Toast.Warning("Storage almost full", "92% of your quota used.");
    private void OnToastInfo(object? sender, EventArgs e) => Toast.Info("New version available");
    private void OnToastAction(object? sender, EventArgs e) =>
        Toast.Show("Message archived", actionText: "Undo", action: () => Report("Toast: undo clicked"));

    private void OnDateChanged(object? sender, DateChangedEventArgs e) => Report($"Date: {e.NewDate:d}");
    private void OnCalendarSelected(object? sender, DateTime date) => Report($"Calendar: {date:d}");
    private void OnTimeChanged(object? sender, EventArgs e)
    {
        if (sender is Components.UI.TimePicker picker)
            Report($"Time: {picker.Time.Hours:00}:{picker.Time.Minutes:00}");
    }
    private void OnComboboxChanged(object? sender, string value) => Report($"Combobox: {value}");

    private void OnTogglePressed(object? sender, bool pressed) => Report($"Toggle: {(pressed ? "on" : "off")}");
    private void OnOtpCompleted(object? sender, string code) => Report($"OTP complete: {code}");
    private void OnPageChanged(object? sender, int page) => Report($"Page: {page}");

    private void OnContextMenuItem(object? sender, EventArgs e) => Report($"Context menu: {(sender as ContextMenuItem)?.Text}");
    private void OnStepChanged(object? sender, int step) => Report($"Stepper: step {step + 1}");
    private void OnStepperConfirmed(object? sender, EventArgs e) => Toast.Success("Workspace created");
    private void OnCarouselChanged(object? sender, int position) => Report($"Carousel: slide {position + 1}");

    private TableRow? _selectedRow;
    private void OnTableRowTapped(object? sender, EventArgs e)
    {
        if (sender is not TableRow row) return;
        if (_selectedRow != null) _selectedRow.IsSelected = false;
        _selectedRow = row;
        row.IsSelected = true;
        Report($"Table: {row.Children.OfType<TableCell>().FirstOrDefault()?.Text}");
    }

    private void OnDropdownItem(object? sender, EventArgs e) => Report($"Dropdown: {(sender as DropdownItem)?.Text}");
    private void OnTabChanged(object? sender, (string Old, string New) e) => Report($"Tab: {e.New}");

    private void OnBreadcrumbClicked(object? sender, EventArgs e)
    {
        if (sender is BreadcrumbItem item)
            Report($"Breadcrumb: {item.Text}");
    }
}
