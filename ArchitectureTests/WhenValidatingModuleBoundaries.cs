using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ArchitectureTests;

public class WhenValidatingModuleBoundaries
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
            typeof(WeatherModeling.Messaging.Adapter.InMemoryMessageQueueListener).Assembly)
        .Build();

    [Fact]
    public void GreetingsModuleShouldOnlyDependOnPublishedInterfacesOfWeatherReportingModule()
    {
        var greetingsTypes = Types()
            .That()
            .ResideInNamespaceMatching("Greetings.*")
            .As("Greetings module types");
        
        var allowedDependenciesForGreetingsModule = Types()
            .That()
            .ResideInAssembly(typeof(WeatherReporting.PublishedInterfaces.IProvideOnDemandWeatherReport).Assembly)
            .Or()
            .ResideInAssemblyMatching("System.*")
            .Or()
            .ResideInAssemblyMatching("Microsoft.*")
            .Or()
            .ResideInNamespaceMatching("Greetings.*")
            .As("Allowed dependencies");

        var disallowedDependenciesForGreetingsModule = Types().That()
            .ResideInAssembly(typeof(WeatherReporting.DomainModel.IRetrieveWeatherReport).Assembly)
            .Or()
            .ResideInAssembly(typeof(WeatherReporting.Application.ServiceCollectionExtensions).Assembly)
            .Or()
            .ResideInAssembly(typeof(WeatherReporting.ExternalSources.Adapter.RandomlyGeneratedWeatherReportRetriever).Assembly)
            .Or()
            .ResideInAssembly(typeof(WeatherReporting.Publishing.Adapter.InmemoryQueuePublisher).Assembly)
            .As("Disallowed dependencies");
        
        var rule = greetingsTypes
            .Should()
            .NotDependOnAny(
                disallowedDependenciesForGreetingsModule)
            .AndShould()
            .OnlyDependOn(allowedDependenciesForGreetingsModule);

        rule.Check(Architecture);
    }
}