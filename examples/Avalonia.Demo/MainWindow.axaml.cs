using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using AvaloniaDemo.Components.UI;

namespace AvaloniaDemo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        foreach (var item in DemoDropdown.FindDescendantsOfType<DropdownItem>())
            item.Clicked += (_, _) => MenuChoice.Text = $"Picked {item.Text}.";

        CountrySelect.ItemsSource = new[] { "Argentina", "Brazil", "Japan", "Kenya", "Norway", "South Africa", "Zimbabwe" };
        VolumeSlider.ValueChanged += (_, value) => VolumeText.Text = $"Volume {value:0}";

        foreach (var crumb in DemoBreadcrumb.Children.OfType<BreadcrumbItem>())
            crumb.Clicked += (_, _) => NavChoice.Text = $"Went to {crumb.Text}.";
        DemoAccordion.ItemToggled += (_, e) => NavChoice.Text = $"Section {e.Value} {(e.IsOpen ? "opened" : "closed")}.";
        DemoTabs.ValueChanged += (_, e) => NavChoice.Text = $"Tab {e.Old} → {e.New}.";
        ToastDefault.Clicked += (_, _) => Toast.Show("Event has been created", "Sunday, December 03, 2023 at 9:00 AM");
        ToastSuccess.Clicked += (_, _) => Toast.Success("Profile saved");
        ToastError.Clicked += (_, _) => Toast.Error("Upload failed", "The file is larger than 10 MB.");
        ToastWarning.Clicked += (_, _) => Toast.Warning("Storage almost full", "92% of your quota used.");
        ToastInfo.Clicked += (_, _) => Toast.Info("New version available");
        ToastAction.Clicked += (_, _) => Toast.Show("Message archived", actionText: "Undo", action: () => FeedbackChoice.Text = "Undo clicked.");
        DeleteAccount.Clicked += async (_, _) =>
        {
            var confirmed = await DeleteDialog.ShowAsync();
            FeedbackChoice.Text = confirmed ? "Account deleted (demo only)." : "Kept the account.";
            if (confirmed) Toast.Success("Account deleted", "This was only a demo.");
        };

        foreach (var row in DemoTable.Rows.OfType<TableRow>().Where(r => r is not TableHeader))
            row.Tapped += (_, _) =>
            {
                foreach (var other in DemoTable.Rows.OfType<TableRow>()) other.IsSelected = other == row;
                DataChoice.Text = $"Selected {((TableCell)row.Children.OfType<TableCell>().First()).Text}.";
            };
        DemoPagination.PageChanged += (_, page) => DataChoice.Text = $"Page {page} of 20.";

        foreach (var item in DemoContextMenu.FindDescendantsOfType<ContextMenuItem>())
            item.Clicked += (_, _) => AdvancedChoice.Text = $"Context menu: {item.Text}.";
        DemoCarousel.PositionChanged += (_, position) => AdvancedChoice.Text = $"Slide {position + 1} of 4.";
        DemoStepper.CurrentStepChanged += (_, step) => AdvancedChoice.Text = $"Step {step + 1} of 3.";
        DemoStepper.Confirmed += (_, _) => Toast.Success("Workspace created");

        var frameworks = new[] { ".NET MAUI", "Avalonia", "Blazor", "Uno Platform", "WinUI", "WPF", "Flutter", "React Native", "SwiftUI", "Jetpack Compose" };
        FrameworkCombobox.ItemsSource = frameworks;
        FrameworkMultiSelect.ItemsSource = frameworks;
        FrameworkMultiSelect.Values = new[] { "Avalonia", "Blazor" };
        DemoTags.Tags = new[] { "avalonia", "design-system" };
        ViewToggleGroup.ValueChanged += (_, value) => FormChoice.Text = $"Toggle group: {value ?? "none"}.";
        MarksToggleGroup.ValuesChanged += (_, values) => FormChoice.Text = $"Toggle group: {(values.Count == 0 ? "none" : string.Join(", ", values))}.";
        DemoOtp.Completed += (_, code) => FormChoice.Text = $"OTP complete: {code}.";
        QuantityInput.ValueChanged += (_, value) => FormChoice.Text = $"Number: {(value.HasValue ? value.ToString() : "empty")}.";
        DemoTags.TagsChanged += (_, tags) => FormChoice.Text = $"Tags: {string.Join(", ", tags)}.";
        FrameworkCombobox.ValueChanged += (_, value) => FormChoice.Text = $"Combobox: {value}.";
        FrameworkMultiSelect.ValuesChanged += (_, values) => FormChoice.Text = $"Multi select: {(values.Count == 0 ? "none" : string.Join(", ", values))}.";
        CopyCommand.Copied += (_, _) => DisplayChoice.Text = $"Copied: {CopyCommand.Text}.";
        CopyLink.Copied += (_, _) => DisplayChoice.Text = $"Copied: {CopyLink.Text}.";
        DocsCard.Clicked += (_, _) => DisplayChoice.Text = $"Link card: {DocsCard.Title}.";
        SourceCard.Clicked += (_, _) => DisplayChoice.Text = $"Link card: {SourceCard.Title}.";
        FileTree.SelectedValueChanged += (_, value) => DisplayChoice.Text = $"Tree: {value}.";

        foreach (var button in DemoButtonGroup.Items.OfType<Components.UI.Button>())
            button.Clicked += (_, _) => GroupChoice.Text = $"Button group: {button.Text}.";
        DemoInputSearch.TextChanged += (_, _) => GroupChoice.Text = $"Input group: {DemoInputSearch.Text}.";
        DemoInputUrl.TextChanged += (_, _) => GroupChoice.Text = $"Input group: https://{DemoInputUrl.Text}.com.";
        foreach (var menu in DemoMenubar.Menus.OfType<MenubarMenu>())
            foreach (var item in menu.Items.OfType<MenubarItem>())
                item.Clicked += (_, _) => GroupChoice.Text = $"Menubar: {menu.Title} > {item.Text}.";
        foreach (var item in DemoNavigationMenu.Items.OfType<NavigationMenuItem>())
        {
            item.Clicked += (_, _) => CommandChoice.Text = $"Navigation menu: {item.Title}.";
            foreach (var link in item.Links.OfType<NavigationMenuLink>())
                link.Clicked += (_, _) => CommandChoice.Text = $"Navigation menu: {item.Title} > {link.Title}.";
        }
        DemoCommand.ItemSelected += (_, item) => CommandChoice.Text = $"Command: {item.Value}.";
        DemoPalette.ItemSelected += (_, item) => CommandChoice.Text = $"Palette: {item.Value}.";
        OpenPalette.Clicked += (_, _) => DemoPalette.SetOpen(true);
        DemoDataTable.ItemsSource = new List<DemoInvoice>
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
        DemoDataTable.SelectionChanged += (_, items) =>
            DataTableChoice.Text = $"Data table: {items.Count} selected ({string.Join(", ", items.Cast<DemoInvoice>().Select(i => i.Number))}).";
        DemoDataTable.RowTapped += (_, item) => DataTableChoice.Text = $"Data table row: {((DemoInvoice)item).Number}.";
        DemoChatInput.Submitted += async (_, text) =>
        {
            DemoChat.Messages.Add(new ChatMessage { Role = ChatRole.User, Sender = "Shewart", AvatarText = "SS", Text = text });
            var reply = new ChatMessage { Role = ChatRole.Assistant, Sender = "ShellUI", AvatarText = "S", IsStreaming = true };
            DemoChat.Messages.Add(reply);
            DemoChatInput.IsLoading = true;
            ChatFormChoice.Text = $"Chat: sent \"{text}\".";
            await System.Threading.Tasks.Task.Delay(1200);
            reply.Text = $"You said: {text}";
            reply.IsStreaming = false;
            DemoChatInput.IsLoading = false;
        };
        DemoFormSubmit.Clicked += (_, _) =>
        {
            DemoEmailField.Error = (DemoEmail.Text ?? string.Empty).Contains('@') ? string.Empty : "Enter a valid email address.";
            DemoPasswordField.Error = (DemoPassword.Text ?? string.Empty).Length >= 8 ? string.Empty : "Use at least 8 characters.";
            DemoEmail.HasError = DemoEmailField.HasError;
            DemoPassword.HasError = DemoPasswordField.HasError;
            ChatFormChoice.Text = DemoForm.IsValid ? "Form: valid." : $"Form: {DemoForm.FormFields().Count(f => f.HasError)} error(s), first is {DemoForm.FirstInvalid?.Label}.";
        };
        var docsPages = new[] { "Installation", "Quick start", "Theming", "Components" };
        var docsPage = 1;
        void ShowDocsPage(int page)
        {
            docsPage = Math.Clamp(page, 0, docsPages.Length - 1);
            DemoPrevNext.PreviousLabel = docsPage > 0 ? docsPages[docsPage - 1] : null;
            DemoPrevNext.NextLabel = docsPage < docsPages.Length - 1 ? docsPages[docsPage + 1] : null;
            PrevNextChoice.Text = $"Page: {docsPages[docsPage]}.";
        }
        DemoPrevNext.PreviousClicked += (_, _) => ShowDocsPage(docsPage - 1);
        DemoPrevNext.NextClicked += (_, _) => ShowDocsPage(docsPage + 1);
        DemoNavbar.SelectedValueChanged += (_, value) => LayoutChoice.Text = $"Navbar: {value}.";
        DemoSidebar.SelectedValueChanged += (_, value) =>
        {
            SidebarPage.Text = string.IsNullOrEmpty(value) ? string.Empty : char.ToUpperInvariant(value[0]) + value.Substring(1);
            LayoutChoice.Text = $"Sidebar: {value}.";
        };
        DemoSidebar.IsCollapsedChanged += (_, collapsed) =>
        {
            SidebarBrand.IsVisible = !collapsed;
            LayoutChoice.Text = $"Sidebar: {(collapsed ? "collapsed" : "expanded")}.";
        };
        SidebarToggle.Clicked += (_, _) => DemoSidebar.Toggle();
        SidebarLogOut.Clicked += (_, _) => LayoutChoice.Text = "Sidebar: Log out.";
        foreach (var (nav, page) in new[] { (BottomNavDefault, BottomNavDefaultPage), (BottomNavPill, BottomNavPillPage), (BottomNavFloating, BottomNavFloatingPage), (BottomNavExpanding, BottomNavExpandingPage), (BottomNavLifted, BottomNavLiftedPage) })
            nav.SelectedValueChanged += (_, value) =>
            {
                page.Text = string.IsNullOrEmpty(value) ? string.Empty : char.ToUpperInvariant(value[0]) + value.Substring(1);
                BottomNavChoice.Text = $"{nav.Variant}: {value}.";
            };
        BottomNavPay.Clicked += (_, _) => BottomNavChoice.Text = "Pay pressed (an action, not a tab).";
        DemoResizable.Resized += (_, _) =>
            LayoutChoice.Text = $"Resizable: first panel is {ResizableFirst.Size / DemoResizable.Panels.OfType<ResizablePanel>().Sum(p => p.Size):P0} of the width.";

        DemoDateRange.RangeChanged += (_, _) =>
            PickerChoice.Text = $"Date range: {DemoDateRange.StartDate:d} to {(DemoDateRange.EndDate.HasValue ? DemoDateRange.EndDate.Value.ToString("d") : "…")}.";
        DemoFileUpload.FilesChanged += (_, files) =>
            PickerChoice.Text = $"Files: {(files.Count == 0 ? "none" : string.Join(", ", files.Select(f => f.Name)))}.";

        for (var i = 1; i <= 30; i++)
        {
            var tag = new TextBlock { Text = $"v1.2.0-beta.{31 - i}", FontSize = 14 };
            tag.Token(TextBlock.ForegroundProperty, ShellToken.Foreground);
            Tags.Children.Add(tag);
        }

        var clicks = 0;
        LoadingButton.Clicked += async (_, _) =>
        {
            LoadingButton.IsLoading = true;
            Progress.Value = (Progress.Value + 20) % 120;
            await System.Threading.Tasks.Task.Delay(1500);
            LoadingButton.IsLoading = false;
            ClickCount.Text = $"Saved {++clicks} time(s).";
        };

        foreach (var token in Enum.GetValues<ShellToken>())
        {
            var chip = new Border { Width = 40, Height = 40, CornerRadius = new CornerRadius(ShellTheme.RadiusMd), BorderThickness = new Thickness(1) };
            chip.Token(Border.BackgroundProperty, token).Token(Border.BorderBrushProperty, ShellToken.Border);
            var label = new TextBlock { Text = token.ToString(), FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center };
            label.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
            Swatches.Children.Add(new StackPanel { Width = 120, Margin = new Thickness(0, 0, 8, 12), Spacing = 4, Children = { chip, label } });
        }

        foreach (var name in Enum.GetValues<IconName>().Where(n => n != IconName.None))
        {
            var label = new TextBlock { Text = name.ToString(), FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center, TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis };
            label.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
            Icons.Children.Add(new StackPanel { Width = 96, Margin = new Thickness(0, 0, 4, 12), Spacing = 6, Children = { new Icon { Kind = name, Size = 20 }, label } });
        }
    }
}

public sealed record DemoInvoice(string Number, string Customer, string Status, decimal Amount, DateTime Date);
