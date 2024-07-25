using Greetings.DomainModel.Objects;

namespace Greetings.DomainModel.Ports;

internal interface IRetrieveGreetings
{
    TodaysGreeting? GetTodaysGreeting();
}