# SagaActivitySourceExtensions

`SagaActivitySourceExtensions` provides a set of extension methods for creating and manipulating `Activity` instances that represent discrete operations within a saga orchestration. These methods integrate with `System.Diagnostics.ActivitySource` to emit OpenTelemetry-compatible tracing spans for saga life-cycle events—saga initiation, step execution, compensation, and their respective failure modes—enabling distributed tracing and observability of long-running business transactions.

## API

### StartSaga

```csharp
public static Activity? StartSaga(this ActivitySource source, string sagaName, string sagaId)
```

Creates and starts a root `Activity` representing the initiation of a named saga instance. The returned activity is configured with the saga name as the operation name and the saga identifier attached as a tag. Returns `null` if no listeners are sampling the activity source.

- **Parameters**:
  - `source` (`ActivitySource`): The source from which the activity is created.
  - `sagaName` (`string`): The logical name of the saga type (e.g., `"OrderFulfillmentSaga"`).
  - `sagaId` (`string`): The unique identifier for this saga instance.
- **Returns**: An `Activity?` representing the saga span, or `null` if unsampled.
- **Throws**: `ArgumentNullException` if `source`, `sagaName`, or `sagaId` is `null`.

### RecordSagaComplete

```csharp
public static Activity? RecordSagaComplete(this ActivitySource source, string sagaName, string sagaId)
```

Creates a standalone `Activity` event that records the successful completion of a saga. This method does not require an existing parent activity; it emits a discrete span denoting the terminal success state. Returns `null` if no listeners are active.

- **Parameters**:
  - `source` (`ActivitySource`): The source from which the activity is created.
  - `sagaName` (`string`): The logical name of the saga type.
  - `sagaId` (`string`): The unique identifier for the completed saga instance.
- **Returns**: An `Activity?` representing the completion event, or `null` if unsampled.
- **Throws**: `ArgumentNullException` if `source`, `sagaName`, or `sagaId` is `null`.

### StartStep

```csharp
public static Activity? StartStep(this ActivitySource source, string sagaName, string sagaId, string stepName)
```

Creates and starts a child `Activity` representing the execution of a named step within an ongoing saga. The caller is expected to have an active parent saga activity in the current `Activity.Current` context so that the step span correctly nests under the saga span. The step name is set as the operation name, and saga identifiers are propagated as tags.

- **Parameters**:
  - `source` (`ActivitySource`): The source from which the activity is created.
  - `sagaName` (`string`): The logical name of the saga type.
  - `sagaId` (`string`): The unique identifier for the saga instance.
  - `stepName` (`string`): The name of the step being executed (e.g., `"ReserveInventory"`).
- **Returns**: An `Activity?` representing the step span, or `null` if unsampled.
- **Throws**: `ArgumentNullException` if `source`, `sagaName`, `sagaId`, or `stepName` is `null`.

### RecordStepFailure

```csharp
public static void RecordStepFailure(this ActivitySource source, string sagaName, string sagaId, string stepName, Exception exception)
```

Records a failure event on the current activity for a saga step. This method sets the status of the active `Activity.Current` to `Error` and attaches the exception details—including message, type name, and stack trace—as tags. If there is no current activity, the call is a no-op.

- **Parameters**:
  - `source` (`ActivitySource`): The source used for any supplemental event recording.
  - `sagaName` (`string`): The logical name of the saga type.
  - `sagaId` (`string`): The unique identifier for the saga instance.
  - `stepName` (`string`): The name of the step that failed.
  - `exception` (`Exception`): The exception that caused the step failure.
- **Returns**: Nothing.
- **Throws**: `ArgumentNullException` if `source`, `sagaName`, `sagaId`, `stepName`, or `exception` is `null`.

### StartCompensation

```csharp
public static Activity? StartCompensation(this ActivitySource source, string sagaName, string sagaId, string stepName)
```

Creates and starts a child `Activity` representing the compensation (rollback) of a previously executed saga step. Like `StartStep`, it relies on `Activity.Current` to establish parentage. The operation name is derived from the step name with a compensation suffix, and saga identifiers are included as tags.

- **Parameters**:
  - `source` (`ActivitySource`): The source from which the activity is created.
  - `sagaName` (`string`): The logical name of the saga type.
  - `sagaId` (`string`): The unique identifier for the saga instance.
  - `stepName` (`string`): The name of the step being compensated.
- **Returns**: An `Activity?` representing the compensation span, or `null` if unsampled.
- **Throws**: `ArgumentNullException` if `source`, `sagaName`, `sagaId`, or `stepName` is `null`.

### RecordCompensationFailure

