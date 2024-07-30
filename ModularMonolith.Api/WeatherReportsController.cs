using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using WeatherReporting.PublishedInterfaces;

namespace ModularMonolith.Api;

[Route("api/[controller]")]
[ApiController]
public class WeatherReportsController(IProvideOnDemandWeatherReport onDemandWeatherReportProvider,
    ConcurrentQueue<OnDemandWeatherReport> queue)
    : ControllerBase
{
    [HttpGet("{city}")]
    public IActionResult GetWeatherReportForCity(string city)
    {
        var weatherReport = onDemandWeatherReportProvider.GetTodaysWeatherFor(city);
        
        return Ok(weatherReport);
    }
    
    // This would usually be an internal process that publishes the event but to keep this
    // example simple, we will trigger it manually
    [HttpPost("publish/{city}")]
    public IActionResult PublishNewWeatherReport(string city)
    {
        var weatherReport = onDemandWeatherReportProvider.GetTodaysWeatherFor(city);
        queue.Enqueue(weatherReport);

        return Accepted();
    }
}