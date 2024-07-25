using Greetings.DomainModel.Objects;
using Greetings.DomainModel.Ports;
using Greetings.PublishedInterfaces;
using WeatherReporting.PublishedInterfaces;
using TodaysGreeting = Greetings.PublishedInterfaces.TodaysGreeting;

namespace Greetings.ApplicationServices;

internal class GreetingsProvider(
    IRetrieveGreetings greetingsRetriever, 
    IProvideOnDemandWeatherReport onDemandWeatherReportProvider) : IProvideGreetings
{
    public TodaysGreeting GetTodaysGreeting()
    {
        var todaysGreeting = greetingsRetriever.GetTodaysGreeting();

        if (todaysGreeting == null)
            return new TodaysGreeting(
                "Sorry, our greeting fairy didn't deliver any messages today, please come back tomorrow!");

        return new TodaysGreeting(todaysGreeting.Message);
    }

    public TodaysWeatherBasedGreeting GetTodaysWeatherBasedGreetingFor(string city)
    {
        var weatherReport = onDemandWeatherReportProvider.GetTodaysWeatherFor(city);

        var greetingsBasedOnCityTemp =
            DomainModel.Objects.TodaysGreeting.BasedOnCityTemperature(new CityTemperature(city,
                weatherReport.TemperatureC));
        
        return new TodaysWeatherBasedGreeting(city, greetingsBasedOnCityTemp.Message);
    }
}