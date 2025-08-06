using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailStockManager.Domain.Common;
using RetailStockManager.Domain.Enums;

namespace RetailStockManager.Domain.Entities
{
    public class StockItem(
    string productId,
    int quantity,
    string location,
    int reorderLevel = 10) : BaseEntity
    {
        private int _quantity = ValidateQuantity(quantity);
        private int _reorderLevel = ValidateReorderLevel(reorderLevel);

        public string ProductId { get; set; } = ValidateProductId(productId);

        public int Quantity
        {
            get => _quantity;
            set => _quantity = ValidateQuantity(value);
        }

        public string Location { get; set; } = ValidateLocation(location);

        public int ReorderLevel
        {
            get => _reorderLevel;
            set => _reorderLevel = ValidateReorderLevel(value);
        }

        public int MaxLevel { get; set; } = reorderLevel * 10; // Default: 10x reorder level
        public bool IsLowStock => Quantity <= ReorderLevel;
        public bool IsOverStock => Quantity >= MaxLevel;

        public StockStatus Status => Quantity switch
        {
            0 => StockStatus.OutOfStock,
            var q when q <= ReorderLevel => StockStatus.LowStock,
            var q when q >= MaxLevel => StockStatus.OverStock,
            _ => StockStatus.InStock
        };

        public void AdjustQuantity(int change, StockMovementType movementType)
        {
            var newQuantity = Quantity + change;
            if (newQuantity < 0)
                throw new InvalidOperationException($"Stock adjustment would result in negative quantity. Current: {Quantity}, Change: {change}");

            Quantity = newQuantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool NeedsReorder() => Status is StockStatus.LowStock or StockStatus.OutOfStock;

        public void SetReorderLevels(int reorderLevel, int? maxLevel = null)
        {
            ReorderLevel = reorderLevel;
            MaxLevel = maxLevel ?? reorderLevel * 10;
            UpdatedAt = DateTime.UtcNow;
        }

        private static string ValidateProductId(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentException("Product ID cannot be empty");
            return productId.Trim();
        }

        private static int ValidateQuantity(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative");
            return quantity;
        }

        private static string ValidateLocation(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentException("Location cannot be empty");
            if (location.Length > 50)
                throw new ArgumentException("Location name cannot exceed 50 characters");
            return location.Trim().ToUpper();
        }

        private static int ValidateReorderLevel(int reorderLevel)
        {
            if (reorderLevel < 0)
                throw new ArgumentException("Reorder level cannot be negative");
            return reorderLevel;
        }
    }
}
