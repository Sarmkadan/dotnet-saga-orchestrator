# IMetricsService

Provides aggregated metrics and statistics about the state and performance of sagas and their steps within the orchestration system, enabling monitoring and analysis of execution patterns, success rates, and durations.

## API

### `MetricsService`

The default implementation of `IMetricsService`.

### `Task<SagaMetrics> GetMetricsAsync()`

Retrieves high-level metrics for all sagas, including counts of total, completed, failed, running, and compensated sagas, as well as overall success and failure rates and average duration.

- **Returns:** A task that resolves to a `SagaMetrics` object containing aggregated saga-level statistics.
- **Throws:** May throw if the underlying data store or monitoring system is unavailable.

### `Task<StepMetrics> GetStepMetricsAsync(string stepName)`

Retrieves detailed metrics for a specific step across all sagas, including execution counts, success and failure rates, average duration, and timestamp of the latest measurement.

- **Parameters:**
  - `stepName` (string): The name of the step to query.
- **Returns:** A task that resolves to a `StepMetrics` object containing step-level statistics.
- **Throws:** Throws `ArgumentException` if `stepName` is null or whitespace. May throw if the underlying data store is unavailable.

### `Task<PerformanceStats> GetPerformanceStatsAsync()`

Retrieves system-wide performance statistics, including total sagas, completed, failed, running, and compensated counts, overall success and failure rates, average saga duration in seconds, and the timestamp of the latest update.

- **Returns:** A task that resolves to a `PerformanceStats` object containing system-level performance metrics.
- **Throws:** May throw if the underlying data store or monitoring system is unavailable.

### `int TotalSagas`

Gets the total number of sagas tracked by the system.

### `int CompletedSagas`

Gets the number of sagas that have completed successfully.

### `int FailedSagas`

Gets the number of sagas that have failed and were not compensated.

### `int RunningSagas`

Gets the number of sagas currently in progress.

### `int CompensatedSagas`

Gets the number of sagas that were successfully compensated after failure.

### `double SuccessRate`

Gets the overall success rate of sagas, expressed as a value between 0.0 and 1.0.

### `double FailureRate`

Gets the overall failure rate of sagas, expressed as a value between 0.0 and 1.0.

### `double AverageDurationSeconds`

Gets the average duration of all completed sagas in seconds.

### `DateTime Timestamp`

Gets the timestamp of when the current metrics snapshot was generated.

### `string StepName`

Gets the name of the step associated with the current step-level metrics.

### `int TotalExecutions`

Gets the total number of times the step has been executed.

### `int SuccessfulExecutions`

Gets the number of successful executions of the step.

### `int FailedExecutions`

Gets the number of failed executions of the step.

### `double SuccessRate`

Gets the success rate of the step, expressed as a value between 0.0 and 1.0.

### `double AverageDurationMs`

Gets the average duration of the step in milliseconds.

### `DateTime Timestamp`

Gets the timestamp of when the current step metrics snapshot was generated.

## Usage
