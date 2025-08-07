using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RetailStockManager.API.Services;
using RetailStockManager.Application.DTOs.Common;
using RetailStockManager.Application.DTOs;
using RetailStockManager.Application.Interfaces.Services;
using RetailStockManager.Domain.Enums;

namespace RetailStockManager.API.Controllers
{
   [ApiController]
   [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController(
    IProductService productService,
    IMapper mapper,
    ThreadSafeStatsService statsService,
    ILogger<ProductsController> logger) : ControllerBase
    {
        private readonly IProductService _productService = productService;
        private readonly IMapper _mapper = mapper;
        private readonly ThreadSafeStatsService _statsService = statsService;
        private readonly ILogger<ProductsController> _logger = logger;

        /// <summary>
        /// Get all products
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all products");

            var products = await _productService.GetAllAsync(cancellationToken);
            return Ok(products);
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetProduct(string id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", id);

            var product = await _productService.GetByIdAsync(id, cancellationToken);

            if (product == null)
            {
                _logger.LogWarning("Product not found: {ProductId}", id);
                return NotFound($"Product with ID {id} not found");
            }

            return Ok(product);
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        [HttpGet("category/{category}")]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(
            ProductCategory category,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting products by category: {Category}", category);

            var products = await _productService.GetByCategoryAsync(category, cancellationToken);
            return Ok(products);
        }

        /// <summary>
        /// Search products with filters
        /// </summary>
        [HttpPost("search")]
        [ProducesResponseType(typeof(PagedResultDto<ProductDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<ProductDto>>> SearchProducts(
            [FromBody] ProductSearchDto searchDto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Searching products with filters");

            var result = await _productService.SearchAsync(searchDto, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Create new product
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> CreateProduct(
            [FromBody] CreateProductDto createDto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new product: {ProductName}", createDto.Name);

            try
            {
                var product = await _productService.CreateAsync(createDto, cancellationToken);

                // Invalidate stats cache
                await _statsService.InvalidateCacheAsync();

                return CreatedAtAction(
                    nameof(GetProduct),
                    new { id = product.Id },
                    product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update existing product
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> UpdateProduct(
            string id,
            [FromBody] UpdateProductDto updateDto,
            CancellationToken cancellationToken)
        {
            if (id != updateDto.Id)
            {
                _logger.LogWarning("ID mismatch: URL {UrlId} vs Body {BodyId}", id, updateDto.Id);
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Updating product: {ProductId}", id);

            try
            {
                var product = await _productService.UpdateAsync(updateDto, cancellationToken);

                // Invalidate stats cache
                await _statsService.InvalidateCacheAsync();

                return Ok(product);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Product not found for update: {ProductId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete product
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(string id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting product: {ProductId}", id);

            try
            {
                await _productService.DeleteAsync(id, cancellationToken);

                // Invalidate stats cache
                await _statsService.InvalidateCacheAsync();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Product not found for deletion: {ProductId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get product statistics
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Dictionary<string, object>>> GetProductStatistics(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting product statistics");

            var stats = await _statsService.GetCachedStatsAsync(cancellationToken);
            return Ok(stats);
        }

        /// <summary>
        /// Refresh statistics cache
        /// </summary>
        [HttpPost("stats/refresh")]
        [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Dictionary<string, object>>> RefreshStatistics(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Refreshing product statistics");

            var stats = await _statsService.RefreshStatsAsync(cancellationToken);
            return Ok(stats);
        }
    }
}
