# SagaOptionsValidation

Provides static helper methods for validating saga configuration options. The class contains a set of overloads that examine the supplied options (or related objects) and report any validation problems as a collection of error messages. Consumers can query whether the options are valid, retrieve the list of issues, or enforce validity by throwing an exception when problems are found.

## API

### `public static IReadOnlyList<string> Validate(...)`
Validates the supplied saga options (or related configuration) and returns a read‑only list of validation error messages.  
- **Parameters:** The method accepts the object to be validated; the exact type varies by overload (e.g., `SagaOptions`, `IServiceCollection`, etc.).  
- **Return value:** An `IReadOnlyList<string>` containing zero or more error messages. An empty list indicates that the validation succeeded.  
- **Exceptions:** Throws `ArgumentNullException` if the supplied argument is `null`. No other exceptions are thrown during normal validation.

### `public static bool IsValid(...)`
Determines whether the supplied saga options (or related configuration) pass validation.  
- **Parameters:** Same parameter shape as the corresponding `Validate` overload.  
- **Return value:** `true` if the validation error list is empty; otherwise `false`.  
- **Exceptions:** Throws `ArgumentNullException` if the supplied argument is `null`.

### `public static void EnsureValid(...)`
Validates the supplied saga options (or related configuration) and throws an exception if any validation errors are found.  
- **Parameters:** Same parameter shape as the corresponding `Validate` overload.  
- **Return value:** None.  
- **Exceptions:**  
  - `ArgumentNullException` if the supplied argument is `null`.  
  - `InvalidOperationException` (or a derived type) containing the concatenated validation error messages when the validation fails.

### `public static IReadOnlyList<string> Validate(...)`  
*(Overload 2)*  
Same contract as the first `Validate` overload but accepts a different parameter type to validate an alternative configuration target (e.g., a step definition collection). See the remarks above for parameters, return value, and exceptions.

### `public static IReadOnlyList<string> Validate(...)`   Validate 3*  
Same contract as the first `Validate` overload but accepts a different parameter type to validate yet another configuration target (e.g., a repository. See the remarks above for parameters, return value, and exceptions.

### `public static IReadOnlyList<string> Validate(...)`  
*(Overload 4)*  
*(Overload 3)*  
Same contract as the first `Validate` overload but accepts a different parameter type to validate a different configuration target (e.g., a saga definition). See the remarks above for parameters, return value, and exceptions.

### `public static IReadOnlyList<string> Validate(...)`  
*(Overload 5)*  
Same contract as the first `Validate` overload but accepts a different parameter type to validate a different configuration target (e.g., a command handler registration). See the remarks above for parameters, return value, and exceptions.

### `public static IReadOnlyList<string> Validate(...)`  
*(Overload 6)*  
Same contract as the first `Validate` overload but accepts a different parameter type to validate a different configuration target (e.g., a compensation handler registration). See the remarks above for parameters, return value, and exceptions.

### `public static IReadOnlyList<string> Validate(...)`  
*(Overload 7)*  
Same contract as the first `Validate` overload but accepts a different parameter type to validate a different configuration target (e.g., a saga persistence configuration). See the remarks above for parameters, return value, and exceptions.

## Usage

```csharp
using DotNetSagaOrchestrator.Validation;

// Validate a saga options instance and act on the result.
var options = new SagaOptions { /* initialize properties */ };
var errors = SagaOptionsValidation.Validate(options);
if (errors.Count > 0)
{
    // Log or display the validation problems.
    foreach (var err in errors)
        Console.WriteLine(err);
}
else
{
    // Options are safe to use.
    var orchestrator = new SagaOrchestrator(options);
}
```

```csharp
using DotNetSagaOrchestrator.Validation;

// Throw an exception if any validation issues exist.
try
{
    SagaOptionsValidation.EnsureValid(options);
    // Proceed knowing the options are valid.
}
catch (InvalidOperationException ex)
{
    // Validation failed; ex.Message contains all error messages.
    Console.Error.WriteLine($"Invalid saga options invalid: {ex.Message}");
}
```

## Notes

- All members are **static** and operate solely on their input arguments; they contain no mutable state. Consequently, the methods are thread‑safe and can be called concurrently from multiple threads without external synchronization.  
- The validation logic does not modify the supplied objects; it only inspects them.  
- If an overload receives a `null` argument, it throws `ArgumentNullException` before performing any validation checks.  
- The `EnsureValid` method throws an exception **only** when the validation error list is non‑empty; the exception type is implementation‑specific but derives from `System.Exception` and includes the full list of messages in its `Message` property.  
- Because the return type is `IReadOnlyList<string>`, callers should treat the list as immutable; attempting to cast it to a mutable list and modify it may lead to undefined behavior.  
- The exact parameter types for each overload are defined elsewhere in the codebase; consult the source or IntelliSense for the precise signatures when invoking these methods.
