using Greetings.ApplicationServices;
using SharedInfrastructure;
using WeatherModeling.ApplicationServices;
using WeatherReporting.ApplicationServices;
using WeatherReporting.PublishedInterfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(QueueFactory.CreateFor<OnDemandWeatherReport>());

AddApplicationModules();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();

AddApplicationModuleEndpoints(app);

app.Run();

void AddApplicationModuleEndpoints(WebApplication webApplication)
{
    webApplication.MapGreetingEndpoints();
    webApplication.MapWeatherReportingEndpoints();
}

void AddApplicationModules()
{
    if (builder.Configuration.GetValue<bool>("UseWeatherReportingApi", false) == false)
    {
        builder.Services.AddGreetingsModule();
        builder.Services.AddWeatherReportingModule();
    }
    else
    {
        builder.Services.AddGreetingsModuleWithWeatherReportingApi();
    }

    builder.Services.AddWeatherModelingModule();
    
}