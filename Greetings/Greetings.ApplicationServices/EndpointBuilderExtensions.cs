using Greetings.PublishedInterfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Greetings.ApplicationServices;

public static class EndpointBuilderExtensions
{
    private const string TAG = "greetings";
    private const string ROUTE_BASE = $"api/{TAG}";
    public static RouteHandlerBuilder MapGreetingEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        return routeBuilder.MapGet($"{ROUTE_BASE}/todays", (IProvideGreetings greetingsProvider) => 
                TypedResults.Ok(greetingsProvider.GetTodaysGreeting())).WithTags(TAG);
    } 
}