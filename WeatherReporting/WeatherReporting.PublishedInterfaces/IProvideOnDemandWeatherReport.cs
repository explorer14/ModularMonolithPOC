namespace WeatherReporting.PublishedInterfaces;

public interface IProvideOnDemandWeatherReport
{
    OnDemandWeatherReport GetTodaysWeatherFor(string city);
}

public record OnDemandWeatherReport(
    decimal TemperatureC, 
    decimal TemperatureF, 
    string City, 
    DateTimeOffset ReportedOn);