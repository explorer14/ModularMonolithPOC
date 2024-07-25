namespace WeatherReporting.DomainModel;

internal record OnDemandWeatherReport(decimal TemperatureC, DateTimeOffset ReportedOn)
{
    public decimal TemperatureF() => 
        32 + (int)(TemperatureC / 0.5556m);
}