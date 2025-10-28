using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
        services.AddSingleton<IPublishWeatherReports, InmemoryQueuePublisher>()
            .AddSingleton<IRetrieveWeatherReport, RandomlyGeneratedWeatherReportRetriever>()
            .AddSingleton<IProvideOnDemandWeatherReport>(serviceProvider =>
                new OnDemandWeatherReportProvider(serviceProvider.GetService<IRetrieveWeatherReport>()));

        return services;
    }

    public static IEndpointRouteBuilder MapWeatherReportingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("api/weather-reports/{city}", (string city, IProvideOnDemandWeatherReport onDemandWeatherReportProvider) =>
        {
            var result = onDemandWeatherReportProvider.GetTodaysWeatherFor(city);
            return Results.Ok(result);
        });

        endpoints.MapPost("/publish/{city}", (string city, IProvideOnDemandWeatherReport onDemandWeatherReportProvider, IPublishWeatherReports weatherReportPublisher) =>
        {
            var weatherReport = onDemandWeatherReportProvider.GetTodaysWeatherFor(city);
            weatherReportPublisher.Publish(weatherReport);
            return Results.Accepted();
        });

        return endpoints;
    }
}