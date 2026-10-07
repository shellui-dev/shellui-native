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

# Build and test (the MAUI demo is left out; it needs the MAUI workload)
Write-Host "Building and testing..." -ForegroundColor Cyan
dotnet test (Join-Path $PSScriptRoot "tests/ShellUI.Native.Tests/ShellUI.Native.Tests.csproj") -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build or tests failed!" -ForegroundColor Red
    exit 1
}

# Only the CLI is published; Core and Templates ship inside the tool package
Write-Host "Packing the CLI..." -ForegroundColor Cyan
dotnet pack (Join-Path $PSScriptRoot "src/ShellUI.Native.CLI/ShellUI.Native.CLI.csproj") -c Release -o (Join-Path $PSScriptRoot "nupkg")

if ($LASTEXITCODE -ne 0) {
    Write-Host "Pack failed!" -ForegroundColor Red
    exit 1
}

$fullVersion = if ($Suffix) { "$Version-$Suffix" } else { $Version }
Write-Host ""
Write-Host "Release preparation complete! Package: ./nupkg" -ForegroundColor Green
Write-Host ""
Write-Host "Next:" -ForegroundColor Yellow
Write-Host "  1. Add a '# ShellUI Native v$fullVersion' section to docs/RELEASE_NOTES.md"
Write-Host "  2. Merge to main, then tag it; the Release workflow publishes to NuGet:"
Write-Host "     git tag -a v$fullVersion -m `"ShellUI Native v$fullVersion`""
Write-Host "     git push origin v$fullVersion"
