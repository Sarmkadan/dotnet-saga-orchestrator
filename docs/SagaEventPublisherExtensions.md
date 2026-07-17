# SagaEventPublisherExtensions

Extension methods for publishing and querying `SagaEvent` instances in a saga orchestration context. These methods provide a convenient way to interact with the event store, subscribe to events, and analyze event patterns across sagas.

## API

### `PublishAsync`

Publishes a saga event asynchronously to the event store.

- **Parameters**:
  - `publisher` (`ISagaEventPublisher`): The event publisher instance.
  - `event` (`SagaEvent`): The event to publish.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: Throws `ArgumentNullException` if `publisher` or `event` is `null`.

### `PublishStepEventAsync`

Publishes a step-specific saga event asynchronously.

- **Parameters**:
  - `publisher` (`ISagaEventPublisher`): The event publisher instance.
  - `stepId` (`string`): The identifier of the saga step emitting the event.
  - `event` (`SagaEvent`): The event to publish.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: Throws `ArgumentNullException` if `publisher`, `stepId`, or `event` is `null`.

### `GetEventsBySeverity`

Retrieves a read-only list of events filtered by severity level.

- **Parameters**:
  - `publisher` (`ISagaEventPublisher`): The event publisher instance.
  - `severity` (`SagaEventSeverity`): The severity level to filter by.
- **Return value**: An `IReadOnlyList<SagaEvent>` containing events matching the severity. Empty if none found.
- **Exceptions**: Throws `ArgumentNullException` if `publisher` is `null`.

### `GetRecentEvents`

Retrieves a read-only list of the most recent events, ordered by timestamp in descending order.

- **Parameters**:
  - `publisher` (`ISagaEventPublisher`): The event publisher instance.
  - `count` (`int`): The maximum number of events to return.
- **Return value**: An `IReadOnlyList<SagaEvent>` containing the most recent events. Empty if none found or `count` is non-positive.
- **Exceptions**: Throws `ArgumentNullException` if `publisher` is `null`.

### `GetEventStatistics`

Aggregates event data into a dictionary of statistics.

- **Parameters**:
  - `publisher` (`ISagaEventPublisher`): The event publisher instance.
- **Return value**: A `Dictionary<string, object>` containing keys such as `"TotalEvents"`, `"EventsBySeverity"`, and `"EventsByType"`. Never `null`.
- **Exceptions**: Throws `ArgumentNullException` if `publisher` is `null`.

### `ExportEventsAsync`

Exports all events to a specified output stream asynchronously.

- **Parameters**:
  - `publisher` (`ISagaEventPublisher`): The event publisher instance.
  - `outputStream` (`Stream`): The stream to write event data to.
  - `format` (`string`): The export format (e.g., `"json"`, `"csv"`).
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**:
  - Throws `ArgumentNullException` if `publisher`, `outputStream`, or `format` is `null`.
  - Throws `ArgumentException` if `outputStream` is not writable.
  - Throws `InvalidOperationException` if the format is unsupported.

### `HasEventsOfSeverity`

Checks whether any events of the specified severity exist.

- **Parameters**:
  - `publisher` (`ISagaEventPublisher`): The event publisher instance.
  - `severity` (`SagaEventSeverity`): The severity level to check.
- **Return value**: `true` if at least one event of the given severity exists; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if `publisher` is `null`.

### `SubscribeToType`

Subscribes to events of a specific type.

- **Parameters**:
  - `publisher` (`ISagaEventPublisher`): The event publisher instance.
  - `eventType` (`string`): The type of events to subscribe to.
  - `handler` (`Action<SagaEvent>`): The callback invoked when a matching event is published.
- **Return value**: An `IDisposable` that, when disposed, unsubscribes the handler.
- **Exceptions**: Throws `ArgumentNullException` if `publisher`, `eventType`, or `handler` is `null`.

### `SubscribeToSeverity`

Subscribes to events of a specific severity level.

- **Parameters**:
  - `publisher` (`ISagaEventPublisher`): The event publisher instance.
  - `severity` (`SagaEventSeverity`): The severity level to subscribe to.
  - `handler` (`Action<SagaEvent>`): The callback invoked when a matching event is published.
- **Return value**: An `IDisposable` that, when disposed, unsubscribes the handler.
- **Exceptions**: Throws `ArgumentNullException` if `publisher`, `severity`, or `handler` is `null`.

### `SubscriptionDisposable`

A disposable object returned by subscription methods to manage event handler lifetimes.

- **Members**:
  - `Dispose()`: Removes the associated event handler from the publisher.

### `Dispose`

Releases resources associated with the subscription.

- **Parameters**: None.
- **Return value**: None.
- **Exceptions**: None.

## Usage

### Publishing and Querying Events
