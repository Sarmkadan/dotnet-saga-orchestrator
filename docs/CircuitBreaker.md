# CircuitBreaker

`CircuitBreaker` is the default, thread-safe implementation of `ICircuitBreaker`. A single instance tracks independent circuit state for each string identifier, allowing callers to protect multiple downstream services or operations without creating a breaker for each one.

For every identifier, the circuit starts `Closed`. Consecutive action failures open it after the configured threshold. Once the configured timeout has elapsed, the next execution becomes a half-open probe: success closes the circuit and failure opens it again. Exceptions raised by an action are recorded and rethrown. Cancellation requested through the supplied token is rethrown without being recorded as a failure.

## Public API

### Constructor

```csharp
public CircuitBreaker(
    int failureThreshold = 5,
    int timeoutSeconds = 60,
    ISagaLogger? logger = null)
```

- `failureThreshold` is the number of consecutive failures that opens a closed circuit. It must be greater than zero.
- `timeoutSeconds` is the time an open circuit waits before allowing a half-open probe.
- `logger` optionally receives circuit state-change events.

### ExecuteAsync

```csharp
Task<bool> ExecuteAsync(Func<Task> action, string identifier)

Task<bool> ExecuteAsync(
    Func<CancellationToken, Task> action,
    string identifier,
    CancellationToken cancellationToken)
```

Executes an asynchronous operation under the circuit associated with `identifier`. These overloads return `true` when the action runs successfully. If the circuit rejects the call, they do not invoke the action and return `false`.

```csharp
Task<T> ExecuteAsync<T>(Func<Task<T>> action, string identifier)

Task<T> ExecuteAsync<T>(
    Func<CancellationToken, Task<T>> action,
    string identifier,
    CancellationToken cancellationToken)
```

Executes an asynchronous operation and returns its result. If the circuit rejects the call, these overloads throw `InvalidOperationException`.

All overloads require a non-null action and a non-null, non-empty identifier. The cancellation overloads throw `OperationCanceledException` when cancelled.

### GetState

```csharp
CircuitBreakerState GetState(string identifier)
```

Returns `Closed`, `Open`, or `HalfOpen` for the identifier. An identifier with no recorded state is `Closed`. For an open circuit whose timeout has elapsed, this method reports `HalfOpen`; the state is claimed for a probe when an execution is attempted.

### Reset

```csharp
void Reset(string identifier)
```

Removes the recorded state for the identifier, returning it to `Closed`.

### EvictStaleEntries

```csharp
int EvictStaleEntries(TimeSpan maxIdleTime)
```

Removes closed circuit entries that have not been accessed within `maxIdleTime` and returns the number selected for eviction. Call this periodically when identifiers are created dynamically to limit retained metrics. Open and half-open entries are not evicted.

## Usage

```csharp
using SagaOrchestrator.Infrastructure.Resilience;

var breaker = new CircuitBreaker(failureThreshold: 3, timeoutSeconds: 30);

var accepted = await breaker.ExecuteAsync(
    async cancellationToken =>
    {
        await paymentClient.SendAsync(request, cancellationToken);
    },
    identifier: "payment-service",
    cancellationToken);

if (!accepted)
{
    // The payment-service circuit is open; use a fallback or retry later.
}
```

Use a stable identifier for each protected dependency. Failures and state transitions are isolated by identifier, and `Reset(identifier)` affects only that dependency.
