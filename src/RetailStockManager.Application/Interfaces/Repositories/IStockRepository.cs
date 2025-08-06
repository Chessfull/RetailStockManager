using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailStockManager.Application.Common.Interfaces;
using RetailStockManager.Domain.Entities;
using RetailStockManager.Domain.Enums;

namespace RetailStockManager.Application.Interfaces.Repositories
{
    public interface IStockRepository : IRepository<StockItem>
    {
        Task<IEnumerable<StockItem>> GetByLocationAsync(
            string location,
            CancellationToken cancellationToken = default);

        Task<StockItem?> GetByProductIdAsync(
            string productId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<StockItem>> GetLowStockItemsAsync(
            CancellationToken cancellationToken = default);

        Task<IEnumerable<StockItem>> GetOverStockItemsAsync(
            CancellationToken cancellationToken = default);

        Task<IEnumerable<StockItem>> GetOutOfStockItemsAsync(
            CancellationToken cancellationToken = default);

        Task<IEnumerable<StockItem>> GetByStatusAsync(
            StockStatus status,
            CancellationToken cancellationToken = default);

        Task<int> GetTotalStockCountAsync(CancellationToken cancellationToken = default);
        Task<decimal> GetTotalStockValueAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<StockItem>> GetItemsNeedingReorderAsync(
            CancellationToken cancellationToken = default);

        Task<IEnumerable<StockItem>> GetMultiLocationStockAsync(
            string productId,
            CancellationToken cancellationToken = default);
    }
}
