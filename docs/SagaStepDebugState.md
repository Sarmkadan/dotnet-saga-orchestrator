# SagaStepDebugState

A data transfer object used for debugging and monitoring the execution state of a single step within a saga orchestration. It captures both the operational context (timestamps, retry counts, status) and the structural context (saga identity, step definition, service endpoint) of a saga step, along with its output and error state.

## API

### `public required string StepId`
The unique identifier of the saga step. Used to correlate this state with the step definition and other runtime artifacts.

### `public required string StepName`
The human-readable name of the step, as defined in the saga workflow. Helps in debugging and logging.

### `public required int StepOrder`
The zero-based index indicating the position of this step within the saga workflow. Determines the order of execution.

### `public required SagaStepStatus Status`
The current execution status of the step. One of `Pending`, `Executing`, `Completed`, `Failed`, `Compensating`, or `Compensated`.

### `public required int RetryCount`
The number of times this step has been retried due to transient failures.

### `public required int MaxRetries`
The maximum number of retry attempts allowed for this step before marking it as permanently failed.

### `public DateTime? StartedAt`
The timestamp when the step began execution. `null` if the step has not yet started.

### `public DateTime? CompletedAt`
The timestamp when the step completed successfully. `null` if the step is not yet completed or failed.

### `public DateTime? CompensatedAt`
The timestamp when the step was successfully compensated. `null` if compensation has not occurred or is not applicable.

### `public string? ErrorMessage`
The error message associated with the most recent failure of this step. `null` if no failure has occurred.

### `public required string ServiceUrl`
The base URL of the service endpoint responsible for executing this step.

### `public required IReadOnlyDictionary<string, object> OutputData`
A read-only dictionary containing the output data produced by this step. Keys are output names; values are the serialized outputs.

### `public static SagaStepDebugState FromStep`
Factory method to create a `SagaStepDebugState` from a step execution context.

**Parameters:**
- `step`: The active step context containing execution metadata and outputs.

**Returns:**
A new `SagaStepDebugState` instance populated with the current state of the step.

**Throws:**
- `ArgumentNullException`: If `step` is `null`.

### `public required string SnapshotId`
The unique identifier of the saga snapshot this state belongs to. Used for reconstructing saga state during recovery.

### `public required string SagaId`
The unique identifier of the parent saga. Links this step state to the overall saga execution.

### `public required string SagaName`
The human-readable name of the saga, as defined in the workflow.

### `public required string DefinitionId`
The unique identifier of the saga definition that this step belongs to.

### `public required string CorrelationId`
A unique identifier used to correlate messages and logs across distributed components during saga execution.

### `public required SagaStatus SagaStatus`
The current status of the parent saga. One of `Running`, `Completed`, `Failed`, or `Compensating`.

### `public required SnapshotTrigger Trigger`
The cause of the snapshot creation. One of `StepCompleted`, `StepFailed`, `StepCompensated`, or `Manual`.

## Usage
