# Architecture Guide

This document explains the architecture, design patterns, and component interactions in the Saga Orchestrator.

## System Overview

The Saga Orchestrator is built using a layered architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────┐
│      Presentation Layer (CLI)               │
│  - Command parsing                          │
│  - User interface                           │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│   Application Layer                         │
│  - SagaOrchestrationService                 │
│  - SagaDefinitionService                    │
│  - CompensationService                      │
│  - Validation & DTOs                        │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│  Infrastructure Layer                       │
│  - HTTP clients (resilient)                 │
│  - Event bus (pub/sub)                      │
│  - Caching                                  │
│  - Webhooks                                 │
│  - Background workers                       │
│  - Rate limiting                            │
│  - Circuit breaker                          │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│      Core Layer                             │
│  - Domain models (Saga, Step, etc.)         │
│  - Extension methods                        │
│  - Builders & utilities                     │
│  - Exception handling                       │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│      Data Layer (Repositories)              │
│  - InMemorySagaRepository                   │
│  - InMemorySagaStepRepository               │
│  - InMemoryCompensationRepository           │
│  - InMemoryDefinitionRepository             │
└─────────────────────────────────────────────┘
```

## Layer Responsibilities

### Presentation Layer

**Purpose**: User-facing interface

**Components**:
- `CliHandler` - Routes CLI commands
- `SagaCliCommand` - Parses and validates input
- Output formatting (JSON, CSV, Table)

**Key Classes**:
- `Presentation/Cli/CliHandler.cs`
- `Presentation/Cli/Commands/SagaCliCommand.cs`

**Responsibilities**:
- Parse command-line arguments
- Validate user input
- Format and display output
- Handle user interactions

### Application Layer

**Purpose**: Business logic orchestration

**Components**:
- `SagaOrchestrationService` - Main orchestrator
- `SagaDefinitionService` - Definition management
- `CompensationService` - Failure handling
- `HealthCheckService` - System health
- `MetricsService` - Statistics

**Key Classes**:
- `Application/Services/*.cs`
- `Application/Validators/*.cs`
- `Application/DTOs/*.cs`
- `Application/Mappers/*.cs`

**Responsibilities**:
- Create and manage sagas
- Execute saga steps
- Trigger compensation
- Validate inputs
- Map domain models to DTOs

### Infrastructure Layer

**Purpose**: Technical concerns and external integrations

**Components**:

1. **HTTP Client Factory**
   - Resilient HTTP client with retry/circuit breaker
   - Service integration

2. **Event Bus**
   - Pub/sub pattern
   - Domain event publishing
   - Webhook integration

3. **Caching Service**
   - In-memory cache with TTL
   - Cache key builder

4. **Background Workers**
   - Timeout monitoring
   - Compensation processing
   - Event archival

5. **Resilience Patterns**
   - Circuit breaker
   - Rate limiter
   - Retry policies

6. **Serialization & Formatting**
   - JSON serialization
   - Output formatting (JSON/CSV/Table)

**Key Classes**:
- `Infrastructure/Http/*.cs`
- `Infrastructure/Events/*.cs`
- `Infrastructure/Caching/*.cs`
- `Infrastructure/BackgroundWorkers/*.cs`
- `Infrastructure/Resilience/*.cs`

### Core Layer

**Purpose**: Domain models and utilities

**Components**:

1. **Domain Models**
   - `Saga` - Main saga entity
   - `SagaDefinition` - Workflow definition
   - `SagaStep` - Step execution state
   - `CompensationTransaction` - Compensation state

2. **Enums**
   - `SagaStatus` - Saga lifecycle states
   - `SagaStepStatus` - Step states
   - `CompensationStatus` - Compensation states
   - `CompensationStrategy` - Compensation algorithms

3. **Utilities**
   - 130+ extension methods
   - Builders (fluent APIs)
   - Exception hierarchy
   - Exception mapper

4. **Constants**
   - Default timeouts
   - Retry policies
   - Rate limits

**Key Classes**:
- `Core/Domain/Models/*.cs`
- `Core/Domain/Enums/*.cs`
- `Core/Extensions/*.cs`
- `Core/Builders/*.cs`
- `Core/Exceptions/*.cs`

### Data Layer

**Purpose**: Persistence abstraction

**Components**:
- `ISagaRepository` - Saga storage
- `ISagaStepRepository` - Step storage
- `ICompensationTransactionRepository` - Compensation storage
- `ISagaDefinitionRepository` - Definition storage

**Implementations**:
- `InMemorySagaRepository` - RAM storage (Phase 1)
- Database repositories (Phase 4: EF Core)

**Key Classes**:
- `Data/Repositories/I*.cs` (interfaces)
- `Data/Repositories/InMemory*.cs` (implementations)

## Design Patterns

### 1. Repository Pattern

Abstracts data access and enables multiple storage backends:

```csharp
public interface ISagaRepository
{
    Task<Saga> GetAsync(string sagaId);
    Task<IEnumerable<Saga>> GetAllAsync();
    Task AddAsync(Saga saga);
    Task UpdateAsync(Saga saga);
}
```

### 2. Pub/Sub Pattern

Event bus decouples saga lifecycle from event handlers:

```csharp
// Publisher
eventBus.Publish(new SagaCompletedEvent(saga.Id));

// Subscriber
eventBus.Subscribe<SagaCompletedEvent>(async @event =>
{
    await webhookHandler.NotifyAsync(@event);
});
```

### 3. Builder Pattern

Fluent APIs for complex object construction:

```csharp
var step = new SagaStepBuilder()
    .WithName("Reserve Inventory")
    .WithTimeout(30)
    .WithRetries(3, 1000)
    .Build();
```

### 4. Strategy Pattern

Multiple compensation algorithms:

```csharp
public enum CompensationStrategy
{
    ReverseOrder,      // LIFO
    ForwardOrder,      // FIFO
    FromFailurePoint,  // Only affected steps
    Parallel,          // Concurrent
    Manual             // Human intervention
}
```

### 5. Circuit Breaker Pattern

Prevent cascade failures:

```csharp
try
{
    await circuitBreaker.ExecuteAsync("service", async () =>
    {
        return await httpClient.GetAsync(url);
    });
}
catch (OpenCircuitException)
{
    // Service is unavailable, fallback
}
```

### 6. Decorator Pattern

HTTP client resilience:

```csharp
httpClient
    .WithRetryPolicy(3, backoff)
    .WithCircuitBreaker(5, 30)
    .WithRateLimit(100);
```

### 7. Observer Pattern

Background workers monitor saga state:

```csharp
// SagaTimeoutWorker observes sagas
while (isRunning)
{
    var timeouts = await GetExpiredSagasAsync();
    foreach (var saga in timeouts)
    {
        await orchestration.AbortSagaAsync(saga.Id);
    }
}
```

## Component Interactions

### Saga Creation & Execution Flow

```
User Command
    ↓
CliHandler.ProcessCommand()
    ↓
SagaOrchestrationService.CreateSagaAsync()
    ↓
Create Saga Entity
    ↓
Save to Repository
    ↓
Publish SagaCreatedEvent
    ↓
EventBus.Publish()
    ↓
WebhookHandler (observes)
EventProcessingWorker (observes)
SagaLogger (observes)
    ↓
Return Saga to User
```

### Step Execution Flow

```
User executes step
    ↓
ExecuteNextStepAsync()
    ↓
Get current step
    ↓
HttpClientFactory.GetClient()
    ↓
Apply CircuitBreaker
    ↓
Apply RateLimit
    ↓
Execute with retry policy
    ↓
Success? ─→ Yes ─→ Mark step completed
           ↓ No
        Timeout?
           ↓ Yes
        Mark as timed out
        Trigger compensation
           ↓ No
        Retry?
           ↓ Yes
        Retry with backoff
           ↓ No
        Mark as failed
        Trigger compensation
```

### Compensation Flow

```
Saga fails
    ↓
SagaTimeoutWorker OR manual request
    ↓
CompensateSagaAsync(sagaId, strategy)
    ↓
Get completed steps
    ↓
Select strategy
    ↓
Strategy: ReverseOrder → Reverse list
Strategy: ForwardOrder → Keep order
Strategy: FromFailurePoint → Skip earlier
Strategy: Parallel → Group all
Strategy: Manual → Pause
    ↓
For each step:
    ↓
HttpClientFactory.GetClient()
    ↓
Execute compensation endpoint
    ↓
Create CompensationTransaction
    ↓
Publish CompensationCompletedEvent
    ↓
Mark saga as Compensated
```

## Data Models

### Saga Entity

```csharp
public class Saga
{
    public string Id { get; set; }                    // Unique saga ID
    public string DefinitionId { get; set; }          // Reference to definition
    public SagaStatus Status { get; set; }            // Current status
    public List<SagaStep> Steps { get; set; }         // Execution steps
    public string CorrelationId { get; set; }         // Distributed tracing ID
    public DateTime CreatedAt { get; set; }           // Creation timestamp
    public DateTime? StartedAt { get; set; }          // Start timestamp
    public DateTime? CompletedAt { get; set; }        // Completion timestamp
    public string Metadata { get; set; }              // Custom data
    public int MaxRetries { get; set; }               // Retry limit
    public int TimeoutSeconds { get; set; }           // Saga timeout
    public int CurrentRetryCount { get; set; }        // Current retry count
}
```

### SagaStep Entity

```csharp
public class SagaStep
{
    public string Id { get; set; }                    // Step ID
    public string SagaId { get; set; }                // Parent saga
    public string DefinitionId { get; set; }          // Step definition reference
    public string Name { get; set; }                  // Display name
    public SagaStepStatus Status { get; set; }        // Current status
    public DateTime? StartedAt { get; set; }          // Execution start
    public DateTime? CompletedAt { get; set; }        // Execution end
    public string Response { get; set; }              // Step response data
    public string ErrorMessage { get; set; }          // Error details
    public int RetryCount { get; set; }               // Retry attempts
}
```

### SagaDefinition Entity

```csharp
public class SagaDefinition
{
    public string Id { get; set; }                    // Definition ID
    public string Name { get; set; }                  // Workflow name
    public string Description { get; set; }           // Documentation
    public List<SagaStepDefinition> Steps { get; set; } // Step definitions
    public DateTime CreatedAt { get; set; }           // Creation time
    public string Metadata { get; set; }              // Custom data
}
```

## Event System

### Domain Events

```csharp
public class SagaCreatedEvent
{
    public string SagaId { get; set; }
    public string DefinitionId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SagaCompletedEvent
{
    public string SagaId { get; set; }
    public int TotalSteps { get; set; }
    public int CompletedSteps { get; set; }
    public TimeSpan Duration { get; set; }
}

public class SagaFailedEvent
{
    public string SagaId { get; set; }
    public string FailureReason { get; set; }
    public int FailedStepIndex { get; set; }
}
```

## Configuration System

### Service Registration

```csharp
services.AddSagaOrchestrator()
    .WithDefaultStepTimeout(30)
    .WithDefaultMaxRetries(3)
    .WithCachingEnabled(true)
    .WithWebhooksEnabled(true)
    .WithTimeoutWorker(true)
    .Build();
```

### Configuration Options

```csharp
public class SagaOptions
{
    public int DefaultSagaTimeoutSeconds { get; set; }
    public int DefaultStepTimeoutSeconds { get; set; }
    public int DefaultMaxRetries { get; set; }
    public int DefaultRetryDelayMs { get; set; }
    public bool CachingEnabled { get; set; }
    public int CacheTtlMinutes { get; set; }
    public bool WebhooksEnabled { get; set; }
    public bool TimeoutWorkerEnabled { get; set; }
    public int TimeoutCheckIntervalSeconds { get; set; }
    public bool ExponentialBackoffEnabled { get; set; }
    public int CircuitBreakerFailureThreshold { get; set; }
}
```

## Extension Methods

The system includes 130+ extension methods organized by concern:

### StringExtensions (30 methods)
- Case conversion (PascalCase, camelCase, snake_case)
- Validation (email, URL, slug)
- Formatting (truncation, padding)
- Pattern matching

### DateTimeExtensions (25 methods)
- Relative formatting ("5 minutes ago")
- Duration calculation
- Business day arithmetic
- Percentile calculations

### CollectionExtensions (25 methods)
- Batching and chunking
- Distinct by selector
- Grouping operations
- Pagination helpers

### EnumExtensions (20 methods)
- Description extraction
- Flag operations
- Safe parsing
- Value conversion

### ValidationExtensions (20 methods)
- Null checks
- Range validation
- Custom validators
- Fluent chains

## Performance Characteristics

### O(1) Operations
- Cache lookups
- Circuit breaker state checks
- Rate limiter bucket update

### O(n) Operations
- Repository queries (in-memory)
- Event bus publishing to subscribers
- Saga status calculation

### O(n log n) Operations
- Sorting sagas by status
- Compensation strategy selection

## Scalability Considerations

### For Horizontal Scaling
1. Replace in-memory repositories with database
2. Use distributed cache (Redis)
3. Switch to distributed event bus (RabbitMQ, Kafka)
4. Implement request tracing (OpenTelemetry)

### For Vertical Scaling
1. Tune background worker intervals
2. Configure cache expiration
3. Adjust circuit breaker thresholds
4. Monitor metrics and adjust timeouts

## Security Considerations

1. **Input Validation**: All user input validated
2. **Error Handling**: Safe error messages (no leaking internals)
3. **Correlation IDs**: Track requests for audit trails
4. **Rate Limiting**: Prevent abuse
5. **API Key Support**: ServiceRegistry supports authentication

## Future Architectural Changes

### Phase 4: Database Persistence
- Entity Framework Core integration
- SQL Server/PostgreSQL support
- Query optimization
- Transaction management

### Phase 5: Distributed Events
- RabbitMQ/Kafka integration
- Event store for all state changes
- Event replay capability
- Dead letter queues

### Phase 6: Distributed Tracing
- OpenTelemetry instrumentation
- Jaeger exporter
- Custom span attributes
- Performance profiling

## Testing Architecture

The design supports:
- Unit testing (mock repositories)
- Integration testing (in-memory components)
- Performance testing (metrics service)
- Health monitoring tests

## References

- Clean Architecture (Robert C. Martin)
- Domain-Driven Design (Eric Evans)
- Microservice Patterns (Sam Newman)
- Release It! (Michael Nygard)
