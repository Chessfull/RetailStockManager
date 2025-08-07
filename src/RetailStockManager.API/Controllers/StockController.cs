using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RetailStockManager.Application.DTOs;
using RetailStockManager.Application.Interfaces.Services;

namespace RetailStockManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class StockController(
    IStockService stockService,
    IMapper mapper,
    ILogger<StockController> logger) : ControllerBase
    {
        private readonly IStockService _stockService = stockService;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<StockController> _logger = logger;

        /// <summary>
        /// Get all stock items
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<StockItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<StockItemDto>>> GetAllStock(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all stock items");

            var stockItems = await _stockService.GetAllAsync(cancellationToken);
            return Ok(stockItems);
        }

        /// <summary>
        /// Get stock by product ID
        /// </summary>
        [HttpGet("product/{productId}")]
        [ProducesResponseType(typeof(StockItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StockItemDto>> GetStockByProductId(
            string productId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting stock for product: {ProductId}", productId);

            var stockItem = await _stockService.GetByProductIdAsync(productId, cancellationToken);

            if (stockItem == null)
            {
                _logger.LogWarning("Stock not found for product: {ProductId}", productId);
                return NotFound($"Stock not found for product {productId}");
            }

            return Ok(stockItem);
        }

        /// <summary>
        /// Get stock by location
        /// </summary>
        [HttpGet("location/{location}")]
        [ProducesResponseType(typeof(IEnumerable<StockItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<StockItemDto>>> GetStockByLocation(
            string location,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting stock for location: {Location}", location);

            var stockItems = await _stockService.GetByLocationAsync(location, cancellationToken);
            return Ok(stockItems);
        }

        /// <summary>
        /// Create stock item
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(StockItemDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<StockItemDto>> CreateStock(
            [FromBody] CreateStockItemDto createDto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating stock for product: {ProductId}", createDto.ProductId);

            try
            {
                var stockItem = await _stockService.CreateAsync(createDto, cancellationToken);

                return CreatedAtAction(
                    nameof(GetStockByProductId),
                    new { productId = stockItem.ProductId },
                    stockItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stock item");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Adjust stock levels
        /// </summary>
        [HttpPost("adjust")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AdjustStock(
            [FromBody] StockAdjustmentDto adjustmentDto,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Adjusting stock for product: {ProductId}, Change: {Change}",
                adjustmentDto.ProductId, adjustmentDto.QuantityChange);

            try
            {
                await _stockService.AdjustStockAsync(adjustmentDto, cancellationToken);
                return Ok(new { message = "Stock adjusted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting stock");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get stock alerts
        /// </summary>
        [HttpGet("alerts")]
        [ProducesResponseType(typeof(IEnumerable<StockAlertDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<StockAlertDto>>> GetStockAlerts(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting stock alerts");

            var alerts = await _stockService.GetStockAlertsAsync(cancellationToken);
            return Ok(alerts);
        }

        /// <summary>
        /// Get low stock items
        /// </summary>
        [HttpGet("low")]
        [ProducesResponseType(typeof(IEnumerable<StockItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<StockItemDto>>> GetLowStockItems(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting low stock items");

            var lowStockItems = await _stockService.GetLowStockItemsAsync(cancellationToken);
            return Ok(lowStockItems);
        }

        /// <summary>
        /// Get stock summary
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(StockSummaryDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<StockSummaryDto>> GetStockSummary(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting stock summary");

            var summary = await _stockService.GetStockSummaryAsync(cancellationToken);
            return Ok(summary);
        }
    }
}
