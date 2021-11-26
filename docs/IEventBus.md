# IEventBus
The `IEventBus` interface provides a lightweight publish‑subscribe mechanism for domain events within the saga orchestrator. It allows components to register interest in specific event types, publish events asynchronously, and inspect or manipulate a retained history of published events for debugging, replay, or saga state reconstruction.

## API
### EventBus
**Purpose**  
Exposes the concrete event‑bus implementation that backs this interface, allowing advanced configuration or direct access when needed.  

**Parameters**  
None.  

**Return Value**  
An instance of type `EventBus`.  

**Exceptions**  
- `InvalidOperationException` – if the underlying bus has not been initialized.

### Subscribe<T>
**Purpose**  
Registers a handler for events of type `T`. Subsequent calls to `PublishAsync<T>` will invoke the registered handler.  

**Parameters**  
None (the handler is supplied via the implementing class’s configuration).  

**Return Value**  
`void`.  

**Exceptions**  
- `ArgumentException` – if `T` does not derive from `DomainEvent`.  
- `InvalidOperationException` – if a subscription for `T` already exists.

### Unsubscribe<T>
**Purpose**  
Removes the previously registered handler for events of type `T`.  

**Parameters**  
None.  

**Return Value**  
`void`.  

**Exceptions**  
- `ArgumentException` – if `T` does not derive from `DomainEvent`.  
- `InvalidOperationException` – if no subscription for `T` exists.

### PublishAsync<T>
**Purpose**  
Asynchronously publishes an event of type `T` to all current subscribers.  

**Parameters**  
- `@event` – The event instance to publish. Must not be `null`.  

**Return Value**  
A `Task` that completes when all synchronous handlers have finished.  

**Exceptions**  
- `ArgumentNullException` – if `@event` is `null`.  
- `InvalidOperationException` – if there are no subscribers for `T`.  
- Any exception thrown by a subscriber handler is propagated through the returned task.

### GetEventHistory
**Purpose**  
Retrieves a read‑only snapshot of all events that have been published through this bus since its creation or the last call to `ClearHistory`.  

**Parameters**  
None.  

**Return Value**  
An `IReadOnlyList<DomainEvent>` containing the events in the order they were published.  

**Exceptions**  
- `ObjectDisposedException` – if the bus has been disposed.

### ClearHistory
**Purpose**  
Erases the retained event history, freeing memory and resetting the audit trail.  

**Parameters**  
None.  

**Return Value**  
`void`.  

**Exceptions**  
- `InvalidOperationException` – if the history is currently locked (e.g., during a replay operation).

### EventId
**Purpose**  
Gets the unique identifier of the most recently published event.  

**Parameters**  
None.  

**Return Value**  
A `string` representing the event’s ID; `null` if no event has been published.  

**Exceptions**  
- None.

### OccurredAt
**Purpose**  
Gets the UTC timestamp indicating when the most recent event was published.  

**Parameters**  
None.  

**Return Value**  
A `DateTime` value; `DateTime.MinValue` if no event has been published.  

**Exceptions**  
- None.

### EventType
**Purpose**  
Gets the CLR type name of the most recent event (e.g., `OrderCreated`).  

**Parameters**  
None.  

**Return Value**  
A `string` containing the event type name; `null` if no event has been published.  

**Exceptions**  
- None.

### SagaId
**Purpose**  
Gets the identifier of the saga associated with the most recent event.  

**Parameters**  
None.  

**Return Value**  
A `string` saga ID; `null` if the event is not saga‑related or no event has been published.  

**Exceptions**  
- None.

### SagaName
**Purpose**  
Gets the logical name of the saga associated with the most recent event.  

**Parameters**  
None.  

**Return Value**  
A `string` saga name; `null` if the event is not saga‑related or no event has been published.  

**Exceptions**  
- None.

### DefinitionId
**Purpose**  
Gets the identifier of the saga definition that the most recent event belongs to.  

**Parameters**  
None.  

