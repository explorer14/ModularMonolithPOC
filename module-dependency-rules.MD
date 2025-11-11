## Agent Prompt
Implement the following module dependency rules as architecture tests in `UnitTest1` using 
the `ArchUnitNet` package that's already installed. Write one test per rule and run the architecture 
tests to ensure they all pass.

## General Rules

1. A module is a namespace folder that is named for the business capability the underlying code supports
2. Each module namespace MUST have two sub-folders: `src` for all the source code for that module
   and `tests` for all the test code for that module
3. All projects within a module namespace MUST start with the namespace folder name.
4. Every module MUST have atleast two class library projects in the `src` folder: `<module-name>.
Application` and `<module-name>.DomainModel` 
5. A module CAN only have zero or one `<module-name>.PublishedInterfaces` project.
6. The `PublishedInterfaces` project contains all the types and interfaces meant to be used 
   across module boundaries.
7. A module CAN have zero or more `<module-name>.<function>.Adapter` projects
8. The adapter projects interact with the outside world and translate the data types to and from 
   the published interfaces.

## Intra-Module Dependency Rules

Within a module the following rules apply:

1. DomainModel project is not allowed to reference any other project within the same module
2. Application project is allowed to reference any other project within the same module
3. Any other module project is only allowed to reference the following kinds of projects:
   4. `PublishedInterfaces` project of another module 
   5. `DomainModel` project of its own module.
   6. `Shared` project

## Inter-Module Dependency Rules

Between modules the following rules apply:

1. A module project MUST only reference `PublishedInterfaces` project of other modules.
2. `PublishedInterfaces` project of one module MUST not reference `PublishedInterfaces` project 
   of another module.

## Shared Dependency Rules

1. Two or more modules CAN share common code by placing it in a `Shared` project in the root of 
   the repository folder.
2. Shared code SHALL be limited to infrastructure code or generically used code

## External Dependency Rules

1. A module CAN reference any external NuGet package.
2. If two or more modules reference the same external NuGet package, they MUST reference the 
   package independently. Different package versions are allowed though keeping them in sync is 
   encouraged.

## Special Rules for API Projects

API projects represent the composition root of the overall modular monolith system.

1. API project is allowed to reference any project in any module.

## Special Rules for Test Projects

Tests can exist at both module levels and root level.

1. Test projects in a module are allowed to reference any projects in that module.
2. Test projects in the root are allowed to reference only non-module projects.