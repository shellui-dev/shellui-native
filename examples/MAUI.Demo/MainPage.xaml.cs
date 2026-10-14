using MAUI.Demo.Components.UI;

namespace MAUI.Demo;

public partial class MainPage : ContentPage
{
    private int _clickCount;

    public MainPage()
    {
        InitializeComponent();
        DataTableDemo.ItemsSource = new List<DemoInvoice>
        {
            new DemoInvoice("INV001", "Shewart Shepherd", "Paid", 250.00m, new DateTime(2026, 9, 2)),
            new DemoInvoice("INV002", "ShellUI", "Pending", 150.00m, new DateTime(2026, 9, 5)),
            new DemoInvoice("INV003", "ShellIcons", "Unpaid", 350.00m, new DateTime(2026, 9, 9)),
            new DemoInvoice("INV004", "ShellDocs", "Paid", 450.00m, new DateTime(2026, 9, 12)),
            new DemoInvoice("INV005", "Shewart Shepherd", "Paid", 550.00m, new DateTime(2026, 9, 14)),
            new DemoInvoice("INV006", "ShellUI Native", "Pending", 200.00m, new DateTime(2026, 9, 18)),
            new DemoInvoice("INV007", "ShellUI", "Unpaid", 300.00m, new DateTime(2026, 9, 21)),
            new DemoInvoice("INV008", "ShellDocs", "Paid", 75.50m, new DateTime(2026, 9, 24)),
            new DemoInvoice("INV009", "ShellIcons", "Paid", 1200.00m, new DateTime(2026, 9, 27)),
            new DemoInvoice("INV010", "ShellUI Native", "Pending", 89.99m, new DateTime(2026, 10, 1)),
            new DemoInvoice("INV011", "Shewart Shepherd", "Unpaid", 640.00m, new DateTime(2026, 10, 4)),
            new DemoInvoice("INV012", "ShellUI", "Paid", 310.25m, new DateTime(2026, 10, 8)),
        };
        CountrySelect.ItemsSource = new List<string> { "United States", "Canada", "United Kingdom", "Germany", "Japan" };
        TagsInput.Tags = new List<string> { "maui", "design-system" };
        FrameworkMultiSelect.ItemsSource = new List<string>
        {
            ".NET MAUI", "Avalonia", "Blazor", "Uno Platform", "WinUI", "WPF", "Flutter", "React Native"
        };
        FrameworkMultiSelect.Values = new List<string> { ".NET MAUI", "Blazor" };
        FrameworkCombobox.ItemsSource = new List<string>
        {
            ".NET MAUI", "Avalonia", "Blazor", "Uno Platform", "WinUI", "WPF", "Flutter", "React Native", "SwiftUI", "Jetpack Compose"
        };

        foreach (var tag in Enumerable.Range(1, 30).Reverse().Select(i => $"v1.2.0-beta.{i}"))
        {
            TagList.Children.Add(new Separator());
            TagList.Children.Add(new Label { Text = tag, FontSize = 14 }.Token(Label.TextColorProperty, ShellToken.Foreground));
        }

        // Filling the samples above raised their change events; start with the hint instead.
        Report("Interact with a component to see its events here.");
    }

    private void Report(string message) => StatusLabel.Text = message;

