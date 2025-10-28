using Greetings.DomainModel.Ports;
using Greetings.PublishedInterfaces;
using Greetings.Storage.Adapter;
using Greetings.WeatherReporting.Adapter;
using Microsoft.Extensions.DependencyInjection;
using WeatherReporting.PublishedInterfaces;

namespace Greetings.ApplicationServices;

public static class ServiceCollectionExtensions
{
    // TODO: can DI be made smarter here so it can auto-detect if it needs to load
    // in-proc/lib dependency or out-of-proc/http dependency?
    public static IServiceCollection AddGreetingsModuleWithWeatherReportingApi(this IServiceCollection services)
    {
        services.AddSingleton<IProvideOnDemandWeatherReport, WeatherReportingApi>();
        return AddServices(services);
    }

    public static IServiceCollection AddGreetingsModule(this IServiceCollection services) => 
        AddServices(services);

    private static IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddControllers().AddApplicationPart(typeof(ServiceCollectionExtensions).Assembly);
        services.AddSingleton<IRetrieveGreetings, InmemoryGreetingsRetriever>();
        services.AddSingleton<IProvideGreetings>(serviceProvider => 
            new GreetingsProvider(greetingsRetriever: serviceProvider.GetService<IRetrieveGreetings>()!,
                onDemandWeatherReportProvider: serviceProvider.GetService<IProvideOnDemandWeatherReport>()));
        
        return services;
    }
}