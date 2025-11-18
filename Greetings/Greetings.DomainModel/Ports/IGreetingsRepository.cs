using Greetings.DomainModel.Objects;

namespace Greetings.DomainModel.Ports;

public interface IGreetingsRepository
{
    Task AddGreeting(TodaysGreeting greeting);
}