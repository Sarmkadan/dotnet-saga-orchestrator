# HealthCheckService

`HealthCheckService` is the read-side diagnostics component behind `IHealthCheckService`. It aggregates saga state from `ISagaRepository` and service health from `IServiceRegistry` to produce a health snapshot and a metrics dictionary. It does not modify sagas or registrations.

The service records its start time when it is constructed. Uptime values are measured from that moment. Both public methods catch exceptions, log them through `ILogger<HealthCheckService>`, and return a fallback result instead of throwing.

## Public API

### Constructor

```csharp
public HealthCheckService(
    ISagaRepository sagaRepository,
    IServiceRegistry serviceRegistry,
    ILogger<HealthCheckService> logger)
```

- `sagaRepository` supplies the saga list used for counts.
- `serviceRegistry` supplies the registered services and their `IsHealthy` flags.
- `logger` receives error logs when a check fails.

All three arguments must be non-null; otherwise the constructor throws `ArgumentNullException`.

### IHealthCheckService

```csharp
public interface IHealthCheckService
{
    Task<HealthCheckResponse> CheckHealthAsync();
    Task<Dictionary<string, object>> GetMetricsAsync();
}
```

Resolve the interface in application code. The class is the default implementation.

### CheckHealthAsync

```csharp
Task<HealthCheckResponse> CheckHealthAsync()
```

Returns a `HealthCheckResponse` with the following values:

| Condition | `Status` | Notes |
|-----------|----------|-------|
| All registered services are healthy | `"healthy"` | Also returned when no services are registered. |
| One or more registered services are unhealthy | `"degraded"` | Saga counts are still reported. |
| An exception occurs while reading sagas or services | `"unhealthy"` | `ActiveSagas` and `TotalSagas` stay at their default of `0`. |

Other response fields:

- `Timestamp` is the current UTC time.
- `Uptime` is the time elapsed since the service was constructed.
- `ActiveSagas` counts sagas whose status is `Running` or `Compensating`.
- `TotalSagas` counts all sagas returned by the repository.

### GetMetricsAsync

```csharp
Task<Dictionary<string, object>> GetMetricsAsync()
```

Returns a dictionary with the keys below. If an exception occurs, the method logs it and returns an empty dictionary.

| Key | Type | Meaning |
|-----|------|---------|
| `totalSagas` | `int` | Number of sagas in the repository. |
| `completedSagas` | `int` | Sagas with status `Completed`. |
| `failedSagas` | `int` | Sagas with status `Failed`. |
| `runningSagas` | `int` | Sagas with status `Running`. Sagas in `Compensating` are not included. |
| `successRate` | `double` | `completedSagas / totalSagas * 100`, or `0` when there are no sagas. |
| `registeredServices` | `int` | Number of services in the registry. |
| `healthyServices` | `int` | Services whose `IsHealthy` is `true`. |
| `unhealthyServices` | `int` | Services whose `IsHealthy` is `false`. |
| `uptime` | `double` | Seconds since the service was constructed. |
| `averageCompletionTime` | `double` | Mean duration in milliseconds of completed sagas, from `CreatedAt` to `CompletedAt`. `0` when none are completed. |
| `memoryUsage` | `long` | Managed heap size in megabytes, from `GC.GetTotalMemory(false)`. |
| `timestamp` | `DateTime` | Current UTC time. |

Saga statuses are compared by their string name (for example `s.Status.ToString() == "Completed"`).

## Usage

The default `AddSagaOrchestrator()` registration does not add `IHealthCheckService`, so register it in the host. `ISagaRepository` must also be available in the container.

```csharp
using Microsoft.Extensions.Logging;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Data.Repositories;
using SagaOrchestrator.Infrastructure.Integration;

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

var sagaRepository = new InMemorySagaRepository();
var serviceRegistry = new ServiceRegistry(loggerFactory.CreateLogger<ServiceRegistry>());

await serviceRegistry.RegisterServiceAsync(new ServiceDescriptor
{
    Name = "OrderService",
    Url = "https://api.orders.example.com",
    Timeout = 5000,
    MaxRetries = 3
});

IHealthCheckService health = new HealthCheckService(
    sagaRepository,
    serviceRegistry,
    loggerFactory.CreateLogger<HealthCheckService>());

var response = await health.CheckHealthAsync();
Console.WriteLine($"Status: {response.Status}, active sagas: {response.ActiveSagas}");

var metrics = await health.GetMetricsAsync();
Console.WriteLine($"Success rate: {metrics["successRate"]}%");
```

In a web host, expose `CheckHealthAsync()` from a health endpoint and map `Status` to an HTTP code that suits your monitoring. The class does not choose status codes itself.
