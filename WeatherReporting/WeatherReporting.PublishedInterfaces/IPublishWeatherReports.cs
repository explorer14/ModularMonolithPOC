namespace WeatherReporting.PublishedInterfaces;

public interface IPublishWeatherReports
{
    void Publish(OnDemandWeatherReport weatherReport);
}