using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RetailStockManager.Application.Common.Interfaces;
using RetailStockManager.Infrastructure.Persistance;

namespace RetailStockManager.Infrastructure.Repositories
{
    public class MongoRepository<T>(
    MongoDbContext context,
    ILogger<MongoRepository<T>> logger) : IRepository<T>
    where T : class
    {
        private readonly IMongoCollection<T> _collection = GetCollection(context);
        private readonly ILogger<MongoRepository<T>> _logger = logger;

        public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting entity by ID: {Id}", id);

            var filter = Builders<T>.Filter.Eq("_id", id);
            var result = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

            if (result == null)
                _logger.LogWarning("Entity not found with ID: {Id}", id);

            return result;
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting all entities of type {Type}", typeof(T).Name);

            var results = await _collection.Find(FilterDefinition<T>.Empty)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {Count} entities", results.Count);
            return results;
        }

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Adding new entity of type {Type}", typeof(T).Name);

            await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);

            _logger.LogInformation("Entity added successfully");
            return entity;
        }

        public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Updating entity of type {Type}", typeof(T).Name);

            // Domain entity'den ID'yi reflection ile al
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty?.GetValue(entity) is not string id)
                throw new InvalidOperationException("Entity must have an Id property");

            var filter = Builders<T>.Filter.Eq("_id", id);
            var result = await _collection.ReplaceOneAsync(filter, entity, cancellationToken: cancellationToken);

            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("No entity found to update with ID: {Id}", id);
                throw new InvalidOperationException($"Entity with ID {id} not found");
            }

            _logger.LogInformation("Entity updated successfully");
            return entity;
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Deleting entity with ID: {Id}", id);

            var filter = Builders<T>.Filter.Eq("_id", id);
            var result = await _collection.DeleteOneAsync(filter, cancellationToken);

            if (result.DeletedCount == 0)
            {
                _logger.LogWarning("No entity found to delete with ID: {Id}", id);
                throw new InvalidOperationException($"Entity with ID {id} not found");
            }

            _logger.LogInformation("Entity deleted successfully");
        }

        public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            var count = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            return count > 0;
        }

        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Finding entities with predicate");

            var results = await _collection.Find(predicate).ToListAsync(cancellationToken);

            _logger.LogDebug("Found {Count} entities matching predicate", results.Count);
            return results;
        }

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting paged results: Page {Page}, Size {PageSize}", page, pageSize);

            var totalCount = (int)await _collection.CountDocumentsAsync(
                FilterDefinition<T>.Empty,
                cancellationToken: cancellationToken);

            var items = await _collection
                .Find(FilterDefinition<T>.Empty)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {Count} of {Total} total items", items.Count, totalCount);
            return (items, totalCount);
        }

        // Collection name convention: class name lowercase + 's'
        private static IMongoCollection<T> GetCollection(MongoDbContext context)
        {
            var collectionName = typeof(T).Name.ToLowerInvariant() + "s";
            return context.Database.GetCollection<T>(collectionName);
        }
    }
}
