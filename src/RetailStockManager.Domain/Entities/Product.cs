using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetailStockManager.Domain.Common;
using RetailStockManager.Domain.Enums;

namespace RetailStockManager.Domain.Entities
{
    public class Product(string name, string description, decimal price, ProductCategory category):BaseEntity
    {
        private string _name = ValidateName(name);
        private decimal _price = ValidatePrice(price);
        public string Name
        {
            get => _name;
            set => _name = ValidateName(value);
        }
        public string Description { get; set; } = description ?? string.Empty;

        public decimal Price
        {
            get => _price;
            set => _price = ValidatePrice(value);
        }
        public ProductCategory Category { get; set; } = category;
        public string Sku { get; init; } = GenerateSku(name, category);
        public List<string> Tags { get; set; } = [];
        public Dictionary<string, string> Metadata { get; set; } = [];
        private static string GenerateSku(string name, ProductCategory category)
        {
            var namePrefix = name.Length >= 3 ? name[..3].ToUpper() : name.ToUpper();
            var categoryCode = category.ToString()[..2].ToUpper();
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()[^6..];

            return $"{categoryCode}-{namePrefix}-{timestamp}";
        }
        public void UpdatePrice(decimal newPrice)
        {
            Price = newPrice; // Validation in setter
            UpdatedAt = DateTime.UtcNow;
        }
        public void AddTag(string tag)
        {
            if (!string.IsNullOrWhiteSpace(tag) && !Tags.Contains(tag))
            {
                Tags.Add(tag);
                UpdatedAt = DateTime.UtcNow;
            }
        }
        public void RemoveTag(string tag)
        {
            if (Tags.Remove(tag))
                UpdatedAt = DateTime.UtcNow;
        }
        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty");
            if (name.Length < 2)
                throw new ArgumentException("Product name must be at least 2 characters");
            if (name.Length > 100)
                throw new ArgumentException("Product name cannot exceed 100 characters");

            return name.Trim();
        }
        private static decimal ValidatePrice(decimal price)
        {
            if (price <= 0)
                throw new ArgumentException("Product price must be greater than zero");
            if (price > 999999.99m)
                throw new ArgumentException("Product price cannot exceed 999,999.99");

            return price;
        }
    }
}
