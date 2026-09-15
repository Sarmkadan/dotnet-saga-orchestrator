# Saga logging

`src/Infrastructure/Logging/LoggingMiddleware.cs` provides the structured logging contract and default logger for saga lifecycle events. Despite the source filename, it does not define ASP.NET Core middleware. It defines `ISagaLogger` and its `SagaLogger` implementation in the `SagaOrchestrator.Infrastructure.Logging` namespace.

`SagaLogger` writes through `Microsoft.Extensions.Logging.ILogger<SagaLogger>`. Its methods add saga and step properties to logging scopes and emit informational, warning, or error events for creation, execution, compensation, completion, failure, execution timelines, and circuit-breaker transitions.

## Public API

### `ISagaLogger`

The interface exposes these operations:

- `void LogSagaCreated(Saga saga)` logs saga creation at information level.
- `void LogStepStarted(Saga saga, SagaStep step)` logs the start of a step at information level.
- `void LogStepCompleted(Saga saga, SagaStep step, TimeSpan duration)` logs successful step completion and its duration in milliseconds at information level.
- `void LogStepFailed(Saga saga, SagaStep step, Exception ex)` logs a step failure, retry count, and exception at error level.
- `void LogCompensationStarted(Saga saga)` logs the compensation strategy at warning level.
- `void LogCompensationCompleted(Saga saga)` logs completed compensation at information level.
- `void LogSagaCompleted(Saga saga, TimeSpan duration)` logs successful saga completion, duration in seconds, and the completed-step count at information level.
- `void LogSagaFailed(Saga saga, Exception ex)` logs a saga failure and exception at error level.
- `void LogExecutionTimeline(Saga saga)` orders the saga's steps by `Order`, then logs a summary followed by each step's status, duration when available, start time, and retry count.
- `void LogCircuitBreakerStateChanged(string identifier, string transition, object? details = null)` logs a circuit-breaker transition at information level and includes `details` in the logging scope when supplied.

### `SagaLogger`

`SagaLogger` is the default public implementation of `ISagaLogger`.

```csharp
public SagaLogger(ILogger<SagaLogger> logger)
```

The constructor requires an `ILogger<SagaLogger>` and throws `ArgumentNullException` when `logger` is `null`. The logging methods use the supplied `Saga`, `SagaStep`, and `Exception` values directly; callers should provide non-null arguments.

## Usage

Inject `ISagaLogger` into the component coordinating a saga, then pass the current domain objects to the appropriate lifecycle methods:

```csharp
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Infrastructure.Logging;

public sealed class OrderSagaRunner
{
    private readonly ISagaLogger _log;

    public OrderSagaRunner(ISagaLogger log) => _log = log;

    public void RecordCompletedStep(Saga saga, SagaStep step, TimeSpan elapsed)
    {
        _log.LogStepStarted(saga, step);
        _log.LogStepCompleted(saga, step, elapsed);
    }
}
```

Configure the underlying `Microsoft.Extensions.Logging` providers and minimum levels in the host application. Structured properties such as `SagaId`, `SagaName`, `StepId`, and `Duration` are then available to providers that preserve scopes and message-template values.
