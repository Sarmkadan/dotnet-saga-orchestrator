# ISagaResponseMapper

The `ISagaResponseMapper` interface defines a contract for mapping saga execution state into standardized response objects. Implementations hold the current state of a saga step or the overall saga (via properties such as `Id`, `Name`, `Status`, and timing data) and expose methods to produce `SagaResponse` or `SagaStepResponse` instances. This abstraction decouples response formatting from saga orchestration logic, allowing consistent output across different saga implementations.

## API

### Properties

- **`string Id`**  
  Gets the unique identifier of the saga or saga step.

- **`string Name`**  
  Gets the human-readable name of the saga or step.

- **`string Status`**  
  Gets the current status (e.g., `"Running"`, `"Completed"`, `"Failed"`).

- **`int Order`**  
  Gets the execution order index of the step within the saga.

- **`DateTime StartedAt`**  
  Gets the UTC timestamp when the saga or step started.

- **`DateTime? CompletedAt`**  
  Gets the UTC timestamp when the saga or step completed, or `null` if still running.

- **`long DurationMs`**  
  Gets the elapsed time in milliseconds since start (or total duration if completed).

- **`int RetryCount`**  
  Gets the number of retry attempts that have been made for this step.

- **`int MaxRetries`**  
  Gets the maximum number of retries allowed for this step.

- **`int TimeoutSeconds`**  
  Gets the timeout value in seconds for the step.

- **`string ServiceName`**  
  Gets the name of the service that executed the step.

- **`string? Error`**  
  Gets the error message if the step failed, or `null` if no error occurred.

### Methods

- **`SagaResponse MapToResponse()`**  
  Maps the current state of the mapper into a single `SagaResponse` object.  
  **Returns:** A `SagaResponse` instance populated with the mapper’s property values.  
  **Throws:** `InvalidOperationException` if the mapper state is inconsistent (e.g., `StartedAt` is default and no step has been executed).

- **`List<SagaResponse> MapToResponses()`**  
  Maps the current state into a list of `SagaResponse` objects. This is typically used when the mapper represents a saga with multiple steps, each producing a separate response.  
  **Returns:** A `List<SagaResponse>` containing one or more response objects.  
  **Throws:** `InvalidOperationException` if no step data is available.

- **`SagaStepResponse MapStepToResponse()`**  
  Maps the current state into a `SagaStepResponse` object, which includes step-specific details (e.g., retry count, timeout).  
  **Returns:** A `SagaStepResponse` instance.  
  **Throws:** `InvalidOperationException` if the mapper does not contain step-level data (e.g., when used for an entire saga rather than a single step).

## Usage

### Example 1: Mapping a completed saga step

```csharp
public SagaStepResponse HandleStepCompletion(ISagaResponseMapper mapper)
{
    // Assume mapper is populated by the saga engine after step execution
    if (mapper.Status == "Failed" && mapper.Error != null)
    {
        // Log or handle failure
        Console.WriteLine($"Step {mapper.Name} failed: {mapper.Error}");
    }

    return mapper.MapStepToResponse();
}
```

### Example 2: Mapping multiple steps for a saga report

```csharp
public List<SagaResponse> GenerateSagaReport(IEnumerable<ISagaResponseMapper> stepMappers)
{
    var responses = new List<SagaResponse>();
    foreach (var mapper in stepMappers)
    {
        responses.AddRange(mapper.MapToResponses());
    }
    return responses;
}
```

## Notes

- **Nullability:** `CompletedAt` and `Error` are nullable. Code consuming these properties should check for `null` before using them, especially when formatting timestamps or error messages.
- **Edge Cases:**  
  - If `DurationMs` is `0` and `StartedAt` is recent, the step may have completed instantly or not yet started.  
  - `MapToResponse()` and `MapStepToResponse()` may throw if the mapper has not been initialized with valid step data (e.g., `StartedAt` equals `DateTime.MinValue`).  
  - `MapToResponses()` may return an empty list if no steps have been recorded.
- **Thread Safety:** The `ISagaResponseMapper` interface does not mandate thread safety. Implementations that mutate properties after construction (e.g., updating `Status` or `CompletedAt` during execution) should synchronize access if the mapper is shared across threads. For read-only usage after initialization, no additional synchronization is required.
