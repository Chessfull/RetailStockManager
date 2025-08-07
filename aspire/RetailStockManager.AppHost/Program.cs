using Aspire.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

// MongoDB container with persistent storage
var mongodb = builder.AddMongoDB("mongodb")
    .WithDataVolume() // Persistent data storage
    .WithMongoExpress(); // Web UI for development

// Redis container for caching
var redis = builder.AddRedis("redis")
    .WithDataVolume() // Persistent cache storage
    .WithRedisInsight(); // Redis management UI

// Kafka container for event streaming
var kafka = builder.AddKafka("kafka")
    .WithKafkaUI(); // Kafka management UI

// 🔧 Main API Service
var apiService = builder.AddProject("api", "../src/RetailStockManager.API/RetailStockManager.API.csproj")
    .WithReference(mongodb) // Automatic connection string injection
    .WithReference(redis)   // Automatic Redis configuration
    .WithReference(kafka)   // Kafka configuration
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

// Frontend placeholder - Next.js için hazır
// var frontend = builder.AddNpmApp("frontend", "../frontend", "dev")
//     .WithReference(apiService)
//     .WithEnvironment("API_URL", apiService.GetEndpoint("https"))
//     .WithHttpEndpoint(port: 3000, env: "PORT");

// Service discovery ve health monitoring
builder.Services.AddHealthChecks();

var app = builder.Build();

app.Run();