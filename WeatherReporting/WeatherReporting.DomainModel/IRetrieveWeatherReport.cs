namespace WeatherReporting.DomainModel;

public interface IRetrieveWeatherReport
{
    OnDemandWeatherReport GetTodaysWeatherFor(string city);
}