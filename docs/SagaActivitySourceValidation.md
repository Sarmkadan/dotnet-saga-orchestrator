# SagaActivitySourceValidation

Provides static validation helpers for saga activity sources. The class contains overloads for validating, checking validity, and ensuring validity of a saga activity source, returning error details or throwing when the source does not meet requirements.

## API

### Validate (overload 1)
```csharp
public static IReadOnlyList<string> Validate()
```
- **Purpose**: Performs validation of the saga activity source associated with this overload and returns any validation errors.
- **Parameters**: None.
- **Return value**: A read‑only list of error messages. An empty list indicates the source is valid.
- **Exceptions**: None thrown under normal operation; returns errors instead of throwing.

### Validate (overload 2)
```csharp
public static IReadOnlyList<string> Validate()
```
- **Purpose**: Same as overload 1, but bound to a different validation context (e.g., a different source type or identifier).
- **Parameters**: None.
- **Return value**: A read‑only list of error messages. Empty list means valid.
- **Exceptions**: None.

### Validate (overload 3)
```csharp
public static IReadOnlyList<string> Validate()
```
- **Purpose**: Same as overload 1, but bound to a third validation context.
- **Parameters**: None.
- **Return value**: A read‑only list of error messages. Empty list means valid.
- **Exceptions**: None.

### Validate (overload 4)
```csharp
public static IReadOnlyList<string> Validate()
```
- **Purpose**: Same as overload 1, but bound to a fourth validation context.
- **Parameters**: None.
- **Return value**: A read‑only list of error messages. Empty list means valid.
- **Exceptions**: None.

### IsValid (overload 1)
```csharp
public static bool IsValid()
```
- **Purpose**: Determines whether the saga activity source for this overload passes validation.
- **Parameters**: None.
- **Return value**: `true` if the source is valid; otherwise `false`.
- **Exceptions**: None.

### IsValid (overload 2)
```csharp
public static bool IsValid()
```
- **Purpose**: Same as overload 1, but for a different validation context.
- **Parameters**: None.
- **Return value**: `true` if the source is valid; otherwise `false`.
- **Exceptions**: None.

### IsValid (overload 3)
```csharp
public static bool IsValid()
```
- **Purpose**: Same as overload 1, but for a third validation context.
- **Parameters**: None.
- **Return value**: `true` if the source is valid; otherwise `false`.
- **Exceptions**: None.

### IsValid (overload 4)
```csharp
public static bool IsValid()
```
- **Purpose**: Same as overload 1, but for a fourth validation context.
- **Parameters**: None.
- **Return value**: `true` if the source is valid; otherwise `false`.
- **Exceptions**: None.

### EnsureValid (overload 1)
```csharp
public static void EnsureValid()
```
- **Purpose**: Validates the saga activity source for this overload and throws an exception if validation fails.
- **Parameters**: None.
- **Return value**: None.
- **Exceptions**: Throws `InvalidOperationException` (or a derived type) containing the validation error messages when the source is invalid.

### EnsureValid (overload 2)
```csharp
public static void EnsureValid()
```
- **Purpose**: Same as overload 1, but for a different validation context.
- **Parameters**: None.
- **Return value**: None.
- **Exceptions**: Throws an exception with validation details when the source is invalid.

### EnsureValid (overload 3)
```csharp
public static void EnsureValid()
```
- **Purpose**: Same as overload 1, but for a third validation context.
- **Parameters**: None.
- **Return value**: None.
- **Exceptions**: Throws an exception with validation details when the source is invalid.

### EnsureValid (overload 4)
```csharp
public static void EnsureValid()
```
- **Purpose**: Same as overload 1, but for a fourth validation context.
- **Parameters**: None.
- **Return value**: None.
- **Exceptions**: Throws an exception with validation details when the source is invalid.

## Usage

### Example 1: Checking validation and reporting errors
```csharp
using YourNamespace; // replace with actual namespace containing SagaActivitySourceValidation

var errors = SagaActivitySourceValidation.Validate(); // choose appropriate overload
if (errors.Count > 0)
{
    foreach (var err in errors)
    {
        Console.WriteLine($"Validation error: {err}");
    }
}
else
{
    Console.WriteLine("Source is valid.");
}
```

### Example 2: Ensuring validity with exception handling
```csharp
using YourNamespace;

try
{
    SagaActivitySourceValidation.EnsureValid(); // choose appropriate overload
    // Proceed with source usage
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Source validation failed: {ex.Message}");
    // Handle failure appropriately
}
```

## Notes

- All members are static and operate without internal mutable state; therefore they are thread‑safe and can be invoked concurrently from multiple threads.
- The class provides four overloads for each method to accommodate different validation contexts (e.g., varying source identifiers, types, or configurations). Select the overload that matches the context of the saga activity source you are validating.
- Validation errors are returned as a read‑only list to prevent modification of the result; callers should treat the list as immutable.
- `EnsureValid` throws only when validation fails; if the source is valid the method completes silently.
- Do not rely on the specific exception type beyond it being derived from `System.Exception`; the message will contain the concatenated validation errors.
