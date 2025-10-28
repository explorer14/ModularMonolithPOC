using Greetings.DomainModel.Ports;

namespace Greetings.Storage.Adapter;

internal class InmemoryGreetingsRetriever : IRetrieveGreetings
{
    private readonly Stack<TodaysGreeting> _greetings = new();

    public InmemoryGreetingsRetriever()
    {
        _greetings.Push(new TodaysGreeting("Hello World!"));
    }
    
    public DomainModel.Objects.TodaysGreeting? GetTodaysGreeting()
    {
        if (_greetings.TryPop(out var todaysGreeting))
            return new DomainModel.Objects.TodaysGreeting(todaysGreeting.Message);
        
        return null;
    }
}

internal record TodaysGreeting(string Message);