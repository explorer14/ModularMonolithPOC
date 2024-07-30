using Microsoft.Extensions.DependencyInjection;
using WeatherReporting.DomainModel;
using WeatherReporting.ExternalSources.Adapter;
using WeatherReporting.PublishedInterfaces;
using WeatherReporting.Publishing.Adapter;

namespace WeatherReporting.ApplicationServices;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherReportingModule(this IServiceCollection services)
    {
        services.AddSingleton<IPublishWeatherReports, InmemoryQueuePublisher>();
        services.AddSingleton<IRetrieveWeatherReport, RandomlyGeneratedWeatherReportRetriever>();
        services.AddSingleton<IProvideOnDemandWeatherReport>(serviceProvider => 
            new OnDemandWeatherReportProvider(serviceProvider.GetService<IRetrieveWeatherReport>()));

        return services;
    }
}