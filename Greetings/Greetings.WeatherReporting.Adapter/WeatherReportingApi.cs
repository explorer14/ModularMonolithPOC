using System.Net.Http.Json;
using WeatherReporting.PublishedInterfaces;

namespace Greetings.WeatherReporting.Adapter;

internal class WeatherReportingApi : IProvideOnDemandWeatherReport
{
    public OnDemandWeatherReport GetTodaysWeatherFor(string city)
    {
        using (var client = new HttpClient()
               {
                   BaseAddress = new Uri("https://localhost:7290")
               })
        {
            var response = client.GetAsync($"api/weatherreports/{city}").Result;

            response.EnsureSuccessStatusCode();
            var data = response.Content.ReadFromJsonAsync<OnDemandWeatherReport>().Result;

            return data;
        }
    }
}