namespace MAUI.Demo;

public partial class MainPage : ContentPage
{
    int clickCount = 0;

    public MainPage()
    {
        InitializeComponent();
        CountrySelect.ItemsSource = new List<string> { "United States", "Canada", "United Kingdom", "Germany" };
    }

    private void OnButtonClicked(object? sender, EventArgs e)
    {
        clickCount++;
        if (sender is Components.UI.Button btn)
        {
            StatusLabel.Text = $"Clicked: {btn.Text} (Total: {clickCount})";
        }
    }

    private async void OnLoadingClicked(object? sender, EventArgs e)
    {
        if (sender is Components.UI.Button btn)
        {
            btn.IsLoading = true;
            btn.Text = "Loading...";
            StatusLabel.Text = "Loading started...";

            await Task.Delay(2000); // Simulate async operation

            btn.IsLoading = false;
            btn.Text = "Click to Load";
            StatusLabel.Text = "Loading complete!";
        }
    }

    private void OnDialogTriggerClicked(object? sender, EventArgs e) => DemoDialog.SetOpen(true);
    private void OnDialogClose(object? sender, EventArgs e) => DemoDialog.SetOpen(false);
    private void OnDialogOpenChanged(object? sender, bool e) { }
    private void OnDrawerTriggerClicked(object? sender, EventArgs e) => DemoDrawer.SetOpen(true);
    private void OnSheetTriggerClicked(object? sender, EventArgs e) => DemoSheet.SetOpen(true);
    private void OnDropdownItem(object? sender, EventArgs e) => StatusLabel.Text = "Dropdown item clicked";
}
