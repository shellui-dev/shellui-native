using System.Xml.Linq;
using ShellUI.Native.CLI.Services;
using ShellUI.Native.Core.Models;

namespace ShellUI.Native.Tests;

// Covers ProjectDetector.DetectPlatform — the pure XML-in / platform-out core.
// Exposed via InternalsVisibleTo so we don't have to swap Directory.GetCurrentDirectory
// during parallel test runs.
public class ProjectDetectorTests
{
    [Fact]
    public void Detects_MAUI_via_UseMaui_property()
    {
        var doc = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <UseMaui>true</UseMaui>
              </PropertyGroup>
            </Project>
            """);
        Assert.Equal(NativePlatform.MAUI, ProjectDetector.DetectPlatform(doc, "fake.csproj"));
    }

    [Fact]
    public void Detects_MAUI_via_multitarget_frameworks_with_android_or_ios()
    {
        var doc = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFrameworks>net10.0-android;net10.0-ios;net10.0-maccatalyst</TargetFrameworks>
              </PropertyGroup>
            </Project>
            """);
        Assert.Equal(NativePlatform.MAUI, ProjectDetector.DetectPlatform(doc, "fake.csproj"));
    }

    [Fact]
    public void Detects_Avalonia_via_PackageReference()
    {
        var doc = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="Avalonia" Version="11.2.1" />
                <PackageReference Include="Avalonia.Desktop" Version="11.2.1" />
              </ItemGroup>
            </Project>
            """);
        Assert.Equal(NativePlatform.Avalonia, ProjectDetector.DetectPlatform(doc, "fake.csproj"));
    }

    [Fact]
    public void Detects_Avalonia_via_App_axaml_beside_csproj()
    {
        var tmp = Directory.CreateTempSubdirectory("shellui-native-tests-avalonia");
        try
        {
            var csprojPath = Path.Combine(tmp.FullName, "MyApp.csproj");
            File.WriteAllText(csprojPath, "<Project Sdk=\"Microsoft.NET.Sdk\" />");
            File.WriteAllText(Path.Combine(tmp.FullName, "App.axaml"), "<Application />");

            var doc = XDocument.Load(csprojPath);
            Assert.Equal(NativePlatform.Avalonia, ProjectDetector.DetectPlatform(doc, csprojPath));
        }
        finally
        {
            tmp.Delete(recursive: true);
        }
    }

    [Fact]
    public void Detects_WinUI_via_UseWinUI_property()
    {
        var doc = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <UseWinUI>true</UseWinUI>
              </PropertyGroup>
            </Project>
            """);
        Assert.Equal(NativePlatform.WinUI, ProjectDetector.DetectPlatform(doc, "fake.csproj"));
    }

    [Fact]
    public void Detects_WPF_via_UseWPF_property()
    {
        var doc = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <UseWPF>true</UseWPF>
              </PropertyGroup>
            </Project>
            """);
        Assert.Equal(NativePlatform.WPF, ProjectDetector.DetectPlatform(doc, "fake.csproj"));
    }

    [Fact]
    public void Returns_Unknown_for_plain_class_library()
    {
        var doc = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);
        Assert.Equal(NativePlatform.Unknown, ProjectDetector.DetectPlatform(doc, "fake.csproj"));
    }

    [Fact]
    public void Prefers_MAUI_over_Avalonia_when_both_are_declared()
    {
        // Edge case: an unusual project references Avalonia packages but is actually
        // MAUI (UseMaui=true wins because it's checked first).
        var doc = XDocument.Parse("""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <UseMaui>true</UseMaui>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Avalonia" Version="11.2.1" />
              </ItemGroup>
            </Project>
            """);
        Assert.Equal(NativePlatform.MAUI, ProjectDetector.DetectPlatform(doc, "fake.csproj"));
    }
}
