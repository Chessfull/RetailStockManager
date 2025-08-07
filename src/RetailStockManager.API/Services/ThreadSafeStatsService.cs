using RetailStockManager.Application.Interfaces.Services;

namespace RetailStockManager.API.Services
{
    public class ThreadSafeStatsService(
        IProductService productService,
        IStockService stockService,
        ILogger<ThreadSafeStatsService> logger)
    {
        private readonly IProductService _productService = productService;
        private readonly IStockService _stockService = stockService;
        private readonly ILogger<ThreadSafeStatsService> _logger = logger;

        private readonly Lock _statsLock = new();
        private DateTime _lastUpdated = DateTime.MinValue;
        private Dictionary<string, object> _cachedStats = [];

        public async Task<Dictionary<string, object>> GetCachedStatsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting cached statistics");


            lock (_statsLock)
            {
       
                if (_cachedStats.Count > 0 && DateTime.UtcNow - _lastUpdated < TimeSpan.FromMinutes(5))
                {
                    _logger.LogDebug("Returning cached statistics");
                    return new Dictionary<string, object>(_cachedStats);
                }
            }

            return await RefreshStatsAsync(cancellationToken);
        }

        public async Task<Dictionary<string, object>> RefreshStatsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Refreshing statistics cache");

            try
            {
                // Calculate stats
                var products = await _productService.GetAllAsync(cancellationToken);
                var stockSummary = await _stockService.GetStockSummaryAsync(cancellationToken);
                var alerts = await _stockService.GetStockAlertsAsync(cancellationToken);

                var categoryStats = products
                    .CountBy(p => p.Category) 
                    .ToDictionary(kvp => kvp.Key.ToString(), kvp => (object)kvp.Value);

                var priceStats = products
                    .AggregateBy( // .NET 9 LINQ yeniliği
                        keySelector: p => p.Category,
                        seed: new { Sum = 0m, Count = 0, Min = decimal.MaxValue, Max = decimal.MinValue },
                        func: (acc, product) => new
                        {
                            Sum = acc.Sum + product.Price,
                            Count = acc.Count + 1,
                            Min = Math.Min(acc.Min, product.Price),
                            Max = Math.Max(acc.Max, product.Price)
                        })
                    .ToDictionary(
                        kvp => kvp.Key.ToString(),
                        kvp => (object)new
                        {
                            Average = kvp.Value.Count > 0 ? kvp.Value.Sum / kvp.Value.Count : 0,
                            kvp.Value.Min,
                            kvp.Value.Max,
                            kvp.Value.Count
                        });

                var newStats = new Dictionary<string, object>
                {
                    ["totalProducts"] = products.Count(),
                    ["totalStockValue"] = stockSummary.TotalValue,
                    ["lowStockItems"] = stockSummary.LowStockItems,
                    ["outOfStockItems"] = stockSummary.OutOfStockItems,
                    ["alertsCount"] = alerts.Count(),
                    ["categoryBreakdown"] = categoryStats,
                    ["priceStatsByCategory"] = priceStats,
                    ["lastUpdated"] = DateTime.UtcNow
                };

                lock (_statsLock)
                {
                    _cachedStats = newStats;
                    _lastUpdated = DateTime.UtcNow;
                }

                _logger.LogInformation("Statistics cache refreshed successfully");
                return new Dictionary<string, object>(newStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh statistics cache");

                // Return cached data if available, empty dict otherwise
                lock (_statsLock)
                {
                    return _cachedStats.Count > 0
                        ? new Dictionary<string, object>(_cachedStats)
                        : [];
                }
            }
        }

        public async Task InvalidateCacheAsync()
        {
            _logger.LogInformation("Invalidating statistics cache");

            // Using EnterScope for more control
            using var scope = _statsLock.EnterScope();

            _cachedStats.Clear();
            _lastUpdated = DateTime.MinValue;

            _logger.LogInformation("Statistics cache invalidated");
            await Task.CompletedTask;
        }
    }
}
