using Greetings.DomainModel.Objects;
using Greetings.DomainModel.Ports;
using Greetings.PublishedInterfaces;
using WeatherReporting.PublishedInterfaces;
using TodaysGreeting = Greetings.PublishedInterfaces.TodaysGreeting;

namespace Greetings.Application;

internal class GreetingsProvider(
    IRetrieveGreetings greetingsRetriever, 
    IProvideOnDemandWeatherReport onDemandWeatherReportProvider,
    IGreetingsRepository greetingsRepository) : IProvideGreetings
{
    [Obsolete("Superseded by GetTodaysGreetingAsync")]
    public TodaysGreeting GetTodaysGreeting()
    {
        return GetTodaysGreetingAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public TodaysWeatherBasedGreeting GetTodaysWeatherBasedGreetingFor(string city)
    {
        var weatherReport = onDemandWeatherReportProvider.GetTodaysWeatherFor(city);

        var greetingsBasedOnCityTemp =
            DomainModel.Objects.TodaysGreeting.BasedOnCityTemperature(new CityTemperature(city,
                weatherReport.TemperatureC));
        
        return new TodaysWeatherBasedGreeting(city, greetingsBasedOnCityTemp.Message);
    }

    public async Task<TodaysGreeting> GetTodaysGreetingAsync()
    {
        var todaysGreeting = greetingsRetriever.GetTodaysGreeting();

        if (todaysGreeting == null)
            return new TodaysGreeting(
                "Sorry, our greeting fairy didn't deliver any messages today, please come back tomorrow!");
        await greetingsRepository.AddGreeting(todaysGreeting);

        return new TodaysGreeting(todaysGreeting.Message);
    }
}