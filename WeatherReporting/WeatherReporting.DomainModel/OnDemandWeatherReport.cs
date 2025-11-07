namespace WeatherReporting.DomainModel;

public record OnDemandWeatherReport(decimal TemperatureC, DateTimeOffset ReportedOn)
{
    public decimal TemperatureF() => 
        Decimal.Round(32 + TemperatureC / 0.5556m);
}