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
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetByCategoryAsync(
            ProductCategory category,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Product>> SearchAsync(
            string searchTerm,
            CancellationToken cancellationToken = default);

        Task<Product?> GetBySkuAsync(
            string sku,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Product>> GetByPriceRangeAsync(
            decimal minPrice,
            decimal maxPrice,
            CancellationToken cancellationToken = default);

        Task<(IEnumerable<Product> Products, int TotalCount)> SearchWithFiltersAsync(
            string? searchTerm,
            ProductCategory? category,
            decimal? minPrice,
            decimal? maxPrice,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Product>> GetByIdsAsync(
            IEnumerable<string> ids,
            CancellationToken cancellationToken = default);

        Task BulkUpdateAsync(
            IEnumerable<Product> products,
            CancellationToken cancellationToken = default);
    }
}
