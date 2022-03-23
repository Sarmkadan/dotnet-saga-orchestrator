// existing content ...

## ISagaEventObserver

The `ISagaEventObserver` interface defines an observer pattern for handling saga lifecycle events, such as saga creation, completion, failure, and compensation initiation. It enables side effects like webhook delivery or event bus publishing in response to these domain events.

### Usage Example

```csharp
using SagaOrchestrator.Infrastructure.Events;
using SagaOrchestrator.Infrastructure.Integration;
using Microsoft.Extensions.Logging;

// Mock dependencies for demonstration
var webhookHandler = new FakeWebhookHandler();
var eventBus = new FakeEventBus();
var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SagaEventObserver>();

// Create observer instance
var observer = new SagaEventObserver(webhookHandler, eventBus, logger);

// Example event handling
await observer.OnSagaCreatedAsync(new SagaCreatedEvent { SagaId = "ORDER-123" });
await observer.OnSagaCompletedAsync(new SagaCompletedEvent { SagaId = "ORDER-123" });
```

Where `FakeWebhookHandler` and `FakeEventBus` are simple implementations:
```csharp
public class FakeWebhookHandler : IWebhookHandler
{
    public IEnumerable<WebhookSubscription> GetSubscriptions() => new List<WebhookSubscription>();
    public Task SendWebhookAsync(string url, object payload) => Task.CompletedTask;
}

public class FakeEventBus : IEventBus
{
    public Task PublishAsync<TEvent>(TEvent @event) where TEvent : class => Task.CompletedTask;
}
```

## IRequestContext

The `IRequestContext` interface provides a mechanism for tracking request-specific metadata, user identity, and performance metrics across distributed operations. It supports correlation IDs, tenant isolation, and timing tracking for debugging and observability.

### Usage Example

```csharp
using SagaOrchestrator.Infrastructure.Context;

// Create context provider and get current context
var provider = new RequestContextProvider();
var context = provider.GetContext();

// Access and modify context properties
context.UserId = "user-123";
context.TenantId = "tenant-456";
context.Metadata["request_type"] = "order_processing";

// Track performance metrics
var tracker = new PerformanceTracker();
tracker.RecordTiming("db_query", 150);
tracker.RecordTiming("api_call", 200);

// Store metrics in context metadata
context.Metadata["db_query_time"] = tracker.GetTiming("db_query");
context.Metadata["total_time"] = tracker.GetTotalElapsedMs();

// Update context with changes
provider.SetContext(context);

// Output context information
Console.WriteLine(context.ToString());
Console.WriteLine(tracker.ToString());
```

This example demonstrates:
1. Creating a request context provider and retrieving the current context
2. Setting user/tenant identifiers and adding metadata
3. Using the performance tracker to record operation timings
4. Storing metrics in the context metadata
5. Updating the context with modified values
6. Outputting context and performance information

## ISagaLogger

The `ISagaLogger` interface provides comprehensive logging capabilities for saga operations, tracking saga creation, step execution, compensation events, and execution timelines. It enables detailed observability into saga workflows for debugging and monitoring purposes.

### Usage Example

```csharp
using SagaOrchestrator.Infrastructure.Logging;
using SagaOrchestrator.Core.Domain.Models;
using Microsoft.Extensions.Logging;

// Create logger factory and logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var sagaLogger = new SagaLogger(loggerFactory.CreateLogger<SagaLogger>());

// Create a sample saga with steps
var saga = new Saga
{
    Id = "ORDER-123",
    Name = "OrderProcessingSaga",
    DefinitionId = "order-processing-v1",
    Status = SagaStatus.Running,
    Steps = new List<SagaStep>
    {
        new SagaStep
        {
            Id = "step-1",
            Name = "ValidateOrder",
            Order = 1,
            Status = SagaStepStatus.Completed,
            StartedAt = DateTime.UtcNow.AddSeconds(-10),
            CompletedAt = DateTime.UtcNow.AddSeconds(-8)
        },
        new SagaStep
        {
            Id = "step-2",
            Name = "ProcessPayment",
            Order = 2,
            Status = SagaStepStatus.Completed,
            StartedAt = DateTime.UtcNow.AddSeconds(-7),
            CompletedAt = DateTime.UtcNow.AddSeconds(-5)
        },
        new SagaStep
        {
            Id = "step-3",
            Name = "ShipOrder",
            Order = 3,
            Status = SagaStepStatus.Failed,
            StartedAt = DateTime.UtcNow.AddSeconds(-4),
            RetryCount = 1
        }
    }
};

// Log saga lifecycle events
sagaLogger.LogSagaCreated(saga);
sagaLogger.LogStepStarted(saga, saga.Steps[0]);
sagaLogger.LogStepCompleted(saga, saga.Steps[0], TimeSpan.FromMilliseconds(2000));
sagaLogger.LogStepStarted(saga, saga.Steps[1]);
sagaLogger.LogStepCompleted(saga, saga.Steps[1], TimeSpan.FromMilliseconds(1500));
sagaLogger.LogStepStarted(saga, saga.Steps[2]);

// Log failed step
sagaLogger.LogStepFailed(saga, saga.Steps[2], new InvalidOperationException("Payment gateway timeout"));

// Log compensation
sagaLogger.LogCompensationStarted(saga);
sagaLogger.LogCompensationCompleted(saga);

// Log final saga status
saga.Status = SagaStatus.Failed;
sagaLogger.LogSagaFailed(saga, new InvalidOperationException("Order processing failed after 3 steps"));

// Log execution timeline
sagaLogger.LogExecutionTimeline(saga);
```

