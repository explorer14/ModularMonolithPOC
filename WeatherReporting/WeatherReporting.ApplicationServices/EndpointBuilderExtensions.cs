using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WeatherReporting.PublishedInterfaces;

namespace WeatherReporting.ApplicationServices;

public static class EndpointBuilderExtensions
{
    private const string TAG = "weather-reports";
    private const string ROUTE_BASE = $"api/{TAG}";
    
    public static RouteHandlerBuilder MapWeatherReportingEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        return routeBuilder.MapPost($"{ROUTE_BASE}/publish/{{city}}", 
                (IProvideOnDemandWeatherReport onDemandWeatherReportProvider,
                    IPublishWeatherReports weatherReportPublisher,
                    string city) =>
            {
                var weatherReport = onDemandWeatherReportProvider.GetTodaysWeatherFor(city);
                weatherReportPublisher.Publish(weatherReport);

                return TypedResults.Accepted(uri: string.Empty);
            })
            .WithTags(TAG);
    } 
}