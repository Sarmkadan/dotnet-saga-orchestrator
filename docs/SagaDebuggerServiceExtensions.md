# SagaDebuggerServiceExtensions

SagaDebuggerServiceExtensions provides a set of extension methods for inspecting and debugging saga orchestrations. These methods enable developers to capture execution snapshots, analyze progress, identify breakpoints, and retrieve timeline summaries for diagnostic and monitoring purposes. The extensions are designed to work with saga orchestrator instances to facilitate runtime introspection without disrupting the orchestration flow.

## API

### CaptureSnapshotWithSummaryAsync

```csharp
public static async Task<(SagaDebugSnapshot Snapshot, string StateSummary)> CaptureSnapshotWithSummaryAsync(
    this ISagaOrchestrator orchestrator,
    SagaTrigger trigger)
```

Captures a snapshot of the current saga state along with a human-readable summary of the state. The snapshot includes detailed execution context, while the summary provides a concise textual representation of the state.

**Parameters**:
- `orchestrator`: The saga orchestrator instance to capture the snapshot from.
- `trigger`: The trigger event that caused the snapshot capture.

**Returns**: A tuple containing the `SagaDebugSnapshot` and a `string` representing the state summary.

**Exceptions**:
- `ArgumentNullException`: Thrown when `orchestrator` or `trigger` is null.
- `InvalidOperationException`: Thrown when the orchestrator is not in a valid state for snapshot capture.

---

### GetLatestSnapshotAsync

```csharp
public static async Task<SagaDebugSnapshot?> GetLatestSnapshotAsync(
    this ISagaOrchestrator orchestrator)
```

Retrieves the most recent snapshot of the saga orchestrator's execution history. Returns null if no snapshots are available.

**Parameters**:
- `orchestrator`: The saga orchestrator instance to query.

**Returns**: The latest `SagaDebugSnapshot` or null if no snapshots exist.

**Exceptions**:
- `ArgumentNullException`: Thrown when `orchestrator` is null.

---

### GetProgressPercentageAsync

```csharp
public static async Task<double?> GetProgressPercentageAsync(
    this ISagaOrchestrator orchestrator)
```

Calculates the current progress of the saga as a percentage. Progress is determined based on completed steps versus total expected steps.

**Parameters**:
- `orchestrator`: The saga orchestrator instance to evaluate.

**Returns**: A nullable `double` representing the progress percentage (0-100), or null if progress cannot be determined.

**Exceptions**:
- `ArgumentNullException`: Thrown when `orchestrator` is null.

---

### HasBreakpointsAsync

```csharp
public static async Task<bool> HasBreakpointsAsync(
    this ISagaOrchestrator orchestrator)
```

Determines whether any breakpoints are currently set in the saga orchestrator.

**Parameters**:
- `orchestrator`: The saga orchestrator instance to check.

**Returns**: `true` if breakpoints exist; otherwise, `false`.

**Exceptions**:
- `ArgumentNullException`: Thrown when `orchestrator` is null.

---

### GetTimelineSummaryAsync

```csharp
public static async Task<string> GetTimelineSummaryAsync(
    this ISagaOrchestrator orchestrator)
```

Generates a timeline summary of the saga's execution history, including key events and state transitions.

**Parameters**:
- `orchestrator`: The saga orchestrator instance to summarize.

**Returns**: A `string` containing the timeline summary.

**Exceptions**:
- `ArgumentNullException`: Thrown when `orchestrator` is null.

---

### FindSnapshotAsync

```csharp
public static async Task<SagaDebugSnapshot?> FindSnapshotAsync(
    this ISagaOrchestrator orchestrator,
    Func<SagaDebugSnapshot, bool> predicate)
```

Searches for a snapshot matching the specified predicate condition.

**Parameters**:
- `orchestrator`: The saga orchestrator instance to search.
- `predicate`: A function to evaluate each snapshot for a match.

**Returns**: The first matching `SagaDebugSnapshot` or null if no match is found.

**Exceptions**:
- `ArgumentNullException`: Thrown when `orchestrator` or `predicate` is null.

---

### GetSnapshotsByTriggerAsync

```csharp
public static async Task<IReadOnlyList<SagaDebugSnapshot>> GetSnapshotsByTriggerAsync(
    this ISagaOrchestrator orchestrator,
    SagaTrigger trigger)
```

Retrieves all snapshots associated with a specific trigger event.

**Parameters**:
- `orchestrator`: The saga orchestrator instance to query.
- `trigger`: The trigger event to filter snapshots by.

**Returns**: A read-only list of `SagaDebugSnapshot` instances.

**Exceptions**:
- `ArgumentNullException`: Thrown when `orchestrator` or `trigger` is null.

---

### GetHitBreakpointsAsync

```csharp
public static async Task<IReadOnlyList<SagaDebugBreakpoint>> GetHitBreakpointsAsync(
    this ISagaOrchestrator orchestrator)
```

Retrieves all breakpoints that have been hit during saga execution.

**Parameters**:
- `orchestrator`: The saga orchestrator instance to query.

**Returns**: A read-only list of `SagaDebugBreakpoint` instances.

**Exceptions**:
- `ArgumentNullException`: Thrown when `orchestrator` is null.

---

### GetMostHitBreakpointAsync

```csharp
public static async Task<SagaDebugBreakpoint?> GetMostHitBreakpointAsync(
    this ISagaOrchestrator orchestrator)
```

Identifies the breakpoint that has been hit the most times during saga execution.

**Parameters**:
- `orchestrator`: The saga orchestrator instance to evaluate.

**Returns**: The most frequently hit `SagaDebugBreakpoint` or null if no breakpoints have been hit.

**Exceptions**:
- `ArgumentNullException`: Thrown when `orchestrator` is null.

---

## Usage

### Example 1: Capturing a Snapshot and Checking Progress

```csharp
var orchestrator = serviceProvider.GetRequiredService<ISagaOrchestrator>();
var trigger = new SagaTrigger("OrderProcessed");

var (snapshot, summary) = await orchestrator.CaptureSnapshotWithSummaryAsync(trigger);
Console.WriteLine($"State Summary: {summary}");

var progress = await orchestrator.GetProgressPercentageAsync();
Console.WriteLine($"Progress: {progress}%");
```

### Example 2: Analyzing Breakpoints

```csharp
var orchestrator = serviceProvider.GetRequiredService<ISagaOrchestrator>();

if (await orchestrator.HasBreakpointsAsync())
{
    var hitBreakpoints = await orchestrator.GetHitBreakpointsAsync();
    var mostHit = await orchestrator.GetMostHitBreakpointAsync();

    Console.WriteLine($"Most hit breakpoint: {mostHit?.Id}");
    Console.WriteLine($"Total hit breakpoints: {hitBreakpoints.Count}");
}
```

---

## Notes

- All methods are asynchronous and should be awaited to ensure proper execution flow.
- Nullable return types indicate potential absence of data (e.g., no snapshots or breakpoints exist).
- Thread safety depends on the underlying `ISagaOrchestrator` implementation; external synchronization may be required in concurrent scenarios.
- `CaptureSnapshotWithSummaryAsync` and `FindSnapshotAsync` may throw if the orchestrator's internal state is inconsistent or corrupted.
- Timeline summaries and progress percentages are approximations and may not reflect real-time state changes during active execution.
