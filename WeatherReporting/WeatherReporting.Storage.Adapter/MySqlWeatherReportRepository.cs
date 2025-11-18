using WeatherReporting.DomainModel;

namespace WeatherReporting.Storage.Adapter;

public class MySqlWeatherReportRepository : IWeatherReportRepository
{
    public Task AddWeatherReport(OnDemandWeatherReport report)
    {
        throw new NotImplementedException();
    }
}