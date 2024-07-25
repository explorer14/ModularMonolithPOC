using Microsoft.AspNetCore.Mvc;
using WeatherReporting.PublishedInterfaces;

namespace ModularMonolith.Api;

[Route("api/[controller]")]
[ApiController]
public class WeatherReportsController(IProvideOnDemandWeatherReport onDemandWeatherReportProvider)
    : ControllerBase
{
    [HttpGet("{city}")]
    public IActionResult GetWeatherReportForCity(string city)
    {
        var weatherReport = onDemandWeatherReportProvider.GetTodaysWeatherFor(city);
        
        return Ok(weatherReport);
    }
}