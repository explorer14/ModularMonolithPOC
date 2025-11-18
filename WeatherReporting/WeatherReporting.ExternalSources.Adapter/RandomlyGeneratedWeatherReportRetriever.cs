using WeatherReporting.DomainModel;

namespace WeatherReporting.ExternalSources.Adapter;

public class RandomlyGeneratedWeatherReportRetriever : IRetrieveWeatherReport
{
    public OnDemandWeatherReport GetTodaysWeatherFor(string city)
    {
        return new OnDemandWeatherReport(city,23m, DateTimeOffset.Now);
    }
}