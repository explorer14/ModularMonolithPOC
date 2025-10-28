using Microsoft.AspNetCore.Mvc;
using WeatherReporting.PublishedInterfaces;

namespace WeatherReporting.ApplicationServices;

[Route("api/weather-reports")]
[Route("api/[controller]")]
[ApiController]
public class WeatherReportsController(
    IProvideOnDemandWeatherReport onDemandWeatherReportProvider) : ControllerBase
{
    [HttpGet("{city}")]
    public Task<IActionResult> GetTodaysWeatherFor(string city) => 
        Task.FromResult<IActionResult>(Ok(onDemandWeatherReportProvider.GetTodaysWeatherFor(city)));
}