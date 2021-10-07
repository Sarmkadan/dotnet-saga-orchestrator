# Saga Visualization Tools Guide

This guide covers how to configure and use the built-in saga visualization services to monitor
execution state, render ASCII diagrams, and stream live updates during saga execution.

---

## Overview

The visualization subsystem consists of two complementary services:

| Component | Type | Responsibility |
|---|---|---|
| `ISagaVisualizationService` | Service | Builds point-in-time snapshots and streams live updates |
| `ISagaStateRenderer` | Renderer | Converts snapshots to human-readable ASCII output |

Snapshots are pure data objects (`SagaVisualizationSnapshot`) and can be serialized to JSON for
external dashboards or logged directly to structured logging sinks.

---

## Configuration

Register both services by calling `AddSagaVisualization()` after `AddSagaOrchestrator()`:

```csharp
// Program.cs / Startup.cs
services.AddSagaOrchestrator();
services.AddSagaVisualization();
```

`AddSagaVisualization()` registers:
- `ISagaVisualizationService` → `SagaVisualizationService` (singleton)
- `ISagaStateRenderer` → `SagaStateRenderer` (singleton)

---

## Getting a Point-in-Time Snapshot

Inject `ISagaVisualizationService` and call `GetSnapshotAsync`:

```csharp
public class OrderController : ControllerBase
{
    private readonly ISagaVisualizationService _viz;

    public OrderController(ISagaVisualizationService viz) => _viz = viz;

    [HttpGet("{sagaId}/state")]
    public async Task<IActionResult> GetState(string sagaId)
    {
        var snapshot = await _viz.GetSnapshotAsync(sagaId);
        return Ok(snapshot);
    }
}
```

The returned `SagaVisualizationSnapshot` includes:

| Property | Description |
|---|---|
| `SagaId` | Unique saga identifier |
| `CorrelationId` | Cross-service correlation identifier |
| `Status` | Current saga status string |
| `ProgressPercent` | Completion percentage (0–100) |
| `CompletedSteps` / `TotalSteps` | Step counts |
| `ElapsedMs` | Milliseconds elapsed since the saga started |
| `IsTerminal` | `true` when saga has reached a final state |
| `Nodes` | Ordered list of step execution details |
| `FailureReason` | Populated only when the saga failed |

---

## Rendering ASCII Output

Inject `ISagaStateRenderer` to produce terminal-friendly output:

```csharp
public class SagaMonitor
{
    private readonly ISagaVisualizationService _viz;
    private readonly ISagaStateRenderer _renderer;

    public SagaMonitor(ISagaVisualizationService viz, ISagaStateRenderer renderer)
    {
        _viz      = viz;
        _renderer = renderer;
    }

    public async Task PrintProgressAsync(string sagaId)
    {
        var snapshot = await _viz.GetSnapshotAsync(sagaId);

        // One-line progress bar
        Console.WriteLine(_renderer.RenderProgressBar(snapshot));

        // Vertical step graph
        Console.WriteLine(_renderer.RenderStateDiagram(snapshot));

        // Full report with header, progress, and step graph
        Console.WriteLine(_renderer.RenderFullReport(snapshot));
    }
}
```

### Example Output

```
[████████████████░░░░] 80.0% (4/5 steps)

  [✓]  1. ReserveInventory    Completed  |  102ms
       |
  [✓]  2. ChargePayment       Completed  |  245ms
       |
  [✓]  3. CreateShipment      Completed  |  88ms
       |
  [►]  4. NotifyCustomer      Executing
       |
  [○]  5. UpdateAuditLog      Pending

============================================================
  OrderProcessing
============================================================
  Saga ID      : f3a1b2c4-...
  Correlation  : e9d8c7b6-...
  Status       : Running
  Progress     : [████████████████░░░░] 80.0% (4/5 steps)
  Elapsed      : 1.23s
  Captured At  : 2025-01-15T10:30:00.000Z
...
```

### Status Icons

| Icon | Status |
|---|---|
| `✓` | Completed |
| `►` | Executing |
| `✗` | Failed |
| `↩` | Compensated |
| `⟳` | WaitingForRetry |
| `⏱` | TimedOut |
| `–` | Skipped |
| `○` | Pending |

---

## Streaming Live State

Use `StreamLiveStateAsync` to poll a saga and react to each state update:

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

await _viz.StreamLiveStateAsync(
    sagaId:       sagaId,
    onUpdate:     async snapshot =>
    {
        Console.Clear();
        Console.WriteLine(_renderer.RenderFullReport(snapshot));
        await Task.CompletedTask;
    },
    pollInterval: TimeSpan.FromSeconds(1),
    cancellationToken: cts.Token);
```

The stream stops automatically when the saga reaches a terminal state
(`Completed`, `Failed`, `Compensated`, `Aborted`, or `TimedOut`), or when the
cancellation token is cancelled.

---

## Getting All Saga Snapshots

Retrieve snapshots for every saga in the system for a dashboard view:

```csharp
var allSnapshots = await _viz.GetAllSnapshotsAsync();

foreach (var s in allSnapshots.Where(s => !s.IsTerminal))
{
    Console.WriteLine($"{s.SagaName,-30} {_renderer.RenderProgressBar(s)}");
}
```

---

## Best Practices

- **Use snapshots for dashboards**: Serialize `SagaVisualizationSnapshot` to JSON and push it
  to a monitoring frontend rather than relying on polling the ASCII renderer in production.
- **Avoid high-frequency polling in production**: A poll interval of 1–5 seconds is suitable
  for interactive CLI tools; prefer event-driven updates (via `SagaEventPublisher`) for
  low-latency dashboards.
- **Combine with the debugger**: `ISagaDebugger` captures immutable debug snapshots at key
  moments. Use `GetTimelineAsync` to correlate visualization state with the debug timeline for
  post-mortem analysis.
- **Terminal detection**: Always check `snapshot.IsTerminal` before scheduling the next poll to
  avoid unnecessary repository reads after the saga has finished.

---

## Troubleshooting

### Snapshot shows stale step data

The `SagaVisualizationService` reads directly from `ISagaRepository`. If step data appears
stale, confirm that `ISagaStepRepository.UpdateAsync` is being called after each step
transition in `SagaOrchestrationService`.

### Progress bar always shows 0%

`ProgressPercent` is calculated from `CompletedSteps / TotalSteps`. If `TotalSteps` is zero,
the saga steps were not initialized. Call `StartSagaAsync` before rendering — steps are
populated inside `InitializeStepsFromDefinition` at saga start.

### ASCII diagram shows `(no steps defined)`

The saga's `Steps` list is empty at snapshot time. Ensure `StartSagaAsync` was called
successfully before calling `GetSnapshotAsync`.

### Renderer throws `ArgumentNullException`

Both `RenderProgressBar`, `RenderStateDiagram`, and `RenderFullReport` require a non-null
`SagaVisualizationSnapshot`. Guard the call site:

```csharp
var snapshot = await _viz.GetSnapshotAsync(sagaId);
if (snapshot != null)
    Console.WriteLine(_renderer.RenderFullReport(snapshot));
```

### Live stream stops immediately

If `StreamLiveStateAsync` stops after the first tick, the saga is already in a terminal state.
Check `snapshot.IsTerminal` and `snapshot.Status` in the `onUpdate` callback to detect this
and display a final report before returning.
