using Microsoft.Extensions.Hosting;

namespace WeatherModeling.Messaging.Adapter;

public sealed class WeatherReportReceiverHost(InMemoryMessageQueueListener queueListener) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory.StartNew(() => 
            queueListener.StartListening(stoppingToken), stoppingToken);
    }
}