# ICircuitBreaker

The `ICircuitBreaker` interface defines a mechanism for managing the execution of potentially unreliable operations in distributed systems. By tracking the success and failure rates of these operations, the component provides a protective layer that temporarily halts execution when a target service is deemed unstable, allowing it time to recover before resuming further attempts.

## API

### Constructors
- `public CircuitBreaker()`
  Initializes a new instance of the `CircuitBreaker`.

### Methods
- `public async Task<bool> ExecuteAsync(Func<Task<bool>> action)`
  Executes an asynchronous action that returns a boolean. If the circuit is open, the action is not executed, and a default response is returned. Throws `CircuitBreakerOpenException` if the circuit is currently open and prevents execution.

- `public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)`
  Executes an asynchronous action that returns a value of type `T`. If the circuit is open, this method does not execute the action. Throws `CircuitBreakerOpenException` if the circuit is currently open.

- `public CircuitBreakerState GetState()`
  Retrieves the current state of the circuit breaker (e.g., Open, Closed, Half-Open).

- `public void Reset()`
  Resets the internal failure counters and transitions the circuit breaker state to Closed.

- `public int EvictStaleEntries()`
  Removes internal tracking entries deemed stale based on time thresholds, returning the count of evicted entries.

### Properties
- `public CircuitBreakerState State`
  Gets the current operational state of the circuit breaker.

- `public int FailureCount`
  Gets the total number of recorded failures since the last reset or initialization.

- `public int SuccessCount`
  Gets the total number of recorded successes since the last reset or initialization.

- `public DateTime LastFailureTime`
  Gets the timestamp of the most recent operation failure.

- `public DateTime LastAccessedAt`
  Gets the timestamp of the most recent interaction with the circuit breaker.

## Usage

### Executing a Protected Operation
```csharp
var circuitBreaker = new CircuitBreaker();

try
{
    var result = await circuitBreaker.ExecuteAsync(async () =>
    {
        // Call to an external service or unstable component
        return await externalService.ProcessRequestAsync();
    });
}
catch (CircuitBreakerOpenException)
{
    // Handle the scenario where the service is currently down
    Logger.LogWarning("Circuit breaker is open; service is temporarily unavailable.");
}
```

### Checking State Before Execution
```csharp
var circuitBreaker = new CircuitBreaker();

if (circuitBreaker.GetState() != CircuitBreakerState.Open)
{
    await circuitBreaker.ExecuteAsync(async () =>
    {
        await database.SaveAsync();
        return true;
    });
}
else
{
    // Fallback logic if the circuit is open
    await secondaryStorage.SaveAsync();
}
```

## Notes

- **Thread Safety:** The implementation is designed to be thread-safe. Multiple threads can safely invoke `ExecuteAsync` concurrently, and internal state counters are updated atomically to ensure consistency across execution contexts.
- **State Transitions:** The circuit breaker state transitions are typically driven by failure thresholds defined in the configuration. A transition to the Open state prevents further calls for a defined cool-down period.
- **Resetting:** The `Reset` method forces the breaker back to a Closed state, regardless of recent failure counts. This should be used with caution, typically when manual intervention or external validation indicates that the downstream service has recovered.
- **Stale Entries:** The `EvictStaleEntries` method allows for periodic cleanup of internal metrics and tracking data to prevent memory growth over long-running processes.
