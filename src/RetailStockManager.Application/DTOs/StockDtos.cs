using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailStockManager.Domain.Enums;

namespace RetailStockManager.Application.DTOs
{
    public record StockItemDto(
    string Id,
    string ProductId,
    int Quantity,
    string Location,
    int ReorderLevel,
    int MaxLevel,
    StockStatus Status,
    bool IsLowStock,
    bool IsOverStock,
    DateTime CreatedAt,
    DateTime UpdatedAt);

    public record CreateStockItemDto(
        string ProductId,
        int Quantity,
        string Location,
        int ReorderLevel = 10,
        int? MaxLevel = null);

    public record UpdateStockItemDto(
        string Id,
        int Quantity,
        string Location,
        int ReorderLevel,
        int MaxLevel);

    public record StockAdjustmentDto(
        string ProductId,
        int QuantityChange,
        StockMovementType MovementType,
        string Location,
        string Reason,
        string UserId)
    {
        public Dictionary<string, object> AdditionalData { get; init; } = [];
    }

    public record StockAlertDto(
        string ProductId,
        string ProductName,
        string Location,
        int CurrentQuantity,
        int ReorderLevel,
        StockStatus Status,
        string AlertMessage,
        DateTime AlertDate);

    public record StockSummaryDto(
        int TotalProducts,
        int LowStockItems,
        int OutOfStockItems,
        int OverStockItems,
        decimal TotalValue);
}
