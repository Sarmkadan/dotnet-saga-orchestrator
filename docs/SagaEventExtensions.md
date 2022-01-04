# SagaEventExtensions

Provides utility extension methods for filtering and querying `SagaEvent` collections based on severity, event types, and error states.

## API

### `FilterBySeverity`
Filters a sequence of `SagaEvent` objects to include only those with the specified severity.

- **Parameters**
  - `events`: The sequence of `SagaEvent` objects to filter.
  - `severity`: The `SagaEventSeverity` to match against each event's `Severity` property.
- **Return Value**
  - An `IEnumerable<SagaEvent>` containing all events from the input sequence that match the specified severity.
- **Exceptions**
  - Throws `ArgumentNullException` if `events` is `null`.

### `FindFirstError`
Locates the first `SagaEvent` in a sequence that represents an error state.

- **Parameters**
  - `events`: The sequence of `SagaEvent` objects to search.
- **Return Value**
  - The first `SagaEvent` with `Severity` equal to `SagaEventSeverity.Error`, or `null` if no such event exists.
- **Exceptions**
  - Throws `ArgumentNullException` if `events` is `null`.

### `HasEventType`
Determines whether any event in the sequence matches the specified event type.

- **Parameters**
  - `events`: The sequence of `SagaEvent` objects to check.
  - `eventType`: The `SagaEventType` to match against each event's `Type` property.
- **Return Value**
  - `true` if at least one event in the sequence has a matching `Type`; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `events` is `null`.

### `ForSaga`
Filters a sequence of `SagaEvent` objects to include only those associated with a specific saga.

- **Parameters**
  - `events`: The sequence of `SagaEvent` objects to filter.
  - `sagaId`: The unique identifier of the saga to match against each event's `SagaId` property.
- **Return Value**
  - An `IEnumerable<SagaEvent>` containing all events from the input sequence where `SagaId` equals the specified `sagaId`.
- **Exceptions**
  - Throws `ArgumentNullException` if `events` is `null`.

## Usage
