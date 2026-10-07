using System.Xml.Linq;
using ShellUI.Native.Core.Models;

namespace ShellUI.Native.CLI.Services;

// Detects the type of native project in the current directory
public static class ProjectDetector
{
    public static ProjectInfo DetectProject()
    {
        var csprojFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj");

        if (csprojFiles.Length == 0)
        {
            throw new Exception("No .csproj file found in current directory. Please run this command from your project root.");
        }

        var csprojPath = csprojFiles[0];
        var projectName = Path.GetFileNameWithoutExtension(csprojPath);

        var doc = XDocument.Load(csprojPath);

        var platform = DetectPlatform(doc, csprojPath);
        var rootNamespace = DetectRootNamespace(doc, projectName);

        return new ProjectInfo
        {
            ProjectPath = csprojPath,
            ProjectName = projectName,
            RootNamespace = rootNamespace,
            Platform = platform
        };
    }

    internal static NativePlatform DetectPlatform(XDocument doc, string csprojPath)
    {
        var sdk = doc.Root?.Attribute("Sdk")?.Value ?? "";

        // Check for MAUI workload
        if (sdk.Contains("Maui", StringComparison.OrdinalIgnoreCase))
            return NativePlatform.MAUI;

        var useMaui = doc.Descendants("UseMaui").FirstOrDefault()?.Value;
        if (useMaui?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
            return NativePlatform.MAUI;

        // Check for Avalonia (PackageReference-based, no dedicated SDK)
        var hasAvaloniaPackage = doc.Descendants("PackageReference")
            .Any(e => (e.Attribute("Include")?.Value ?? "").StartsWith("Avalonia", StringComparison.OrdinalIgnoreCase));
        if (hasAvaloniaPackage)
            return NativePlatform.Avalonia;

        var projectDir = Path.GetDirectoryName(csprojPath) ?? ".";
        if (File.Exists(Path.Combine(projectDir, "App.axaml")))
            return NativePlatform.Avalonia;

        // Check for WinUI
        var useWinUI = doc.Descendants("UseWinUI").FirstOrDefault()?.Value;
        if (useWinUI?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
            return NativePlatform.WinUI;

        // Check for WPF
        if (sdk.Contains("Wpf", StringComparison.OrdinalIgnoreCase))
            return NativePlatform.WPF;

        var useWpf = doc.Descendants("UseWPF").FirstOrDefault()?.Value;
        if (useWpf?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
            return NativePlatform.WPF;

        // Try to detect from output type and target framework
        var targetFramework = doc.Descendants("TargetFramework").FirstOrDefault()?.Value ?? "";
        var targetFrameworks = doc.Descendants("TargetFrameworks").FirstOrDefault()?.Value ?? "";

        // MAUI typically targets multiple platforms
        if (targetFrameworks.Contains("android") || targetFrameworks.Contains("ios"))
            return NativePlatform.MAUI;

        return NativePlatform.Unknown;
    }

    private static string DetectRootNamespace(XDocument doc, string projectName)
    {
        var rootNamespace = doc.Descendants("RootNamespace").FirstOrDefault()?.Value;
        return rootNamespace ?? projectName;
    }
}
