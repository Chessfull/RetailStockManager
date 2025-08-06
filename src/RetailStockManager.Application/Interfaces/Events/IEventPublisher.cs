using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailStockManager.Application.Interfaces.Events
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T eventData, string topic, CancellationToken cancellationToken = default) where T : class;
        Task PublishBatchAsync<T>(IEnumerable<T> events, string topic, CancellationToken cancellationToken = default) where T : class;
    }

    public interface IDomainEvent
    {
        string EventId { get; }
        DateTime OccurredAt { get; }
        string EventType { get; }
    }
}
