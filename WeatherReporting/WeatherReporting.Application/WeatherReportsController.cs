using Microsoft.AspNetCore.Mvc;
using WeatherReporting.PublishedInterfaces;

namespace WeatherReporting.Application;

[Route("api/weather-reports")]
[Route("api/[controller]")]
[ApiController]
public class WeatherReportsController(
    IProvideOnDemandWeatherReport onDemandWeatherReportProvider,
    IPublishWeatherReports weatherReportPublisher) : ControllerBase
{
    [HttpGet("{city}")]
    public Task<IActionResult> GetTodaysWeatherFor(string city) => 
        Task.FromResult<IActionResult>(Ok(onDemandWeatherReportProvider.GetTodaysWeatherFor(city)));

    [HttpPost("/publish/{city}")]
    public Task<IActionResult> PublishWeatherReportFor(string city)
    {
        var weatherReport = onDemandWeatherReportProvider.GetTodaysWeatherFor(city);
        weatherReportPublisher.Publish(weatherReport);
        return Task.FromResult<IActionResult>(Accepted());
    }
}