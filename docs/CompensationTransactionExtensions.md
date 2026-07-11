# CompensationTransactionExtensions

Provides extension methods for `CompensationTransaction` objects to inspect and manipulate compensation transaction state, timing, and retry logic within a Saga orchestration context.

## API

### `IsActive(CompensationTransaction transaction)`
Determines whether the compensation transaction is currently active.
- **Returns**: `true` if the transaction is active; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `transaction` is `null`.

### `IsCompletedSuccessfully(CompensationTransaction transaction)`
Determines whether the compensation transaction completed successfully.
- **Returns**: `true` if the transaction completed successfully; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `transaction` is `null`.

### `IsFailed(CompensationTransaction transaction)`
Determines whether the compensation transaction failed.
- **Returns**: `true` if the transaction failed; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `transaction` is `null`.

### `GetDurationMs(CompensationTransaction transaction)`
Gets the duration of the compensation transaction in milliseconds, if available.
- **Returns**: The duration in milliseconds, or `null` if not available.
- **Throws**: `ArgumentNullException` if `transaction` is `null`.

### `GetElapsedTimeMs(CompensationTransaction transaction)`
Gets the elapsed time since the compensation transaction started in milliseconds, if available.
- **Returns**: The elapsed time in milliseconds, or `null` if not available.
- **Throws**: `ArgumentNullException` if `transaction` is `null`.

### `DeepCopy(CompensationTransaction transaction)`
Creates a deep copy of the compensation transaction.
- **Returns**: A new `CompensationTransaction` instance with copied properties.
- **Throws**: `ArgumentNullException` if `transaction` is `null`.

### `UpdateRequestPayload(CompensationTransaction transaction, object payload)`
Updates the request payload of the compensation transaction.
- **Parameters**:
  - `transaction`: The compensation transaction to update.
  - `payload`: The new payload to assign.
- **Throws**: `ArgumentNullException` if `transaction` is `null` or `payload` is `null`.

### `HasExceededMaxRetries(CompensationTransaction transaction)`
Determines whether the compensation transaction has exceeded its maximum retry count.
- **Returns**: `true` if the maximum retries have been exceeded; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `transaction` is `null`.

### `GetSummary(CompensationTransaction transaction)`
Generates a summary string describing the compensation transaction state.
- **Returns**: A summary string.
- **Throws**: `ArgumentNullException` if `transaction` is `null`.

### `CanSafelyRetry(CompensationTransaction transaction)`
Determines whether the compensation transaction can be safely retried based on its current state.
- **Returns**: `true` if the transaction can be safely retried; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `transaction` is `null`.

## Usage

### Example 1: Checking Transaction State and Retry Eligibility
