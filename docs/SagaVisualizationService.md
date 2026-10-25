# SagaVisualizationService

`SagaVisualizationService` is the default implementation of `ISagaVisualizationService`. It reads saga state from `ISagaRepository` and turns it into `SagaVisualizationSnapshot` objects: plain data that describes a saga's status, progress, timing, and ordered step graph. It can return a snapshot once, return snapshots for every saga, or poll a single saga and push each new snapshot to a callback until the saga finishes.

The service never changes sagas or steps. Every call reads the repository again, so the snapshots reflect the current state and nothing is cached.

## Registration

`AddSagaVisualization()` registers `SagaVisualizationService` as a singleton behind the `ISagaVisualizationService` interface. Call it after `AddSagaOrchestrator()`, because the service needs an `ISagaRepository` from the container:

```csharp
services.AddSagaOrchestrator();
services.AddSagaVisualization();
```

The concrete class is not registered on its own, so resolve `ISagaVisualizationService` in application code.

## Constructor

```csharp
public SagaVisualizationService(
    ISagaRepository sagaRepository,
    ILogger<SagaVisualizationService> logger)
```

- `sagaRepository` supplies the sagas that every method reads.
- `logger` receives error and warning logs described in the sections below.

Both arguments must be non-null; otherwise the constructor throws `ArgumentNullException`.

## Public API

The public surface is the `ISagaVisualizationService` interface:

```csharp
public interface ISagaVisualizationService
{
    Task<SagaVisualizationSnapshot> GetSnapshotAsync(string sagaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SagaVisualizationSnapshot>> GetAllSnapshotsAsync(CancellationToken cancellationToken = default);
    Task StreamLiveStateAsync(
        string sagaId,
        Func<SagaVisualizationSnapshot, Task> onUpdate,
        TimeSpan pollInterval,
        CancellationToken cancellationToken = default);
}
```

### GetSnapshotAsync

```csharp
Task<SagaVisualizationSnapshot> GetSnapshotAsync(string sagaId, CancellationToken cancellationToken = default)
```

Builds a point-in-time snapshot for one saga.

- `sagaId` is the saga identifier. A null, empty, or whitespace value throws `ArgumentException` before the repository is queried.
- If the repository has no saga with that ID, the method throws `KeyNotFoundException` with the message `Saga '<sagaId>' not found.`
- Any other exception from the repository is logged at error level through `ILogger<SagaVisualizationService>` and rethrown.
- `cancellationToken` is accepted but not forwarded to the repository, and the method does not check it.

### GetAllSnapshotsAsync

```csharp
Task<IReadOnlyList<SagaVisualizationSnapshot>> GetAllSnapshotsAsync(CancellationToken cancellationToken = default)
```

Builds one snapshot for every saga the repository returns, using the same logic as `GetSnapshotAsync`. The result is a read-only list, and the method does not filter or sort the sagas. An exception while reading or building snapshots is logged at error level and rethrown. As with `GetSnapshotAsync`, the token is not forwarded to the repository.

### StreamLiveStateAsync

```csharp
Task StreamLiveStateAsync(
    string sagaId,
    Func<SagaVisualizationSnapshot, Task> onUpdate,
    TimeSpan pollInterval,
    CancellationToken cancellationToken = default)
```

Polls one saga and calls `onUpdate` with a snapshot on every tick. The method runs until one of these happens:

1. The saga reaches a terminal state. The callback receives that final snapshot (`IsTerminal == true`), and then the method returns.
2. `cancellationToken` is cancelled. The method returns normally, not with an exception.
3. Any other exception occurs. This includes the `KeyNotFoundException` for an unknown saga and any exception thrown by `onUpdate`. The method logs a warning through the logger and returns normally.

- `sagaId` is validated the same way as in `GetSnapshotAsync`, and throws `ArgumentException` when it is empty.
- `onUpdate` must be non-null; otherwise the method throws `ArgumentNullException`.
- `pollInterval` is the delay between polls. The method does not validate it. A negative value makes `Task.Delay` throw, and the method then stops through the warning path described above.

Because errors end the stream without an exception, callers that need to know why a stream stopped should watch the logs or record their own state inside `onUpdate`.

## Snapshot shape

`SagaVisualizationSnapshot` and `VisualizationNode` are the data objects returned by the service. Their JSON property names are camelCase (`sagaId`, `progressPercent`, `stepId`, and so on), so they can be serialized directly.

| Property | Meaning |
|---|---|
| `SagaId`, `CorrelationId`, `SagaName` | Identifiers and name. `SagaName` uses the definition name and falls back to the saga ID when there is no definition. |
| `Status` | `SagaStatus` as a string, for example `"Running"` or `"Completed"`. |
| `Nodes` | One `VisualizationNode` per step, ordered by the step's `Order`. |
| `CompletedSteps`, `TotalSteps` | Count of steps with status `Completed`, and count of all steps. |
| `ProgressPercent` | `CompletedSteps / TotalSteps * 100`, or `0` when the saga has no steps. Failed or pending steps do not count. |
| `ElapsedMs` | Time from `StartedAt` to `CompletedAt` or `FailedAt`. For a saga without either, the time is measured to the current UTC time. |
| `FailureReason` | The saga's failure reason, or `null`. |
| `CapturedAt` | UTC time when the snapshot was built. |
| `IsTerminal` | `true` when the status is `Completed`, `Compensated`, `Failed`, `Aborted`, or `TimedOut`. |

Each `VisualizationNode` has `Index` (one-based position in the saga), `StepId`, `Name`, `Status` (the step status as a string), `StartedAt`, `CompletedAt`, `RetryCount`, `ErrorMessage`, and `DurationMs`. `DurationMs` is `CompletedAt - StartedAt` in milliseconds, and is `null` unless both timestamps are present.

## Usage

### Read a single snapshot

```csharp
var viz = provider.GetRequiredService<ISagaVisualizationService>();

try
{
    var snapshot = await viz.GetSnapshotAsync(sagaId);

    Console.WriteLine(
        $"{snapshot.SagaName} [{snapshot.Status}]: " +
        $"{snapshot.CompletedSteps}/{snapshot.TotalSteps} steps ({snapshot.ProgressPercent:F0}%)");

    foreach (var node in snapshot.Nodes)
    {
        Console.WriteLine($"  {node.Index}. {node.Name} - {node.Status}");
    }
}
catch (KeyNotFoundException)
{
    Console.WriteLine($"Saga {sagaId} does not exist.");
}
```

### Stream updates until the saga finishes

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

await viz.StreamLiveStateAsync(
    sagaId,
    async snapshot =>
    {
        await dashboardHub.PushAsync(snapshot);
        if (snapshot.IsTerminal)
            Console.WriteLine($"Saga finished with status {snapshot.Status}.");
    },
    pollInterval: TimeSpan.FromMilliseconds(500),
    cancellationToken: cts.Token);
```

The stream stops on its own after the terminal snapshot is delivered, or when the five-minute token fires. In both cases `StreamLiveStateAsync` returns normally.

### List every saga for a dashboard

```csharp
var snapshots = await viz.GetAllSnapshotsAsync();
var json = System.Text.Json.JsonSerializer.Serialize(snapshots);
```

## Related types

- [ISagaVisualizationService](ISagaVisualizationService.md) - the interface this class implements.
- `ISagaStateRenderer` - renders snapshots as text; registered by the same `AddSagaVisualization()` call.
- [Visualization guide](visualization-guide.md) - broader overview of the visualization tools.
