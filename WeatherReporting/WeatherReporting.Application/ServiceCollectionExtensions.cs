using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WeatherReporting.DomainModel;
using WeatherReporting.ExternalSources.Adapter;
using WeatherReporting.PublishedInterfaces;
using WeatherReporting.Publishing.Adapter;
using WeatherReporting.Storage.Adapter;

namespace WeatherReporting.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherReportingModule(this IServiceCollection services)
    {
        services.AddSingleton<IPublishWeatherReports, InmemoryQueuePublisher>()
            .AddSingleton<IRetrieveWeatherReport, RandomlyGeneratedWeatherReportRetriever>()
            .AddSingleton<IProvideOnDemandWeatherReport>(serviceProvider =>
                new OnDemandWeatherReportProvider(serviceProvider.GetService<IRetrieveWeatherReport>()))
            .AddSingleton<IWeatherReportRepository, MySqlWeatherReportRepository>();

        return services;
    }

    public static IEndpointRouteBuilder MapWeatherReportingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var tag = "weather-reports";
        var routeBase = $"api/{tag}";
        endpoints.MapGet($"{routeBase}/{{city}}", (
            string city,
            IProvideOnDemandWeatherReport onDemandWeatherReportProvider
            ) =>
        {
            var result = onDemandWeatherReportProvider.GetTodaysWeatherFor(city);
            return Results.Ok(result);
        })
        .Produces<PublishedInterfaces.OnDemandWeatherReport>(StatusCodes.Status200OK)
        .Produces<PublishedInterfaces.OnDemandWeatherReport>(StatusCodes.Status500InternalServerError)
        .WithTags(tag);

        endpoints.MapPost($"{routeBase}/publish/{{city}}", (
            string city,
            IProvideOnDemandWeatherReport onDemandWeatherReportProvider,
            IPublishWeatherReports weatherReportPublisher
            ) =>
        {
            var weatherReport = onDemandWeatherReportProvider.GetTodaysWeatherFor(city);
            weatherReportPublisher.Publish(weatherReport);
            return Results.Accepted();
        })
        .Produces<PublishedInterfaces.OnDemandWeatherReport>(StatusCodes.Status200OK)
        .Produces<PublishedInterfaces.OnDemandWeatherReport>(StatusCodes.Status500InternalServerError)
        .WithTags(tag);

        return endpoints;
    }
}