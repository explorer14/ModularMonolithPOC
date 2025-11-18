using Greetings.DomainModel.Ports;

namespace Greetings.Storage.Adapter;

public class SqlServerGreetingsRepository(GreetingsDbContext dbContext)  : IGreetingsRepository
{
    public async Task AddGreeting(DomainModel.Objects.TodaysGreeting greeting)
    {
        var newGreetings = new Greetings()
        {
            Id = Guid.NewGuid(),
            City = greeting.Message,
            GeneratedOn = DateTime.UtcNow
        };
        await dbContext.Greetings.AddAsync(newGreetings);
        var rowsAffected = await dbContext.SaveChangesAsync();
        
        Console.WriteLine($"Rows affected: {rowsAffected}");
    }
}