using Greetings.DomainModel.Ports;
using Greetings.PublishedInterfaces;
using Greetings.Storage.Adapter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WeatherReporting.PublishedInterfaces;
using Microsoft.EntityFrameworkCore;

namespace Greetings.Application;

public static class ServiceCollectionExtensions
{
    // TODO: can DI be made smarter here so it can auto-detect if it needs to load
    // in-proc/lib dependency or out-of-proc/http dependency?
    public static IServiceCollection AddGreetingsModuleWithWeatherReportingApi(this IServiceCollection services)
    {
        services.AddSingleton<IProvideOnDemandWeatherReport, WeatherReportingApi.Adapter.WeatherReportingApi>();
        return AddServices(services);
    }

    public static IServiceCollection AddGreetingsModule(this IServiceCollection services) =>
        AddServices(services);

    private static IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddDbContext<GreetingsDbContext>(options => 
            options.UseSqlServer(
                connectionString: "Server=greetings-module-db-server;Database=GreetingsDB;user=sa;Password=SuperPass123;MultipleActiveResultSets=true"));
        services.AddSingleton<IRetrieveGreetings, InmemoryGreetingsRetriever>();
        services.AddSingleton<IProvideGreetings>(serviceProvider =>
            new GreetingsProvider(greetingsRetriever: serviceProvider.GetService<IRetrieveGreetings>()!,
                onDemandWeatherReportProvider: serviceProvider.GetService<IProvideOnDemandWeatherReport>()))
            .AddSingleton<IGreetingsRepository, SqlServerGreetingsRepository>();

        return services;
    }

    // The endpoint approach works better than the controller approach 
    // because it offers more control over which endpoints are exposed.
    // With controllers you need to call AddControllers() per module
    // which is pointless because the first call to AddControllers() will expose
    // all the endpoints across all modules which is hidden coupling.
    // This can make unplugging modules risky because you'd think by removing the 
    // module registration call that that would disable the module, but since 
    // the AddControllers() also exists in other modules, the controller endpoints
    // of all modules will still be exposed.
    
    // Solution: Make exposing endpoints (for HTTP consumption)
    // separate and explicit from registering the modules (class libraries).
    // This way a module can still be used in the system, it just won't expose 
    // any endpoints.
    // Also allows deferring endpoints if its not clear if they are needed yet.
    
    // Downside: two function calls as opposed to one. Unplugging libs whilst 
    // leaving the endpoints in, throws exceptions.
    
    // Reason why we can't have one function do both: libs are registered on IServiceCollection
    // whereas the endpoints are registered on IEndpointRouteBuilder which is only available
    // once the app builder has built the application which freezes IServiceCollection.
    // This means trying to add endpoints before registering libs will not work!
    public static IEndpointRouteBuilder MapGreetingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var tag = "greetings";
        var routeBase = $"api/{tag}";
        endpoints.MapGet($"{routeBase}/todays/{{city}}", (string city, IProvideGreetings greetingsProvider) =>
        {
            var result = greetingsProvider.GetTodaysWeatherBasedGreetingFor(city);
            return Results.Ok(result);
        })
        .Produces<TodaysWeatherBasedGreeting>(StatusCodes.Status200OK)
        .Produces<TodaysWeatherBasedGreeting>(StatusCodes.Status500InternalServerError)
        .WithTags(tag);

        endpoints.MapGet($"{routeBase}/todays", (IProvideGreetings greetingsProvider) =>
        {
            var result = greetingsProvider.GetTodaysGreeting();
            return Results.Ok(result);
        })
        .Produces<TodaysWeatherBasedGreeting>(StatusCodes.Status200OK)
        .Produces<TodaysWeatherBasedGreeting>(StatusCodes.Status500InternalServerError)
        .WithTags(tag);

        return endpoints;
    }
}