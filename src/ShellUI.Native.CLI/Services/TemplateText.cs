namespace ShellUI.Native.CLI.Services;

// Turns template text into code for one project. The placeholder namespace becomes the project's
// root namespace plus the components folder: Components/UI → <App>.Components.UI, and
// Components/Native → <App>.Components.Native.
public static class TemplateText
{
    public const string DefaultComponentsPath = "Components/UI";

    // Used when the project also has ShellUI (Blazor), whose components live in Components/UI.
    public const string BesideBlazorComponentsPath = "Components/Native";

    public static string ComponentsNamespace(string rootNamespace, string componentsPath)
    {
        var folder = componentsPath.Replace('\\', '/').Trim('/').Replace('/', '.');
        return folder.Length == 0 ? rootNamespace : $"{rootNamespace}.{ProjectDetector.SafeNamespace(folder)}";
    }

    public static string Apply(string content, string rootNamespace, string componentsPath) =>
        content.Replace("YourProjectNamespace.Components.UI", ComponentsNamespace(rootNamespace, componentsPath))
               .Replace("YourProjectNamespace", rootNamespace);
}
