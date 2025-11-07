using System.Net.Http.Json;
using WeatherReporting.PublishedInterfaces;

namespace Greetings.WeatherReportingApi.Adapter;

/// <summary>
/// Proxy client to the Weather Reporting API. Should ideally be a nuget package
/// </summary>
public class WeatherReportingApi : IProvideOnDemandWeatherReport
{
    public OnDemandWeatherReport GetTodaysWeatherFor(string city)
    {
        using (var client = new HttpClient()
               {
                   BaseAddress = new Uri("https://localhost:7043")
               })
        {
            var response = client.GetAsync($"api/weather-reports/{city}").Result;

            response.EnsureSuccessStatusCode();
            var data = response.Content.ReadFromJsonAsync<OnDemandWeatherReport>().Result;

            return data;
        }
    }
}