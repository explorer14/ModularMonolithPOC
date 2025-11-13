# CreateModule.ps1
# Usage: .\CreateModule.ps1 -ModuleName "YourModuleName"

param(
    [Parameter(Mandatory=$true)]
    [string]$ModuleName
)

$ErrorActionPreference = "Stop"

Write-Host "Creating module: $ModuleName" -ForegroundColor Cyan

# Get script directory (solution root)
$solutionDir = $PSScriptRoot

# Create folder structure
$moduleDir = Join-Path $solutionDir $ModuleName
$srcDir = Join-Path $moduleDir "src"

Write-Host "Creating directories..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path $srcDir -Force | Out-Null

# Define projects
$projects = @("Application", "DomainModel", "PublishedInterfaces")

foreach ($project in $projects) {
    $projectName = "$ModuleName.$project"
    $projectPath = Join-Path $srcDir $projectName

    Write-Host "Creating project: $projectName" -ForegroundColor Yellow

    # Use dotnet CLI to create classlib project
    dotnet new classlib -n $projectName -o $projectPath -f net9.0 | Out-Null

    # Enable ImplicitUsings and Nullable in the csproj
    $csprojPath = Join-Path $projectPath "$projectName.csproj"
    $csprojContent = Get-Content -Path $csprojPath -Raw

    # Add ImplicitUsings and Nullable if not present
    if ($csprojContent -notmatch "ImplicitUsings") {
        $csprojContent = $csprojContent -replace "(<TargetFramework>net9.0</TargetFramework>)", "`$1`n    <ImplicitUsings>enable</ImplicitUsings>`n    <Nullable>enable</Nullable>"
        Set-Content -Path $csprojPath -Value $csprojContent
    }

    # Remove default Class1.cs if it exists
    $class1Path = Join-Path $projectPath "Class1.cs"
    if (Test-Path $class1Path) {
        Remove-Item $class1Path -Force
    }

    # Create DoNotDelete.cs
    $doNotDeletePath = Join-Path $projectPath "DoNotDelete.cs"
    $doNotDeleteContent = @"
namespace $projectName;

// This file is here to ensure the project compiles
public class DoNotDelete
{
}
"@
    Set-Content -Path $doNotDeletePath -Value $doNotDeleteContent
}

# Add projects to solution
# Find the first .sln file in the solution directory
$solutionFiles = Get-ChildItem -Path $solutionDir -Filter "*.sln" | Select-Object -First 1

if ($solutionFiles) {
    $solutionFile = $solutionFiles.FullName
    Write-Host "Found solution file: $($solutionFiles.Name)" -ForegroundColor Gray
    Write-Host "Adding projects to solution..." -ForegroundColor Yellow

    foreach ($project in $projects) {
        $projectName = "$ModuleName.$project"
        $projectPath = Join-Path $srcDir "$projectName\$projectName.csproj"

        Write-Host "  Adding $projectName to solution" -ForegroundColor Gray
        dotnet sln $solutionFile add $projectPath --solution-folder "$ModuleName/src" | Out-Null
    }

    Write-Host "Projects added to solution successfully" -ForegroundColor Green
}
else {
    Write-Host "Warning: No solution file found in the directory. Projects were not added to a solution." -ForegroundColor Yellow
}

# Add references to ArchitectureTests using dotnet CLI
$archTestsProjectPath = Join-Path $solutionDir "ArchitectureTests\ArchitectureTests.csproj"

if (Test-Path $archTestsProjectPath) {
    Write-Host "Adding project references to ArchitectureTests..." -ForegroundColor Yellow

    foreach ($project in $projects) {
        $projectName = "$ModuleName.$project"
        $projectPath = Join-Path $srcDir "$projectName\$projectName.csproj"

        Write-Host "  Adding reference to $projectName" -ForegroundColor Gray
        dotnet add $archTestsProjectPath reference $projectPath | Out-Null
    }

    Write-Host "ArchitectureTests project updated successfully" -ForegroundColor Green
}

Write-Host ""
Write-Host "Module '$ModuleName' created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Created structure:" -ForegroundColor Cyan
Write-Host "  $ModuleName/" -ForegroundColor White
Write-Host "    src/" -ForegroundColor White
Write-Host "      $ModuleName.Application/" -ForegroundColor White
Write-Host "      $ModuleName.DomainModel/" -ForegroundColor White
Write-Host "      $ModuleName.PublishedInterfaces/" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Reload the solution in Rider (File -> Reload All Projects)" -ForegroundColor White
Write-Host "  2. Add the new projects to the solution if needed" -ForegroundColor White
