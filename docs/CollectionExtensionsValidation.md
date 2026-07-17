# CollectionExtensionsValidation

Static helper class that provides lightweight validation for collections and dictionaries. The methods return descriptive error messages, a boolean validity flag, or enforce validity by throwing an exception when the target collection does not meet the expected criteria.

## API

### `public static IReadOnlyList<string> Validate<T>(IEnumerable<T> items)`

**Purpose**  
Examines the supplied enumerable and returns a list of validation error messages. An empty list indicates that the collection passes validation.

**Parameters**  
- `items`: The collection to validate. Passing `null` results in an `ArgumentNullException`.

**Return value**  
An `IReadOnlyList<string>` containing zero or more error messages. If the list is empty, the collection is considered valid.

**Exceptions**  
- `ArgumentNullException` if `items` is `null`.

---

### `public static IReadOnlyList<string> Validate<TKey, TValue>(IDictionary<TKey, TValue> dictionary)`

**Purpose**  
Examines the supplied dictionary and returns a list of validation error messages. An empty list indicates that the dictionary passes validation.

**Parameters**  
- `dictionary`: The dictionary to validate. Passing `null` results in an `ArgumentNullException`.

**Return value**  
An `IReadOnlyList<string>` containing zero or more error messages. If the list is empty, the dictionary is considered valid.

**Exceptions**  
- `ArgumentNullException` if `dictionary` is `null`.

---

### `public static bool IsValid<T>(IEnumerable<T> items)`

**Purpose**  
Determines whether the supplied enumerable passes validation without returning the detailed messages.

**Parameters**  
- `items`: The collection to validate. Passing `null` results in an `ArgumentNullException`.

**Return value**  
`true` if the collection is valid (i.e., `Validate<T>(items)` returns an empty list); otherwise `false`.

**Exceptions**  
- `ArgumentNullException` if `items` is `null`.

---

### `public static bool IsValid<TKey, TValue>(IDictionary<TKey, TValue> dictionary)`

**Purpose**  
Determines whether the supplied dictionary passes validation without returning the detailed messages.

**Parameters**  
- `dictionary`: The dictionary to validate. Passing `null` results in an `ArgumentNullException`.

**Return value**  
`true` if the dictionary is valid (i.e., `Validate<TKey,TValue>(dictionary)` returns an empty list); otherwise `false`.

**Exceptions**  
- `ArgumentNullException` if `dictionary` is `null`.

---

### `public static void EnsureValid<T>(IEnumerable<T> items)`

**Purpose**  
Throws an exception if the supplied enumerable fails validation. The exception message aggregates all validation errors returned by `Validate<T>`.

**Parameters**  
- `items`: The collection to validate. Passing `null` results in an `ArgumentNullException`.

**Exceptions**  
- `ArgumentNullException` if `items` is `null`.  
- `InvalidOperationException` (or a derived exception type) if `Validate<T>(items)` returns a non‑empty list, containing the concatenated validation messages.

---

### `public static void EnsureValid<TKey, TValue>(IDictionary<TKey, TValue> dictionary)`

**Purpose**  
Throws an exception if the supplied dictionary fails validation. The exception message aggregates all validation errors returned by `Validate<TKey,TValue>`.

**Parameters**  
- `dictionary`: The dictionary to validate. Passing `null` results in an `ArgumentNullException`.

**Exceptions**  
- `ArgumentNullException` if `dictionary` is `null`.  
- `InvalidOperationException` (or a derived exception type) if `Validate<TKey,TValue>(dictionary)` returns a non‑empty list, containing the concatenated validation messages.

## Usage

```csharp
using System.Collections.Generic;
using System.Linq;

// Validate a list of saga step identifiers
List<string> stepIds = GetStepIds();
IReadOnlyList<string> errors = CollectionExtensionsValidation.Validate(stepIds);
if (errors.Any())
{
    // Handle validation problems, e.g., log or return a bad request
    Logger.Warning("Step IDs validation failed: {Errors}", string.Join(", ", errors));
}
else
{
    // Proceed with processing
    ProcessSteps(stepIds);
}
```

```csharp
using System.Collections.Generic;

// Ensure a dictionary of correlation IDs is not null or empty before starting a saga
IDictionary<string, object> correlationData = LoadCorrelationData();
// Throws InvalidOperationException with details if the dictionary is invalid
CollectionExtensionsValidation.EnsureValid(correlationData);

// At this point we know correlationData is safe to use
StartSaga(correlationData);
```

## Notes

- An empty collection (`Count == 0`) or empty dictionary is considered valid; only `null` inputs trigger an argument exception.  
- The validation logic does not inspect individual elements or key/value pairs for `null` unless the underlying implementation (not exposed here) chooses to do so.  
- All members are pure functions that depend solely on their input parameters; they maintain no internal state and are therefore thread‑safe for concurrent invocation.  
- The `EnsureValid` overloads are intended for guard‑clause scenarios where a failure should abort the current operation; they wrap the validation result in an exception to avoid boilerplate checking code.  
- Consumers should treat the returned string list as read‑only; modifying it has no effect on the internal state of the class.