### Usage Example

```csharp
using SagaOrchestrator.Infrastructure.Integration;
using Microsoft.Extensions.Logging;

// Create logger factory for ServiceRegistry
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<ServiceRegistry>();

// Create service registry instance
var serviceRegistry = new ServiceRegistry(logger);

// Register external payment service
var paymentService = new ServiceDescriptor(
    name: "payment-service",
    url: "https://api.payments.example.com/v1"
);

paymentService.ApiKey = "pk_test_abc123";
paymentService.Timeout = 60;
paymentService.MaxRetries = 5;
paymentService.Metadata["service_type"] = "payment_processor";
paymentService.Metadata["region"] = "eu-west-1";

await serviceRegistry.RegisterServiceAsync(paymentService);

// Register shipping service
var shippingService = new ServiceDescriptor(
    name: "shipping-service",
    url: "https://api.shipping.example.com/v2"
);
shippingService.ApiKey = "sk_test_xyz789";
shippingService.Timeout = 45;
shippingService.MaxRetries = 3;

await serviceRegistry.RegisterServiceAsync(shippingService);

// Check service health
var isPaymentHealthy = await serviceRegistry.IsServiceHealthyAsync("payment-service");
Console.WriteLine($"Payment service healthy: {isPaymentHealthy}");

// Update service health status
await serviceRegistry.UpdateServiceHealthAsync("shipping-service", true);

// Retrieve all registered services
var allServices = await serviceRegistry.GetAllServicesAsync();
foreach (var service in allServices)
{
    Console.WriteLine(service.ToString());
}

// Get specific service details
var paymentServiceDetails = await serviceRegistry.GetServiceAsync("payment-service");
if (paymentServiceDetails != null)
{
    Console.WriteLine($"Payment service URL: {paymentServiceDetails.Url}");
    Console.WriteLine($"Payment service timeout: {paymentServiceDetails.Timeout}s");
}

// Unregister a service when it's no longer needed
await serviceRegistry.UnregisterServiceAsync("shipping-service");
```

This example demonstrates:
1. Creating a service registry with logging support
2. Registering multiple external services with configuration
3. Checking and updating service health status
4. Retrieving all registered services
5. Getting specific service details by name
6. Unregistering services when they're no longer needed

The `ServiceDescriptor` class provides properties for:
- Service identification (`Name`, `Url`)
- Authentication (`ApiKey`)
- Configuration (`Timeout`, `MaxRetries`)
- Health tracking (`IsHealthy`, `RegisteredAt`, `LastHealthCheckTime`)
- Custom metadata storage (`Metadata` dictionary)
- String representation via `ToString()`

## SagaMessageTemplates

The `SagaMessageTemplates` class provides a centralized collection of formatted message templates for saga event notifications, error reporting, and status updates. It offers both concise and detailed message formats for various saga lifecycle events including creation, step execution, compensation, and timeout scenarios.

### Usage Example

