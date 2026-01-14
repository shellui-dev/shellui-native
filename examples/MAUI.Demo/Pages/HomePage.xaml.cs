namespace MAUI.Demo.Pages;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private async void OnButtonsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//buttons");
    }

    private async void OnFormsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//forms");
    }

    private async void OnCardsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//cards");
    }
}
