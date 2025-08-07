using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace RetailStockManager.Infrastructure.Persistance
{
    public class MongoDbContext(IOptions<MongoDbSettings> options, ILogger<MongoDbContext> logger)
    {
        private readonly MongoDbSettings _settings = options.Value;
        private readonly ILogger<MongoDbContext> _logger = logger;
        private readonly Lazy<IMongoDatabase> _database = new(() =>
        {
            var client = new MongoClient(options.Value.ConnectionString);
            return client.GetDatabase(options.Value.DatabaseName);
        });

        public IMongoDatabase Database => _database.Value;

        // Collections
        public IMongoCollection<Domain.Entities.Product> Products =>
            Database.GetCollection<Domain.Entities.Product>("products");

        public IMongoCollection<Domain.Entities.StockItem> StockItems =>
            Database.GetCollection<Domain.Entities.StockItem>("stock_items");

        // Health check connection test
        public async Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await Database.RunCommandAsync<object>("{ping:1}", cancellationToken: cancellationToken);
                _logger.LogInformation("MongoDB connection successful");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MongoDB connection failed");
                return false;
            }
        }
    }
}