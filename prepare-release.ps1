# ShellUI Native Release Preparation Script
# Updates version in Directory.Build.props and prepares for release

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [string]$Suffix = ""
)

$ErrorActionPreference = "Stop"

Write-Host "Preparing ShellUI Native release v$Version" -ForegroundColor Cyan

# Update Directory.Build.props
$propsPath = Join-Path $PSScriptRoot "Directory.Build.props"
$content = Get-Content $propsPath -Raw

$content = $content -replace '<ShellUINativeVersion>[^<]+</ShellUINativeVersion>', "<ShellUINativeVersion>$Version</ShellUINativeVersion>"
$content = $content -replace '<ShellUINativeVersionSuffix>[^<]*</ShellUINativeVersionSuffix>', "<ShellUINativeVersionSuffix>$Suffix</ShellUINativeVersionSuffix>"

Set-Content $propsPath $content
Write-Host "Updated Directory.Build.props to version $Version" -ForegroundColor Green

# Build all projects
Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build successful!" -ForegroundColor Green

# Pack NuGet packages
Write-Host "Creating NuGet packages..." -ForegroundColor Cyan

$projects = @(
    "src/ShellUI.Native.Core/ShellUI.Native.Core.csproj",
    "src/ShellUI.Native.Templates/ShellUI.Native.Templates.csproj",
    "src/ShellUI.Native.CLI/ShellUI.Native.CLI.csproj"
)

foreach ($project in $projects) {
    $projectPath = Join-Path $PSScriptRoot $project
    Write-Host "Packing $project..." -ForegroundColor Yellow
    dotnet pack $projectPath -c Release -o ./nupkg
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Pack failed for $project!" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "Release preparation complete!" -ForegroundColor Green
Write-Host "NuGet packages are in ./nupkg" -ForegroundColor Cyan
Write-Host ""
Write-Host "To publish:" -ForegroundColor Yellow
Write-Host "  dotnet nuget push ./nupkg/*.nupkg -s https://api.nuget.org/v3/index.json -k YOUR_API_KEY"
