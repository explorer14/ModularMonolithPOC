# Modular Monolith Solution Generator Script
# This script generates a modular monolith solution structure

param()

Write-Host "=== Modular Monolith Solution Generator ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Ask for application name
$appName = Read-Host "Enter the name of your application"

if ([string]::IsNullOrWhiteSpace($appName)) {
    Write-Host "Application name cannot be empty. Exiting." -ForegroundColor Red
    exit 1
}

# Step 2: Create solution folder and solution file
Write-Host ""
Write-Host "Creating solution folder and solution file..." -ForegroundColor Yellow

if (-not (Test-Path $appName)) {
    New-Item -ItemType Directory -Path $appName | Out-Null
    Write-Host "Created folder: $appName" -ForegroundColor Green
}

Set-Location $appName

# Create solution file
dotnet new sln -n $appName
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to create solution file." -ForegroundColor Red
    exit 1
}

# Step 3: Create .gitignore
Write-Host ""
Write-Host "Creating .gitignore file..." -ForegroundColor Yellow
dotnet new gitignore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to create .gitignore file." -ForegroundColor Red
    exit 1
}

# Step 4: Initialize git repository
Write-Host ""
Write-Host "Initializing git repository..." -ForegroundColor Yellow
git init
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to initialize git repository." -ForegroundColor Red
    exit 1
}

# Step 5: Ask for module names in a loop
$modules = @()
$continue = $true

Write-Host ""
Write-Host "=== Module Creation ===" -ForegroundColor Cyan

while ($continue) {
    Write-Host ""
    $moduleName = Read-Host "Enter the name of the module you want to create"

    if ([string]::IsNullOrWhiteSpace($moduleName)) {
        Write-Host "Module name cannot be empty." -ForegroundColor Red
    }
    else {
        $modules += $moduleName
        Write-Host "Module '$moduleName' added to the list." -ForegroundColor Green
    }

    Write-Host ""
    $response = Read-Host "Do you want to create another module? (y/n)"

    if ($response -ne "y" -and $response -ne "Y") {
        $continue = $false
    }
}

if ($modules.Count -eq 0) {
    Write-Host "No modules to create. Exiting." -ForegroundColor Red
    exit 1
}

# Step 6: Create module structure for each module
Write-Host ""
Write-Host "=== Creating Module Projects ===" -ForegroundColor Cyan

foreach ($moduleName in $modules) {
    Write-Host ""
    Write-Host "Creating module: $moduleName" -ForegroundColor Yellow

    # Create module folder structure
    $moduleRoot = $moduleName
    $srcFolder = Join-Path $moduleRoot "src"
    $testsFolder = Join-Path $moduleRoot "tests"

    New-Item -ItemType Directory -Path $srcFolder -Force | Out-Null
    New-Item -ItemType Directory -Path $testsFolder -Force | Out-Null

    # Create class library projects in src folder
    $domainModelProject = Join-Path $srcFolder "$moduleName.DomainModel"
    $applicationProject = Join-Path $srcFolder "$moduleName.Application"
    $publishedInterfacesProject = Join-Path $srcFolder "$moduleName.PublishedInterfaces"

    Write-Host "  Creating $moduleName.DomainModel..." -ForegroundColor Gray
    dotnet new classlib -o $domainModelProject

    Write-Host "  Creating $moduleName.Application..." -ForegroundColor Gray
    dotnet new classlib -o $applicationProject

    Write-Host "  Creating $moduleName.PublishedInterfaces..." -ForegroundColor Gray
    dotnet new classlib -o $publishedInterfacesProject

    # Create test project in tests folder
    $testProject = Join-Path $testsFolder "$moduleName.Application.Tests"

    Write-Host "  Creating $moduleName.Application.Tests..." -ForegroundColor Gray
    dotnet new xunit -o $testProject

    # Step 7: Install test packages
    Write-Host "  Installing test packages..." -ForegroundColor Gray
    dotnet add $testProject package Moq
    dotnet add $testProject package FluentAssertions --version "7.*"

    # Step 8: Add projects to solution
    Write-Host "  Adding projects to solution..." -ForegroundColor Gray
    dotnet sln add $domainModelProject
    dotnet sln add $applicationProject
    dotnet sln add $publishedInterfacesProject
    dotnet sln add $testProject

    Write-Host "Module $moduleName created successfully!" -ForegroundColor Green
}

# Step 9: Create ArchitectureTests project
Write-Host ""
Write-Host "=== Creating ArchitectureTests Project ===" -ForegroundColor Cyan

$archTestProject = "ArchitectureTests"
dotnet new xunit -o $archTestProject
Write-Host "Created ArchitectureTests project." -ForegroundColor Green

# Add references to all module projects
Write-Host "Adding references to module projects..." -ForegroundColor Yellow
foreach ($moduleName in $modules) {
    $domainModelProject = Join-Path $moduleName "src/$moduleName.DomainModel/$moduleName.DomainModel.csproj"
    $applicationProject = Join-Path $moduleName "src/$moduleName.Application/$moduleName.Application.csproj"
    $publishedInterfacesProject = Join-Path $moduleName "src/$moduleName.PublishedInterfaces/$moduleName.PublishedInterfaces.csproj"

    dotnet add $archTestProject reference $domainModelProject
    dotnet add $archTestProject reference $applicationProject
    dotnet add $archTestProject reference $publishedInterfacesProject
}

# Install ArchUnit packages
Write-Host "Installing ArchUnit packages..." -ForegroundColor Yellow
dotnet add $archTestProject package TngTech.ArchUnitNET.xUnit
dotnet add $archTestProject package TngTech.ArchUnitNET

# Add ArchitectureTests to solution
dotnet sln add $archTestProject

Write-Host "ArchitectureTests project created successfully!" -ForegroundColor Green

# Step 10: Build solution to validate
Write-Host ""
Write-Host "=== Building Solution ===" -ForegroundColor Cyan
Write-Host "Running 'dotnet build' to validate solution..." -ForegroundColor Yellow

dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "SUCCESS! Solution created and built successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Solution location: $(Get-Location)" -ForegroundColor Cyan
    Write-Host "Modules created: $($modules -join ', ')" -ForegroundColor Cyan
}
else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "ERROR: Solution build failed!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "Please review the errors above and fix them manually." -ForegroundColor Yellow
    exit 1
}
