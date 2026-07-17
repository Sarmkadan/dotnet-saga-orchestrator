# SagaActivitySourceExtensionsValidation

The `SagaActivitySourceExtensionsValidation` class provides a centralized set of static utility methods for validating input parameters and state conditions required by the `SagaActivitySourceExtensions` API. It exposes validation logic through three distinct patterns per operation: returning a list of error messages, returning a boolean validity flag, or throwing an exception immediately upon failure. This design allows callers to choose between pre-flight checks, conditional logic flows, or fail-fast enforcement when initiating saga operations, recording completions, starting steps, or handling compensations.

## API

### Validation Methods Returning Error Lists

These methods perform validation checks and return a read-only list of error messages. If the validation succeeds, the returned list is empty.

*   **`public static IReadOnlyList<string> ValidateStartSaga`**
    *   **Purpose:** Validates the parameters required to start a new saga instance.
    *   **Parameters:** Accepts the standard arguments required for starting a saga (typically saga ID, correlation ID, and initial state).
    *   **Return Value:** An `IReadOnlyList<string>` containing descriptive error messages if validation fails; otherwise, an empty list.
    *   **Throws:** Does not throw exceptions; errors are returned via the list.

*   **`public static IReadOnlyList<string> ValidateRecordSagaComplete`**
    *   **Purpose:** Validates the parameters required to record a saga as successfully completed.
    *   **Parameters:** Accepts arguments identifying the saga instance to complete.
    *   **Return Value:** An `IReadOnlyList<string>` containing error messages if the saga cannot be marked complete; otherwise, an empty list.
    *   **Throws:** Does not throw exceptions.

*   **`public static IReadOnlyList<string> ValidateStartStep`**
    *   **Purpose:** Validates the parameters required to begin execution of a specific step within a saga.
    *   **Parameters:** Accepts arguments identifying the saga, the step definition, and step-specific context.
    *   **Return Value:** An `IReadOnlyList<string>` containing error messages if the step cannot be started; otherwise, an empty list.
    *   **Throws:** Does not throw exceptions.

*   **`public static IReadOnlyList<string> ValidateRecordStepFailure`**
    *   **Purpose:** Validates the parameters required to record a failure for a specific saga step.
    *   **Parameters:** Accepts arguments identifying the saga, the failed step, and the exception or error details.
    *   **Return Value:** An `IReadOnlyList<string>` containing error messages if the failure cannot be recorded; otherwise, an empty list.
    *   **Throws:** Does not throw exceptions.

*   **`public static IReadOnlyList<string> ValidateStartCompensation`**
    *   **Purpose:** Validates the parameters required to initiate a compensation action for a previously failed step.
    *   **Parameters:** Accepts arguments identifying the saga and the step requiring compensation.
    *   **Return Value:** An `IReadOnlyList<string>` containing error messages if compensation cannot be started; otherwise, an empty list.
    *   **Throws:** Does not throw exceptions.

*   **`public static IReadOnlyList<string> ValidateRecordCompensationFailure`**
    *   **Purpose:** Validates the parameters required to record a failure occurring during a compensation action.
    *   **Parameters:** Accepts arguments identifying the saga, the step, and the compensation error details.
    *   **Return Value:** An `IReadOnlyList<string>` containing error messages if the compensation failure cannot be recorded; otherwise, an empty list.
    *   **Throws:** Does not throw exceptions.

### Boolean Validation Methods

These methods perform the same checks as the list-returning counterparts but return a simple boolean result for use in conditional logic.

*   **`public static bool IsValidStartSaga`**
    *   **Purpose:** Determines if the provided arguments are valid for starting a saga.
    *   **Return Value:** `true` if validation passes; `false` otherwise.
    *   **Throws:** Does not throw exceptions.

*   **`public static bool IsValidRecordSagaComplete`**
    *   **Purpose:** Determines if the provided arguments are valid for recording saga completion.
    *   **Return Value:** `true` if validation passes; `false` otherwise.
    *   **Throws:** Does not throw exceptions.

*   **`public static bool IsValidStartStep`**
    *   **Purpose:** Determines if the provided arguments are valid for starting a saga step.
    *   **Return Value:** `true` if validation passes; `false` otherwise.
    *   **Throws:** Does not throw exceptions.

*   **`public static bool IsValidStartCompensation`**
    *   **Purpose:** Determines if the provided arguments are valid for starting a compensation.
    *   **Return Value:** `true` if validation passes; `false` otherwise.
    *   **Throws:** Does not throw exceptions.