```csharp
public static void RecordCompensationFailure(this ActivitySource source, string sagaName, string sagaId, string stepName, Exception exception)
```

Records a failure event on the current activity for a compensation operation. It sets the status of `Activity.Current` to `Error` and attaches the exception details as tags. If there is no current activity, the call is a no-op.

- **Parameters**:
  - `source` (`ActivitySource`): The source used for any supplemental event recording.
  - `sagaName` (`string`): The logical name of the saga type.
  - `sagaId` (`string`): The unique identifier for the saga instance.
  - `stepName` (`string`): The name of the step whose compensation failed.
  - `exception` (`Exception`): The exception that caused the compensation failure.
- **Returns**: Nothing.
- **Throws**: `ArgumentNullException` if `source`, `sagaName`, `sagaId`, `stepName`, or `exception` is `null`.

## Usage

### Example 1: Successful Saga with Steps

```csharp
using System.Diagnostics;

ActivitySource source = new("SagaOrchestrator", "1.0.0");
string sagaId = Guid.NewGuid().ToString("N");

// Start the saga
using Activity? sagaActivity = source.StartSaga("OrderFulfillmentSaga", sagaId);
if (sagaActivity is null) return;

try
{
    // Execute first step
    using Activity? reserveStep = source.StartStep("OrderFulfillmentSaga", sagaId, "ReserveInventory");
    // ... perform inventory reservation ...
    reserveStep?.SetStatus(ActivityStatusCode.Ok);

    // Execute second step
    using Activity? paymentStep = source.StartStep("OrderFulfillmentSaga", sagaId, "ProcessPayment");
    // ... process payment ...
    paymentStep?.SetStatus(ActivityStatusCode.Ok);

    // Record saga completion
    source.RecordSagaComplete("OrderFulfillmentSaga", sagaId);
}
catch (Exception ex)
{
    source.RecordStepFailure("OrderFulfillmentSaga", sagaId, "ProcessPayment", ex);
    throw;
}
```

### Example 2: Step Failure Triggering Compensation

```csharp
using System.Diagnostics;

ActivitySource source = new("SagaOrchestrator", "1.0.0");
string sagaId = Guid.NewGuid().ToString("N");

using Activity? sagaActivity = source.StartSaga("TravelBookingSaga", sagaId);
if (sagaActivity is null) return;

try
{
    // Book flight
    using (Activity? flightStep = source.StartStep("TravelBookingSaga", sagaId, "BookFlight"))
    {
        // ... book flight ...
        flightStep?.SetStatus(ActivityStatusCode.Ok);
    }

    // Book hotel — this will fail
    using (Activity? hotelStep = source.StartStep("TravelBookingSaga", sagaId, "BookHotel"))
    {
        throw new InvalidOperationException("Hotel unavailable");
    }
}
catch (Exception ex)
{
    source.RecordStepFailure("TravelBookingSaga", sagaId, "BookHotel", ex);

    // Compensate the previously successful flight booking
    using Activity? compensateFlight = source.StartCompensation("TravelBookingSaga", sagaId, "BookFlight");
    try
    {
        // ... cancel flight booking ...
        compensateFlight?.SetStatus(ActivityStatusCode.Ok);
    }
    catch (Exception compEx)
    {
        source.RecordCompensationFailure("TravelBookingSaga", sagaId, "BookFlight", compEx);
    }

    throw;
}
```

## Notes

- All methods that return `Activity?` may return `null` when no `ActivityListener` is configured to sample or listen to the provided `ActivitySource`. Callers must guard against null returns before calling `Dispose` or setting status on the returned activity.
- `RecordStepFailure` and `RecordCompensationFailure` operate on `Activity.Current`. If the calling context has no active activity (e.g., the corresponding `StartStep` or `StartCompensation` returned `null` due to sampling), these methods silently do nothing. No exception is thrown for a missing current activity beyond the explicit null-argument checks.
- `StartStep` and `StartCompensation` establish parent-child relationships via `Activity.Current` at the time of the call. If the caller neglects to set the saga activity as current (e.g., by not using a `using` block or manually setting `Activity.Current`), the resulting spans will not be correctly nested under the saga root span.
- These methods are not thread-safe by design; they rely on the ambient `Activity.Current` which is stored in `AsyncLocal` storage and flows with `ExecutionContext`. In multi-threaded scenarios without proper execution-context flow (e.g., raw `Thread` usage without context propagation), parentage may be lost or spans may leak across unrelated operations. Use `Task`-based asynchrony or explicitly flow `Activity` context when spawning work across threads.
- The `ActivitySource` instance passed to each method should be long-lived and typically matches the source used to create the saga root activity. Using different sources for steps versus the saga root will result in disconnected traces.
