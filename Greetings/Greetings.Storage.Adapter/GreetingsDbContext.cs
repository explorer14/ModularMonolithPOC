using Microsoft.EntityFrameworkCore;

namespace Greetings.Storage.Adapter;

public class GreetingsDbContext(DbContextOptions<GreetingsDbContext> options) : DbContext(options)
{
    internal DbSet<Greetings> Greetings { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Greetings>().ToTable("Greetings");
        modelBuilder.Entity<Greetings>().HasKey(x => x.Id);
        modelBuilder.Entity<Greetings>().Property(x => x.Id).ValueGeneratedNever();
        
        base.OnModelCreating(modelBuilder);
    }
}