# SagaExtensions

`SagaExtensions` is a static helper class that extends the `Saga` domain model with a set of convenient query and mutation operations.  
It provides read‑only accessors for common saga state information, utilities for inspecting the current step, and lightweight metadata handling.  
All members are extension methods that operate on an instance of `Saga`; they do not modify the saga’s internal state except for the metadata helpers.

## API

| Member | Purpose | Parameters | Return Value | Throws |
|--------|---------|------------|--------------|--------|
| **`IsCompleted`** | Indicates whether the saga has reached a terminal state (completed or failed). | `this Saga saga` | `bool` | None |
| **`IsTerminal`** | Determines if the saga is in a terminal status (either `Completed` or `Failed`). | `this Saga saga` | `bool` | None |
| **`GetDurationSeconds`** | Returns the elapsed time in seconds from the saga’s creation to its current state. | `this Saga saga` | `double?` (null if start time is missing) | None |
| **`GetCurrentStepIndex`** | Retrieves the zero‑based index of the step that is currently being executed. | `this Saga saga` | `int` | None |
| **`GetFailedSteps`** | Provides a read‑only list of all steps that have failed. | `this Saga saga` | `IReadOnlyList<SagaStep>` | None |
| **`GetCompletedSteps`** | Provides a read‑only list of all steps that have completed successfully. | `this Saga saga` | `IReadOnlyList<SagaStep>` | None |
| **`HasPendingSteps`** | Indicates whether there are any steps that have not yet been started or are still in progress. | `this Saga saga` | `bool` | None |
| **`GetRetryDelaySeconds`** | Returns the configured retry delay (in seconds) for the current step. | `this Saga saga` | `int` | None |
| **`GetCompletionPercentage`** | Calculates the percentage of steps that have completed relative to the total number of steps. | `this Saga saga` | `int` (0–100) | None |
| **`AddMetadata`** | Adds or updates a key/value pair in the saga’s metadata dictionary. | `this Saga saga`, `string key`, `object value` | `void` | None |
| **`GetMetadata<T>`** | Retrieves a typed value from the saga’s metadata dictionary. | `this Saga saga`, `string key` | `T?` (null if key missing or cast fails) | None |
| **`CanRetry`** | Determines whether the saga can be retried based on its current state and retry policy. | `this Saga saga` | `bool` | None |
| **`IncrementRetry`** | Increments the retry counter for the current step and returns the new value. | `this Saga saga` | `int` | None |

> **Note**: All extension methods are safe to call on a `null` saga instance; they will throw a `ArgumentNullException` for the `saga` parameter.

## Usage

