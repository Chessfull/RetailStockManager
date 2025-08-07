using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RetailStockManager.Application.Interfaces.Repositories;
using RetailStockManager.Domain.Entities;
using RetailStockManager.Domain.Enums;
using RetailStockManager.Infrastructure.Persistance;

namespace RetailStockManager.Infrastructure.Repositories
{
    public class ProductRepository(
    MongoDbContext context,
    ILogger<ProductRepository> logger) : MongoRepository<Product>(context, logger), IProductRepository
    {
        private readonly IMongoCollection<Product> _products = context.Products;
        private readonly ILogger<ProductRepository> _logger = logger;

        public async Task<IEnumerable<Product>> GetByCategoryAsync(
            ProductCategory category,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting products by category: {Category}", category);

            var filter = Builders<Product>.Filter.Eq(p => p.Category, category);
            var products = await _products.Find(filter).ToListAsync(cancellationToken);

            _logger.LogDebug("Found {Count} products in category {Category}", products.Count, category);
            return products;
        }

        public async Task<IEnumerable<Product>> SearchAsync(
            string searchTerm,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Searching products with term: {SearchTerm}", searchTerm);

            // MongoDB text search with multiple fields
            var filter = Builders<Product>.Filter.Or(
                Builders<Product>.Filter.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Product>.Filter.Regex(p => p.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Product>.Filter.Regex(p => p.Sku, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );

            var products = await _products.Find(filter).ToListAsync(cancellationToken);

            _logger.LogDebug("Found {Count} products matching search term", products.Count);
            return products;
        }

        public async Task<Product?> GetBySkuAsync(
            string sku,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting product by SKU: {Sku}", sku);

            var filter = Builders<Product>.Filter.Eq(p => p.Sku, sku);
            var product = await _products.Find(filter).FirstOrDefaultAsync(cancellationToken);

            if (product == null)
                _logger.LogWarning("Product not found with SKU: {Sku}", sku);

            return product;
        }

        public async Task<IEnumerable<Product>> GetByPriceRangeAsync(
            decimal minPrice,
            decimal maxPrice,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting products in price range: {MinPrice} - {MaxPrice}", minPrice, maxPrice);

            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Gte(p => p.Price, minPrice),
                Builders<Product>.Filter.Lte(p => p.Price, maxPrice)
            );

            var products = await _products.Find(filter).ToListAsync(cancellationToken);

            _logger.LogDebug("Found {Count} products in price range", products.Count);
            return products;
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> SearchWithFiltersAsync(
            string? searchTerm,
            ProductCategory? category,
            decimal? minPrice,
            decimal? maxPrice,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Advanced search with filters");

            // 🎯 .NET 8 C# 12: Collection expressions for filters
            var filters = new List<FilterDefinition<Product>>();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                filters.Add(Builders<Product>.Filter.Or(
                    Builders<Product>.Filter.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                    Builders<Product>.Filter.Regex(p => p.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
                ));
            }

            if (category.HasValue)
                filters.Add(Builders<Product>.Filter.Eq(p => p.Category, category.Value));

            if (minPrice.HasValue)
                filters.Add(Builders<Product>.Filter.Gte(p => p.Price, minPrice.Value));

            if (maxPrice.HasValue)
                filters.Add(Builders<Product>.Filter.Lte(p => p.Price, maxPrice.Value));

            var combinedFilter = filters.Count > 0
                ? Builders<Product>.Filter.And(filters)
                : FilterDefinition<Product>.Empty;

            var totalCount = (int)await _products.CountDocumentsAsync(combinedFilter, cancellationToken: cancellationToken);

            var products = await _products
                .Find(combinedFilter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Advanced search returned {Count} of {Total} products", products.Count, totalCount);
            return (products, totalCount);
        }

        public async Task<IEnumerable<Product>> GetByIdsAsync(
            IEnumerable<string> ids,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting products by IDs");

            var filter = Builders<Product>.Filter.In(p => p.Id, ids);
            var products = await _products.Find(filter).ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {Count} products by IDs", products.Count);
            return products;
        }

        public async Task BulkUpdateAsync(
            IEnumerable<Product> products,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Bulk updating products");

            var updates = products.Select(product =>
                new ReplaceOneModel<Product>(
                    Builders<Product>.Filter.Eq(p => p.Id, product.Id),
                    product
                )
            );

            var result = await _products.BulkWriteAsync(updates, cancellationToken: cancellationToken);

            _logger.LogInformation("Bulk update completed: {ModifiedCount} products updated", result.ModifiedCount);
        }
    }
}
