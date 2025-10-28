using Microsoft.Extensions.DependencyInjection;
using WeatherReporting.DomainModel;
using WeatherReporting.ExternalSources.Adapter;
using WeatherReporting.PublishedInterfaces;
using WeatherReporting.Publishing.Adapter;

namespace WeatherReporting.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherReportingModule(this IServiceCollection services)
    {
        services
            .AddControllers().AddApplicationPart(typeof(ServiceCollectionExtensions).Assembly);
        
        services.AddSingleton<IPublishWeatherReports, InmemoryQueuePublisher>()
            .AddSingleton<IRetrieveWeatherReport, RandomlyGeneratedWeatherReportRetriever>()
            .AddSingleton<IProvideOnDemandWeatherReport>(serviceProvider => 
                new OnDemandWeatherReportProvider(serviceProvider.GetService<IRetrieveWeatherReport>()));

        return services;
    }
}