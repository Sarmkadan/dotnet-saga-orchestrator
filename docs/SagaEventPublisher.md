# SagaEventPublisher

`SagaEventPublisher` manages the lifecycle of domain events within a saga orchestration context. It provides subscription, publication, retrieval, and export capabilities for `SagaEvent` instances, acting as a centralized event store and dispatcher for long-running business transactions.

## API

### `public void Subscribe`

Registers a handler or observer that will be notified when events are published. The exact subscription mechanism depends on the internal implementation, but the method establishes a listener relationship without returning a value.

- **Parameters:** Not shown in the signature; typically accepts a delegate or observer instance.
- **Returns:** `void`
- **Throws:** May throw `ArgumentNullException` if a required subscriber argument is null.

### `public async Task PublishAsync`

Publishes a `SagaEvent` asynchronously, persisting it internally and notifying all active subscribers. Two overloads exist, likely differentiated by event payload or context parameters.

- **Parameters:** A `SagaEvent` or event-defining arguments.
- **Returns:** `Task` representing the asynchronous operation.
- **Throws:** May throw `InvalidOperationException` if the publisher has been disposed or is in an invalid state.

### `public async Task PublishAsync`

Second overload of the publish method. See the description above; the distinction is in the accepted parameter set.

- **Parameters:** Alternative event specification.
- **Returns:** `Task`
- **Throws:** Same conditions as the first overload.

### `public List<SagaEvent> GetSagaEvents`

Retrieves all events associated with a specific saga instance.

- **Parameters:** Likely a saga identifier (not shown in signature).
- **Returns:** `List<SagaEvent>` containing matching events, or an empty list if none exist.
- **Throws:** No exceptions documented for missing sagas; returns empty list.

### `public List<SagaEvent> GetEventsByType`

Filters and returns events of a particular type or category.

- **Parameters:** A type discriminator (string, enum, or `Type`).
- **Returns:** `List<SagaEvent>` filtered by the specified type.
- **Throws:** May throw `ArgumentException` for an unrecognized type specifier.

### `public List<SagaEvent> GetAllEvents`

Returns every `SagaEvent` currently held by the publisher, regardless of saga or type affiliation.

- **Parameters:** None.
- **Returns:** `List<SagaEvent>` of all stored events.
- **Throws:** None.

### `public int GetEventCount`

Provides the total number of events stored in the publisher.

- **Parameters:** None.
- **Returns:** `int` representing the count.
- **Throws:** None.

### `public void ClearEvents`

Removes all stored events and resets internal state. Subscriptions are typically preserved unless explicitly cleared elsewhere.

- **Parameters:** None.
- **Returns:** `void`
- **Throws:** None.

### `public async Task ExportEventsAsync`

Asynchronously serializes and exports all stored events to a predetermined format or destination (e.g., file, stream, or external storage).

- **Parameters:** May accept an output target (not shown in signature).
- **Returns:** `Task`
- **Throws:** May throw `IOException` or `InvalidOperationException` if the export destination is unavailable.

## Usage

### Example 1: Basic Publish and Retrieve

```csharp
var publisher = new SagaEventPublisher();
publisher.Subscribe(myHandler);

var orderCreated = new SagaEvent("OrderCreated", orderId, new { Amount = 250.00 });
await publisher.PublishAsync(orderCreated);

var sagaEvents = publisher.GetSagaEvents(orderId);
Console.WriteLine($"Events for saga {orderId}: {sagaEvents.Count}");
```

### Example 2: Export After Multiple Publications

```csharp
var publisher = new SagaEventPublisher();

await publisher.PublishAsync(new SagaEvent("PaymentProcessed", paymentId));
await publisher.PublishAsync(new SagaEvent("InventoryReserved", inventoryId));

if (publisher.GetEventCount() > 0)
{
    await publisher.ExportEventsAsync(outputStream);
    publisher.ClearEvents();
}
```

## Notes

- **Thread Safety:** `PublishAsync` and `ExportEventsAsync` are asynchronous and should be awaited to avoid race conditions. `GetSagaEvents`, `GetAllEvents`, `GetEventCount`, and `ClearEvents` are synchronous and may reflect stale state if called concurrently with an in-flight publish. External synchronization is recommended when mixing reads and writes across threads.
- **Empty Results:** All `Get*` methods return empty lists rather than null when no matching events exist. Callers should guard against null only if the publisher itself is null.
- **Subscription Lifetime:** `Subscribe` establishes a listener that remains active until explicitly removed or the publisher is disposed. Failing to unsubscribe can lead to memory leaks in long-lived applications.
- **Export Consistency:** `ExportEventsAsync` captures the event set at the moment of invocation. Events published during the export operation may or may not appear in the exported payload depending on internal snapshot semantics.
- **Clear Semantics:** `ClearEvents` does not affect subscribers; they remain registered and will receive future publications after the clear operation.