**Return Value**  
A `string` definition ID; `null` if the event is not saga‑related or no event has been published.  

**Exceptions**  
- None.

### StepCount
**Purpose**  
Gets the total number of steps defined in the saga associated with the most recent event.  

**Parameters**  
None.  

**Return Value**  
An `int` step count; `0` if the event is not saga‑related or no event has been published.  

**Exceptions**  
- None.

### StepId
**Purpose**  
Gets the identifier of the current step within the saga for the most recent event.  

**Parameters**  
None.  

**Return Value**  
A `string` step ID; `null` if the event is not saga‑related or no event has been published.  

**Exceptions**  
- None.

### StepName
**Purpose**  
Gets the descriptive name of the current step within the saga for the most recent event.  

**Parameters**Parameters  
None.  

**Return Value**  
A `string` step name; `null` if the event is not saga‑related or no event has been published.  

**Exceptions**  
- None.

### Order
**Purpose**  
Gets the zero‑based order index of the current step within the saga for the most recent event.  

**Parameters**  
None.  

**Return Value**  
An `int` order value; `-1` if the event is not saga‑related or no event has been published.  

**Exceptions**  
- None.

## Usage
### Subscribing and publishing an event
```csharp
using DotnetSagaOrchestrator.Events;

// Assume bus is obtained via DI or factory
IEventBus bus = GetEventBus();

// Subscribe to OrderCreated events
bus.Subscribe<OrderCreated>();

// Later, when an order is created:
var @event = new OrderCreated
{
    EventId = Guid.NewGuid().ToString(),
    OccurredAt = DateTime.UtcNow,
    SagaId = order.SagaName = "OrderProcessing",
    DefinitionId = "order-process-def",
    StepCount = 3,
    StepId = "step-1",
    StepName = "Validate",
    Order = 0
};

await bus.PublishAsync(@event);

// Retrieve history
IReadOnlyList<DomainEvent> history = bus.GetEventBus bus = GetEventBus();

// Clear history before a test run
bus.ClearHistory();

// Publish a series of events
await bus.PublishAsync(new OrderCreated { /* … */ });
await bus.PublishAsync(new PaymentReceived { /* … */ });
await bus.PublishAsync<OrderShipped>(new OrderShipped { /* … */ });

// Verify that exactly three events were recorded
int count = bus.GetEventHistory.Count; // should be 3
```

### Inspecting the most recent event
string lastEventId = bus.EventId;      // ID of OrderShipped
```

## Notes
- The interface does **not** enforce a particular threading model; implementations must document their own thread‑safety guarantees. The default implementation in this repository is safe for concurrent calls to `Subscribe<T>`, `Unsubscribe<T>`, `PublishAsync<T>`, `GetEventHistory`/`ClearHistory`Subscribe<T>` and `PublishAsync<T>` but **not** for concurrent modifications of the subscription list while a publish operation is in progress.  
- Handlers should avoid throwing unhandled exceptions; if they do, the exception will be propagated via the returned task of `PublishAsync<T>`, potentially faulting the caller.  
- Repeated calls to `Subscribe<T>` for the same event type without an intervening `Unsubscribe<T>` will result in an `InvalidOperationException`.  
- `GetEventHistory` returns a snapshot; modifications to the returned list do not affect the internal history.  
- `ClearHistory` is not reversible; once cleared, the event log cannot be restored.  
- The property members (`EventId`, `OccurredAt`, `EventType`, `SagaId`, `SagaName`, `DefinitionId`, `StepCount`, `StepId`, `StepName`, `Order`) reflect the state of the **most recent** event published through the bus. If no event has been published, they return default values (`null` for strings, `DateTime.MinValue` for dates, `0` or `-1` for numerics).  
- Implementations should consider memory leaks: long‑running buses that retain history indefinitely may need periodic calls to `ClearHistory` or a bounded history strategy.  
- Generic type constraints are not expressed in the interface itself; implementers should restrict `T` to types derived from `DomainEvent` to maintain consistency with the event model.
