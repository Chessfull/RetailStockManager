using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailStockManager.Application.DTOs.Common;
using RetailStockManager.Application.DTOs;
using RetailStockManager.Application.Interfaces.Repositories;
using RetailStockManager.Application.Interfaces.Services;
using RetailStockManager.Domain.Entities;
using RetailStockManager.Domain.Enums;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace RetailStockManager.Application.Interfaces.Services
{
    public class ProductService(
     IProductRepository productRepository,
     IMapper mapper,
     ILogger<ProductService> logger) : IProductService
    {
        private readonly IProductRepository _productRepository = productRepository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<ProductService> _logger = logger;

        public async Task<ProductDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting product by ID: {ProductId}", id);

            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting all products");

            var products = await _productRepository.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto createDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating product: {ProductName}", createDto.Name);

            // Check if SKU would be unique (business logic)
            var product = _mapper.Map<Product>(createDto);
            var existingBySku = await _productRepository.GetBySkuAsync(product.Sku, cancellationToken);

            if (existingBySku != null)
            {
                // Generate new SKU if collision
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()[^6..];
                var namePrefix = createDto.Name.Length >= 3 ? createDto.Name[..3].ToUpper() : createDto.Name.ToUpper();
                var categoryCode = createDto.Category.ToString()[..2].ToUpper();

                // Use reflection to set SKU (since it's init-only)
                var skuProperty = typeof(Product).GetProperty(nameof(Product.Sku));
                skuProperty?.SetValue(product, $"{categoryCode}-{namePrefix}-{timestamp}");
            }

            var createdProduct = await _productRepository.AddAsync(product, cancellationToken);

            _logger.LogInformation("Product created successfully: {ProductId}", createdProduct.Id);
            return _mapper.Map<ProductDto>(createdProduct);
        }

        public async Task<ProductDto> UpdateAsync(UpdateProductDto updateDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating product: {ProductId}", updateDto.Id);

            var existingProduct = await _productRepository.GetByIdAsync(updateDto.Id, cancellationToken);
            if (existingProduct == null)
                throw new InvalidOperationException($"Product with ID {updateDto.Id} not found");

            // Update properties
            existingProduct.Name = updateDto.Name;
            existingProduct.Description = updateDto.Description;
            existingProduct.UpdatePrice(updateDto.Price);
            existingProduct.Category = updateDto.Category;

            // Update tags if provided
            if (updateDto.Tags.Any())
            {
                existingProduct.Tags.Clear();
                foreach (var tag in updateDto.Tags)
                    existingProduct.AddTag(tag);
            }

            var updatedProduct = await _productRepository.UpdateAsync(existingProduct, cancellationToken);

            _logger.LogInformation("Product updated successfully: {ProductId}", updatedProduct.Id);
            return _mapper.Map<ProductDto>(updatedProduct);
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting product: {ProductId}", id);

            await _productRepository.DeleteAsync(id, cancellationToken);

            _logger.LogInformation("Product deleted successfully: {ProductId}", id);
        }

        public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(
            ProductCategory category,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting products by category: {Category}", category);

            var products = await _productRepository.GetByCategoryAsync(category, cancellationToken);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<PagedResultDto<ProductDto>> SearchAsync(
            ProductSearchDto searchDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Searching products with filters");

            var (products, totalCount) = await _productRepository.SearchWithFiltersAsync(
                searchDto.SearchTerm,
                searchDto.Category,
                searchDto.MinPrice,
                searchDto.MaxPrice,
                searchDto.Page,
                searchDto.PageSize,
                cancellationToken);

            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            return PagedResultDto<ProductDto>.Create(
                productDtos,
                searchDto.Page,
                searchDto.PageSize,
                totalCount);
        }

        public async Task<ProductDto?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting product by SKU: {Sku}", sku);

            var product = await _productRepository.GetBySkuAsync(sku, cancellationToken);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        public async Task<IEnumerable<ProductDto>> GetByIdsAsync(
            IEnumerable<string> ids,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting products by IDs");

            var products = await _productRepository.GetByIdsAsync(ids, cancellationToken);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task BulkUpdatePricesAsync(
            Dictionary<string, decimal> priceUpdates,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Bulk updating prices for {Count} products", priceUpdates.Count);

            var products = await _productRepository.GetByIdsAsync(priceUpdates.Keys, cancellationToken);

            foreach (var product in products)
            {
                if (priceUpdates.TryGetValue(product.Id, out var newPrice))
                {
                    product.UpdatePrice(newPrice);
                }
            }

            await _productRepository.BulkUpdateAsync(products, cancellationToken);

            _logger.LogInformation("Bulk price update completed");
        }

        public async Task<IEnumerable<ProductSummaryDto>> GetTopSellingProductsAsync(
            int count = 10,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Getting top selling products");

            // Bu implementation şimdilik basic - gerçek satış datası olmadığı için
            var products = await _productRepository.GetAllAsync(cancellationToken);
            var topProducts = products.Take(count);

            return _mapper.Map<IEnumerable<ProductSummaryDto>>(topProducts);
        }

        public async Task<bool> IsSkuUniqueAsync(string sku, CancellationToken cancellationToken = default)
        {
            var product = await _productRepository.GetBySkuAsync(sku, cancellationToken);
            return product == null;
        }
    }
}
