# SagaResponse
Represents the serialized state of a saga execution, providing identifiers, timing information, step‑wise statistics, and a collection of detailed step responses. It is typically returned by the orchestrator API to convey the current progress and outcome of a long‑running business transaction.

## API
### Id (`string`)
- **Purpose:** Unique identifier for the saga instance.
- **Parameters:** None.
- **Return value:** The saga’s GUID or string‑based ID.
- **Throws:** None.

### CorrelationId (`string`)
- **Purpose:** Identifier used to correlate the saga with external messages or traces.
- **Parameters:** None.
- **Return value:** The correlation ID assigned when the saga was started.
- **Throws:** None.

### Status (`string`)
- **Purpose:** Current lifecycle status of the saga (e.g., `Running`, `Completed`, `Failed`).
- **Parameters:** None.
- **Return value:** A string representing the status.
- **Throws:** None.

### DefinitionId (`string`)
- **Purpose:** Identifier of the saga definition (template) that this instance follows.
- **Parameters:** None.
- **Return value:** The definition ID.
- **Throws:** None.

### DefinitionName (`string`)
- **Purpose:** Human‑readable name of the saga definition.
- **Parameters:** None.
- **Return value:** The definition name.
- **Throws:** None.

### StartedAt (`DateTime`)
- **Purpose:** Timestamp indicating when the saga execution began.
- **Parameters:** None.
- **Return value:** UTC date‑time of saga start.
- **Throws:** None.

### CompletedAt (`DateTime?`)
- **Purpose:** Timestamp indicating when the saga finished (successfully or with failure). Null while the saga is still running.
- **Parameters:** None.
- **Return value:** UTC date‑time of completion, or `null`.
- **Throws:** None.

### FailureReason (`string?`)
- **Purpose:** Optional descriptive message explaining why the saga failed. Populated only when `Status` indicates a failure state.
- **Parameters:** None.
- **Return value:** Failure reason string, or `null` if the saga succeeded or is still running.
- **Throws:** None.

### StepCount (`int`)
- **Purpose:** Total number of steps defined in the saga workflow.
- **Parameters:** None.
- **Return value:** Total step count.
- **Throws:** None.

### CompletedSteps (`int`)
- **Purpose:** Number of steps that have finished successfully.
- **Parameters:** None.
- **Return value:** Count of completed steps.
- **Throws:** None.

### FailedSteps (`int`)
- **Purpose:** Number of steps that ended in a failure state.
- **Parameters:** None.
- **Return value:** Count of failed steps.
- **Throws:** None.

### RetryCount (`int`)
- **Purpose:** Total number of retry attempts executed across all steps.
- **Parameters:** None.
- **Return value:** Aggregate retry count.
- **Throws:** None.

### Steps (`List<SagaStepResponse>`)
- **Purpose:** Detailed responses for each step in the saga, containing step‑specific status, timing, and error information.
- **Parameters:** None.
- **Return value:** Read‑only list of `SagaStepResponse` objects.
- **Throws:** None.

### FromSaga (`static SagaResponse FromSaga(Saga saga)`)
- **Purpose:** Factory method that maps a domain `Saga` entity to its API representation.
- **Parameters:**
  - `saga`: The domain model instance to convert. Must not be `null`.
- **Return value:** A new `SagaResponse` populated with the values from `saga`.
- **Throws:** `ArgumentNullException` if `saga` is `null`.

## Usage
```csharp
// Example 1: Creating a SagaResponse from a domain Saga object
var saga = await _sagaRepository.GetByIdAsync(sagaId);
SagaResponse response = SagaResponse.FromSaga(saga);
return Ok(response);
```

```csharp
// Example 2: Inspecting saga progress and step details
if (response.Status == "Failed")
{
    Console.WriteLine($"Saga {response.Id} failed: {response.FailureReason}");
    foreach (var step in response.Steps)
    {
        Console.WriteLine(
            $"Step {step.Name} (Order {step.Order}) - {step.Status}");
    }
}
else
{
    Console.WriteLine(
        $"Saga {response.Id} is {response.Status}. " +
        $"{response.CompletedSteps}/{response.StepCount} steps done.");
}
```

## Notes
- `CompletedAt` and `FailureReason` are only meaningful when the saga is no longer in a `Running` state; otherwise they remain `null`/`null`.
- The invariant `StepCount >= CompletedSteps + FailedSteps` holds; any remaining steps are considered pending or in progress.
- The `Steps` list is exposed as a mutable `List<SagaStepResponse>` for convenience, but consumers should treat it as read‑only after the `SagaResponse` is obtained to avoid unintended side effects.
- All property setters occur only during object construction (via `FromSaga` or internal constructors); therefore, once instantiated, the instance is safe for concurrent read access by multiple threads. No internal locking is performed, so external synchronization is required if the instance is mutated after creation.
