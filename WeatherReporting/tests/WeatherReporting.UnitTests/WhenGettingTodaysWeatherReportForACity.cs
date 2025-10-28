using FluentAssertions;
using WeatherReporting.ApplicationServices;
using WeatherReporting.DomainModel;
using WeatherReporting.PublishedInterfaces;

namespace WeatherReporting.UnitTests;

public class WhenGettingTodaysWeatherReportForACity
{
    [Fact]
    public void GivenCityShouldReturnWeatherReport()
    {
        var expectedWeatherReport = new PublishedInterfaces.OnDemandWeatherReport(TemperatureC: 10,
            TemperatureF:50,
            City:"any city",
            ReportedOn: new DateTimeOffset(2024, 7, 30, 15, 16, 0, TimeSpan.FromHours(2)));
        
        var weatherReportProvider = new OnDemandWeatherReportProvider(
            new StubWeatherRetriever(
                new OnDemandWeatherReport(
                    TemperatureC: 10,
                    ReportedOn: new DateTimeOffset(2024, 7, 30, 15, 16, 0, TimeSpan.FromHours(2)))));
        var actualWeatherReport = weatherReportProvider.GetTodaysWeatherFor("any city");
        actualWeatherReport.Should().Be(expectedWeatherReport);
    }
}

internal class StubWeatherRetriever(OnDemandWeatherReport weatherReportToReturn) 
    : IRetrieveWeatherReport
{
    public OnDemandWeatherReport GetTodaysWeatherFor(string city) => 
        weatherReportToReturn;
}