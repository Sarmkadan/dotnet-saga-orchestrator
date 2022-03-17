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
