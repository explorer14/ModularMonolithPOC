param(
    [string] $ModuleName,
    [string] $SolutionNameToAddTo
)

$SolutionPath = Join-Path -Path $PSScriptRoot -ChildPath "$SolutionNameToAddTo.sln"

New-Item -Path $PSScriptRoot -ItemType "directory" -Name $ModuleName
Set-Location -Path $PSScriptRoot\$ModuleName
Invoke-Expression -Command "dotnet new classlib -n $ModuleName.DomainModel"
Invoke-Expression -Command "dotnet sln $SolutionPath add $PSScriptRoot\$ModuleName\$ModuleName.DomainModel\$ModuleName.DomainModel.csproj"
Invoke-Expression -Command "dotnet new classlib -n $ModuleName.ApplicationServices"
Invoke-Expression -Command "dotnet sln $SolutionPath add $PSScriptRoot\$ModuleName\$ModuleName.ApplicationServices\$ModuleName.ApplicationServices.csproj"
Invoke-Expression -Command "dotnet new classlib -n $ModuleName.PublishedInterfaces"
Invoke-Expression -Command "dotnet sln $SolutionPath add $PSScriptRoot\$ModuleName\$ModuleName.PublishedInterfaces\$ModuleName.PublishedInterfaces.csproj"