```csharp
using SagaOrchestrator.Infrastructure.Messaging;

// Simple formatted messages
var createdMessage = SagaMessageTemplates.SagaCreated.Format("ORDER-123", "OrderProcessingSaga", 5);
Console.WriteLine(createdMessage);
// Output: Saga 'OrderProcessingSaga' (ID: ORDER-123) created with 5 steps

var startedMessage = SagaMessageTemplates.StepStarted.Format("ProcessPayment", 2);
Console.WriteLine(startedMessage);
// Output: Executing step 2: ProcessPayment

var completedMessage = SagaMessageTemplates.StepCompleted.Format("ValidateOrder", 150);
Console.WriteLine(completedMessage);
// Output: Step 'ValidateOrder' completed in 150ms

var failedMessage = SagaMessageTemplates.StepFailed.Format("ShipOrder", "Connection timeout");
Console.WriteLine(failedMessage);
// Output: Step 'ShipOrder' failed: Connection timeout

var withRetryMessage = SagaMessageTemplates.StepFailed.WithRetry("ProcessPayment", "Gateway error", 2, 3);
Console.WriteLine(withRetryMessage);
// Output: Step 'ProcessPayment' failed (attempt 2/3): Gateway error

var completedSagaMessage = SagaMessageTemplates.SagaCompleted.Format("OrderProcessingSaga", 2500, 5, 5);
Console.WriteLine(completedSagaMessage);
// Output: Saga 'OrderProcessingSaga' completed successfully in 2500ms (5/5 steps)

var timeoutMessage = SagaMessageTemplates.SagaTimeout.Format("OrderProcessingSaga", 300);
Console.WriteLine(timeoutMessage);
// Output: Saga 'OrderProcessingSaga' exceeded timeout limit of 300 seconds

var stepTimeoutMessage = SagaMessageTemplates.SagaTimeout.StepTimeout("ProcessPayment", 60);
Console.WriteLine(stepTimeoutMessage);
// Output: Step 'ProcessPayment' exceeded timeout limit of 60 seconds

// Detailed formatted messages
var detailedCreated = SagaMessageTemplates.SagaCreated.Detailed("ORDER-123", "OrderProcessingSaga", "order-processing-v1", 5);
Console.WriteLine(detailedCreated);
```

## TimelineEntry

The `TimelineEntry` record represents a single entry in a saga's debug timeline, capturing chronological events such as snapshots, state transitions, and breakpoint hits. Each entry provides metadata about when an event occurred, its type, and associated saga context, enabling comprehensive debugging and post-mortem analysis of saga execution.

### Usage Example

```csharp
using SagaOrchestrator.Infrastructure.Debugging;
using SagaOrchestrator.Core.Domain.Models;
using System;
using System.Collections.Generic;

// Create a timeline entry from a snapshot
var snapshot = new SagaDebugSnapshot
{
    SnapshotId = "snap-001",
    SequenceNumber = 1,
    SagaId = "ORDER-123",
    SagaStatus = SagaStatus.Running,
    Trigger = SnapshotTrigger.Breakpoint,
    CapturedAt = DateTime.UtcNow,
    Label = "Payment step breakpoint triggered",
    CompletedStepCount = 2,
    Steps = new List<SagaStep>()
};

var snapshotEntry = TimelineEntry.FromSnapshot(snapshot);

// Create a timeline entry from a saga event
var sagaEvent = new SagaEvent
{
    EventId = Guid.NewGuid().ToString(),
    SagaId = "ORDER-123",
    EventName = "PaymentProcessed",
    Description = "Payment gateway successfully processed payment of $99.99",
    StepName = "ProcessPayment",
    Timestamp = DateTime.UtcNow,
    Data = new Dictionary<string, object>
    {
        { "amount", 99.99m },
        { "currency", "USD" },
        { "gateway", "stripe" }
    }
};

var eventEntry = TimelineEntry.FromSagaEvent(sagaEvent);

// Access timeline entry properties
Console.WriteLine($"Entry ID: {snapshotEntry.EntryId}");
Console.WriteLine($"Kind: {snapshotEntry.Kind}");
Console.WriteLine($"Timestamp: {snapshotEntry.Timestamp:O}");
Console.WriteLine($"Title: {snapshotEntry.Title}");
Console.WriteLine($"Description: {snapshotEntry.Description}");
Console.WriteLine($"Snapshot ID: {snapshotEntry.SnapshotId}");
Console.WriteLine($"Step Name: {snapshotEntry.StepName}");
Console.WriteLine($"Metadata Count: {snapshotEntry.Metadata.Count}");
```

## IOutputFormatter

