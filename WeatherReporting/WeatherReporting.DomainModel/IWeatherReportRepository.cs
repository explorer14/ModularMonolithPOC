namespace WeatherReporting.DomainModel;

public interface IWeatherReportRepository
{
    Task AddWeatherReport(OnDemandWeatherReport report);
}