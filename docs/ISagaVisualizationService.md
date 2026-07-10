# ISagaVisualizationService

Provides asynchronous access to snapshots of a saga's execution state for visualization purposes, including live streaming and historical data retrieval.

## API

### `SagaVisualizationService`
The concrete implementation of `ISagaVisualizationService` that provides saga visualization capabilities.

### `public async Task<SagaVisualizationSnapshot> GetSnapshotAsync()`
Retrieves the current snapshot of the saga's execution state.
- **Parameters**: None.
- **Return value**: A `SagaVisualizationSnapshot` representing the current state of the saga.
- **Exceptions**: Throws if the saga is not found or if the snapshot cannot be generated.

### `public async Task<IReadOnlyList<SagaVisualizationSnapshot>> GetAllSnapshotsAsync()`
Retrieves all historical snapshots of the saga's execution state.
- **Parameters**: None.
- **Return value**: A read-only list of `SagaVisualizationSnapshot` objects representing historical states.
- **Exceptions**: Throws if the saga is not found or if snapshots cannot be retrieved.

### `public async Task StreamLiveStateAsync(Action<SagaVisualizationSnapshot> callback)`
Streams live updates of the saga's execution state to the provided callback.
- **Parameters**:
  - `callback`: An action invoked with each new snapshot as the saga progresses.
- **Return value**: None.
- **Exceptions**: Throws if the saga is not found or if streaming cannot be initiated.

### `public string SagaId`
Gets the unique identifier of the saga.
- **Type**: `string`
- **Access**: Read-only

### `public string CorrelationId`
Gets the correlation identifier associated with the saga.
- **Type**: `string`
- **Access**: Read-only

### `public string SagaName`
Gets the name of the saga.
- **Type**: `string`
- **Access**: Read-only

### `public string Status`
Gets the current status of the saga (e.g., "Running", "Completed", "Failed").
- **Type**: `string`
- **Access**: Read-only

### `public List<VisualizationNode> Nodes`
Gets the list of nodes representing the saga's execution steps.
- **Type**: `List<VisualizationNode>`
- **Access**: Read-only

### `public int CompletedSteps`
Gets the number of steps completed in the saga.
- **Type**: `int`
- **Access**: Read-only

### `public int TotalSteps`
Gets the total number of steps in the saga.
- **Type**: `int`
- **Access**: Read-only

### `public double ProgressPercent`
Gets the percentage of completion for the saga (0.0 to 100.0).
- **Type**: `double`
- **Access**: Read-only

### `public double ElapsedMs`
Gets the elapsed time in milliseconds since the saga started.
- **Type**: `double`
- **Access**: Read-only

### `public string? FailureReason`
Gets the reason for failure if the saga is in a terminal failed state; otherwise, `null`.
- **Type**: `string?`
- **Access**: Read-only

### `public DateTime CapturedAt`
Gets the timestamp when the snapshot was captured.
- **Type**: `DateTime`
- **Access**: Read-only

### `public bool IsTerminal`
Gets a value indicating whether the saga has reached a terminal state (completed or failed).
- **Type**: `bool`
- **Access**: Read-only

### `public int Index`
Gets the index of the step in the saga's execution sequence.
- **Type**: `int`
- **Access**: Read-only

### `public string StepId`
Gets the unique identifier of the step.
- **Type**: `string`
- **Access**: Read-only

### `public string Name`
Gets the name of the step.
- **Type**: `string`
- **Access**: Read-only

### `public string Status`
Gets the status of the step (e.g., "Pending", "Running", "Completed", "Failed").
- **Type**: `string`
- **Access**: Read-only

## Usage

### Retrieving the current snapshot of a saga
