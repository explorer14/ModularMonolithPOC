using Greetings.PublishedInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace ModularMonolith.Api;

[Route("api/[controller]")]
[ApiController]
public class GreetingsController(IProvideGreetings greetingsProvider) : ControllerBase
{
    [HttpGet("todays")]
    public IActionResult GetTodaysGreeting() => 
        Ok(greetingsProvider.GetTodaysGreeting());
    
    [HttpGet("todays/{city}")]
    public IActionResult GetTodaysWeatherBasedGreeting(string city) => 
        Ok(greetingsProvider.GetTodaysWeatherBasedGreetingFor(city));
}