    // One category at a time: hidden groups are not laid out.
    private string _category = "buttons";
    private void OnCategoryChanged(object? sender, string? value)
    {
        // A single-select toggle group can be cleared by tapping the pressed item; keep one on.
        if (value is null) { CategoryPicker.Value = _category; return; }
        _category = value;
        GroupButtons.IsVisible = value == "buttons";
        GroupForms.IsVisible = value == "forms";
        GroupFeedback.IsVisible = value == "feedback";
        GroupOverlays.IsVisible = value == "overlays";
        GroupNavigation.IsVisible = value == "navigation";
        GroupData.IsVisible = value == "data";
        GroupCharts.IsVisible = value == "charts";
        _ = PageScroll.ScrollToAsync(0, 0, false);
    }

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
    private void OnDateRangeChanged(object? sender, EventArgs e) =>
        Report($"Date range: {DateRangeDemo.StartDate:d} to {(DateRangeDemo.EndDate.HasValue ? DateRangeDemo.EndDate.Value.ToString("d") : "…")}");
    private void OnFilesChanged(object? sender, IReadOnlyList<FileResult> files) =>
        Report($"Files: {(files.Count == 0 ? "none" : string.Join(", ", files.Select(f => f.FileName)))}");
    private void OnComboboxChanged(object? sender, string value) => Report($"Combobox: {value}");

    private void OnTogglePressed(object? sender, bool pressed) => Report($"Toggle: {(pressed ? "on" : "off")}");
    private void OnOtpCompleted(object? sender, string code) => Report($"OTP complete: {code}");
    private void OnPageChanged(object? sender, int page) => Report($"Page: {page}");

    private void OnToggleGroupChanged(object? sender, string? value) => Report($"Toggle group: {value ?? "none"}");
    private void OnToggleGroupValues(object? sender, IReadOnlyList<string> values) =>
        Report($"Toggle group: {(values.Count == 0 ? "none" : string.Join(", ", values))}");
    private void OnNumberChanged(object? sender, decimal? value) => Report($"Number: {(value.HasValue ? value.ToString() : "empty")}");
    private void OnTagsChanged(object? sender, IReadOnlyList<string> tags) => Report($"Tags: {string.Join(", ", tags)}");

    private void OnCopied(object? sender, EventArgs e) => Report($"Copied: {(sender as CopyButton)?.Text}");
    private void OnMultiSelectChanged(object? sender, IReadOnlyList<string> values) =>
        Report($"Multi select: {(values.Count == 0 ? "none" : string.Join(", ", values))}");
    private void OnLinkCardClicked(object? sender, EventArgs e) => Report($"Link card: {(sender as LinkCard)?.Title}");
    private void OnTreeSelected(object? sender, string? value) => Report($"Tree: {value}");

    private void OnContextMenuItem(object? sender, EventArgs e) => Report($"Context menu: {(sender as ContextMenuItem)?.Text}");
    private void OnStepChanged(object? sender, int step) => Report($"Stepper: step {step + 1}");
    private void OnStepperConfirmed(object? sender, EventArgs e) => Toast.Success("Workspace created");
    private void OnCarouselChanged(object? sender, int position) => Report($"Carousel: slide {position + 1}");

    private TableRow? _selectedRow;
    private static readonly string[] DocsPages = { "Installation", "Quick start", "Theming", "Components" };
    private int _docsPage = 1;
    private void OnPrevious(object? sender, EventArgs e) => ShowDocsPage(_docsPage - 1);
    private void OnNext(object? sender, EventArgs e) => ShowDocsPage(_docsPage + 1);
    private void ShowDocsPage(int page)
    {
        _docsPage = Math.Clamp(page, 0, DocsPages.Length - 1);
        PrevNextDemo.PreviousLabel = _docsPage > 0 ? DocsPages[_docsPage - 1] : string.Empty;
        PrevNextDemo.NextLabel = _docsPage < DocsPages.Length - 1 ? DocsPages[_docsPage + 1] : string.Empty;
        Report($"Prev next: page {DocsPages[_docsPage]}");
    }

    private void OnFormSubmit(object? sender, EventArgs e)
    {
        FormEmailField.Error = (FormEmail.Text ?? string.Empty).Contains('@') ? string.Empty : "Enter a valid email address.";
        FormPasswordField.Error = (FormPassword.Text ?? string.Empty).Length >= 8 ? string.Empty : "Use at least 8 characters.";
        FormEmail.HasError = FormEmailField.HasError;
        FormPassword.HasError = FormPasswordField.HasError;
        Report(FormDemo.IsValid ? "Form: valid" : $"Form: {FormDemo.FormFields().Count(f => f.HasError)} error(s), first is {FormDemo.FirstInvalid?.Label}");
    }

