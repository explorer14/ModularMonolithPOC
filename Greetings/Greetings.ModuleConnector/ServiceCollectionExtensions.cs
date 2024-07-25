using Greetings.ApplicationServices;
using Greetings.DomainModel.Ports;
using Greetings.PublishedInterfaces;
using Greetings.Storage.Adapter;
using Microsoft.Extensions.DependencyInjection;

namespace Greetings.ModuleConnector;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGreetingsModule(this IServiceCollection services)
    {
        services.AddSingleton<IRetrieveGreetings, InmemoryGreetingsRetriever>();
        services.AddSingleton<IProvideGreetings, GreetingsProvider>();
        
        return services;
    }
}