using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using WeatherModeling.Messaging.Adapter;
using WeatherReporting.PublishedInterfaces;

namespace WeatherModeling.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherModelingModule(this IServiceCollection services)
    {
        services.AddSingleton(serviceProvider => new InMemoryMessageQueueListener(weatherReport =>
        {
            Console.WriteLine($"Running the weather model for {weatherReport}");
            return Task.CompletedTask;
        }, 
            serviceProvider.GetService<ConcurrentQueue<OnDemandWeatherReport>>()));
        
        services.AddHostedService<WeatherReportReceiverHost>();
        return services;
    }
}