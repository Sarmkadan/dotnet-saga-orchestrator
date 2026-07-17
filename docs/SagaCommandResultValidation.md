# SagaCommandResultValidation

Provides static validation utilities for saga command results. This type offers a consistent API to check whether a command result is valid, retrieve validation errors, and enforce validity by throwing when the result is invalid. Generic and non‑generic overloads allow validation of both typed and untyped command results.

## API

### `Validate` (non‑generic)

```csharp
public static IReadOnlyList<string> Validate(object commandResult)
```

Validates an untyped command result and returns a read‑only list of error messages.

- **Parameters**:  
  `commandResult` — the command result object to validate. May be `null`.

- **Returns**:  
  An `IReadOnlyList<string>` containing zero or more validation error descriptions. An empty list indicates a valid result.

- **Throws**:  
  Does not throw.

---

### `Validate<T>`

```csharp
public static IReadOnlyList<string> Validate<T>(T commandResult)
```

Validates a strongly‑typed command result and returns a read‑only list of error messages.

- **Parameters**:  
  `commandResult` — the command result of type `T` to validate. May be `null`.

- **Returns**:  
  An `IReadOnlyList<string>` containing zero or more validation error descriptions.

- **Throws**:  
  Does not throw.

---

### `IsValid` (non‑generic)

```csharp
public static bool IsValid(object commandResult)
```

Determines whether an untyped command result is valid.

- **Parameters**:  
  `commandResult` — the command result object to check.

- **Returns**:  
  `true` if the command result is valid (no validation errors); otherwise `false`.

- **Throws**:  
  Does not throw.

---

### `IsValid<T>`

```csharp
public static bool IsValid<T>(T commandResult)
```

Determines whether a strongly‑typed command result is valid.

- **Parameters**:  
  `commandResult` — the command result of type `T` to check.

- **Returns**:  
  `true` when the command result is valid; otherwise `false`.

- **Throws**:  
  Does not throw.

---

### `EnsureValid` (non‑generic)

```csharp
public static void EnsureValid(object result)
```

Ensures that an untyped command result is valid, throwing an exception if it is not.

- **Parameters**:  
  `result` — the command result object to validate.

- **Throws**:  
  Throws an exception (typically `InvalidOperationException` or a custom saga validation exception) when the command result is invalid. The exception message includes the validation errors.

---

### `EnsureValid<T>`

```csharp
public static void EnsureValid<T>(T result)
```

Ensures that a strongly‑typed command result is valid, throwing an exception if it is not.

- **Parameters**:  
  `result` — the command result of type `T` to validate.

- **Throws**:  
  Throws an exception when the command result is invalid, with details of the validation failures.

---

## Usage

### Example 1: Checking validity before processing

```csharp
var commandResult = saga.Execute(command);

if (SagaCommandResultValidation.IsValid(commandResult))
{
    MarkAsCompleted(commandResult);
}
else
{
    var errors = SagaCommandResultValidation.Validate(commandResult);
    LogErrors(errors);
}
```

### Example 2: Enforcing validity with `EnsureValid`

```csharp
public void Handle<T>(T commandResult)
{
    // Throws if the command result is invalid, preventing further processing.
    SagaCommandResultValidation.EnsureValid(commandResult);

    // At this point the result is guaranteed to be valid.
    Persist(commandResult);
    NotifySuccess();
}
```

---

## Notes

- **Null handling**: All methods accept `null` as a command result. The validation logic treats `null` according to the underlying rules — typically as an invalid state, which will produce error messages and cause `EnsureValid` to throw.
- **Immutability**: The returned `IReadOnlyList<string>` from `Validate` is a snapshot of errors at the time of the call. Subsequent changes to the command result do not affect the returned list.
- **Thread safety**: All members are static and do not mutate shared state. They are safe to call concurrently from multiple threads, provided the command result objects themselves are not being mutated during validation.
- **Exception type**: The exact exception type thrown by `EnsureValid` is determined by the internal validation framework. Callers should catch a base exception type (e.g., `Exception` or a saga‑specific base exception) unless the concrete type is documented elsewhere.
- **Performance**: `Validate` and `IsValid` perform the same underlying validation work. Prefer `IsValid` when only the boolean outcome is needed, to avoid allocating the error list unnecessarily.