The `IOutputFormatter` interface provides multi-format output formatting for saga data, supporting JSON, CSV, and table formats. It enables flexible serialization of saga information for CLI tools, APIs, and logging purposes.

### Usage Example

```csharp
using SagaOrchestrator.Infrastructure.Formatting;
using SagaOrchestrator.Core.Domain.Models;
using Microsoft.Extensions.Logging;

// Create serializer and formatter
var serializer = new SagaJsonSerializer();
var formatter = new OutputFormatter(serializer);

// Create sample sagas for demonstration
var sagas = new List<Saga>
{
    new Saga
    {
        Id = "ORDER-123",
        Name = "OrderProcessingSaga",
        DefinitionId = "order-processing-v1",
        Status = SagaStatus.Running,
        CreatedAt = DateTime.UtcNow,
        Steps = new List<SagaStep>
        {
            new SagaStep
            {
                Id = "step-1",
                Name = "ValidateOrder",
                Order = 1,
                Status = SagaStepStatus.Completed,
                StartedAt = DateTime.UtcNow.AddSeconds(-30),
                CompletedAt = DateTime.UtcNow.AddSeconds(-25)
            },
            new SagaStep
            {
                Id = "step-2",
                Name = "ProcessPayment",
                Order = 2,
                Status = SagaStepStatus.Completed,
                StartedAt = DateTime.UtcNow.AddSeconds(-20),
                CompletedAt = DateTime.UtcNow.AddSeconds(-15)
            },
            new SagaStep
            {
                Id = "step-3",
                Name = "ShipOrder",
                Order = 3,
                Status = SagaStepStatus.Pending
            }
        }
    },
    new Saga
    {
        Id = "ORDER-456",
        Name = "UserRegistrationSaga",
        DefinitionId = "user-registration-v2",
        Status = SagaStatus.Completed,
        CreatedAt = DateTime.UtcNow.AddMinutes(-5),
        Steps = new List<SagaStep>
        {
            new SagaStep
            {
                Id = "step-1",
                Name = "CreateUser",
                Order = 1,
                Status = SagaStepStatus.Completed
            },
            new SagaStep
            {
                Id = "step-2",
                Name = "SendWelcomeEmail",
                Order = 2,
                Status = SagaStepStatus.Completed
            }
        }
    }
};

// Format as indented JSON (generic type)
var jsonOutput = formatter.FormatAsJson(sagas);
Console.WriteLine("JSON Output:");
Console.WriteLine(jsonOutput);
Console.WriteLine();

// Format specific saga as JSON
var singleSagaJson = formatter.FormatAsJson(sagas[0]);
Console.WriteLine("Single Saga JSON:");
Console.WriteLine(singleSagaJson);
Console.WriteLine();

// Format as table
var tableOutput = formatter.FormatAsTable(sagas);
Console.WriteLine("Table Output:");
Console.WriteLine(tableOutput);
Console.WriteLine();

// Format as CSV
var csvOutput = formatter.FormatAsCsv(sagas);
Console.WriteLine("CSV Output:");
Console.WriteLine(csvOutput);
```

## IHttpClientFactory

The `IHttpClientFactory` interface provides a factory for creating `HttpClient` instances with built-in resilience policies. It handles HTTP client configuration, retry logic, circuit breaking, and timeout management for external service calls, ensuring reliable communication with external APIs while preventing cascading failures.

### Usage Example

```csharp
using SagaOrchestrator.Infrastructure.Http;
using System.Net.Http;

// Create HTTP client factory
var httpClientFactory = new HttpClientFactory();

// Configure HTTP client settings
var config = new HttpClientConfiguration
{
    BaseUrl = "https://api.example.com/v1",
    TimeoutSeconds = 60,
    AuthToken = "your-auth-token-here",
    DefaultHeaders = new Dictionary<string, string>
    {
        { "Accept", "application/json" },
        { "X-Request-Id", Guid.NewGuid().ToString() }
    }
};

// Create a named HTTP client
var client = httpClientFactory.CreateClient("payment-service", config);

// Make a request to an external API
var request = new HttpRequestMessage(HttpMethod.Get, "/orders/123");

// Send request and get deserialized response
var order = await httpClientFactory.SendAsync<Order>(client, request);

Console.WriteLine($"Order retrieved: {order.Id}");
```

Where `Order` is a simple POCO:

```csharp
public class Order
{
    public string Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
}
```

## ISagaSerializer

