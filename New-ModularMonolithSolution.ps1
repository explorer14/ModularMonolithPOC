# Modular Monolith Solution Generator
# This script creates a modular monolith solution structure with interactive prompts

function New-ModularMonolithSolution {
    Write-Host "=== Modular Monolith Solution Generator ===" -ForegroundColor Cyan
    Write-Host ""

    # Ask for application name
    $appName = Read-Host "Enter the name of the application"
    if ([string]::IsNullOrWhiteSpace($appName)) {
        Write-Host "Application name cannot be empty. Exiting." -ForegroundColor Red
        return
    }

    # Create solution folder and navigate to it
    $solutionPath = Join-Path (Get-Location) $appName
    if (-not (Test-Path $solutionPath)) {
        New-Item -Path $solutionPath -ItemType Directory | Out-Null
        Write-Host "Created folder: $solutionPath" -ForegroundColor Green
    }

    Set-Location $solutionPath

    # Create solution file
    Write-Host "Creating solution file..." -ForegroundColor Yellow
    dotnet new sln --name $appName
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to create solution file." -ForegroundColor Red
        return
    }

    # Create .gitignore
    Write-Host "Creating .gitignore file..." -ForegroundColor Yellow
    dotnet new gitignore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to create .gitignore file." -ForegroundColor Red
    }

    # Initialize git repository
    Write-Host "Initializing git repository..." -ForegroundColor Yellow
    git init
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to initialize git repository." -ForegroundColor Red
    }

    # Collect module names
    $modules = @()
    do {
        Write-Host ""
        $moduleName = Read-Host "Enter the name of a module (or press Enter to skip)"
        if (-not [string]::IsNullOrWhiteSpace($moduleName)) {
            $modules += $moduleName
            Write-Host "Module '$moduleName' added to the list." -ForegroundColor Green
        }

        if ($modules.Count -gt 0) {
            $continue = Read-Host "Do you want to create another module? (y/n)"
        } else {
            $continue = "y"
        }
    } while ($continue -eq "y" -or $continue -eq "Y")

    if ($modules.Count -eq 0) {
        Write-Host "No modules to create. Exiting." -ForegroundColor Yellow
        return
    }

    # Create modules
    foreach ($moduleName in $modules) {
        Write-Host ""
        Write-Host "Creating module: $moduleName" -ForegroundColor Cyan

        # Create module folder structure
        $modulePath = Join-Path (Get-Location) $moduleName
        $srcPath = Join-Path $modulePath "src"
        $testsPath = Join-Path $modulePath "tests"

        New-Item -Path $srcPath -ItemType Directory -Force | Out-Null
        New-Item -Path $testsPath -ItemType Directory -Force | Out-Null

        # Create DomainModel project
        Write-Host "  Creating $moduleName.DomainModel..." -ForegroundColor Yellow
        $domainModelPath = Join-Path $srcPath "$moduleName.DomainModel"
        dotnet new classlib -o $domainModelPath

        # Create Application project
        Write-Host "  Creating $moduleName.Application..." -ForegroundColor Yellow
        $applicationPath = Join-Path $srcPath "$moduleName.Application"
        dotnet new classlib -o $applicationPath

        # Create PublishedInterfaces project
        Write-Host "  Creating $moduleName.PublishedInterfaces..." -ForegroundColor Yellow
        $publishedInterfacesPath = Join-Path $srcPath "$moduleName.PublishedInterfaces"
        dotnet new classlib -o $publishedInterfacesPath

        # Create Application.Tests project
        Write-Host "  Creating $moduleName.Application.Tests..." -ForegroundColor Yellow
        $testsProjectPath = Join-Path $testsPath "$moduleName.Application.Tests"
        dotnet new xunit -o $testsProjectPath

        # Add NuGet packages to test project
        Write-Host "  Adding Moq package to test project..." -ForegroundColor Yellow
        dotnet add $testsProjectPath package Moq

        Write-Host "  Adding FluentAssertions package to test project..." -ForegroundColor Yellow
        dotnet add $testsProjectPath package FluentAssertions --version "7.*"

        # Add projects to solution
        Write-Host "  Adding projects to solution..." -ForegroundColor Yellow
        dotnet sln add $domainModelPath
        dotnet sln add $applicationPath
        dotnet sln add $publishedInterfacesPath
        dotnet sln add $testsProjectPath
    }

    # Build solution
    Write-Host ""
    Write-Host "Building solution to validate..." -ForegroundColor Cyan
    dotnet build

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "Solution created successfully!" -ForegroundColor Green
        Write-Host "Location: $solutionPath" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "Solution created but build failed. Please review the errors above." -ForegroundColor Red
    }
}

# Main loop
do {
    New-ModularMonolithSolution
    Write-Host ""
    $continue = Read-Host "Do you want to create another solution? (y/n)"
} while ($continue -eq "y" -or $continue -eq "Y")

Write-Host ""
Write-Host "Thank you for using Modular Monolith Solution Generator!" -ForegroundColor Cyan
