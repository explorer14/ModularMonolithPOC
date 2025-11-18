namespace Greetings.PublishedInterfaces;

public interface IProvideGreetings
{
    [Obsolete("Superseded by GetTodaysGreetingAsync")]
    TodaysGreeting GetTodaysGreeting();

    TodaysWeatherBasedGreeting GetTodaysWeatherBasedGreetingFor(string city);
    
    Task<TodaysGreeting> GetTodaysGreetingAsync();
}

public record TodaysWeatherBasedGreeting(string City, string Message);

public record TodaysGreeting(string Message);