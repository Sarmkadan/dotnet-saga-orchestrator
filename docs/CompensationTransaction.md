# CompensationTransaction

A `CompensationTransaction` represents a compensating action within a Saga orchestration workflow, enabling rollback or correction of previously executed steps when a Saga fails or requires adjustment. It tracks the lifecycle of compensation operations, including initiation, execution, completion, and failure states, along with retry logic and payload data for idempotency and debugging.

## API

### Properties

- **`Id`** (string)
  A unique identifier for the compensation transaction. Used to correlate compensation actions with specific Saga steps and ensure traceability across the orchestration system.

- **`SagaId`** (string)
  The identifier of the parent Saga to which this compensation transaction belongs. Enables grouping and querying of all compensation activities within a single Saga instance.

- **`StepId`** (string)
  The identifier of the Saga step that this compensation transaction is associated with. Used to map compensations to their originating actions for precise rollback targeting.

- **`StepName`** (string)
  A human-readable name for the Saga step being compensated. Provides context during debugging and monitoring by clarifying the purpose of the compensation action.

- **`Order`** (int)
  The execution order of this compensation transaction relative to other compensations in the same Saga. Determines the sequence in which compensations are applied during rollback.

- **`Status`** (CompensationStatus)
  The current state of the compensation transaction (e.g., `Pending`, `InProgress`, `Completed`, `Failed`). Indicates the lifecycle stage and informs retry or completion logic.

- **`CompensationUrl`** (string)
  The endpoint or resource URL to invoke for executing the compensation action. Typically points to a service or handler responsible for undoing the effects of the original step.

- **`RequestPayload`** (Dictionary<string, object>)
  The payload sent to the compensation endpoint. Contains the data required to perform the compensation action, such as identifiers or state snapshots from the original step.

- **`ResponsePayload`** (Dictionary<string, object>)
  The payload received from the compensation endpoint after execution. May include confirmation details, updated state, or error information from the compensation service.

- **`InitiatedAt`** (DateTime)
  The timestamp when the compensation transaction was initialized. Marks the start of the transaction’s lifecycle for monitoring and timeout calculations.

- **`CompletedAt`** (DateTime?)
  The timestamp when the compensation transaction successfully completed. `null` if the transaction is still in progress or failed. Used to calculate duration and verify completion.

- **`FailedAt`** (DateTime?)
  The timestamp when the compensation transaction failed. `null` if the transaction succeeded or is still pending. Helps diagnose timing issues or service failures during compensation.

- **`ErrorMessage`** (string?)
  A descriptive error message if the compensation transaction failed. Provides context for debugging and alerting when compensation actions do not complete as expected.

- **`RetryCount`** (int)
  The number of times this compensation transaction has been retried after a failure. Used to enforce retry limits and backoff strategies defined by `MaxRetries`.

- **`MaxRetries`** (int)
  The maximum number of retry attempts allowed for this compensation transaction before marking it as permanently failed. Configures the system’s tolerance for transient failures.

- **`TimeoutSeconds`** (int)
  The maximum duration, in seconds, that the compensation transaction is allowed to execute before timing out. Ensures long-running compensations do not block the Saga indefinitely.

### Methods

- **`CompensationTransaction`** (constructor)
  Initializes a new instance of the `CompensationTransaction` class. Sets default values for `Status`, `RetryCount`, `MaxRetries`, `TimeoutSeconds`, and timestamps. Requires `Id`, `SagaId`, `StepId`, `StepName`, `Order`, `CompensationUrl`, and `RequestPayload` to be provided at construction.

- **`Initialize`** (void)
  Prepares the compensation transaction for execution by setting its initial state. Sets `Status` to `Pending`, records `InitiatedAt`, and resets `RetryCount` and `FailedAt` to default values. Throws if the transaction is already initialized or completed.

- **`Start`** (void)
  Begins execution of the compensation action by transitioning the transaction to `InProgress` status. Invokes the endpoint specified by `CompensationUrl` with the payload in `RequestPayload`. Updates `Status` to `Completed` or `Failed` based on the outcome, and records `CompletedAt` or `FailedAt` and `ErrorMessage` accordingly. Throws if the transaction is not in a valid state (e.g., not initialized or already completed).

- **`Complete`** (void)
  Marks the compensation transaction as successfully completed. Sets `Status` to `Completed` and records `CompletedAt`. Throws if the transaction is not in a valid state (e.g., not in progress or already completed).

## Usage

### Example 1: Initializing and Executing a Compensation Transaction
