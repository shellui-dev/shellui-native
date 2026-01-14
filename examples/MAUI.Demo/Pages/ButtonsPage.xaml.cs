namespace MAUI.Demo.Pages;

public partial class ButtonsPage : ContentPage
{
    private int _clickCount = 0;

    public ButtonsPage()
    {
        InitializeComponent();
    }

    private void OnInteractiveClicked(object? sender, EventArgs e)
    {
        _clickCount++;
        ClickCountLabel.Text = $"Clicks: {_clickCount}";
    }
}
