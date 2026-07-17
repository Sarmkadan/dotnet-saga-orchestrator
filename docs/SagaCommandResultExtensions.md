# SagaCommandResultExtensions

Provides a set of static extension methods for creating, transforming, and inspecting `SagaCommandResult` and `SagaCommandResult<T>` instances. These helpers simplify common patterns such as converting between typed and untyped results, attaching errors, combining multiple results, and checking for failure conditions.

## API

### `ToTypedResult<T>`
```csharp
public static SagaCommandResult<T> ToTypedResult<T>(this SagaCommandResult result, T data)
```
Converts an untyped `SagaCommandResult` into a typed `SagaCommandResult<T>` by attaching a payload of type `T`.  
- **Parameters**:  
  - `result` – The untyped result to convert.  
  - `data` – The value to store as the result’s payload.  
- **Returns**: A new `SagaCommandResult<T>` containing the given data and inheriting the success/failure state from the original result.  
- **Throws**: `ArgumentNullException` if `result` is `null`.

### `ToUntypedResult`
```csharp
public static SagaCommandResult ToUntypedResult(this SagaCommandResult<T> result)
```
Strips the type information from a typed `SagaCommandResult<T>`, returning an untyped `SagaCommandResult` that preserves the success/failure state and any attached errors.  
- **Parameters**:  
  - `result` – The typed result to convert.  
- **Returns**: A new `SagaCommandResult` with the same error state.  
- **Throws**: `ArgumentNullException` if `result` is `null`.

### `WithError`
```csharp
public static SagaCommandResult WithError(this SagaCommandResult result, string error)
```
Attaches an error message to an untyped `SagaCommandResult`. The result is marked as failed.  
- **Parameters**:  
  - `result` – The result to which the error is added.  
  - `error` – A description of the error.  
- **Returns**: A new `SagaCommandResult` with the error appended and its failure flag set.  
- **Throws**: `ArgumentNullException` if `result` or `error` is `null`.

### `WithError<T>`
```csharp
public static SagaCommandResult<T> WithError<T>(this SagaCommandResult<T> result, string error)
```
Attaches an error message to a typed `SagaCommandResult<T>`. The result is marked as failed.  
- **Parameters**:  
  - `result` – The typed result to modify.  
  - `error` – A description of the error.  
- **Returns**: A new `SagaCommandResult<T>` with the error appended and its failure flag set.  
- **Throws**: `ArgumentNullException` if `result` or `error` is `null`.

### `ToPaginatedResult<T>`
```csharp
public static PaginatedResult<T> ToPaginatedResult<T>(this SagaCommandResult<IEnumerable<T>> result, int totalCount, int page, int pageSize)
```
Converts a typed `SagaCommandResult` containing a collection into a `PaginatedResult<T>` with pagination metadata.  
- **Parameters**:  
  - `result` – The result holding the collection of items.  
  - `totalCount` – The total number of items across all pages.  
  - `page` – The current page number (1‑based).  
  - `pageSize` – The number of items per page.  
- **Returns**: A `PaginatedResult<T>` that wraps the items and includes pagination information.  
- **Throws**: `ArgumentNullException` if `result` is `null`; `ArgumentOutOfRangeException` if `totalCount`, `page`, or `pageSize` are less than 1.

### `Combine`
```csharp
public static SagaCommandResult Combine(this SagaCommandResult first, SagaCommandResult second)
```
Combines two untyped `SagaCommandResult` instances. The resulting result is a failure if either input is a failure; all errors from both inputs are merged.  
- **Parameters**:  
  - `first` – The first result to combine.  
  - `second` – The second result to combine.  
- **Returns**: A new `SagaCommandResult` that aggregates the success/failure state and errors of both inputs.  
- **Throws**: `ArgumentNullException` if either parameter is `null`.

### `AsFailure`
```csharp
public static SagaCommandResult AsFailure(this SagaCommandResult result)
```
Forces an untyped `SagaCommandResult` into a failure state, regardless of its current state. Existing errors are preserved.  
- **Parameters**:  
  - `result` – The result to mark as failed.  
- **Returns**: A new `SagaCommandResult` with its failure flag set.  
- **Throws**: `ArgumentNullException` if `result` is `null`.

### `AsFailure<T>`
```csharp
public static SagaCommandResult<T> AsFailure<T>(this SagaCommandResult<T> result)
```
Forces a typed `SagaCommandResult<T>` into a failure state. Existing errors are preserved.  
- **Parameters**:  
  - `result` – The typed result to mark as failed.  
- **Returns**: A new `SagaCommandResult<T>` with its failure flag set.  
- **Throws**: `ArgumentNullException` if `result` is `null`.

### `HasError`
```csharp
public static bool HasError(this SagaCommandResult result)
```
Determines whether an untyped `SagaCommandResult` contains any error messages.  
- **Parameters**:  
  - `result` – The result to inspect.  
- **Returns**: `true` if the result has at least one error; otherwise `false`.  
- **Throws**: `ArgumentNullException` if `result` is `null`.

### `HasError<T>`
```csharp
public static bool HasError<T>(this SagaCommandResult<T> result)
```
Determines whether a typed `SagaCommandResult<T>` contains any error messages.  
- **Parameters**:  
  - `result` – The typed result to inspect.  
- **Returns**: `true` if the result has at least one error; otherwise `false`.  
- **Throws**: `ArgumentNullException` if `result` is `null`.

## Usage

### Example 1: Creating a typed result with an error and converting to untyped
```csharp
var successResult = SagaCommandResult.Ok();
var typedResult = successResult.ToTypedResult("order-123");

if (!typedResult.HasError())
{
    // Attach an error to simulate a failure
    var failedResult = typedResult.WithError("Payment declined");
    var untypedFailure = failedResult.ToUntypedResult();
    Console.WriteLine($"Failed: {untypedFailure.HasError()}"); // True
}
```

### Example 2: Combining multiple results and checking for failures
```csharp
var step1 = SagaCommandResult.Ok();
var step2 = SagaCommandResult.Ok().WithError("Timeout");
var step3 = SagaCommandResult.Ok();

var combined = step1.Combine(step2).Combine(step3);
if (combined.HasError())
{
    // combined is a failure because step2 had an error
    var final = combined.AsFailure();
    // final is guaranteed to be a failure
}
```

## Notes

- All methods are **pure** – they do not modify the input instances but return new `SagaCommandResult` or `SagaCommandResult<T>` objects. This makes them safe to use in concurrent scenarios as long as the input objects themselves are not mutated externally.  
- Passing `null` as any required parameter (including the `this` extension target) will throw `ArgumentNullException`.  
- The `Combine` method merges errors from both results; if both are failures, the combined result contains all errors from both.  
- `AsFailure` and `AsFailure<T>` do not clear existing errors; they only set the failure flag. To add an error, use `WithError` or `WithError<T>` first.  
- `ToPaginatedResult<T>` expects the source result to contain an `IEnumerable<T>`. If the source result is a failure, the returned `PaginatedResult<T>` will also be a failure, and the pagination metadata may be inconsistent – callers should verify success before using the paginated data.  
- Thread‑safety: Because these methods are static and operate on immutable‑by‑convention result objects, they are inherently thread‑safe. However, if the underlying `SagaCommandResult` implementation is mutable, external synchronization may be required.