The `ISagaSerializer` interface provides JSON serialization capabilities for saga entities, supporting both compact and indented formatting. It handles polymorphic serialization, enum conversions, and custom type formatting for saga domain objects including `SagaStatus`, `SagaStepStatus`, `CompensationStatus`, `CompensationStrategy`, and `DateTime` values.

### Usage Example

```csharp
using SagaOrchestrator.Infrastructure.Serialization;
using SagaOrchestrator.Core.Domain.Models;
using System;

// Create serializer instance
var serializer = new SagaJsonSerializer();

// Create a sample saga for serialization
var saga = new Saga
{
  Id = "ORDER-123",
  Name = "OrderProcessingSaga",
  DefinitionId = "order-processing-v1",
  Status = SagaStatus.Running,
  CreatedAt = DateTime.UtcNow,
  Steps = new List<SagaStep>
  {
    new SagaStep
    {
      Id = "step-1",
      Name = "ValidateOrder",
      Order = 1,
      Status = SagaStepStatus.Completed,
      StartedAt = DateTime.UtcNow.AddSeconds(-30),
      CompletedAt = DateTime.UtcNow.AddSeconds(-25)
    },
    new SagaStep
    {
      Id = "step-2",
      Name = "ProcessPayment",
      Order = 2,
      Status = SagaStepStatus.Completed,
      StartedAt = DateTime.UtcNow.AddSeconds(-20),
      CompletedAt = DateTime.UtcNow.AddSeconds(-15)
    }
  }
};

// Serialize saga to compact JSON
var compactJson = serializer.Serialize(saga);
Console.WriteLine("Compact JSON:");
Console.WriteLine(compactJson);

// Serialize saga to indented JSON
var indentedJson = serializer.SerializeIndented(saga);
Console.WriteLine("\nIndented JSON:");
Console.WriteLine(indentedJson);

// Deserialize JSON back to saga object
var jsonString = @"{
  \"id\": \"ORDER-456\",
  \"name\": \"UserRegistrationSaga\",
  \"definitionId\": \"user-registration-v2\",
  \"status\": \"completed\",
  \"createdAt\": \"2024-01-15T10:30:00Z\",
  \"steps\": []
}";
var deserializedSaga = serializer.Deserialize<Saga>(jsonString);
Console.WriteLine($"\nDeserialized saga: {deserializedSaga?.Name} with status {deserializedSaga?.Status}");
```


## SagaActivitySource

The `SagaActivitySource` class provides OpenTelemetry instrumentation for saga orchestration telemetry. It emits spans for saga lifecycle events including saga start, step execution, compensation, and completion, enabling distributed tracing and observability across saga workflows.

### Usage Example

```csharp
using SagaOrchestrator.Infrastructure.Telemetry;
using System.Diagnostics;

// Register the source with OpenTelemetry
// services.AddOpenTelemetry()
//   .WithTracing(b => b.AddSource(SagaActivitySource.Name));

// Start a saga execution span
var sagaActivity = SagaActivitySource.StartSaga(
  sagaId: "ORDER-123",
  definitionId: "order-processing-v1",
  correlationId: "corr-456"
);

try
{
  // Execute saga steps with individual spans
  var step1Activity = SagaActivitySource.StartStep(
    sagaId: "ORDER-123",
    stepId: "step-1",
    stepName: "ValidateOrder",
    order: 1,
    attempt: 1
  );
  
  // Simulate step execution
  await Task.Delay(100);
  step1Activity?.Dispose();
  
  // Start another step
  var step2Activity = SagaActivitySource.StartStep(
    sagaId: "ORDER-123",
    stepId: "step-2",
    stepName: "ProcessPayment",
    order: 2,
    attempt: 1
  );
  
  // Simulate step execution
  await Task.Delay(150);
  step2Activity?.Dispose();
  
  // Record successful saga completion
  var completeActivity = SagaActivitySource.RecordSagaComplete(
    sagaId: "ORDER-123",
    finalStatus: "Completed",
    totalSteps: 2
  );
  completeActivity?.Dispose();
}
catch (Exception ex)
{
  // Record step failure
  SagaActivitySource.RecordStepFailure(step2Activity, ex.Message);
  
  // Start compensation for failed step
  var compensationActivity = SagaActivitySource.StartCompensation(
    sagaId: "ORDER-123",
    compensationId: "comp-1",
    stepName: "ProcessPayment",
    stepOrder: 2
  );
  
  // Simulate compensation execution
  await Task.Delay(50);
  
  // Record compensation failure if any
  SagaActivitySource.RecordCompensationFailure(compensationActivity, "Compensation failed");
  compensationActivity?.Dispose();
  
  // Record saga completion with failure status
  var completeActivity = SagaActivitySource.RecordSagaComplete(
    sagaId: "ORDER-123",
    finalStatus: "Compensated",
    totalSteps: 2
  );
  completeActivity?.Dispose();
}
finally
{
  sagaActivity?.Dispose();
}
```

