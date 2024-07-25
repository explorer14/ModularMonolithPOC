namespace Greetings.PublishedInterfaces;

public interface IProvideGreetings
{
    TodaysGreeting GetTodaysGreeting();

    TodaysWeatherBasedGreeting GetTodaysWeatherBasedGreetingFor(string city);
}

public record TodaysWeatherBasedGreeting(string City, string Message);

public record TodaysGreeting(string Message);