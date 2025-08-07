using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using RetailStockManager.Application.Interfaces.Repositories;
using RetailStockManager.Domain.Entities;
using RetailStockManager.Domain.Enums;
using RetailStockManager.Infrastructure.Persistance;

namespace RetailStockManager.Infrastructure.Repositories
{
    public class StockRepository(
    MongoDbContext context,
    ILogger<StockRepository> logger) : MongoRepository<StockItem>(context, logger), IStockRepository
    {
        private readonly IMongoCollection<StockItem> _stockItems = context.StockItems;
        private readonly ILogger<StockRepository> _logger = logger;

        public async Task<IEnumerable<StockItem>> GetByLocationAsync(
            string location,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting stock items by location: {Location}", location);

            var filter = Builders<StockItem>.Filter.Eq(s => s.Location, location.ToUpper());
            var stockItems = await _stockItems.Find(filter).ToListAsync(cancellationToken);

            _logger.LogDebug("Found {Count} stock items in location {Location}", stockItems.Count, location);
            return stockItems;
        }

        public async Task<StockItem?> GetByProductIdAsync(
            string productId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting stock item by product ID: {ProductId}", productId);

            var filter = Builders<StockItem>.Filter.Eq(s => s.ProductId, productId);
            var stockItem = await _stockItems.Find(filter).FirstOrDefaultAsync(cancellationToken);

            if (stockItem == null)
                _logger.LogWarning("Stock item not found for product: {ProductId}", productId);

            return stockItem;
        }

        public async Task<IEnumerable<StockItem>> GetLowStockItemsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting low stock items");

            // MongoDB expression for computed property IsLowStock
            var filter = Builders<StockItem>.Filter.Where(s => s.Quantity <= s.ReorderLevel && s.Quantity > 0);
            var stockItems = await _stockItems.Find(filter).ToListAsync(cancellationToken);

            _logger.LogDebug("Found {Count} low stock items", stockItems.Count);
            return stockItems;
        }

        public async Task<IEnumerable<StockItem>> GetOverStockItemsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting overstocked items");

            var filter = Builders<StockItem>.Filter.Where(s => s.Quantity >= s.MaxLevel);
            var stockItems = await _stockItems.Find(filter).ToListAsync(cancellationToken);

            _logger.LogDebug("Found {Count} overstocked items", stockItems.Count);
            return stockItems;
        }

        public async Task<IEnumerable<StockItem>> GetOutOfStockItemsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting out of stock items");

            var filter = Builders<StockItem>.Filter.Eq(s => s.Quantity, 0);
            var stockItems = await _stockItems.Find(filter).ToListAsync(cancellationToken);

            _logger.LogDebug("Found {Count} out of stock items", stockItems.Count);
            return stockItems;
        }

        public async Task<IEnumerable<StockItem>> GetByStatusAsync(
            StockStatus status,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting stock items by status: {Status}", status);

            var filter = status switch
            {
                StockStatus.OutOfStock => Builders<StockItem>.Filter.Eq(s => s.Quantity, 0),
                StockStatus.LowStock => Builders<StockItem>.Filter.Where(s => s.Quantity <= s.ReorderLevel && s.Quantity > 0),
                StockStatus.OverStock => Builders<StockItem>.Filter.Where(s => s.Quantity >= s.MaxLevel),
                StockStatus.InStock => Builders<StockItem>.Filter.Where(s => s.Quantity > s.ReorderLevel && s.Quantity < s.MaxLevel),
                _ => FilterDefinition<StockItem>.Empty
            };

            var stockItems = await _stockItems.Find(filter).ToListAsync(cancellationToken);

            _logger.LogDebug("Found {Count} stock items with status {Status}", stockItems.Count, status);
            return stockItems;
        }

        public async Task<int> GetTotalStockCountAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting total stock count");

            // MongoDB aggregation for sum
            var pipeline = new[]
            {
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "total", new BsonDocument("$sum", "$quantity") }
            })
        };

            var result = await _stockItems.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync(cancellationToken);
            var totalCount = result?["total"].AsInt32 ?? 0;

            _logger.LogDebug("Total stock count: {TotalCount}", totalCount);
            return totalCount;
        }

        public async Task<decimal> GetTotalStockValueAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting total stock value");

            // Simplified - gerçek implementasyon product fiyatlarını kullanacak
            var totalCount = await GetTotalStockCountAsync(cancellationToken);
            var estimatedValue = totalCount * 10m; // Ortalama 10₺ varsayımı

            _logger.LogDebug("Estimated total stock value: {TotalValue}", estimatedValue);
            return estimatedValue;
        }

        public async Task<IEnumerable<StockItem>> GetItemsNeedingReorderAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting items needing reorder");

            var filter = Builders<StockItem>.Filter.Where(s => s.Quantity <= s.ReorderLevel);
            var stockItems = await _stockItems.Find(filter).ToListAsync(cancellationToken);

            _logger.LogDebug("Found {Count} items needing reorder", stockItems.Count);
            return stockItems;
        }

        public async Task<IEnumerable<StockItem>> GetMultiLocationStockAsync(
            string productId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting multi-location stock for product: {ProductId}", productId);

            var filter = Builders<StockItem>.Filter.Eq(s => s.ProductId, productId);
            var stockItems = await _stockItems.Find(filter).ToListAsync(cancellationToken);

            _logger.LogDebug("Found {Count} stock items across locations for product", stockItems.Count);
            return stockItems;
        }
    }
}
