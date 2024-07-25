namespace Greetings.PublishedInterfaces;

public interface IProvideGreetings
{
    TodaysGreeting GetTodaysGreeting();
}

public record TodaysGreeting(string Message);