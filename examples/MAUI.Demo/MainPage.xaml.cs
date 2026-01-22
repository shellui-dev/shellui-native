namespace MAUI.Demo;

public partial class MainPage : ContentPage
{
    int clickCount = 0;

    public MainPage()
    {
        InitializeComponent();
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
}
