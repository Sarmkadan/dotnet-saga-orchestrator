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

## IServiceRegistry

The `IServiceRegistry` interface provides a centralized registry for tracking external microservices used by saga steps. It maintains service endpoints, health status, configuration metadata, and enables runtime service discovery and health monitoring across distributed operations.

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

## IWebhookHandler

The `IWebhookHandler` interface manages webhook subscriptions and reliable delivery of events to external systems. It provides methods for subscribing, unsubscribing, and sending webhooks, as well as retrieving a list of active subscriptions.

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