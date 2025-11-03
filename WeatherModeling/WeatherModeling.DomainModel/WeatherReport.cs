namespace WeatherModeling.DomainModel;

public record WeatherReport(
    decimal TemperatureC, 
    decimal TemperatureF, 
    string City, 
    DateTimeOffset ReportedOn);