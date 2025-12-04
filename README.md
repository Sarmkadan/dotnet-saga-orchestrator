# Distributed Saga Orchestrator for .NET Microservices

A comprehensive, production-ready distributed saga orchestrator for .NET microservices implementing the Saga pattern with compensating transactions, automatic retry logic, timeout handling, and persistence.

## Features

### Core Capabilities
- **Saga Orchestration**: Coordinate business transactions across multiple microservices
- **Compensating Transactions**: Automatic rollback with configurable compensation strategies
- **Retry Logic**: Exponential backoff and configurable retry policies per step
- **Timeout Handling**: Automatic detection and handling of step timeouts
- **Persistence**: In-memory and file-based persistence options
- **Correlation**: Distributed tracing through correlation IDs

### Compensation Strategies
- **Reverse Order (LIFO)**: Compensate in reverse order of completion (default)
- **Forward Order (FIFO)**: Compensate steps in execution order
- **From Failure Point**: Compensate only failed step and subsequent steps
- **Parallel**: Execute all compensations concurrently
- **Manual**: External intervention for complex scenarios

### Saga Statuses
- **Pending**: Saga created but not initialized
- **Initialized**: Saga definition loaded, ready to start
- **Running**: Actively executing steps
- **Completed**: All steps succeeded
- **Failed**: One or more steps failed
- **Compensating**: Compensation in progress
- **Compensated**: Rollback completed
- **Aborted**: Manually aborted
- **TimedOut**: Exceeded overall timeout

## Quick Start

### Installation

Clone the repository:
```bash
git clone https://sarmkadan.com/dotnet-saga-orchestrator.git
cd dotnet-saga-orchestrator
```

### Building

```bash
dotnet build
```

### Running

```bash
dotnet run
```

## Usage Example

```csharp
// Setup DI
var services = new ServiceCollection();
services.AddSagaOrchestrator();
var provider = services.BuildServiceProvider();

// Get services
var definitionService = provider.GetRequiredService<SagaDefinitionService>();
var orchestration = provider.GetRequiredService<SagaOrchestrationService>();

// Create saga definition
var definition = await definitionService.CreateDefinitionAsync(
    "Order Processing",
    "Process orders across microservices");

// Add steps
var step = new SagaStepDefinition(
    "Reserve Inventory",
    "inventory-service",
    "http://inventory/reserve",
    "http://inventory/release");
    
await definitionService.AddStepAsync(definition.Id, step);

// Create and execute saga
var saga = await orchestration.CreateSagaAsync(definition);
await orchestration.StartSagaAsync(saga.Id);

// Execute steps
for (int i = 0; i < saga.Steps.Count; i++)
{
    await orchestration.ExecuteNextStepAsync(saga.Id);
}
```

## Architecture

### Domain Models
- **Saga**: Main saga orchestration entity
- **SagaDefinition**: Workflow definition with steps
- **SagaStep**: Individual step execution state
- **SagaStepDefinition**: Step configuration
- **CompensationTransaction**: Compensation execution state

### Services
- **SagaOrchestrationService**: Main orchestrator
- **SagaDefinitionService**: Definition management
- **CompensationService**: Compensation handling

### Repository Pattern
- **ISagaRepository**: Saga persistence
- **ISagaStepRepository**: Step persistence
- **ICompensationTransactionRepository**: Compensation tracking
- **ISagaDefinitionRepository**: Definition storage

## Configuration

### Default Settings
```csharp
public static class SagaConstants
{
    public const int DefaultSagaTimeoutSeconds = 300;
    public const int DefaultStepTimeoutSeconds = 30;
    public const int DefaultMaxRetries = 3;
    public const int DefaultRetryDelayMs = 1000;
}
```

### Custom Configuration
```csharp
var saga = await orchestration.CreateSagaAsync(
    definition,
    maxRetries: 5,
    timeoutSeconds: 600);
```

## Phase 1: Core Architecture

This release includes:
- ✅ 25+ C# classes with full implementations
- ✅ Complete domain model layer
- ✅ Full-featured service layer
- ✅ In-memory repository implementations
- ✅ Dependency injection configuration
- ✅ Comprehensive exception hierarchy
- ✅ Constants and enums
- ✅ Working demo application
- ✅ 1500+ lines of production code

## Future Roadmap

### Phase 2
- Database persistence (SQL Server, PostgreSQL)
- HTTP client integration
- Event sourcing
- Unit and integration tests

### Phase 3
- REST API endpoints
- gRPC service definitions
- Message queue integration (RabbitMQ, Kafka)
- Monitoring and metrics

### Phase 4
- Web dashboard
- Advanced compensation strategies
- Distributed tracing integration
- Circuit breaker pattern

## Contributing

Contributions are welcome! Please follow the project structure and code style guidelines.

## License

MIT License - Copyright (c) 2026 Vladyslav Zaiets

See LICENSE file for details.

## Contact

- Website: https://sarmkadan.com
- Email: rutova2@gmail.com
