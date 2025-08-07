using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.MongoDb;
using Microsoft.Extensions.Caching.Hybrid;
using RetailStockManager.API.Mappings;
using RetailStockManager.API.Services;
using RetailStockManager.API.Validators;
using RetailStockManager.Application.Common.Interfaces;
using RetailStockManager.Application.Interfaces.Repositories;
using RetailStockManager.Application.Interfaces.Services;
using RetailStockManager.Infrastructure.Persistance;
using RetailStockManager.Infrastructure.Repositories;
using RetailStockManager.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .Enrich.FromLogContext());


builder.Services.AddControllers();

builder.Services.AddOpenApi();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3001") // Next.js
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(30),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };
});

// AutoMapper Configuration
builder.Services.AddAutoMapper(typeof(ProductMappingProfile));

// FluentValidation Configuration
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();

//  Repository Layer - Generic and Specific
builder.Services.AddScoped(typeof(IRepository<>), typeof(MongoRepository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
//builder.Services.AddScoped<IStockRepository, StockRepository>();

// Application Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStockService, StockService>();

// Infrastructure Services
builder.Services.AddScoped<ICacheService, HybridCacheService>();
builder.Services.AddScoped<ThreadSafeStatsService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<MongoDbHealthCheck>("mongodb")
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), ["live"]);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Middleware Pipeline
app.UseSerilogRequestLogging();
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();

// Map Controllers - bizim API endpoints'lerimiz
app.MapControllers();

// Health Check Endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("live")
});

// Global Exception Handling
app.UseExceptionHandler("/error");
app.Map("/error", (HttpContext context) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogError("An unhandled exception occurred");
    return Results.Problem("An error occurred processing your request");
});

var summaries = new[]
{
    "Low Stock", "Normal Stock", "High Stock", "Overstock", "Critical", "Optimal", "Warning", "Good", "Excellent", "Perfect"
};

app.MapGet("/api/test/stock-status", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new StockStatusTest
        (
            $"Product-{index:D3}",
            Random.Shared.Next(0, 100),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetStockStatusTest")
.WithTags("Test");

// Application startup
try
{
    Log.Information("Starting RetailStockManager API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

record StockStatusTest(string ProductCode, int Quantity, string? Status)
{
    public string StockLevel => Quantity switch
    {
        0 => "Out of Stock",
        < 10 => "Low Stock",
        < 50 => "Normal Stock",
        < 100 => "High Stock",
        _ => "Overstock"
    };
}