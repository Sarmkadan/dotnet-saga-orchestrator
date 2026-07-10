# SagaDebuggerService

A service for capturing, inspecting, and debugging sagas within the `dotnet-saga-orchestrator` framework. It enables runtime inspection of saga state, timeline navigation, and breakpoint-based debugging to facilitate troubleshooting and testing of saga execution flows.

## API

### `SagaDebuggerService`
The primary service class providing saga debugging capabilities. Instantiated by the orchestrator to manage snapshots, breakpoints, and timeline traversal.

### `async Task<SagaDebugSnapshot> CaptureSnapshotAsync()`
Captures a current snapshot of the saga’s state and execution context.
- **Returns**: A `SagaDebugSnapshot` representing the saga’s state at the time of capture.
- **Throws**: `InvalidOperationException` if the saga is not in a valid state for snapshot capture.

### `Task<IReadOnlyList<SagaDebugSnapshot>> GetSnapshotsAsync()`
Retrieves all previously captured snapshots in chronological order.
- **Returns**: An immutable list of `SagaDebugSnapshot` instances, ordered from oldest to newest.
- **Throws**: `InvalidOperationException` if snapshots are unavailable or corrupted.

### `Task<SagaDebugSnapshot?> GetSnapshotAsync(Guid snapshotId)`
Retrieves a specific snapshot by its unique identifier.
- **Parameters**:
  - `snapshotId` (Guid): The identifier of the snapshot to retrieve.
- **Returns**: The matching `SagaDebugSnapshot` if found; otherwise, `null`.
- **Throws**: `InvalidOperationException` if the snapshot store is inaccessible.

### `Task PurgeSnapshotsAsync()`
Removes all stored snapshots from the debugger’s history.
- **Throws**: `InvalidOperationException` if the purge operation fails due to permissions or storage issues.

### `async Task<SagaDebugSnapshot> TravelToAsync(Guid snapshotId)`
Advances the saga’s execution state to match a previously captured snapshot.
- **Parameters**:
  - `snapshotId` (Guid): The identifier of the target snapshot.
- **Returns**: The `SagaDebugSnapshot` representing the state after travel.
- **Throws**:
  - `InvalidOperationException` if the snapshot does not exist.
  - `InvalidOperationException` if the saga cannot be rewound to the target state.

### `async Task<SagaDebugTimeline> GetTimelineAsync()`
Retrieves the timeline of saga execution events leading up to the current state.
- **Returns**: A `SagaDebugTimeline` containing ordered execution events and transitions.
- **Throws**: `InvalidOperationException` if the timeline cannot be reconstructed.

### `Task<SagaDebugBreakpoint> SetBreakpointAsync(Guid snapshotId, string condition)`
Adds a breakpoint tied to a specific snapshot and optional execution condition.
- **Parameters**:
  - `snapshotId` (Guid): The identifier of the snapshot where the breakpoint is set.
  - `condition` (string): An optional expression to evaluate when the breakpoint is hit (e.g., `"Step == 3"`).
- **Returns**: The created `SagaDebugBreakpoint` instance.
- **Throws**:
  - `ArgumentException` if the condition is malformed.
  - `InvalidOperationException` if the snapshot does not exist.

### `Task<bool> RemoveBreakpointAsync(Guid breakpointId)`
Removes a breakpoint by its unique identifier.
- **Parameters**:
  - `breakpointId` (Guid): The identifier of the breakpoint to remove.
- **Returns**: `true` if the breakpoint was found and removed; otherwise, `false`.
- **Throws**: `InvalidOperationException` if the breakpoint store is inaccessible.

### `Task<IReadOnlyList<SagaDebugBreakpoint>> GetBreakpointsAsync()`
Retrieves all active breakpoints.
- **Returns**: An immutable list of `SagaDebugBreakpoint` instances.
- **Throws**: `InvalidOperationException` if breakpoints cannot be read.

### `Task ClearBreakpointsAsync()`
Removes all active breakpoints.
- **Throws**: `InvalidOperationException` if the clear operation fails.

### `async Task<bool> CheckBreakpointAsync(Guid breakpointId)`
Checks whether a breakpoint is currently active and would be triggered in the current saga state.
- **Parameters**:
  - `breakpointId` (Guid): The identifier of the breakpoint to check.
- **Returns**: `true` if the breakpoint exists and its condition is satisfied; otherwise, `false`.
- **Throws**: `InvalidOperationException` if the breakpoint store is inaccessible.

## Usage

### Example 1: Capturing and Inspecting Snapshots
