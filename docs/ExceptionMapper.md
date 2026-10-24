# ExceptionMapper

Centralizes exception-to-HTTP-response mapping for the saga orchestrator. It provides static methods to classify known exception types (saga-specific, validation, not-found, timeout) and to extract structured error codes, along with an instance-based `ErrorResponse` model that carries the mapped status code, message, details, request identifier, and timestamp back to callers.

## API

### Static Members

#### `MapException(Exception exception)`
Maps any exception to a tuple containing an appropriate `HttpStatusCode` and a human-readable message string. Internally inspects the exception type and its hierarchy, applying predefined mappings for saga exceptions, validation errors, not-found conditions, and timeouts. Unrecognized exceptions default to `500 Internal Server Error`.

- **Parameters**: `exception` — the exception to classify and map.
- **Returns**: `(HttpStatusCode statusCode, string message)`.
- **Throws**: `ArgumentNullException` when `exception` is `null`.

#### `IsSagaException(Exception exception)`
Determines whether the given exception originates from saga orchestration logic (e.g., compensation failures, state persistence errors, or orchestration contract violations).

- **Parameters**: `exception` — the exception to test.
- **Returns**: `true` if the exception is a saga-specific exception; otherwise `false`.
- **Throws**: `ArgumentNullException` when `exception` is `null`.

#### `IsValidationError(Exception exception)`
Checks whether the exception represents a request validation failure (e.g., malformed input, missing required fields, or business-rule violations detected before saga execution).

- **Parameters**: `exception` — the exception to test.
- **Returns**: `true` if the exception is a validation error; otherwise `false`.
- **Throws**: `ArgumentNullException` when `exception` is `null`.

#### `IsNotFound(Exception exception)`
Tests whether the exception indicates a missing resource (aggregate, entity, or external service endpoint) that the saga expected to find.

- **Parameters**: `exception` — the exception to test.
- **Returns**: `true` if the exception represents a not-found condition; otherwise `false`.
- **Throws**: `ArgumentNullException` when `exception` is `null`.

#### `IsTimeout(Exception exception)`
Identifies exceptions caused by timeouts during saga step execution, such as external service calls exceeding their allotted duration or internal step deadlines being breached.

- **Parameters**: `exception` — the exception to test.
- **Returns**: `true` if the exception is a timeout-related failure; otherwise `false`.
- **Throws**: `ArgumentNullException` when `exception` is `null`.

#### `GetErrorCode(Exception exception)`
Extracts a stable, machine-readable error code string from the exception. For recognized exception types this returns a domain-specific code (e.g., `"SAGA_COMPENSATION_FAILED"`, `"VALIDATION_ERROR"`); for unknown exceptions it returns a generic fallback code.

- **Parameters**: `exception` — the exception from which to derive the code.
- **Returns**: A non-null, non-empty error code string.
- **Throws**: `ArgumentNullException` when `exception` is `null`.

#### `FromException(Exception exception, string? requestId = null)`
Builds a sanitized `ErrorResponse` instance from an exception, combining the mapped message with the correlation identifier and the current UTC timestamp. This is the primary factory method for producing a standardized error payload. The response never contains the exception's stack trace, inner exceptions, or (unless the type is allowlisted, see `Details`) its message.

- **Parameters**:
  - `exception` — the exception to convert.
  - `requestId` — optional correlation identifier, typically the request trace identifier. When `null` or whitespace, a new identifier is generated with `SagaIdGenerator.GenerateRequestId()`, so `RequestId` is always non-empty.
- **Returns**: A populated `ErrorResponse` instance.
- **Throws**: `ArgumentNullException` when `exception` is `null`.

#### `ErrorResponseLoggingExtensions.ToErrorResponseAndLog(this ILogger logger, Exception exception, string? requestId = null)`
Lives in `SagaOrchestrator.Infrastructure.Logging`. Calls `FromException` and, when the mapped status code is 5xx, logs the full exception (message and stack trace) through `logger` with the same `RequestId` that is returned to the client. Client errors (4xx) are not logged here.

