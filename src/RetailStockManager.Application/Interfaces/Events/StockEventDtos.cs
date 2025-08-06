using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailStockManager.Domain.Enums;

namespace RetailStockManager.Application.Interfaces.Events
{
    public record StockAdjustedEvent(
    string ProductId,
    int OldQuantity,
    int NewQuantity,
    int QuantityChange,
    StockMovementType MovementType,
    string Location,
    string Reason,
    string UserId) : IDomainEvent
    {
        public string EventId { get; init; } = Guid.NewGuid().ToString();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public string EventType { get; init; } = nameof(StockAdjustedEvent);

        // 🎯 Additional event data
        public Dictionary<string, object> AdditionalData { get; init; } = [];
    }

    public record ProductCreatedEvent(
        string ProductId,
        string Name,
        ProductCategory Category,
        decimal Price,
        string CreatedBy) : IDomainEvent
    {
        public string EventId { get; init; } = Guid.NewGuid().ToString();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public string EventType { get; init; } = nameof(ProductCreatedEvent);
    }

    public record StockLowEvent(
        string ProductId,
        string ProductName,
        string Location,
        int CurrentQuantity,
        int ReorderLevel) : IDomainEvent
    {
        public string EventId { get; init; } = Guid.NewGuid().ToString();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
        public string EventType { get; init; } = nameof(StockLowEvent);
    }
}
