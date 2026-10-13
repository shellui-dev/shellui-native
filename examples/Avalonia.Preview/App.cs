using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using AvaloniaDemo.Components.UI;

namespace AvaloniaPreview;

public class App : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        RequestedThemeVariant = Program.Theme;
        ShellTheme.EnsureInitialized();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is ISingleViewApplicationLifetime single)
        {
            var host = new Border { Padding = new Thickness(24), Child = Load(Program.Component) };
            host.Token(Border.BackgroundProperty, ShellToken.Background);
            single.MainView = host;
        }

        base.OnFrameworkInitializationCompleted();
    }

    // A preview is Previews/<name>.axaml: the markup a docs page shows, in a panel that places it.
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The previews are in this assembly, which the project file keeps whole.")]
    private static Control Load(string name)
    {
        if (name.Length > 0 && name.All(c => char.IsAsciiLetterOrDigit(c) || c == '-'))
        {
            try
            {
                return (Control)AvaloniaXamlLoader.Load(new Uri($"avares://AvaloniaPreview/Previews/{name}.axaml"));
            }
            catch (Exception)
            {
                // No such file: say so below.
            }
        }

        var message = new TextBlock
        {
            Text = name.Length == 0 ? "Add ?c=<name> to the address to pick a preview." : $"There is no preview named \"{name}\".",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
        };
        message.Token(TextBlock.ForegroundProperty, ShellToken.MutedForeground);
        return message;
    }
}