- **Parameters**: `logger` — the logger that receives the full detail; `exception` — the exception to convert; `requestId` — as for `FromException`.
- **Returns**: A sanitized `ErrorResponse` instance.
- **Throws**: `ArgumentNullException` when `logger` or `exception` is `null`.

### Instance Properties (`ErrorResponse`)

#### `Code` (string)
The machine-readable error code derived from the exception, as returned by `GetErrorCode`. Always non-null and non-empty.

#### `Message` (string)
The human-readable message produced by `MapException`, suitable for display in logs or client-facing responses.

#### `Details` (string?)
The exception message, copied only for an allowlisted set of types whose messages are written for callers: `ArgumentException` (and derived types), `ValidationException`, `InvalidSagaDefinitionException`, and `SagaNotFoundException`. For every other exception, including unmapped exception types such as `SqlException` or `IOException`, `Details` is `null`. The message is never copied from a stack trace, inner exception, or any other internal source.

#### `RequestId` (string)
The correlation identifier passed to `FromException`, or a generated `req_` identifier when none was supplied. Return it to the client and use it to find the full exception in server logs.

#### `Timestamp` (DateTime)
The UTC instant at which `FromException` was called, capturing when the error response was generated.

## Usage

### Example 1: Middleware mapping exceptions to HTTP responses

```csharp
try
{
    await _sagaOrchestrator.ExecuteAsync(orderSaga, cancellationToken);
}
catch (Exception ex)
{
    var (statusCode, message) = ExceptionMapper.MapException(ex);
    var errorResponse = _logger.ToErrorResponseAndLog(ex, requestId: _context.TraceIdentifier);

    _context.Response.StatusCode = (int)statusCode;
    _context.Response.ContentType = "application/json";
    await _context.Response.WriteAsync(
        JsonSerializer.Serialize(errorResponse), cancellationToken);
}
```

### Example 2: Conditional handling based on exception classification

```csharp
catch (Exception ex) when (ExceptionMapper.IsValidationError(ex))
{
    var error = ExceptionMapper.FromException(ex, requestId);
    _logger.LogWarning("Validation failure {Code} for request {RequestId}",
        error.Code, error.RequestId);
    return Results.BadRequest(error);
}
catch (Exception ex) when (ExceptionMapper.IsTimeout(ex))
{
    var error = ExceptionMapper.FromException(ex, requestId);
    _logger.LogError("Saga step timed out {Code}. Initiating retry policy.", error.Code);
    return Results.StatusCode((int)HttpStatusCode.GatewayTimeout, error);
}
catch (Exception ex)
{
    // Full exception is logged server-side under error.RequestId; the client only sees the sanitized payload.
    var error = _logger.ToErrorResponseAndLog(ex, requestId);
    return Results.InternalServerError(error);
}
```

## Notes

- All static classification and mapping methods throw `ArgumentNullException` when passed a `null` exception. Callers must guard against `null` before invoking them.
- `MapException` and `GetErrorCode` rely on the runtime type of the exception, including base types. Custom exceptions that inherit from recognized saga or validation base types are classified correctly without additional registration.
- `FromException` always sets `Timestamp` to `DateTime.UtcNow` at the moment of invocation. Two calls with the same exception and request identifier will produce instances differing only in their timestamps.
- Sanitization is not environment-dependent: the same rules apply in development and production. Unmapped and non-allowlisted exceptions never expose their message, stack trace, or inner exception details to clients. Log the full exception server-side (see `ToErrorResponseAndLog`) instead.
- The `Details` property is populated only for the allowlisted exception types listed above. It is `null` otherwise. Consumers should not assume it is always present.
- The static methods are thread-safe; they perform no shared state mutation and operate solely on the provided exception argument. The `ErrorResponse` instance is immutable after construction and safe to share across threads.
- When `requestId` is omitted, `null`, or whitespace in `FromException`, a new `req_` identifier is generated, so `RequestId` is always non-empty and satisfies `ExceptionMapperValidation`. Supply the request trace identifier when one exists so client reports match server logs.
