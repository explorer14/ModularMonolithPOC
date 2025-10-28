using Greetings.DomainModel.Ports;
using Greetings.PublishedInterfaces;
using Greetings.Storage.Adapter;
using Greetings.WeatherReporting.Adapter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WeatherReporting.PublishedInterfaces;

namespace Greetings.Application;

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
        services.AddSingleton<IRetrieveGreetings, InmemoryGreetingsRetriever>();
        services.AddSingleton<IProvideGreetings>(serviceProvider =>
            new GreetingsProvider(greetingsRetriever: serviceProvider.GetService<IRetrieveGreetings>()!,
                onDemandWeatherReportProvider: serviceProvider.GetService<IProvideOnDemandWeatherReport>()));

        return services;
    }

    public static IEndpointRouteBuilder MapGreetingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("api/greetings/todays/{city}", (string city, IProvideGreetings greetingsProvider) =>
        {
            var result = greetingsProvider.GetTodaysWeatherBasedGreetingFor(city);
            return Results.Ok(result);
        })
        .Produces<TodaysWeatherBasedGreeting>(StatusCodes.Status200OK)
        .Produces<TodaysWeatherBasedGreeting>(StatusCodes.Status500InternalServerError);

        endpoints.MapGet("api/greetings/todays", (IProvideGreetings greetingsProvider) =>
        {
            var result = greetingsProvider.GetTodaysGreeting();
            return Results.Ok(result);
        })
        .Produces<TodaysWeatherBasedGreeting>(StatusCodes.Status200OK)
        .Produces<TodaysWeatherBasedGreeting>(StatusCodes.Status500InternalServerError);

        return endpoints;
    }
}