using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaDemo.Components.UI;
using Xunit;

namespace ShellUI.Native.Avalonia.Tests;

// Token bindings follow the theme, and every test's new Application gets the palettes.
public class ThemeTests
{
    [AvaloniaFact]
    public void Token_brushes_follow_SetTheme()
    {
        var surface = new Border { Width = 40, Height = 40 }.Token(Border.BackgroundProperty, ShellToken.Background);
        Ui.Show(surface, ThemeVariant.Light);
        Assert.Equal(ShellTheme.Light[ShellToken.Background], ((ISolidColorBrush)surface.Background!).Color);

        ShellTheme.SetTheme(ThemeVariant.Dark);
        Ui.Settle();
        Assert.True(ShellTheme.IsDarkMode);
        Assert.Equal(ShellTheme.Dark[ShellToken.Background], ((ISolidColorBrush)surface.Background!).Color);
        Assert.Equal(ShellTheme.Dark[ShellToken.Primary], ShellTheme.Get(ShellToken.Primary));
    }

    [AvaloniaFact]
    public void ThemeChanged_is_raised_when_the_variant_changes()
    {
        Ui.Show(new Border(), ThemeVariant.Light);
        var raised = 0;
        void OnChanged(object? s, System.EventArgs e) => raised++;
        ShellTheme.ThemeChanged += OnChanged;
        try
        {
            ShellTheme.ToggleTheme();
            Ui.Settle();
            Assert.True(raised >= 1);
        }
        finally
        {
            ShellTheme.ThemeChanged -= OnChanged;
        }
    }
}
