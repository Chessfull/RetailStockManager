using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailStockManager.Infrastructure.Persistance
{
    public class MongoDbSettings(string connectionString, string databaseName)
    {
        public string ConnectionString { get; init; } = connectionString;
        public string DatabaseName { get; init; } = databaseName;

        // Default constructor için
        public MongoDbSettings() : this(string.Empty, string.Empty) { }
    }
}