## ICircuitBreaker

The `ICircuitBreaker` interface implements the circuit breaker pattern for fault tolerance. It prevents cascading failures by monitoring service calls and temporarily blocking requests to failing services, allowing them to recover before resuming normal operation.


### Usage Example


```csharp
using SagaOrchestrator.Infrastructure.Resilience;
using Microsoft.Extensions.Logging;

// Create circuit breaker with default settings (5 failures threshold, 60 second timeout)
var circuitBreaker = new CircuitBreaker(failureThreshold: 5, timeoutSeconds: 60);

// Execute an asynchronous action with circuit breaker protection
var success = await circuitBreaker.ExecuteAsync(async () =>
{
    // Call external service
    var response = await httpClient.GetAsync("https://api.example.com/orders");
    response.EnsureSuccessStatusCode();
}, "payment-gateway");

if (!success)
{
    Console.WriteLine("Circuit breaker prevented call to failing service");
}

// Execute with return value
var order = await circuitBreaker.ExecuteAsync(async () =>
{
    var response = await httpClient.GetFromJsonAsync<Order>("https://api.example.com/orders/123");
    return response;
}, "order-service");

// Check circuit breaker state
var state = circuitBreaker.GetState("payment-gateway");
Console.WriteLine($"Circuit breaker state: {state}");

// Reset circuit breaker for a specific service
circuitBreaker.Reset("payment-gateway");

// Access metrics properties
var metricsState = circuitBreaker.State;
var failures = circuitBreaker.FailureCount;
var successes = circuitBreaker.SuccessCount;
var lastFailure = circuitBreaker.LastFailureTime;
var lastAccessed = circuitBreaker.LastAccessedAt;

// Clean up stale metrics periodically
var evictedCount = circuitBreaker.EvictStaleEntries(TimeSpan.FromHours(1));
```


## SagaDebuggerService

The `SagaDebuggerService` provides distributed saga debugging capabilities through immutable snapshot capture, time-travel inspection, and breakpoint-based execution control. It captures `SagaDebugSnapshot` objects at key execution moments (manual, breakpoint triggers, or step transitions) and stores them in memory for later analysis. Snapshots can be retrieved, inspected, or used to restore saga state to any prior point in time, enabling comprehensive debugging and post-mortem analysis without modifying the live orchestration flow. Breakpoints allow pausing execution at specific steps to capture intermediate state.


### Usage Example

