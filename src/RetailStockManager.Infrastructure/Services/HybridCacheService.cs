using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using RetailStockManager.Application.Common.Interfaces;

namespace RetailStockManager.Infrastructure.Services
{
    public class HybridCacheService(
    HybridCache hybridCache,
    ILogger<HybridCacheService> logger) : ICacheService
    {
        private readonly HybridCache _hybridCache = hybridCache;
        private readonly ILogger<HybridCacheService> _logger = logger;

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            _logger.LogDebug("Getting cache value for key: {Key}", key);

            try
            {
                var result = await _hybridCache.GetOrCreateAsync<T>(
                    key,
                    async (cancel) => default!, // Factory function
                    cancellationToken: cancellationToken);

                if (result != null)
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                else
                    _logger.LogDebug("Cache miss for key: {Key}", key);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
        {
            _logger.LogDebug("Setting cache value for key: {Key}", key);

            try
            {
                var options = new HybridCacheEntryOptions
                {
                    Expiration = expiry ?? TimeSpan.FromHours(1),
                    LocalCacheExpiration = TimeSpan.FromMinutes(5)
                };

                await _hybridCache.SetAsync(key, value, options, tags: null, cancellationToken);

                _logger.LogDebug("Cache value set successfully for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Removing cache value for key: {Key}", key);

            try
            {
                // 🔥 .NET 9 YENİLİK: HybridCache remove operations
                await _hybridCache.RemoveAsync(key, cancellationToken);

                _logger.LogDebug("Cache value removed successfully for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            // HybridCache'de direct exists yok, get ile kontrol ediyoruz
            var value = await GetAsync<object>(key, cancellationToken);
            return value != null;
        }

        public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Pattern-based removal not directly supported in HybridCache for pattern: {Pattern}", pattern);

            // HybridCache pattern-based removal desteklemiyor
            // Bu functionality için ayrı implementation gerekebilir
            await Task.CompletedTask;
        }
    }
}
