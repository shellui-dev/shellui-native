using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using AvaloniaDemo.Components.UI;
using Xunit;

namespace ShellUI.Native.Avalonia.Tests;

// Every component and block can be created, laid out and drawn on its own, in both themes.
public class ComponentSmokeTests
{
    private static readonly string[] Namespaces = { typeof(ShellTheme).Namespace!, "AvaloniaDemo.Components.Blocks" };

    public static IEnumerable<object[]> Components() =>
        typeof(ShellTheme).Assembly.GetTypes()
            .Where(t => Namespaces.Contains(t.Namespace) && t.IsPublic && !t.IsAbstract
                        && typeof(Control).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(t => t.Name)
            .Select(t => new object[] { t.Name });

    [AvaloniaTheory]
    [MemberData(nameof(Components))]
    public void Renders_in_light_and_dark(string typeName)
    {
        var type = typeof(ShellTheme).Assembly.GetTypes().Single(t => t.Name == typeName && Namespaces.Contains(t.Namespace));
        foreach (var theme in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        {
            var control = (Control)Activator.CreateInstance(type)!;
            var window = Ui.Show(control, theme);
            Assert.NotNull(window.CaptureRenderedFrame());
            window.Close();
        }
    }
}
