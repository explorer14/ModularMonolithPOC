using System.Net;
using Greetings.PublishedInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace Greetings.ApplicationServices;

[Route("api/[controller]")]
[ApiController]
public class GreetingsController(IProvideGreetings greetingsProvider) : ControllerBase
{
    [ProducesResponseType<TodaysWeatherBasedGreeting>((int)HttpStatusCode.OK)]
    [ProducesResponseType<TodaysWeatherBasedGreeting>((int)HttpStatusCode.InternalServerError)]
    [HttpGet("todays/{city}")]
    public Task<IActionResult> GetTodaysWeatherForCity(string city) =>
        Task.FromResult<IActionResult>(
            Ok(greetingsProvider.GetTodaysWeatherBasedGreetingFor(city)));
}