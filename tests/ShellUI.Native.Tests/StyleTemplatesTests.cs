using System.Xml.Linq;
using ShellUI.Native.Core.Models;
using ShellUI.Native.Templates;

namespace ShellUI.Native.Tests;

// `shellui-native init` writes these dictionaries into the user's project, where the XAML
// compiler rejects malformed XML (e.g. "--" inside a comment) and fails their build.
public class StyleTemplatesTests
{
    public static IEnumerable<object[]> Dictionaries() => new[]
    {
        new object[] { nameof(StyleTemplates.ThemeResourceDictionary), StyleTemplates.ThemeResourceDictionary },
        new object[] { nameof(StyleTemplates.DarkThemeResourceDictionary), StyleTemplates.DarkThemeResourceDictionary },
    };

    [Theory]
    [MemberData(nameof(Dictionaries))]
    public void Theme_dictionary_is_well_formed_xml(string name, string xaml)
    {
        var ex = Record.Exception(() => XDocument.Parse(xaml));
        Assert.True(ex is null, $"{name} is not valid XML: {ex?.Message}");
    }

    // Every token the `shell` template publishes at runtime should be available in the XAML too.
    [Theory]
    [MemberData(nameof(Dictionaries))]
    public void Theme_dictionary_defines_every_shell_token(string name, string xaml)
    {
        var shell = ComponentRegistry.GetComponentContent("shell", NativePlatform.MAUI)!;
        var enumBody = shell[shell.IndexOf("public enum ShellToken", StringComparison.Ordinal)..];
        enumBody = enumBody[(enumBody.IndexOf('{') + 1)..enumBody.IndexOf('}')];
        var tokens = enumBody.Split(new[] { ',', '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        var keys = XDocument.Parse(xaml).Root!.Elements()
            .Select(e => (string?)e.Attribute(XName.Get("Key", "http://schemas.microsoft.com/winfx/2009/xaml")))
            .ToHashSet();

        foreach (var token in tokens)
            Assert.True(keys.Contains($"ShellUI{token}"), $"{name} is missing ShellUI{token}");
    }
}
