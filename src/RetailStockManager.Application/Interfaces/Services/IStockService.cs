using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailStockManager.Application.DTOs.Common;
using RetailStockManager.Application.DTOs;
using RetailStockManager.Domain.Enums;

namespace RetailStockManager.Application.Interfaces.Services
{
    public interface IStockService
    {
        Task<StockItemDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<StockItemDto?> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default);
        Task<IEnumerable<StockItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<StockItemDto> CreateAsync(CreateStockItemDto createDto, CancellationToken cancellationToken = default);
        Task<StockItemDto> UpdateAsync(UpdateStockItemDto updateDto, CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);

        Task AdjustStockAsync(StockAdjustmentDto adjustmentDto, CancellationToken cancellationToken = default);

        Task<IEnumerable<StockItemDto>> GetByLocationAsync(
            string location,
            CancellationToken cancellationToken = default);

        Task TransferStockAsync(
            string productId,
            string fromLocation,
            string toLocation,
            int quantity,
            string userId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<StockAlertDto>> GetStockAlertsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<StockItemDto>> GetLowStockItemsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<StockItemDto>> GetOverStockItemsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<StockItemDto>> GetOutOfStockItemsAsync(CancellationToken cancellationToken = default);

        ValueTask<StockSummaryDto> GetStockSummaryAsync(CancellationToken cancellationToken = default);

        Task<PagedResultDto<StockItemDto>> GetStockReportAsync(
            string? location = null,
            StockStatus? status = null,
            int page = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default);
    }
}
