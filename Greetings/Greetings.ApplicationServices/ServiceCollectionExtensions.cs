using Greetings.DomainModel.Ports;
using Greetings.PublishedInterfaces;
using Greetings.Storage.Adapter;
using Greetings.WeatherReporting.Adapter;
using Microsoft.Extensions.DependencyInjection;
using WeatherReporting.PublishedInterfaces;

namespace Greetings.ApplicationServices;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGreetingsModuleWithWeatherReportingApi(this IServiceCollection services)
    {
        services.AddSingleton<IProvideOnDemandWeatherReport, WeatherReportingApi>();
        return AddServices(services);
    }

    public static IServiceCollection AddGreetingsModule(this IServiceCollection services) => 
        AddServices(services);

    private static IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddSingleton<IRetrieveGreetings, InmemoryGreetingsRetriever>();
        services.AddSingleton<IProvideGreetings>(serviceProvider => 
            new GreetingsProvider(serviceProvider.GetService<IRetrieveGreetings>(),
                serviceProvider.GetService<IProvideOnDemandWeatherReport>()));
        
        return services;
    }
}