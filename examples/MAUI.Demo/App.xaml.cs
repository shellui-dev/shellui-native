using Microsoft.Extensions.DependencyInjection;

namespace MAUI.Demo;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		// Publish the ShellUI theme tokens before the first page resolves {DynamicResource ShellUI*}.
		Components.UI.ShellTheme.EnsureInitialized();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}