using System.Collections.Concurrent;

namespace SharedInfrastructure;

public static class QueueFactory
{
    public static ConcurrentQueue<TEvent> CreateFor<TEvent>() => new();
}