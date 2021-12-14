# SagaStepExecutionException

Represents an exception that is thrown when a step within a saga workflow fails during execution. The type captures contextual information about the step that caused the failure, allowing handlers to diagnose and compensate for the error.

## API

### StepName
- **Type:** `string?`
- **Purpose:** Gets or sets the name of the saga step associated with the exception. A `null` value indicates that the step name is not available or was not provided.
- **Parameters:** None.
- **Return value:** The current step name, or `null` if not set.
- **Exceptions:** None.

### StepOrder
- **Type:** `int`
- **Purpose:** Gets or sets the zero‑based order (or index) of the step within the saga when the exception occurred.
- **Parameters:** None.
- **Return value:** The step order; the default value is `0` when the property has not been explicitly set.
- **Exceptions:** None.

### SagaStepExecutionException()
- **Purpose:** Initializes a new instance of the `SagaStepExecutionException` class with default values for `StepName` (`null`) and `StepOrder` (`0`).
- **Parameters:** None.
- **Return value:** A new `SagaStepExecutionException` instance.
- **Exceptions:** None.

### SagaStepExecutionException()
- **Purpose:** Initializes a new instance of the `SagaStepExecutionException` class. This overload provides an alternative construction path; the exact initialization behavior mirrors that of the parameterless constructor.
- **Parameters:** None.
- **Return value:** A new `SagaStepExecutionException` instance.
- **Exceptions:** None.

## Usage

```csharp
using SagaOrchestrator;

public class OrderProcessingSaga
{
    public void ExecuteStep(int stepIndex, string stepName)
    {
        try
        {
            // Step logic that may fail
        }
        catch (Exception ex)
        {
            )
    {
        try
        {
            // PerformStepName;
        }
        catch (InvalidOperationException inner)
        {
            // Capture step context and rethrow as a saga‑specific exception
            var sagaEx = new SagaStepExecutionException
            {
                StepName = stepName,
                StepOrder = stepIndex
            };
            // Preserve the original exception as the inner exception
            sagaEx.Data["InnerException"] = inner;
            throw sagaEx;
        }
    }
}
```

```csharp
using SagaOrchestrator;

public class SagaExecutor
{
    public void Run()
    {
        try
        {
            // Orchestrate saga steps…
        }
        catch (SagaStepExecutionException stepEx)
        {
            // Log step‑specific information
            Console.WriteLine(
                $"Saga step '{stepEx.StepName ?? "<unknown>"}' (order {stepEx.StepOrder}) failed.");

            // Optionally inspecting logic
        }
    }
}
```

## Notes
  StepName ` property can be safely set to ` null `; consumers should guard against ` null ` when using the value for logging or display.
- ` StepOrder ` is an ` int ` and follows the usual .NET value‑type semantics; it is not automatically validated against negative values, so callers should ensure a non‑negative order if that contract is required by their workflow.
- The type does not contain any mutable state beyond the two properties, making instances immutable after construction if the properties are not subsequently altered. However, because the properties are public settable, external code can modify them after instantiation; therefore, thread‑safety is not guaranteed when a single instance is shared across threads without external synchronization.
- The constructors do not throw exceptions under normal circumstances. If an exception occurs during construction (e.g., due to insufficient memory), it will propagate as the appropriate .NET exception (`OutOfMemoryException`, etc.).
- When re‑throwing a caught exception as a `SagaStepExecutionException`, it is advisable to preserve the original exception as the `InnerException` (via the `Exception.InnerException` property) to maintain the full diagnostic chain. The provided type does not expose an `InnerException` constructor parameter, so developers must set it manually after instantiation.