*   **`public static bool IsValidRecordStepFailure`**
    *   **Purpose:** Determines if the provided arguments are valid for recording a step failure.
    *   **Return Value:** `true` if validation passes; `false` otherwise.
    *   **Throws:** Does not throw exceptions.

### Enforcement Methods (Fail-Fast)

These methods perform validation and immediately throw an exception if the check fails, ensuring that execution proceeds only with valid data.

*   **`public static void EnsureValidStartSaga`**
    *   **Purpose:** Enforces validity for starting a saga.
    *   **Throws:** Throws an `ArgumentException` (or derived exception) containing validation error details if the check fails. Returns normally if valid.

*   **`public static void EnsureValidRecordSagaComplete`**
    *   **Purpose:** Enforces validity for recording saga completion.
    *   **Throws:** Throws an exception if the check fails.

*   **`public static void EnsureValidStartStep`**
    *   **Purpose:** Enforces validity for starting a saga step.
    *   **Throws:** Throws an exception if the check fails.

*   **`public static void EnsureValidStartCompensation`**
    *   **Purpose:** Enforces validity for starting a compensation.
    *   **Throws:** Throws an exception if the check fails.

*   **`public static void EnsureValidRecordStepFailure`**
    *   **Purpose:** Enforces validity for recording a step failure.
    *   **Throws:** Throws an exception if the check fails.

## Usage

### Example 1: Pre-flight Validation with Error Reporting
This example demonstrates using the `Validate` methods to collect all potential errors before attempting an operation, allowing for comprehensive user feedback or logging.

```csharp
using System;
using System.Linq;
using DotNetSagaOrchestrator; // Hypothetical namespace

public class SagaController
{
    public void AttemptStartSaga(string sagaId, string correlationId)
    {
        // Perform validation without throwing
        var errors = SagaActivitySourceExtensionsValidation.ValidateStartSaga(sagaId, correlationId);

        if (errors.Count > 0)
        {
            // Log all validation issues
            Console.WriteLine("Failed to start saga:");
            foreach (var error in errors)
            {
                Console.WriteLine($"- {error}");
            }
            return;
        }

        // Proceed with actual saga start logic only if valid
        Console.WriteLine($"Starting saga {sagaId}...");
    }
}
```

### Example 2: Fail-Fast Enforcement
This example demonstrates using the `Ensure` methods within a critical path where invalid state should immediately halt execution and propagate an exception.

```csharp
using System;
using DotNetSagaOrchestrator; // Hypothetical namespace

public class StepExecutor
{
    public void ExecuteStep(string sagaId, int stepId, object context)
    {
        // Throw immediately if parameters are invalid
        SagaActivitySourceExtensionsValidation.EnsureValidStartStep(sagaId, stepId, context);

        try 
        {
            // Execute step logic
            PerformStepLogic(stepId, context);
        }
        catch (Exception ex)
        {
            // Ensure failure recording parameters are also valid before logging
            SagaActivitySourceExtensionsValidation.EnsureValidRecordStepFailure(sagaId, stepId, ex);
            
            // Record failure logic here
            Console.WriteLine($"Recorded failure for step {stepId} in saga {sagaId}");
        }
    }

    private void PerformStepLogic(int stepId, object context) 
    {
        // Implementation details
    }
}
```

## Notes

*   **Thread Safety:** As this class consists entirely of static methods that operate on input parameters without maintaining internal mutable state, all members are inherently thread-safe. Multiple threads may call validation methods concurrently without risk of race conditions.
*   **Exception Handling:** The `Ensure...` methods will throw exceptions immediately upon the first detected validation failure. The specific exception type is typically `ArgumentException` or `ArgumentNullException`, depending on whether the issue is a missing value or an invalid format. Callers should wrap these calls in try-catch blocks if they intend to handle validation failures gracefully rather than letting them propagate.
*   **Empty Collections:** When using the `Validate...` methods, an empty `IReadOnlyList<string>` indicates success. Do not check for `null`; the contract guarantees a non-null list instance is always returned.
*   **Consistency:** The boolean `IsValid...` methods and the `Validate...` list methods perform identical logical checks. If `IsValidStartSaga` returns `false`, `ValidateStartSaga` is guaranteed to return a non-empty list, and `EnsureValidStartSaga` is guaranteed to throw.