```csharp
using SagaOrchestrator.Infrastructure.Debugging;
using SagaOrchestrator.Core.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Threading;

// Setup dependencies
var sagaRepository = new FakeSagaRepository();
var sagaStepRepository = new FakeSagaStepRepository();
var eventPublisher = new SagaEventPublisher();
var options = new DebuggerOptions { IsEnabled = true, MaxSnapshotsPerSaga = 100 };
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

// Create debugger service
var debugger = new SagaDebuggerService(
    sagaRepository,
    sagaStepRepository,
    eventPublisher,
    options);

// Create and start a saga
var saga = new Saga
{
    Id = "ORDER-123",
    Name = "OrderProcessingSaga",
    DefinitionId = "order-processing-v1",
    Status = SagaStatus.Running,
    CreatedAt = DateTime.UtcNow
};

await sagaRepository.AddAsync(saga);

// Manually capture a snapshot
var snapshot1 = await debugger.CaptureSnapshotAsync(
    saga.Id,
    SnapshotTrigger.Manual,
    "Initial state captured"
);

Console.WriteLine($"Captured snapshot: {snapshot1.SnapshotId} (seq: {snapshot1.SequenceNumber})");

// Simulate step execution
saga.Status = SagaStatus.Running;
await sagaRepository.UpdateAsync(saga);

// Capture another snapshot after step progress
var snapshot2 = await debugger.CaptureSnapshotAsync(
    saga.Id,
    SnapshotTrigger.Manual,
    "After step execution"
);

// Set a breakpoint on a specific step
var breakpoint = await debugger.SetBreakpointAsync(
    saga.Id,
    "ProcessPayment",
    "Pause before payment processing"
);

Console.WriteLine($"Breakpoint set: {breakpoint.BreakpointId} on step '{breakpoint.StepName}'");

// Get all snapshots for this saga
var allSnapshots = await debugger.GetSnapshotsAsync(saga.Id);
Console.WriteLine($"Total snapshots: {allSnapshots.Count}");

// Travel back in time to a specific snapshot
var restoredSnapshot = await debugger.TravelToAsync(saga.Id, snapshot1.SnapshotId);
Console.WriteLine($"Restored to snapshot: {restoredSnapshot.SequenceNumber}");

// Get timeline for debugging
var timeline = await debugger.GetTimelineAsync(saga.Id);
Console.WriteLine($"Timeline contains {timeline.Entries.Count} entries");

// Get all breakpoints for this saga
var breakpoints = await debugger.GetBreakpointsAsync(saga.Id);
Console.WriteLine($"Active breakpoints: {breakpoints.Count}");

// Check if a breakpoint would be hit
var breakpointHit = await debugger.CheckBreakpointAsync(saga.Id, "ProcessPayment");
Console.WriteLine($"Breakpoint hit: {breakpointHit}");

// Remove a breakpoint
var removed = await debugger.RemoveBreakpointAsync(breakpoint.BreakpointId);
Console.WriteLine($"Breakpoint removed: {removed}");

// Clear all breakpoints
await debugger.ClearBreakpointsAsync(saga.Id);

// Purge all snapshots for cleanup
await debugger.PurgeSnapshotsAsync(saga.Id);
```

Where `FakeSagaRepository` and `FakeSagaStepRepository` are simple in-memory implementations:


```csharp
public class FakeSagaRepository : ISagaRepository
{
    private readonly Dictionary<string, Saga> _sagas = new();
    
    public Task<Saga?> GetByIdAsync(string id, CancellationToken ct = default) =>
        Task.FromResult(_sagas.TryGetValue(id, out var saga) ? saga : null);
    
    public Task AddAsync(Saga entity, CancellationToken ct = default)
    {
        _sagas[entity.Id] = entity;
        return Task.CompletedTask;
    }
    
    public Task UpdateAsync(Saga entity, CancellationToken ct = default)
    {
        _sagas[entity.Id] = entity;
        return Task.CompletedTask;
    }
}

public class FakeSagaStepRepository : ISagaStepRepository
{
    private readonly Dictionary<string, List<SagaStep>> _steps = new();
    
    public Task<List<SagaStep>> GetBySagaIdAsync(string sagaId, CancellationToken ct = default) =>
        Task.FromResult(_steps.TryGetValue(sagaId, out var steps) ? steps : new List<SagaStep>());
    
    public Task UpdateAsync(SagaStep entity, CancellationToken ct = default)
    {
        if (!_steps.TryGetValue(entity.SagaId, out var steps))
            steps = new List<SagaStep>();
        var existing = steps.FirstOrDefault(s => s.Id == entity.Id);
        if (existing != null) existing = entity;
        else steps.Add(entity);
        _steps[entity.SagaId] = steps;
        return Task.CompletedTask;
    }
}
```

### Usage Example

```csharp
using SagaOrchestrator.Infrastructure.Integration;
using SagaOrchestrator.Infrastructure.Events;

// Create webhook handler instance
var webhookHandler = new WebhookHandler(
    new HttpClientFactory(),
    new EventBus(),
    new LoggerFactory().CreateLogger<WebhookHandler>());

// Subscribe to webhook
await webhookHandler.SubscribeWebhookAsync("https://example.com/webhook", new[] { "OrderCreated", "OrderUpdated" });

// Send webhook
await webhookHandler.SendWebhookAsync<OrderCreatedEvent>("https://example.com/webhook", new OrderCreatedEvent { OrderId = "ORDER-123" });

// Get active subscriptions
var subscriptions = webhookHandler.GetSubscriptions();
Console.WriteLine(subscriptions.Count);
```

This example demonstrates:
1. Creating a webhook handler instance
2. Subscribing to a webhook with specific event types
3. Sending a webhook with an `OrderCreatedEvent` payload
4. Retrieving a list of active subscriptions