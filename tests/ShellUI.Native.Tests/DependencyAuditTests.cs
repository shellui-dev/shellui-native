using System.Text.RegularExpressions;
using ShellUI.Native.Core.Models;
using ShellUI.Native.Templates;

namespace ShellUI.Native.Tests;

// A component installed on its own has to build, so every type it uses from another template
// must come from its declared dependencies (directly or through them), its family (adding a part
// installs the whole family) or shell (init always installs it).
public class DependencyAuditTests
{
    private static readonly NativePlatform[] Platforms = { NativePlatform.MAUI, NativePlatform.Avalonia };

    private static readonly Regex Declaration = new(
        @"^\s*(?:public|internal)\s+(?:static\s+|sealed\s+|abstract\s+|partial\s+)*(?:class|enum|interface|record|struct)\s+(\w+)",
        RegexOptions.Multiline);

    private static readonly Regex CommentsAndStrings = new(@"//[^\n]*|/\*.*?\*/|""(?:[^""\\\n]|\\.)*""", RegexOptions.Singleline);

    // Places where a name can only be a type: new X(…), X name =, <X>, is / as X, (X)cast, : Base, typeof(X).
    private static readonly Regex[] TypePositions =
    {
        new(@"\bnew\s+([A-Z]\w*)\s*[({<]"),
        new(@"(?<![.\w])([A-Z]\w*)(?:\?|\[\])?\s+[a-z_]\w*\s*(?:[=;,)]|=>)"),
        new(@"<\s*([A-Z]\w*)\s*[>,]"),
        new(@",\s*([A-Z]\w*)\s*>"),
        new(@"\b(?:is|as)\s+([A-Z]\w*)\b"),
        new(@"\(\s*([A-Z]\w*)\s*\)\s*[\w(]"),
        new(@"(?:class|record|struct|interface)\s+\w+(?:<[^>]*>)?\s*:\s*([A-Z]\w*)"),
        new(@"typeof\(\s*([A-Z]\w*)\s*\)"),
    };

    public static IEnumerable<object[]> Cases() =>
        from name in ComponentRegistry.Components.Keys
        from platform in Platforms
        select new object[] { name, platform };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Types_from_other_templates_come_from_dependencies(string name, NativePlatform platform)
    {
        var owners = TypeOwners(platform);
        var allowed = new HashSet<string> { name, "shell" };
        AddClosure(name, allowed);
        if (ComponentRegistry.GetFamily(name) is { } family)
        {
            allowed.Add(family);
            AddClosure(family, allowed);
        }

        var code = CommentsAndStrings.Replace(ComponentRegistry.GetComponentContent(name, platform) ?? string.Empty, string.Empty);
        var missing = TypePositions
            .SelectMany(pattern => pattern.Matches(code).Select(m => m.Groups[1].Value))
            .Distinct()
            .Where(type => owners.TryGetValue(type, out var from) && !from.Overlaps(allowed))
            .Select(type => $"{type} (from {string.Join(", ", owners[type])})")
            .ToList();

        Assert.True(missing.Count == 0,
            $"{platform} '{name}' uses {string.Join("; ", missing)} without depending on it.");
    }

    // Which templates declare each type, per platform.
    private static Dictionary<string, HashSet<string>> TypeOwners(NativePlatform platform)
    {
        var owners = new Dictionary<string, HashSet<string>>();
        foreach (var name in ComponentRegistry.Components.Keys)
        {
            var content = ComponentRegistry.GetComponentContent(name, platform) ?? string.Empty;
            foreach (Match m in Declaration.Matches(content))
            {
                if (!owners.TryGetValue(m.Groups[1].Value, out var set))
                    owners[m.Groups[1].Value] = set = new HashSet<string>();
                set.Add(name);
            }
        }
        return owners;
    }

    private static void AddClosure(string name, HashSet<string> into)
    {
        foreach (var dependency in ComponentRegistry.GetMetadata(name)?.Dependencies ?? new List<string>())
            if (into.Add(dependency))
                AddClosure(dependency, into);
    }
}
