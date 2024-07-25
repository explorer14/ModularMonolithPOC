using Greetings.DomainModel.Objects;

namespace Greetings.DomainModel.Ports;

public interface IRetrieveGreetings
{
    TodaysGreeting? GetTodaysGreeting();
}