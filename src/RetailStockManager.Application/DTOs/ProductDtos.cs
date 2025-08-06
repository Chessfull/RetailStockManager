using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailStockManager.Domain.Enums;

namespace RetailStockManager.Application.DTOs
{
    public record ProductDto(
    string Id,
    string Name,
    string Description,
    decimal Price,
    ProductCategory Category,
    string Sku,
    DateTime CreatedAt,
    DateTime UpdatedAt)
    {
        public List<string> Tags { get; init; } = [];
        public Dictionary<string, string> Metadata { get; init; } = [];
    }

    public record CreateProductDto(
        string Name,
        string Description,
        decimal Price,
        ProductCategory Category)
    {
        public List<string> Tags { get; init; } = [];
        public Dictionary<string, string> Metadata { get; init; } = [];
    }

    public record UpdateProductDto(
        string Id,
        string Name,
        string Description,
        decimal Price,
        ProductCategory Category)
    {
        public List<string> Tags { get; init; } = [];
        public Dictionary<string, string> Metadata { get; init; } = [];
    }

    public record ProductSearchDto(
        string? SearchTerm = null,
        ProductCategory? Category = null,
        decimal? MinPrice = null,
        decimal? MaxPrice = null,
        int Page = 1,
        int PageSize = 10);

    public record ProductSummaryDto(
        string Id,
        string Name,
        decimal Price,
        ProductCategory Category,
        string Sku);
}
