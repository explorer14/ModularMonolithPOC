using Microsoft.Extensions.DependencyInjection;
using WeatherReporting.DomainModel;
using WeatherReporting.ExternalSources.Adapter;
using WeatherReporting.PublishedInterfaces;

namespace WeatherReporting.ApplicationServices;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherReportingModule(this IServiceCollection services)
    {
        services.AddSingleton<IRetrieveWeatherReport, RandomlyGeneratedWeatherReportRetriever>();
        services.AddSingleton<IProvideOnDemandWeatherReport>(serviceProvider => 
            new OnDemandWeatherReportProvider(serviceProvider.GetService<IRetrieveWeatherReport>()));

        return services;
    }
}