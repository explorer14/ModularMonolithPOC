using Greetings.DomainModel.Ports;
using Greetings.PublishedInterfaces;

namespace Greetings.ApplicationServices;

public class GreetingsProvider(IRetrieveGreetings greetingsRetriever) : IProvideGreetings
{
    public TodaysGreeting GetTodaysGreeting()
    {
        var todaysGreeting = greetingsRetriever.GetTodaysGreeting();

        if (todaysGreeting == null)
            return new TodaysGreeting(
                "Sorry, our greeting fairy didn't deliver any messages today, please come back tomorrow!");

        return new TodaysGreeting(todaysGreeting.Message);
    }
}