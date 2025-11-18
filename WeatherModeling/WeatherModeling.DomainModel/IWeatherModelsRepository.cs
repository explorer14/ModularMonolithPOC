namespace WeatherModeling.DomainModel;

public interface IWeatherModelsRepository
{
    Task AddModel(WeatherModel model);
    Task<WeatherModel?> GetModel(Guid modelId);
}