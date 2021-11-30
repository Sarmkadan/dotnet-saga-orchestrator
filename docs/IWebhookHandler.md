# IWebhookHandler

The `IWebhookHandler` interface facilitates the management and dispatch of webhook notifications within the `dotnet-saga-orchestrator` framework, providing a structured mechanism for saga participants to communicate events asynchronously to external services.

## API

### Members

*   **`WebhookHandler`**
    Initializes a new instance of the `WebhookHandler` class.
*   **`Task SubscribeWebhookAsync(...)`**
    Registers a new webhook subscription.
*   **`Task UnsubscribeWebhookAsync(...)`**
    Removes an existing webhook subscription.
*   **`Task SendWebhookAsync<T>(...)`**
    Dispatches a payload of type `T` to the configured webhook URL. Throws `HttpRequestException` on network failure or serialization errors.
*   **`List<WebhookSubscription> GetSubscriptions()`**
    Retrieves the list of active webhook subscriptions.
*   **`string Id`**
    Gets the unique identifier of the webhook handler instance.
*   **`string Url`**
    Gets the primary URL endpoint associated with the handler.
*   **`string WebhookUrl`**
    Gets the specific destination URL for webhook delivery.
*   **`string[] EventTypes`**
    Gets the collection of event types handled by this instance.
*   **`string EventTypes`**
    Gets the string representation of the event types.
*   **`DateTime CreatedAt`**
    Gets the timestamp indicating when the handler was created.
*   **`DateTime LastUpdated`**
    Gets the timestamp of the last update to the handler configuration.
*   **`bool IsActive`**
    Gets a value indicating whether the webhook handler is currently active.
*   **`int DeliveryCount`**
    Gets the total count of successful deliveries.
*   **`int FailureCount`**
    Gets the total count of failed delivery attempts.

## Usage

```csharp
// Example 1: Subscribing to an event
var handler = new WebhookHandler();
await handler.SubscribeWebhookAsync("OrderCreated", "https://api.example.com/webhooks/order");
```

```csharp
// Example 2: Sending a payload
var payload = new OrderCreatedEvent { OrderId = 12345 };
await handler.SendWebhookAsync<OrderCreatedEvent>(payload);
```

## Notes

*   **Thread Safety**: Implementations are expected to be thread-safe regarding subscription management and asynchronous dispatch operations.
*   **Async/Await**: All network-dependent operations (`SubscribeWebhookAsync`, `UnsubscribeWebhookAsync`, `SendWebhookAsync`) must be awaited to prevent blocking the orchestration process.
*   **Member Ambiguity**: Note the presence of duplicate property identifiers (`Url` and `WebhookUrl`; `string[] EventTypes` and `string EventTypes`); ensure implementations explicitly define these to satisfy the interface contract.
*   **Error Handling**: `SendWebhookAsync` requires robust error handling at the calling level to manage retries or logging for failed delivery attempts, indicated by `FailureCount`.
