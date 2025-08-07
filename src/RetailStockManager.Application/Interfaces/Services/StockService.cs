using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailStockManager.Application.DTOs.Common;
using RetailStockManager.Application.DTOs;
using RetailStockManager.Application.Interfaces.Repositories;
using RetailStockManager.Domain.Entities;
using RetailStockManager.Domain.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace RetailStockManager.Application.Interfaces.Services
{
    public class StockService(
     IStockRepository stockRepository,
     IProductRepository productRepository,
     IMapper mapper,
     ILogger<StockService> logger) : IStockService
    {
        private readonly IStockRepository _stockRepository = stockRepository;
        private readonly IProductRepository _productRepository = productRepository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<StockService> _logger = logger;

        public async Task<StockItemDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var stockItem = await _stockRepository.GetByIdAsync(id, cancellationToken);
            return stockItem != null ? _mapper.Map<StockItemDto>(stockItem) : null;
        }

        public async Task<StockItemDto?> GetByProductIdAsync(string productId, CancellationToken cancellationToken = default)
        {
            var stockItem = await _stockRepository.GetByProductIdAsync(productId, cancellationToken);
            return stockItem != null ? _mapper.Map<StockItemDto>(stockItem) : null;
        }

        public async Task<IEnumerable<StockItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var stockItems = await _stockRepository.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<StockItemDto>>(stockItems);
        }

        public async Task<StockItemDto> CreateAsync(CreateStockItemDto createDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating stock item for product: {ProductId}", createDto.ProductId);

            // Verify product exists
            var product = await _productRepository.GetByIdAsync(createDto.ProductId, cancellationToken);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {createDto.ProductId} not found");

            var stockItem = _mapper.Map<StockItem>(createDto);
            var createdStock = await _stockRepository.AddAsync(stockItem, cancellationToken);

            _logger.LogInformation("Stock item created successfully: {StockId}", createdStock.Id);
            return _mapper.Map<StockItemDto>(createdStock);
        }

        public async Task<StockItemDto> UpdateAsync(UpdateStockItemDto updateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating stock item: {StockId}", updateDto.Id);

            var existingStock = await _stockRepository.GetByIdAsync(updateDto.Id, cancellationToken);
            if (existingStock == null)
                throw new InvalidOperationException($"Stock item with ID {updateDto.Id} not found");

            // Update properties
            existingStock.Quantity = updateDto.Quantity;
            existingStock.Location = updateDto.Location;
            existingStock.SetReorderLevels(updateDto.ReorderLevel, updateDto.MaxLevel);

            var updatedStock = await _stockRepository.UpdateAsync(existingStock, cancellationToken);

            _logger.LogInformation("Stock item updated successfully: {StockId}", updatedStock.Id);
            return _mapper.Map<StockItemDto>(updatedStock);
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            await _stockRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task AdjustStockAsync(StockAdjustmentDto adjustmentDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Adjusting stock for product: {ProductId}, Change: {Change}",
                adjustmentDto.ProductId, adjustmentDto.QuantityChange);

            var stockItem = await _stockRepository.GetByProductIdAsync(adjustmentDto.ProductId, cancellationToken);
            if (stockItem == null)
                throw new InvalidOperationException($"Stock not found for product {adjustmentDto.ProductId}");

            stockItem.AdjustQuantity(adjustmentDto.QuantityChange, adjustmentDto.MovementType);

            await _stockRepository.UpdateAsync(stockItem, cancellationToken);

            _logger.LogInformation("Stock adjustment completed for product: {ProductId}", adjustmentDto.ProductId);
        }

        public async Task<IEnumerable<StockItemDto>> GetByLocationAsync(
            string location,
            CancellationToken cancellationToken = default)
        {
            var stockItems = await _stockRepository.GetByLocationAsync(location, cancellationToken);
            return _mapper.Map<IEnumerable<StockItemDto>>(stockItems);
        }

        public async Task TransferStockAsync(
            string productId,
            string fromLocation,
            string toLocation,
            int quantity,
            string userId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Transferring stock: Product {ProductId}, {Quantity} from {From} to {To}",
                productId, quantity, fromLocation, toLocation);

            // Get source stock
            var sourceStock = await _stockRepository.GetByProductIdAsync(productId, cancellationToken);
            if (sourceStock == null || sourceStock.Location != fromLocation)
                throw new InvalidOperationException($"Source stock not found for product {productId} at {fromLocation}");

            if (sourceStock.Quantity < quantity)
                throw new InvalidOperationException($"Insufficient stock. Available: {sourceStock.Quantity}, Requested: {quantity}");

            // Adjust source
            sourceStock.AdjustQuantity(-quantity, StockMovementType.Transfer);
            await _stockRepository.UpdateAsync(sourceStock, cancellationToken);

            // Handle destination stock
            // Bu kısım basitleştirilmiş - gerçek implementasyon daha complex olacak

            _logger.LogInformation("Stock transfer completed");
        }

        public async Task<IEnumerable<StockAlertDto>> GetStockAlertsAsync(CancellationToken cancellationToken = default)
        {
            var lowStockItems = await _stockRepository.GetLowStockItemsAsync(cancellationToken);
            var outOfStockItems = await _stockRepository.GetOutOfStockItemsAsync(cancellationToken);

            var alerts = new List<StockAlertDto>();

            foreach (var item in lowStockItems)
            {
                alerts.Add(new StockAlertDto(
                    item.ProductId,
                    $"Product-{item.ProductId[^6..]}", // Simplified product name
                    item.Location,
                    item.Quantity,
                    item.ReorderLevel,
                    item.Status,
                    $"Low stock alert: {item.Quantity} remaining",
                    DateTime.UtcNow));
            }

            foreach (var item in outOfStockItems)
            {
                alerts.Add(new StockAlertDto(
                    item.ProductId,
                    $"Product-{item.ProductId[^6..]}", // Simplified product name
                    item.Location,
                    item.Quantity,
                    item.ReorderLevel,
                    item.Status,
                    "Out of stock!",
                    DateTime.UtcNow));
            }

            return alerts;
        }

        public async Task<IEnumerable<StockItemDto>> GetLowStockItemsAsync(CancellationToken cancellationToken = default)
        {
            var stockItems = await _stockRepository.GetLowStockItemsAsync(cancellationToken);
            return _mapper.Map<IEnumerable<StockItemDto>>(stockItems);
        }

        public async Task<IEnumerable<StockItemDto>> GetOverStockItemsAsync(CancellationToken cancellationToken = default)
        {
            var stockItems = await _stockRepository.GetOverStockItemsAsync(cancellationToken);
            return _mapper.Map<IEnumerable<StockItemDto>>(stockItems);
        }

        public async Task<IEnumerable<StockItemDto>> GetOutOfStockItemsAsync(CancellationToken cancellationToken = default)
        {
            var stockItems = await _stockRepository.GetOutOfStockItemsAsync(cancellationToken);
            return _mapper.Map<IEnumerable<StockItemDto>>(stockItems);
        }

        public async ValueTask<StockSummaryDto> GetStockSummaryAsync(CancellationToken cancellationToken = default)
        {
            var allStock = await _stockRepository.GetAllAsync(cancellationToken);
            var lowStock = await _stockRepository.GetLowStockItemsAsync(cancellationToken);
            var outOfStock = await _stockRepository.GetOutOfStockItemsAsync(cancellationToken);
            var overStock = await _stockRepository.GetOverStockItemsAsync(cancellationToken);

            return new StockSummaryDto(
                TotalProducts: allStock.Count(),
                LowStockItems: lowStock.Count(),
                OutOfStockItems: outOfStock.Count(),
                OverStockItems: overStock.Count(),
                TotalValue: 0 // Simplified - would need product prices
            );
        }

        public async Task<PagedResultDto<StockItemDto>> GetStockReportAsync(
            string? location = null,
            StockStatus? status = null,
            int page = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            // Simplified implementation
            var allStock = await _stockRepository.GetAllAsync(cancellationToken);

            var filtered = allStock.AsQueryable();

            if (!string.IsNullOrEmpty(location))
                filtered = filtered.Where(s => s.Location.Equals(location, StringComparison.OrdinalIgnoreCase));

            if (status.HasValue)
                filtered = filtered.Where(s => s.Status == status.Value);

            var totalCount = filtered.Count();
            var items = filtered.Skip((page - 1) * pageSize).Take(pageSize);

            var itemDtos = _mapper.Map<IEnumerable<StockItemDto>>(items);

            return PagedResultDto<StockItemDto>.Create(itemDtos, page, pageSize, totalCount);
        }
    }
}
