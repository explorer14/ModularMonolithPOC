I want to auto generate these architecture tests using a source generator, however, source generators cannot scan the whole solution for module projects. Since I am using ArchUnitNet for these tests, in order to build the `Architecture` object to apply the rules against, it needs to load module assemblies using a public type in each project by doing `typeof(SomePublicType).Assembly`. However, since source generators cannot scan any code outside of the project they are referenced in, they also cannot emit the name of the type to load the assembly for.

So let's build this in 3 parts:

## Part 1:

Modify the `mod-mono-gen.ps1` script to also create a dummy public class called `DoNotDelete.cs` in each module project during the creation of module projects.

## Part 2:

Create a `DiscoverModules.targets` file in the `ArchitectureTests` project that can scan the solution for all the module projects and output the list of module projects (including their namespaces) into a file called `modules.mod`. This output should be added to the `AdditionalFiles` section of the `ArchitectureTests` project bootstrapped by the powershell script.

## Part 3:

Write a incremental source generator that can read this additional file `modules.mod` and string replace project names into the test code templates for each scenario. For loading the architecture assemblies, the generator can simply use the `DoNoDelete` class created in Part 1 by doing `typeof(<module-project-name>.DoNotDelete).Assembly`.

To start with only implement part 1, I will tell you when to implement the rest.