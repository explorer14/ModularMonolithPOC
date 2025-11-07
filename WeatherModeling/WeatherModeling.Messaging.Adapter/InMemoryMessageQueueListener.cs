using System.Collections.Concurrent;
using WeatherModeling.DomainModel;
using WeatherReporting.PublishedInterfaces;

namespace WeatherModeling.Messaging.Adapter;

public class InMemoryMessageQueueListener
{
    private readonly Func<WeatherReport, Task> _weatherReportEventHandler;
    private ConcurrentQueue<OnDemandWeatherReport> _queue;

    public InMemoryMessageQueueListener(
        Func<WeatherReport, Task> weatherReportEventHandler,
        ConcurrentQueue<OnDemandWeatherReport> queue)
    {
        _weatherReportEventHandler = weatherReportEventHandler;
        _queue = queue;
    }
    
    public async Task StartListening(CancellationToken stoppingToken)
    {
        Console.WriteLine("Starting listening for new weather reports...");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_queue.TryDequeue(out var weatherReport))
                {
                    await _weatherReportEventHandler.Invoke(new WeatherReport(
                        weatherReport.TemperatureC,
                        weatherReport.TemperatureF, 
                        weatherReport.City, 
                        weatherReport.ReportedOn));
                }
                else
                {
                    Console.WriteLine("Failed to dequeue message will try again...");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            await Task.Delay(2000, stoppingToken);
        }
    }
}