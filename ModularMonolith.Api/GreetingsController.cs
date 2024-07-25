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
}