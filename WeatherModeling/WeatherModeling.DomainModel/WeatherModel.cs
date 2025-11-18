namespace WeatherModeling.DomainModel;

public class WeatherModel(string name, string description, double[] parameters)
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public double[] Parameters { get; } = parameters;

    public Guid Id { get; } = Guid.NewGuid();
}