using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailStockManager.Domain.Enums
{
    public enum StockStatus
    {
        OutOfStock = 0,
        LowStock = 1,
        InStock = 2,
        OverStock = 3
    }

    public enum StockMovementType
    {
        Purchase = 1,      
        Sale = 2,          
        Transfer = 3,      
        Adjustment = 4,    
        Damage = 5,        
        Return = 6        
    }
}
