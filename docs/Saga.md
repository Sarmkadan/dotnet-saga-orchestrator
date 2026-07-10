# Saga

A `Saga` represents a long-running, distributed transaction pattern implementation that coordinates multiple steps (`SagaStep`) to achieve atomicity through compensation logic. It tracks the lifecycle of the saga, including initialization, execution, failure, and compensation, while maintaining correlation and metadata for observability and recovery.

## API

### `public string Id`
A unique identifier for the saga instance. Used for correlation and lookup across distributed systems.

### `public string CorrelationId`
An external identifier linking this saga to a broader business process or workflow. May be shared across multiple sagas.

### `public SagaStatus Status`
The current lifecycle state of the saga. Possible values include `Initializing`, `Running`, `Completed`, `Failed`, `Compensating`, and `Compensated`.

### `public SagaDefinition Definition`
The declarative definition of the saga, including its steps, compensation policies, and timeouts. Immutable after initialization.

### `public List<SagaStep> Steps`
The ordered collection of steps that comprise the saga. Each step defines an action and its corresponding compensation logic.

### `public DateTime StartedAt`
The timestamp when the saga was initiated. Set during `Initialize` or `Start`.

### `public DateTime? CompletedAt`
The timestamp when the saga successfully completed all steps. `null` if the saga is incomplete or failed.

### `public DateTime? FailedAt`
The timestamp when the saga entered a terminal failure state. `null` if the saga completed or is still running.

### `public DateTime? CompensationStartedAt`
The timestamp when compensation was initiated. `null` if compensation has not started or is not applicable.

### `public string? FailureReason`
A descriptive reason for the saga's failure, populated when `Fail` is invoked. `null` if the saga is not in a failed state.

### `public int RetryCount`
The number of times the saga has been retried after transient failures. Incremented automatically on eligible retries.

### `public int MaxRetries`
The maximum number of retry attempts allowed before the saga is marked as failed. Configured during saga definition.

### `public int TimeoutSeconds`
The maximum duration (in seconds) the saga is allowed to run before timing out. Configured during saga definition.

### `public Dictionary<string, object> Metadata`
A key-value store for arbitrary contextual data associated with the saga. Used for tracing, debugging, or business-specific extensions.

### `public void Initialize()`
Initializes the saga by setting `StartedAt` to the current UTC time and transitioning the `Status` to `Initializing`. Throws if the saga is already initialized.

### `public void Start()`
Transitions the saga from `Initializing` to `Running` and begins executing the first step. Throws if the saga is not in `Initializing` status or has no steps.

### `public void Complete()`
Marks the saga as successfully completed by setting `CompletedAt` to the current UTC time and transitioning the `Status` to `Completed`. Throws if the saga is not in `Running` status or has incomplete steps.

### `public void Fail(string reason)`
Terminates the saga in a failed state, recording the provided `reason` in `FailureReason`, setting `FailedAt` to the current UTC time, and transitioning the `Status` to `Failed`. Throws if the saga is not in a mutable state (e.g., already completed or compensating).

### `public void BeginCompensation()`
Initiates compensation logic for the saga by transitioning the `Status` to `Compensating` and setting `CompensationStartedAt` to the current UTC time. Throws if the saga is not in `Failed` status or compensation is not defined.

### `public void CompleteCompensation()`
Finalizes compensation by transitioning the `Status` to `Compensated` and setting `CompletedAt` to the current UTC time. Throws if the saga is not in `Compensating` status.

## Usage

### Example 1: Basic Saga Execution
