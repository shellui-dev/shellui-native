using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using AvaloniaDemo.Components.UI;
using ShellUI.Native.Avalonia.Tests;

[assembly: AvaloniaTestApplication(typeof(TestApp))]
// One UI thread runs every test; in parallel, one test's waits would run another's work mid-test.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]

namespace ShellUI.Native.Avalonia.Tests;

// The app a user's project has after `shellui-native init`: Fluent, the Inter font and the ShellUI
// theme. Rendering goes through Skia, so tests can lay out, draw and send input without a screen.
public class TestApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        ShellTheme.EnsureInitialized();
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApp>()
        .UseSkia()
        .WithInterFont()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
