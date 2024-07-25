using WeatherReporting.DomainModel;
using WeatherReporting.PublishedInterfaces;
using OnDemandWeatherReport = WeatherReporting.PublishedInterfaces.OnDemandWeatherReport;

namespace WeatherReporting.ApplicationServices;

internal class OnDemandWeatherReportProvider : IProvideOnDemandWeatherReport
{
    private readonly IRetrieveWeatherReport _weatherReportRetriever;

    internal OnDemandWeatherReportProvider(IRetrieveWeatherReport weatherReportRetriever)
    {
        _weatherReportRetriever = weatherReportRetriever;
    }
    
    public OnDemandWeatherReport GetTodaysWeatherFor(string city)
    {
        var report = _weatherReportRetriever.GetTodaysWeatherFor(city);
        
        return new OnDemandWeatherReport(
            report.TemperatureC, 
            report.TemperatureF(), 
            city, 
            report.ReportedOn);
    }
}