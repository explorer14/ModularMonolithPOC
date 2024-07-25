namespace Greetings.DomainModel.Objects;

internal record TodaysGreeting(string Message)
{
    internal static TodaysGreeting BasedOnCityTemperature(CityTemperature cityTemperature)
    {
        if (cityTemperature.TemperatureC < 10)
            return new TodaysGreeting($"Its quite chilly today in {cityTemperature.City}. Wrap up warm!");
        
        if (cityTemperature.TemperatureC > 10 && cityTemperature.TemperatureC < 20)
            return new TodaysGreeting($"Seems like its pleasant in {cityTemperature.City} today. Have a great day!");
        
        if (cityTemperature.TemperatureC > 20 && cityTemperature.TemperatureC < 30)
            return new TodaysGreeting($"Its warming up in {cityTemperature.City} today. Get some ice creams!");
        
        return new TodaysGreeting($"Its quite hot in {cityTemperature.City} today. Make sure to get a sun screen!");
    }
}

internal record CityTemperature(string City, decimal TemperatureC);