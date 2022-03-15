# dotnet-saga-orchestrator

A production-ready distributed saga orchestrator for .NET microservices implementing the Saga pattern with compensating transactions.

![Build](https://github.com/sarmkadan/dotnet-saga-orchestrator/actions/workflows/build.yml/badge.svg)
![License](https://img.shields.io/badge/License-MIT-blue.svg)

## Installation

### Method 1: Clone from Repository

```bash
git clone https://github.com/Sarmkadan/dotnet-saga-orchestrator.git
cd dotnet-saga-orchestrator
dotnet build
```

### Method 2: Add NuGet Package

```bash
dotnet add package Zaiets.dotnet.saga.orchestrator
```

## Quick Start

```csharp
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Application.Services;

// Setup dependency injection
var services = new ServiceCollection();
services.AddSagaOrchestrator();
var provider = services.BuildServiceProvider();

// Get services
var definitionService = provider.GetRequiredService<SagaDefinitionService>();
var orchestration = provider.GetRequiredService<SagaOrchestrationService>();

// Create saga definition
var definition = await definitionService.CreateDefinitionAsync("Order Processing", "Process orders");

// Add a step
await definitionService.AddStepAsync(definition.Id, new SagaStepDefinition(
    "Reserve Inventory", "inventory-service", "http://inventory/reserve", "http://inventory/release"));

// Execute saga
var saga = await orchestration.CreateSagaAsync(definition);
await orchestration.StartSagaAsync(saga.Id);
await orchestration.ExecuteNextStepAsync(saga.Id);
```

## Configuration

The orchestrator can be configured via `ServiceCollection` extensions:

```csharp
var services = new ServiceCollection();
services.AddSagaOrchestrator()
    .WithDefaultStepTimeout(60)
    .WithDefaultMaxRetries(5)
    .Build();
```

## Examples

For practical implementations of the saga orchestrator, check out the [examples/](examples/) directory:

- [BasicUsage.cs](examples/BasicUsage.cs): A minimal setup showing the core flow.
- [AdvancedUsage.cs](examples/AdvancedUsage.cs): Demonstrates configuration, retry policies, and error handling.
- [IntegrationExample.cs](examples/IntegrationExample.cs): Shows how to register the orchestrator in a Dependency Injection container.
- Other advanced scenarios include [MoneyTransfer.cs](examples/MoneyTransfer.cs) and [OrderProcessing.cs](examples/OrderProcessing.cs).

## Docker

This project includes a `docker-compose.yml` file to quickly spin up the orchestrator alongside necessary dependencies like Redis for distributed scenarios, as well as mock services for testing.

### Running with Docker

```bash
docker-compose up -d
```

This will start:
- `saga-orchestrator`: The main application.
- `redis`: Used for caching and distributed coordination.
- Mock services (inventory, payment, shipping) for testing examples.
- Prometheus and Grafana for metrics monitoring (optional).

You can stop the services using:

```bash
docker-compose down
```

## SagaOrchestratorBenchmarks

The `SagaOrchestratorBenchmarks` class provides performance benchmarks for measuring the throughput, efficiency, and memory usage of the saga orchestrator under various load scenarios. It uses BenchmarkDotNet to track execution time, memory allocations, and operations per second across key operations like definition creation, step addition, saga creation, and step execution.


### Usage Example

```csharp
using System;
using SagaOrchestrator.Benchmarks;

// Run all benchmarks and display results
SagaOrchestratorBenchmarks.Main(Array.Empty<string>());

// Or run benchmarks programmatically
var benchmarks = new SagaOrchestratorBenchmarks();
benchmarks.Setup(); // Initialize services and create baseline definition/saga
benchmarks.CreateDefinition(); // Benchmark definition creation
benchmarks.AddStep(); // Benchmark step addition
benchmarks.CreateSaga(); // Benchmark saga creation
benchmarks.StartSaga(); // Benchmark saga startup
benchmarks.ExecuteNextStep(); // Benchmark step execution
```

## Performance Benchmarks

Performance benchmarks are available to measure the throughput and efficiency of the saga orchestrator. These benchmarks help identify performance characteristics under different loads and configurations.

### Running Benchmarks

To run the benchmarks, use the following commands:

```bash
# Navigate to benchmarks directory
cd benchmarks

# Run all benchmarks (generates detailed report)
dotnet run -c Release -- --filter *

# Run specific benchmark
# Example: benchmark saga creation performance
dotnet run --project benchmarks/dotnet-saga-orchestrator.Benchmarks.csproj -c Release -- --filter SagaOrchestratorBenchmarks.CreateSagaInstance_Benchmark
```

The benchmarks include:
- **CreateSagaDefinition_Benchmark**: Measures the time to create saga definitions with varying step counts
- **CreateSagaInstance_Benchmark**: Measures saga instance creation performance
- **ExecuteSagaSteps_Benchmark**: Measures the throughput of saga step execution
- **ListSagas_Benchmark**: Measures the performance of listing sagas
- **GetSagaById_Benchmark**: Measures the performance of retrieving individual sagas

Each benchmark tracks:
- **Execution time**: How fast operations complete
- **Memory allocation**: GC pressure and memory usage
- **Throughput**: Operations per second

### Sample Benchmark Results

The following table shows typical benchmark results (your actual results may vary based on hardware):

| Benchmark | Saga Steps | Iterations | Mean (ms) | Allocated (B) | Throughput |
|-----------|------------|------------|-----------|---------------|------------|
| CreateSagaDefinition | 1 | 100 | ~1.2 | ~8,500 | ~83 ops/s |
| CreateSagaDefinition | 5 | 100 | ~3.8 | ~22,000 | ~26 ops/s |
| CreateSagaDefinition | 10 | 100 | ~7.5 | ~45,000 | ~13 ops/s |
| CreateSagaInstance | 1 | 1000 | ~0.8 | ~5,200 | ~1,250 ops/s |
| CreateSagaInstance | 5 | 1000 | ~2.1 | ~14,000 | ~476 ops/s |
| CreateSagaInstance | 10 | 1000 | ~4.3 | ~28,500 | ~232 ops/s |
| ExecuteSagaSteps | 1 | 1000 | ~12.5 | ~38,000 | ~80 ops/s |
| ExecuteSagaSteps | 5 | 1000 | ~35.2 | ~115,000 | ~28 ops/s |
| ExecuteSagaSteps | 10 | 1000 | ~68.7 | ~230,000 | ~14 ops/s |
| ListSagas | 5 | 100 | ~15.8 | ~62,000 | ~63 ops/s |
| GetSagaById | 1 | 1000 | ~0.4 | ~2,800 | ~2,500 ops/s |
| GetSagaById | 5 | 1000 | ~0.9 | ~6,500 | ~1,111 ops/s |

### Interpreting Results

- **Mean**: Average execution time per operation (lower is better)
- **Allocated**: Memory allocated per operation (lower is better)
- **Throughput**: Operations per second (higher is better)

These benchmarks help identify:
- Performance bottlenecks in saga execution
- Memory efficiency of different operations
- Scalability characteristics as saga complexity increases

## IEventBus

The `IEventBus` interface provides an in-memory pub/sub mechanism for saga events. It enables loose coupling between saga components by allowing event publishers to notify subscribers without direct dependencies. The event bus maintains a history of published events and supports clearing this history when needed.


### Key Features

- Type-safe event subscription and publishing
- Thread-safe operations using locks
- Event history tracking with retrieval and clearing capabilities
- Generic interface supporting any `DomainEvent` type

### Usage Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Infrastructure.Events;

// Setup dependency injection
var services = new ServiceCollection();
services.AddSingleton<IEventBus, EventBus>();
var provider = services.BuildServiceProvider();

// Get the event bus
var eventBus = provider.GetRequiredService<IEventBus>();

// Define an event handler for SagaCreatedEvent
async Task HandleSagaCreated(SagaCreatedEvent @event)
{
    Console.WriteLine($"Saga created: {@event.SagaName} ({@event.SagaId}) with {@event.StepCount} steps");
}

// Subscribe to events
// Note: Subscribe must be called before any events are published
// to ensure handlers receive events

eventBus.Subscribe<SagaCreatedEvent>(HandleSagaCreated);

// Publish an event
var sagaCreatedEvent = new SagaCreatedEvent
{
    SagaId = Guid.NewGuid().ToString(),
    SagaName = "Order Processing Saga",
    DefinitionId = Guid.NewGuid().ToString(),
    StepCount = 5
};

await eventBus.PublishAsync(sagaCreatedEvent);

// Unsubscribe when no longer needed
eventBus.Unsubscribe<SagaCreatedEvent>(HandleSagaCreated);

// Access event history
var history = eventBus.GetEventHistory();
Console.WriteLine($"Total events published: {history.Count}");

// Clear event history when needed
eventBus.ClearHistory();
```

## IRateLimiter

The `IRateLimiter` interface provides token bucket rate limiting for API and service call throttling. It implements a sliding window rate limiting algorithm with configurable thresholds, making it ideal for controlling the rate of outgoing requests to external services or APIs.


### Key Features

- Token bucket algorithm for smooth rate limiting
- Sliding window implementation
- Thread-safe operations
- Configurable requests per second
- Status monitoring and reset capabilities

### Usage Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Infrastructure.RateLimiting;

// Setup dependency injection
var services = new ServiceCollection();
services.AddSingleton<IRateLimiter, TokenBucketRateLimiter>();
var provider = services.BuildServiceProvider();

// Get the rate limiter
var rateLimiter = provider.GetRequiredService<IRateLimiter>();

// Allow requests with rate limiting
bool canProceed = await rateLimiter.AllowAsync("api-service", 10); // 10 requests per second

if (canProceed)
{
    // Make API call
    Console.WriteLine("Request allowed");
}
else
{
    Console.WriteLine("Rate limit exceeded");
}

// Check current rate limit status
var status = await rateLimiter.GetStatusAsync("api-service");
Console.WriteLine($"Available: {status.AvailableTokens}/{status.TotalTokens}, Limited: {status.IsLimited}");

// Reset the rate limiter for a specific key
rateLimiter.Reset("api-service");
```

## SagaActivitySourceExtensions

The `SagaActivitySourceExtensions` class provides extension methods for `SagaActivitySource` to simplify telemetry operations for saga execution, step tracking, and compensation transactions. These methods add contextual tags and handle common validation patterns while maintaining compatibility with distributed tracing systems like OpenTelemetry.


### Usage Example

```csharp
using System.Diagnostics;
using SagaOrchestrator.Infrastructure.Telemetry;

// Start a saga with telemetry
var sagaActivity = SagaActivitySourceExtensions.StartSaga(
    sagaId: Guid.NewGuid().ToString(),
    definitionId: Guid.NewGuid().ToString(),
    correlationId: Guid.NewGuid().ToString(),
    sagaType: "OrderProcessing",
    tenantId: "tenant-123"
);

if (sagaActivity != null)
{
    // Start a step with retry context
    var stepActivity = SagaActivitySourceExtensions.StartStep(
        sagaId: sagaActivity.Context.TraceId.ToString(),
        stepId: Guid.NewGuid().ToString(),
        stepName: "Reserve Inventory",
        order: 1,
        attempt: 1,
        stepType: "InventoryStep",
        serviceName: "inventory-service"
    );

    try
    {
        // Execute step logic...
        Console.WriteLine("Step executed successfully");
        stepActivity?.SetStatus(ActivityStatusCode.Ok);
    }
    catch (Exception ex)
    {
        // Record step failure with exception details
        SagaActivitySourceExtensions.RecordStepFailure(
            stepActivity,
            errorMessage: "Failed to reserve inventory",
            exception: ex
        );
        
        // Start compensation for the failed step
        var compensationActivity = SagaActivitySourceExtensions.StartCompensation(
            sagaId: sagaActivity.Context.TraceId.ToString(),
            compensationId: Guid.NewGuid().ToString(),
            stepName: "Reserve Inventory",
            stepOrder: 1,
            compensationType: "InventoryCompensation",
            compensatingService: "inventory-service"
        );
        
        try
        {
            // Execute compensation logic...
            Console.WriteLine("Compensation executed successfully");
            compensationActivity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception compensationEx)
        {
            // Record compensation failure
            SagaActivitySourceExtensions.RecordCompensationFailure(
                compensationActivity,
                errorMessage: "Failed to compensate inventory reservation",
                exception: compensationEx
            );
        }
    }
    finally
    {
        // Record saga completion with metrics
        var duration = TimeSpan.FromSeconds(45);
        SagaActivitySourceExtensions.RecordSagaComplete(
            sagaId: sagaActivity.Context.TraceId.ToString(),
            finalStatus: "Compensated",
            totalSteps: 5,
            duration: duration,
            completedSteps: 2,
            failedSteps: 3
        );
    }
}
```

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

