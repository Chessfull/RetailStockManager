using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailStockManager.Domain.Common
{
    public abstract class BaseEntity(string? id=null)
    {
        public string Id { get; set; } = id ?? Guid.NewGuid().ToString();
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsNew => CreatedAt == UpdatedAt;

    }
}
