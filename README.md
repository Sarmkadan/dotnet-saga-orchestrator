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
