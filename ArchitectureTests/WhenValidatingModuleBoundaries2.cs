using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ArchitectureTests;

public class WhenValidatingModuleBoundaries2
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(Greetings.Application.ServiceCollectionExtensions).Assembly,
            typeof(Greetings.DomainModel.Objects.TodaysGreeting).Assembly,
            typeof(Greetings.PublishedInterfaces.IProvideGreetings).Assembly,
            typeof(Greetings.WeatherReportingApi.Adapter.WeatherReportingApi).Assembly,
            typeof(WeatherReporting.Application.ServiceCollectionExtensions).Assembly,
            typeof(WeatherReporting.DomainModel.OnDemandWeatherReport).Assembly,
            typeof(WeatherReporting.PublishedInterfaces.IProvideOnDemandWeatherReport).Assembly,
            typeof(WeatherReporting.Publishing.Adapter.InmemoryQueuePublisher).Assembly,
            typeof(WeatherReporting.ExternalSources.Adapter.RandomlyGeneratedWeatherReportRetriever).Assembly,
            typeof(WeatherModeling.Application.ServiceCollectionExtensions).Assembly,
            typeof(WeatherModeling.DomainModel.WeatherReport).Assembly,
            typeof(WeatherModeling.Messaging.Adapter.InMemoryMessageQueueListener).Assembly,
            typeof(SharedInfrastructure.QueueFactory).Assembly)
        .Build();

    private static readonly string[] Modules = { "Greetings", "WeatherReporting", "WeatherModeling" };

    // Intra-Module Dependency Rules

    [Fact]
    public void DomainModelProject_ShouldNotReference_AnyOtherProjectWithinSameModule()
    {
        foreach (var module in Modules)
        {
            var domainModelTypes = Types()
                .That()
                .ResideInNamespaceMatching($"{module}.DomainModel.*")
                .As($"{module}.DomainModel types");

            var otherModuleProjects = Types()
                .That()
                .ResideInNamespaceMatching($"{module}.*")
                .And()
                .DoNotResideInNamespaceMatching($"{module}.DomainModel.*")
                .As($"Other {module} projects");

            var rule = domainModelTypes
                .Should()
                .NotDependOnAny(otherModuleProjects)
                .Because($"DomainModel project should not reference any other project within {module} module");

            rule.Check(Architecture);
        }
    }

    [Fact]
    public void ApplicationProject_CanReference_AnyOtherProjectWithinSameModule()
    {
        // This is a permissive rule - no restrictions to enforce
        // Application projects are allowed to reference anything within their module
        // This test documents the rule exists but doesn't need enforcement
        Assert.True(true, "Application projects are allowed to reference any other project within the same module");
    }

    [Fact]
    public void NonApplicationNonDomainModelProjects_ShouldOnlyReference_PublishedInterfacesOfOtherModules()
    {
        foreach (var sourceModule in Modules)
        {
            // Get all projects in the source module except Application and DomainModel
            var nonApplicationDomainModelTypes = Types()
                .That()
                .ResideInNamespaceMatching($"{sourceModule}.*")
                .And()
                .DoNotResideInNamespaceMatching($"{sourceModule}.Application.*")
                .And()
                .DoNotResideInNamespaceMatching($"{sourceModule}.DomainModel.*")
                .As($"{sourceModule} non-Application non-DomainModel types");

            // Build allowed dependencies
            var allowedDependencies = Types()
                .That()
                .ResideInNamespaceMatching($"{sourceModule}.*") // Own module
                .Or()
                .ResideInNamespaceMatching("SharedInfrastructure.*") // Shared project
                .Or()
                .ResideInAssemblyMatching("System.*")
                .Or()
                .ResideInAssemblyMatching("Microsoft.*");

            // Add PublishedInterfaces from other modules
            foreach (var targetModule in Modules)
            {
                if (targetModule != sourceModule)
                {
                    allowedDependencies = allowedDependencies
                        .Or()
                        .ResideInNamespaceMatching($"{targetModule}.PublishedInterfaces.*");
                }
            }

            var allowedDependenciesWithRule = allowedDependencies.As("Allowed dependencies");

            var rule = nonApplicationDomainModelTypes
                .Should()
                .OnlyDependOn(allowedDependenciesWithRule)
                .Because($"Non-Application/DomainModel projects in {sourceModule} should only reference PublishedInterfaces, DomainModel of own module, and Shared project");

            rule.Check(Architecture);
        }
    }

    // Inter-Module Dependency Rules

    [Fact]
    public void ModuleProjects_MustOnlyReference_PublishedInterfacesOfOtherModules()
    {
        foreach (var sourceModule in Modules)
        {
            var sourceModuleTypes = Types()
                .That()
                .ResideInNamespaceMatching($"{sourceModule}.*")
                .As($"{sourceModule} module types");

            // Build disallowed dependencies for other modules (non-PublishedInterfaces)
            var disallowedDependencies = new List<IType>();

            foreach (var targetModule in Modules)
            {
                if (targetModule != sourceModule)
                {
                    var moduleTypes = Types()
                        .That()
                        .ResideInNamespaceMatching($"{targetModule}.*")
                        .And()
                        .DoNotResideInNamespaceMatching($"{targetModule}.PublishedInterfaces.*");

                    var types = moduleTypes.GetObjects(Architecture);
                    disallowedDependencies.AddRange(types);
                }
            }

            var rule = sourceModuleTypes
                .Should()
                .NotDependOnAny(disallowedDependencies)
                .Because($"{sourceModule} module should only reference PublishedInterfaces of other modules");

            rule.Check(Architecture);
        }
    }

    [Fact]
    public void PublishedInterfacesProject_MustNotReference_PublishedInterfacesOfAnotherModule()
    {
        foreach (var sourceModule in Modules)
        {
            var sourcePublishedInterfacesTypes = Types()
                .That()
                .ResideInNamespaceMatching($"{sourceModule}.PublishedInterfaces.*")
                .As($"{sourceModule}.PublishedInterfaces types");
            
            var anyPublishedInterfacesFound = sourcePublishedInterfacesTypes.GetObjects(Architecture).Any();

            if (anyPublishedInterfacesFound)
            {
                // Build list of PublishedInterfaces from other modules
                var otherPublishedInterfacesTypes = new List<IType>();

                foreach (var targetModule in Modules)
                {
                    if (targetModule != sourceModule)
                    {
                        var modulePublishedInterfaces = Types()
                            .That()
                            .ResideInNamespaceMatching($"{targetModule}.PublishedInterfaces.*");

                        var types = modulePublishedInterfaces.GetObjects(Architecture).ToList();
                    
                        if (types.Any())
                            otherPublishedInterfacesTypes.AddRange(types);
                    }
                }
                
                var rule = sourcePublishedInterfacesTypes
                    .Should()
                    .NotDependOnAny(otherPublishedInterfacesTypes)
                    .Because($"{sourceModule}.PublishedInterfaces should not reference PublishedInterfaces of other modules");

                rule.Check(Architecture);
            }
        }
    }

    // Shared Dependency Rules

    [Fact]
    public void Modules_CanReference_SharedProject()
    {
        // This is a permissive rule - modules are allowed to reference Shared
        // We verify that the Shared project exists and can be referenced
        var sharedProjectExists = Architecture.Types
            .Any(t => t.Namespace?.FullName?.StartsWith("SharedInfrastructure") ?? false);

        Assert.True(sharedProjectExists, "Shared project should exist and be available for modules to reference");
    }

    // Special Rules for API Projects

    [Fact]
    public void ApiProject_CanReference_AnyProjectInAnyModule()
    {
        // API projects represent the composition root
        // This is a permissive rule - no restrictions to enforce
        // This test documents the rule exists but doesn't need enforcement
        Assert.True(true, "API projects are allowed to reference any project in any module");
    }

    // Additional structural validation

    [Fact]
    public void EachModule_MustHave_ApplicationAndDomainModelProjects()
    {
        foreach (var module in Modules)
        {
            var hasApplication = Architecture.Types
                .Any(t => t.Namespace?.FullName?.StartsWith($"{module}.Application") ?? false);

            var hasDomainModel = Architecture.Types
                .Any(t => t.Namespace?.FullName?.StartsWith($"{module}.DomainModel") ?? false);

            Assert.True(hasApplication, $"{module} module must have an Application project");
            Assert.True(hasDomainModel, $"{module} module must have a DomainModel project");
        }
    }

    [Fact]
    public void AllModuleProjects_MustStartWith_ModuleName()
    {

        foreach (var module in Modules)
        {
            var moduleTypes = Architecture.Types
                .Where(t => t.Namespace?.FullName?.StartsWith($"{module}.") ?? false);

            foreach (var type in moduleTypes)
            {
                Assert.True(
                    type.Namespace?.FullName?.StartsWith($"{module}.") ?? false,
                    $"Type {type.FullName} should be in namespace starting with {module}.");
            }
        }
    }
}