# InMemorySagaStepRepositoryValidation

Provides validation utilities for in-memory saga step repositories, ensuring configuration consistency and correctness before runtime execution.

## API

### `Validate(IEnumerable<SagaStep> steps)`

Validates a collection of `SagaStep` instances for structural and semantic correctness. Returns a list of error messages describing any violations found.

- **Parameters**
  - `steps`: The collection of `SagaStep` instances to validate.
- **Return value**
  - An `IReadOnlyList<string>` containing zero or more error messages. Empty if validation passes.
- **Exceptions**
  - Throws `ArgumentNullException` if `steps` is `null`.

### `Validate(IReadOnlyCollection<SagaStep> steps)`

Validates a read-only collection of `SagaStep` instances for structural and semantic correctness. Returns a list of error messages describing any violations found.

- **Parameters**
  - `steps`: The read-only collection of `SagaStep` instances to validate.
- **Return value**
  - An `IReadOnlyList<string>` containing zero or more error messages. Empty if validation passes.
- **Exceptions**
  - Throws `ArgumentNullException` if `steps` is `null`.

### `IsValid(IEnumerable<SagaStep> steps)`

Determines whether a collection of `SagaStep` instances is valid without producing error messages. Returns `true` if all validation rules pass.

- **Parameters**
  - `steps`: The collection of `SagaStep` instances to evaluate.
- **Return value**
  - `true` if the collection is valid; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `steps` is `null`.

### `IsValid(IReadOnlyCollection<SagaStep> steps)`

Determines whether a read-only collection of `SagaStep` instances is valid without producing error messages. Returns `true` if all validation rules pass.

- **Parameters**
  - `steps`: The read-only collection of `SagaStep` instances to evaluate.
- **Return value**
  - `true` if the collection is valid; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `steps` is `null`.

### `EnsureValid(IEnumerable<SagaStep> steps)`

Validates a collection of `SagaStep` instances and throws an exception if any validation rule is violated.

- **Parameters**
  - `steps`: The collection of `SagaStep` instances to validate.
- **Exceptions**
  - Throws `ArgumentNullException` if `steps` is `null`.
  - Throws `InvalidOperationException` with a descriptive message if validation fails.

### `EnsureValid(IReadOnlyCollection<SagaStep> steps)`

Validates a read-only collection of `SagaStep` instances and throws an exception if any validation rule is violated.

- **Parameters**
  - `steps`: The read-only collection of `SagaStep` instances to validate.
- **Exceptions**
  - Throws `ArgumentNullException` if `steps` is `null`.
  - Throws `InvalidOperationException` with a descriptive message if validation fails.

## Usage

```csharp
// Example 1: Basic validation with error reporting
var steps = new[]
{
    new SagaStep("step1", "compensate1"),
    new SagaStep("step2", "compensate2")
};
var errors = InMemorySagaStepRepositoryValidation.Validate(steps);
if (errors.Count > 0)
{
    Console.WriteLine("Validation failed:");
    foreach (var error in errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Example 2: Fail-fast validation
try
{
    InMemorySagaStepRepositoryValidation.EnsureValid(steps);
    Console.WriteLine("Repository configuration is valid.");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Repository configuration error: {ex.Message}");
}
```

## Notes

- Validation rules include checks for duplicate step names, null or empty step names, null or empty compensation references, and circular compensation dependencies.
- All methods are thread-safe and may be called concurrently from multiple threads without additional synchronization.
- The validation logic does not mutate the input collections; all methods operate read-only on the provided `IEnumerable<T>` or `IReadOnlyCollection<T>`.
- Performance characteristics are linear with respect to the number of steps, making these methods suitable for use during application startup or configuration phases.
