namespace WeatherModeling.DomainModel;

internal record WeatherReport(
    decimal TemperatureC, 
    decimal TemperatureF, 
    string City, 
    DateTimeOffset ReportedOn);