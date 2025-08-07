# RetailStockManager | Modern Retail Stock Management System

![.NET](https://img.shields.io/badge/.NET%209-%23512BD4.svg?style=for-the-badge&logo=.net&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![MongoDB](https://img.shields.io/badge/MongoDB-%234ea94b.svg?style=for-the-badge&logo=mongodb&logoColor=white)
![Redis](https://img.shields.io/badge/redis-%23DD0031.svg?style=for-the-badge&logo=redis&logoColor=white)
![Apache Kafka](https://img.shields.io/badge/Apache%20Kafka-000?style=for-the-badge&logo=apachekafka)
![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)
![AWS](https://img.shields.io/badge/AWS-%23FF9900.svg?style=for-the-badge&logo=amazon-aws&logoColor=white)
![Terraform](https://img.shields.io/badge/terraform-%235835CC.svg?style=for-the-badge&logo=terraform&logoColor=white)

A **modern retail stock management system** built with **.NET 9** and **Clean Architecture**. This project demonstrates **enterprise-level** development practices with **real-time event streaming**, **multi-level caching**, and **cloud-native deployment** strategies.

## 🎯 Project Overview

RetailStockManager is designed to handle **high-volume retail operations** with a focus on **performance**, **scalability**, and **maintainability**. The system implements **event-driven architecture** for real-time stock updates and **advanced caching strategies** for optimal performance.

### Key Features
- 🔥 **.NET 9** with latest performance features (HybridCache, System.Threading.Lock)
- 🏗️ **Clean Architecture** with SOLID principles
- 📡 **Event-driven architecture** with Kafka
- ⚡ **Multi-level caching** (L1 Memory + L2 Redis)
- 🌩️ **Cloud-native** deployment with AWS ECS
- 🚀 **Real-time notifications** for stock alerts
- 📊 **Advanced analytics** with LINQ optimizations

## 🏛️ Architecture Overview

### Clean Architecture Layers
```
┌─────────────────────────────────────────────────────────┐
│                    API Layer                            │
│  Controllers, Dependency Injection, HTTP Concerns      │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┐
│               Application Layer                         │
│  Use Cases, DTOs, Service Interfaces, Validation      │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┐
│              Infrastructure Layer                       │
│  MongoDB, Redis, Kafka, External Services             │
└─────────────────────┬───────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────┘
│                 Domain Layer                            │
│  Business Entities, Domain Events, Business Rules     │
└─────────────────────────────────────────────────────────┘
```

### Event-Driven Architecture
- **Stock Changes** → Kafka Events → Real-time Analytics
- **Low Stock Alerts** → Automatic Notifications
- **Product Updates** → Cache Invalidation
- **Audit Trail** → Event Sourcing Pattern

## 🚀 Technology Stack

**Backend Framework**
- .NET 9 with latest C# 13 features
- ASP.NET Core Web API with Controllers
- Clean Architecture pattern

**Data Storage**
- MongoDB for document-based product data
- Redis for high-performance caching
- Event sourcing for audit trails

**Event Streaming**
- Apache Kafka for real-time events
- Producer/Consumer patterns
- Event-driven microservices communication

**Cloud Infrastructure**
- AWS ECS for container orchestration
- AWS DocumentDB (MongoDB-compatible)
- AWS ElastiCache (Redis)
- AWS MSK (Managed Kafka)

**DevOps & Deployment**
- Docker containerization
- .NET Aspire for development orchestration
- Terraform for Infrastructure as Code
- GitHub Actions for CI/CD

### Performance Optimizations Samples
```csharp
// HybridCache - L1 (Memory) + L2 (Redis) coordination
services.AddHybridCache(options => {
    options.DefaultEntryOptions = new() {
        Expiration = TimeSpan.FromMinutes(30),        // L2 cache
        LocalCacheExpiration = TimeSpan.FromMinutes(5) // L1 cache
    };
});

// System.Threading.Lock - 15% faster than Monitor
private readonly Lock _statsLock = new();
lock (_statsLock) { /* thread-safe operations */ }

// CountBy & AggregateBy - 30% faster LINQ operations
var categoryStats = products.CountBy(p => p.Category);
var priceStats = products.AggregateBy(p => p.Category, /* aggregation logic */);
```

## 📡 Event Streaming Architecture

### Kafka Topics
- `stock-events` - Stock level changes
- `product-events` - Product lifecycle events
- `alert-events` - Low stock notifications
- `audit-events` - System operations audit

### Event Flow
```
Product Update → Domain Event → Kafka Producer → Multiple Consumers
                                                      ↓
                                           ┌─ Analytics Service
                                           ├─ Notification Service  
                                           └─ Cache Invalidation
```

## 🛠️ Getting Started

### Prerequisites
- .NET 9 SDK
- Docker Desktop
- Git

### Quick Start with Aspire
```bash
# Clone the repository
git clone https://github.com/yourusername/RetailStockManager.git
cd RetailStockManager

# Start all services with Aspire
cd aspire/RetailStockManager.AppHost
dotnet run

# Access Aspire Dashboard
# http://localhost:15888
```

### What Aspire Starts
- 🗄️ **MongoDB** - Document database
- ⚡ **Redis** - Cache layer
- 📡 **Kafka** - Event streaming
- 🌐 **API** - REST endpoints
- 📊 **Management UIs** - MongoExpress, RedisInsight, KafkaUI

### Manual Setup
```bash
# Build the solution
dotnet build

# Run API manually
cd src/RetailStockManager.API
dotnet run

# API available at: https://localhost:5001
```

## 📊 API Endpoints

### Products
- `GET /api/products` - List all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

### Stock Management
- `GET /api/stock` - View all stock items
- `POST /api/stock/adjust` - Adjust stock levels
- `GET /api/stock/alerts` - Get stock alerts
- `GET /api/stock/low` - Get low stock items

### Analytics
- `GET /api/products/stats` - Product statistics
- `GET /api/stock/summary` - Stock summary

## 🌩️ Cloud Deployment

### AWS Infrastructure
```bash
# Deploy with Terraform
cd terraform
terraform init
terraform plan
terraform apply

# Infrastructure includes:
# - ECS Fargate cluster
# - Application Load Balancer
# - DocumentDB cluster
# - ElastiCache Redis
# - MSK Kafka cluster
# - VPC with public/private subnets
```

### CI/CD Pipeline
- **GitHub Actions** for automated testing
- **Docker** image building and pushing to ECR
- **Blue/Green deployments** with ECS
- **Health checks** and automatic rollback

________________________________________________________________________________________________________________________

# :incoming_envelope: Contact Information :incoming_envelope:

For any questions or further information, please don't hesitate to contact me :pray:

Email: merttopcu.dev@gmail.com

LinkedIn: https://www.linkedin.com/in/mert-topcu/

Happy Coding ❤️
