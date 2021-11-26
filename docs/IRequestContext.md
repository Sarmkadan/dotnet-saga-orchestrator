# IRequestContext

The `IRequestContext` interface defines the contract for a request-scoped context used throughout the `dotnet-saga-orchestrator` library. It captures correlation, user, and tenant identifiers, tracks execution timing, and stores arbitrary metadata. Implementations are expected to be created at the start of a request and disposed or cleared when the request completes. The interface provides methods to record and retrieve performance timings, manage nested or cloned contexts via `GetContext`/`SetContext`, and produce a human-readable string representation.

## API

### `public string CorrelationId`

Gets the unique correlation identifier for the current request. This value is set at context creation and should not change during the lifetime of the context.

### `public string? UserId`

Gets or sets the identifier of the user associated with the request. May be `null` if the request is unauthenticated.

### `public string? TenantId`

Gets or sets the tenant identifier for multi-tenant scenarios. May be `null` if the system does not use tenancy.

### `public DateTime StartTime`

Gets the UTC timestamp when the context was created. This value is set once and is immutable after construction.

### `public Dictionary<string, object> Metadata`

Gets a mutable dictionary of arbitrary key-value pairs associated with the request. Consumers can add, update, or remove entries as needed. The dictionary is not thread-safe by default.

### `public RequestContext`

Gets the underlying `RequestContext` object that implements this interface. This property exposes the concrete type for advanced scenarios where direct access to implementation details is required.

### `public override string ToString()`

Returns a string representation of the context, typically including the correlation ID, user ID, tenant ID, start time, and a summary of recorded timings. The exact format is implementation-defined.

### `public IRequestContext GetContext()`

Creates and returns a new `IRequestContext` instance that is a shallow copy of the current context. The new context shares the same `CorrelationId`, `UserId`, `TenantId`, `StartTime`, and a reference to the same `Metadata` dictionary. Timings are not copied; the new context starts with an empty timing store.

**Returns:** A new `IRequestContext` instance.

### `public void SetContext(IRequestContext context)`

Replaces the current context's state with the state of the provided `context`. After calling this method, the current context will have the same correlation ID, user ID, tenant ID, start time, metadata, and timings as the supplied context. This is typically used to restore a previously saved context.

**Parameters:**
- `context` – The `IRequestContext` whose state will be copied into the current instance.

**Throws:**
- `ArgumentNullException` – if `context` is `null`.

### `public PerformanceTracker`

Gets the `PerformanceTracker` instance associated with this context. The tracker is used to record and retrieve timing measurements.

### `public void RecordTiming(string key, long elapsedMilliseconds)`

Records a timing measurement identified by `key`. If a timing with the same key already exists, its value is overwritten.

**Parameters:**
- `key` – A non-null, non-empty string that identifies the timing.
- `elapsedMilliseconds` – The elapsed time in milliseconds to record.

**Throws:**
- `ArgumentNullException` – if `key` is `null`.
- `ArgumentException` – if `key` is empty or consists only of whitespace.

### `public long? GetTiming(string key)`

Retrieves the recorded timing for the specified `key`.

**Parameters:**
- `key` – The key of the timing to retrieve.

**Returns:** The recorded elapsed time in milliseconds, or `null` if no timing exists for the given key.

**Throws:**
- `ArgumentNullException` – if `key` is `null`.

### `public Dictionary<string, long> GetAllTimings()`

Returns a new dictionary containing all recorded timings. The returned dictionary is a copy; modifications to it do not affect the internal timing store.

**Returns:** A `Dictionary<string, long>` with all recorded key-timing pairs.

### `public long GetTotalElapsedMs()`

Calculates and returns the total elapsed time in milliseconds since the context's `StartTime` (i.e., `DateTime.UtcNow - StartTime`). This value is computed at the time of the call and is not cached.

**Returns:** The total elapsed milliseconds as a `long`.

### `public void Clear()`

Resets the context to its initial state. This clears all metadata entries, removes all recorded timings, and resets the `UserId` and `TenantId` to `null`. The `CorrelationId` and `StartTime` remain unchanged.

## Usage

### Example 1: Basic request tracking with timing

```csharp
public void ProcessOrder(IRequestContext context)
{
    // Record the start of a processing step
    var start = DateTime.UtcNow;
    // ... perform work ...
    var elapsed = (long)(DateTime.UtcNow - start).TotalMilliseconds;
    context.RecordTiming("ValidateOrder", elapsed);

    // Later, retrieve the timing
    long? validationTime = context.GetTiming("ValidateOrder");
    Console.WriteLine($"Validation took {validationTime} ms");

    // Get total request duration
    long total = context.GetTotalElapsedMs();
    Console.WriteLine($"Total request time: {total} ms");
}
```

### Example 2: Creating a child context and restoring state

```csharp
public async Task ExecuteSagaStep(IRequestContext parentContext)
{
    // Create a child context that inherits correlation, user, tenant, and metadata
    IRequestContext childContext = parentContext.GetContext();

    // Record a timing specific to this step
    childContext.RecordTiming("Step1", 150);

    // After the step completes, merge timings back into the parent
    parentContext.SetContext(childContext);

    // The parent now contains the "Step1" timing
    long? step1Timing = parentContext.GetTiming("Step1");
    // step1Timing == 150
}
```

## Notes

- **Thread safety:** The `IRequestContext` interface does not guarantee thread safety. Concurrent access to `Metadata`, `RecordTiming`, `GetTiming`, `GetAllTimings`, `Clear`, and `SetContext` from multiple threads may result in data corruption. Use external synchronization (e.g., locks) if the context is shared across threads.
- **Metadata dictionary:** The `Metadata` property returns a reference to the same dictionary used internally. Modifications to the dictionary are immediately visible to all code holding a reference to the context. The dictionary is not cloned by `GetContext()`.
- **Timing keys:** Keys passed to `RecordTiming` and `GetTiming` are case-sensitive. Overwriting an existing key replaces the previous value without warning.
- **`GetTotalElapsedMs`:** This method uses `DateTime.UtcNow` and is subject to system clock resolution and adjustments. For high-precision measurements, consider using `Stopwatch` and recording the elapsed time manually.
- **`Clear()` behavior:** After calling `Clear()`, the context retains its original `CorrelationId` and `StartTime`. All other state is reset. This is useful for reusing a context object in a pool, but note that `GetTotalElapsedMs()` will continue to increase because `StartTime` is unchanged.
- **`SetContext` vs `GetContext`:** `GetContext` creates a new instance; `SetContext` mutates the existing instance. Calling `SetContext` with a context that was obtained via `GetContext` is a common pattern for propagating timings from a child back to a parent.
