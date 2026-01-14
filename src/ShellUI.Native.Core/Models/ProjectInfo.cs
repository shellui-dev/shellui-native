namespace ShellUI.Native.Core.Models;

// Information about the detected project
public class ProjectInfo
{
    public required string ProjectPath { get; set; }
    public required string ProjectName { get; set; }
    public required string RootNamespace { get; set; }
    public NativePlatform Platform { get; set; }
}
