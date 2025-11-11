Create a Powershell script to generate the solution structure for a modular monolith as follows. 

1. The script shall be interactive and run in a loop until the user decides to quit.

2. First question the script shall ask the user is the name of the application they want to 
   create. 
3. Script shall then invoke `dotnet new sln` command to create a solution file with the chosen 
   name inside of a folder of the same name. If the folder doesn't exist, it will be created.
4. Script shall then invoke `dotnet new gitnignore` command to create a .gitignore file.
5. Script shall then invoke `git init` command to initialize a git repository in the root folder 

6. The script shall then ask the user the name of the modules they desire to create, in a loop. 
   At every iteration, the script shall ask the user if they want to create another module, as 
   long as the user presses "y" or "Y", the script will continue to ask for the module name. 
   Once the user presses "n" or "N", the script will stop asking for module names. 
   The script shall then invoke `dotnet new classlib -o {modulename}` command to create the following structure in the root folder:

| Component                                         | Type                     |
|---------------------------------------------------|--------------------------|
| {modulename}/src                                  | Folder                   |
| {modulename}/src/{modulename}.DomainModel         | C# Class Library Project |
| {modulename}/src/{modulename}.Application         | C# Class Library Project |
| {modulename}/src/{modulename}.PublishedInterfaces | C# Class Library Project |
| {modulename}/tests                                | Folder                   |
| {modulename}/tests/{modulename}.Application.Tests | C# xUnit Test Project    |

7. The script shall then invoke `dotnet add package` against the test projects to install the 
   following packages:
   8. Latest `Moq` package and
   9. Version 7.x of `FluentAssertions` package

10. The script shall then add these all these projects to the solution file using `dotnet sln add` 
   command.

11. The script shall then add an xUnit test project called `ArchitectureTests` in the root of 
    the solution, add references 
    to all module projects in the `src` folders and finally, install the 
    latest version of `TngTech.ArchUnitNET.xUnit` and `TngTech.ArchUnitNET` packages.

12. The script shall then run `dotnet build` command to validate that the solution compiles 
   without failure.

13. If there are no failures, the script should terminate with a success message 