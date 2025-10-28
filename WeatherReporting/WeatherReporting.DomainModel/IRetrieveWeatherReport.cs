namespace WeatherReporting.DomainModel;

internal interface IRetrieveWeatherReport
{
    OnDemandWeatherReport GetTodaysWeatherFor(string city);
}