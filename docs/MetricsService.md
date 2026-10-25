# MetricsService

`MetricsService` is the read-only statistics component behind `IMetricsService`. It reads sagas and saga steps from the repositories and calculates counts, success and failure rates, durations, and percentiles. It does not change any saga or step. Every call reads the current repository contents, so it computes fresh numbers each time and keeps no cache.

Each public method catches exceptions, logs them through `ILogger<MetricsService>`, and rethrows them. Unlike `HealthCheckService`, it never returns a fallback result.

## Registration

`AddSagaOrchestrator()` registers `MetricsService` as a singleton behind the `IMetricsService` interface. The concrete class is not registered on its own, so resolve `IMetricsService` in application code. Resolving `MetricsService` directly from the container fails unless you add that registration yourself.

## Public API

### Constructor

```csharp
public MetricsService(
    ISagaRepository sagaRepository,
    ISagaStepRepository stepRepository,
    ILogger<MetricsService> logger)
```

- `sagaRepository` supplies the sagas used for saga-level and duration statistics.
- `stepRepository` is injected and null-checked, but the current methods read steps through the saga's `Steps` collection, so it is not used for queries.
- `logger` receives error logs when a calculation fails.

All three arguments must be non-null; otherwise the constructor throws `ArgumentNullException`.

### IMetricsService

```csharp
public interface IMetricsService
{
    Task<SagaMetrics> GetMetricsAsync();
    Task<StepMetrics> GetStepMetricsAsync(string stepName);
    Task<PerformanceStats> GetPerformanceStatsAsync();
    Task<MetricsSummary> GetSummaryAsync();
}
```

Resolve the interface in application code. The class is the default implementation.

### GetMetricsAsync

```csharp
Task<SagaMetrics> GetMetricsAsync()
```

Counts sagas by status across the whole repository.

- `TotalSagas`, `CompletedSagas`, `FailedSagas`, `RunningSagas`, and `CompensatedSagas` are counts by status. Each saga is counted only under its own status.
- `SuccessRate` is `CompletedSagas / TotalSagas * 100`. `FailureRate` is `FailedSagas / TotalSagas * 100`. Both are percentages from `0` to `100`, and both are `0` when there are no sagas.
- `AverageDurationSeconds` is the mean of `(CompletedAt - CreatedAt)` over completed sagas. A completed saga with no `CompletedAt` uses the current UTC time. The value is `0` when no saga has completed.
- `Timestamp` is the current UTC time.

### GetStepMetricsAsync

```csharp
Task<StepMetrics> GetStepMetricsAsync(string stepName)
```

Collects every step named `stepName` from all sagas and counts them by status.

- **Parameters:**
  - `stepName` (string): The exact step name to match, compared with `==`.
- `TotalExecutions`, `SuccessfulExecutions`, and `FailedExecutions` count the matching steps by status. Steps in other states count toward `TotalExecutions` only.
- `SuccessRate` is `SuccessfulExecutions / TotalExecutions * 100`, or `0` when no step matches.
- `AverageDurationMs` is the mean of `(CompletedAt - StartedAt)` in milliseconds over completed steps. Missing timestamps fall back to the current UTC time. The value is `0` when no matching step has completed.
- `StepName` echoes the argument, and `Timestamp` is the current UTC time.

The method does not validate `stepName`. A null or whitespace value simply matches no steps, unless a step's name is also that value.

### GetPerformanceStatsAsync

```csharp
Task<PerformanceStats> GetPerformanceStatsAsync()
```

Computes duration statistics for completed sagas, using the same duration as `GetMetricsAsync` (`CompletedAt - CreatedAt`, in seconds).

- `AverageDurationSeconds` is taken from `GetMetricsAsync()`.
- `MinDurationSeconds` and `MaxDurationSeconds` are the smallest and largest durations. Both are `0` when no saga has completed.
- `MedianDurationSeconds` is the middle value. For an even count it is the average of the two middle values.
- `P95DurationSeconds` and `P99DurationSeconds` use the nearest-rank method: the value at index `ceil(n * p / 100) - 1`, clamped to the valid range.
- `Timestamp` is the current UTC time.

This method reads the saga repository twice, once through `GetMetricsAsync()` and once directly. Repository contents can change between the two reads.

### GetSummaryAsync

```csharp
Task<MetricsSummary> GetSummaryAsync()
```

Returns a compact summary as a `MetricsSummary` DTO.

- `TotalSagas` is the total number of sagas.
- `ByStatus` is a dictionary with the keys `Completed`, `Failed`, `Running`, `Compensated`, and `Pending`. Each value is that status's count. Statuses not listed here, such as `Initialized` or `Compensating`, are not included.
- `AverageDurationSeconds` is the same value as in `GetMetricsAsync`.
- `CompensationRate` is `CompensatedSagas / TotalSagas * 100`, or `0` when there are no sagas.
- `Timestamp` is the current UTC time.

### Result types

| Type | Defined in | Fields |
|------|------------|--------|
| `SagaMetrics` | `MetricsService.cs` | `TotalSagas`, `CompletedSagas`, `FailedSagas`, `RunningSagas`, `CompensatedSagas`, `SuccessRate`, `FailureRate`, `AverageDurationSeconds`, `Timestamp` |
| `StepMetrics` | `MetricsService.cs` | `StepName`, `TotalExecutions`, `SuccessfulExecutions`, `FailedExecutions`, `SuccessRate`, `AverageDurationMs`, `Timestamp` |
| `PerformanceStats` | `MetricsService.cs` | `AverageDurationSeconds`, `MinDurationSeconds`, `MaxDurationSeconds`, `MedianDurationSeconds`, `P95DurationSeconds`, `P99DurationSeconds`, `Timestamp` |
| `MetricsSummary` | `Application/DTOs/MetricsSummary.cs` | `TotalSagas`, `ByStatus`, `AverageDurationSeconds`, `CompensationRate`, `Timestamp` |

Saga and step statuses are compared by their string names (for example `s.Status.ToString() == "Completed"`).

## Usage

The example below resolves `IMetricsService` from the container set up by `AddSagaOrchestrator()`.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Configuration;

var services = new ServiceCollection();
services.AddLogging();
services.AddSagaOrchestrator();

var provider = services.BuildServiceProvider();
IMetricsService metrics = provider.GetRequiredService<IMetricsService>();

SagaMetrics saga = await metrics.GetMetricsAsync();
Console.WriteLine($"Total: {saga.TotalSagas}, success: {saga.SuccessRate:F1}%, failed: {saga.FailedSagas}");

StepMetrics step = await metrics.GetStepMetricsAsync("Reserve Inventory");
Console.WriteLine($"{step.StepName}: {step.SuccessfulExecutions}/{step.TotalExecutions} succeeded, avg {step.AverageDurationMs:F0} ms");

PerformanceStats perf = await metrics.GetPerformanceStatsAsync();
Console.WriteLine($"p95: {perf.P95DurationSeconds:F2}s, p99: {perf.P99DurationSeconds:F2}s");
```

Because `MetricsService` recomputes from the repositories on every call, poll it at a rate that suits your monitoring. Avoid calling it on every request in a hot path.
