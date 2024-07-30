using System.Collections.Concurrent;
using WeatherReporting.PublishedInterfaces;

namespace WeatherReporting.Publishing.Adapter;

internal class InmemoryQueuePublisher : IPublishWeatherReports
{
    private readonly ConcurrentQueue<OnDemandWeatherReport> _queue;

    public InmemoryQueuePublisher(ConcurrentQueue<OnDemandWeatherReport> queue)
    {
        _queue = queue;
    }
    public void Publish(OnDemandWeatherReport weatherReport)
    {
        _queue.Enqueue(weatherReport);
    }
}