    private async void OnChatSubmitted(object? sender, string text)
    {
        ChatDemo.Messages.Add(new ChatMessage { Role = ChatRole.User, Sender = "Shewart", AvatarText = "SS", Text = text });
        var reply = new ChatMessage { Role = ChatRole.Assistant, Sender = "ShellUI", AvatarText = "S", IsStreaming = true };
        ChatDemo.Messages.Add(reply);
        ChatInputDemo.IsLoading = true;
        Report($"Chat: sent \"{text}\"");
        await Task.Delay(1200);
        reply.Text = $"You said: {text}";
        reply.IsStreaming = false;
        ChatInputDemo.IsLoading = false;
    }

    private void OnDataTableSelection(object? sender, IReadOnlyList<object> items) =>
        Report($"Data table: {items.Count} selected ({string.Join(", ", items.Cast<DemoInvoice>().Select(i => i.Number))})");
    private void OnDataTableRow(object? sender, object item) => Report($"Data table row: {((DemoInvoice)item).Number}");
    private void OnTableRowTapped(object? sender, EventArgs e)
    {
        if (sender is not TableRow row) return;
        if (_selectedRow != null) _selectedRow.IsSelected = false;
        _selectedRow = row;
        row.IsSelected = true;
        Report($"Table: {row.Children.OfType<TableCell>().FirstOrDefault()?.Text}");
    }

    private void OnNavigationMenuLink(object? sender, EventArgs e) => Report($"Navigation menu: {(sender as NavigationMenuLink)?.Title}");
    private void OnNavigationMenuItem(object? sender, EventArgs e) => Report($"Navigation menu: {(sender as NavigationMenuItem)?.Title}");
    private void OnCommandSelected(object? sender, CommandItem item) => Report($"Command: {item.Value}");
    private void OnOpenCommandPalette(object? sender, EventArgs e) => CommandPaletteDemo.SetOpen(true);
    private void OnMenubarItem(object? sender, EventArgs e) => Report($"Menubar: {(sender as MenubarItem)?.Text}");
    private void OnInputGroupChanged(object? sender, TextChangedEventArgs e) => Report($"Input group: {e.NewTextValue}");
    private void OnNavbarChanged(object? sender, string? value) => Report($"Navbar: {value}");
    private void OnSidebarChanged(object? sender, string? value)
    {
        SidebarPage.Text = value is null ? string.Empty : char.ToUpperInvariant(value[0]) + value[1..];
        Report($"Sidebar: {value}");
    }
    private void OnSidebarToggle(object? sender, EventArgs e) => SidebarDemo.Toggle();
    private void OnSidebarCollapsed(object? sender, bool collapsed)
    {
        SidebarBrand.IsVisible = !collapsed;
        Report($"Sidebar: {(collapsed ? "collapsed" : "expanded")}");
    }
    private void OnSidebarLogOut(object? sender, EventArgs e) => Report("Sidebar: Log out");
    private void OnResized(object? sender, EventArgs e) =>
        Report($"Resizable: first panel is {ResizableFirst.Size / ResizableDemo.Panels.Sum(p => p.Size):P0} of the width");

    private void OnDropdownItem(object? sender, EventArgs e) => Report($"Dropdown: {(sender as DropdownItem)?.Text}");
    private void OnTabChanged(object? sender, (string Old, string New) e) => Report($"Tab: {e.New}");

    private void OnBreadcrumbClicked(object? sender, EventArgs e)
    {
        if (sender is BreadcrumbItem item)
            Report($"Breadcrumb: {item.Text}");
    }
}

public sealed record DemoInvoice(string Number, string Customer, string Status, decimal Amount, DateTime Date);
