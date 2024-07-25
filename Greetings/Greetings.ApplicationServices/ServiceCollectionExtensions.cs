using Greetings.DomainModel.Ports;
using Greetings.PublishedInterfaces;
using Greetings.Storage.Adapter;
using Microsoft.Extensions.DependencyInjection;
using WeatherReporting.PublishedInterfaces;

namespace Greetings.ApplicationServices;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGreetingsModule(this IServiceCollection services)
    {
        services.AddSingleton<IRetrieveGreetings, InmemoryGreetingsRetriever>();
        services.AddSingleton<IProvideGreetings>(serviceProvider => 
            new GreetingsProvider(serviceProvider.GetService<IRetrieveGreetings>(),
                serviceProvider.GetService<IProvideOnDemandWeatherReport>()));
        
        return services;
    }
}