using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailStockManager.Application.DTOs.Common;
using RetailStockManager.Application.DTOs;

namespace RetailStockManager.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<ProductDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ProductDto> CreateAsync(CreateProductDto createDto, CancellationToken cancellationToken = default);
        Task<ProductDto> UpdateAsync(UpdateProductDto updateDto, CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);

        Task<IEnumerable<ProductDto>> GetByCategoryAsync(
            Domain.Enums.ProductCategory category,
            CancellationToken cancellationToken = default);

        Task<PagedResultDto<ProductDto>> SearchAsync(
            ProductSearchDto searchDto,
            CancellationToken cancellationToken = default);

        Task<ProductDto?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

        Task<IEnumerable<ProductDto>> GetByIdsAsync(
            IEnumerable<string> ids,
            CancellationToken cancellationToken = default);

        Task BulkUpdatePricesAsync(
            Dictionary<string, decimal> priceUpdates,
            CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductSummaryDto>> GetTopSellingProductsAsync(
            int count = 10,
            CancellationToken cancellationToken = default);

        Task<bool> IsSkuUniqueAsync(string sku, CancellationToken cancellationToken = default);
    }
}
