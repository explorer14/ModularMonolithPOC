namespace Greetings.Storage.Adapter;

internal class Greetings
{
    public Guid Id { get; set; }
    public string City { get; set; }
    public string Message { get; set; }
    public DateTime GeneratedOn { get; set; }
    public decimal TemperatureC { get; set; }
}