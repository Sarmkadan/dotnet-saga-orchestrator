# RetryPolicyTests

Unit test class for the `RetryPolicy` class, verifying behavior of retry configuration, delay calculation, and identifier generation.

## API

### `Constructor_NegativeMaxRetries_ThrowsArgumentException()`
Verifies that the `RetryPolicy` constructor throws an `ArgumentException` when the `maxRetries` parameter is negative.

### `Constructor_NegativeInitialDelay_ThrowsArgumentException()`
Verifies that the `RetryPolicy` constructor throws an `ArgumentException` when the `initialDelay` parameter is negative.

### `Constructor_BackoffMultiplierBelowOne_ThrowsArgumentException()`
Verifies that the `RetryPolicy` constructor throws an `ArgumentException` when the `backoffMultiplier` parameter is less than one.

### `Constructor_MaxDelayLessThanInitialDelay_ThrowsArgumentException()`
Verifies that the `RetryPolicy` constructor throws an `ArgumentException` when the `maxDelay` parameter is less than the `initialDelay`.

### `CalculateDelay_FirstAttempt_ReturnsInitialDelay()`
Verifies that the first retry attempt returns the `initialDelay` value without applying any backoff.

### `CalculateDelay_SecondAttempt_AppliesExponentialBackoff()`
Verifies that the second retry attempt applies the exponential backoff multiplier to the `initialDelay`.

### `CalculateDelay_ThirdAttempt_SquaresTheMultiplier()`
Verifies that the third retry attempt applies the square of the backoff multiplier to the `initialDelay`.

### `CalculateDelay_LargeAttemptNumber_CapsAtMaxDelay()`
Verifies that when the attempt number would result in a delay exceeding `maxDelay`, the returned delay is capped at `maxDelay`.

### `CalculateDelay_AttemptBelowOne_ThrowsArgumentException()`
Verifies that the `CalculateDelay` method throws an `ArgumentException` when the attempt number is less than one.

### `CalculateDelay_AttemptExceedsMaxRetries_ThrowsInvalidOperationException()`
Verifies that the `CalculateDelay` method throws an `InvalidOperationException` when the attempt number exceeds the configured `maxRetries`.

### `CreateLinear_AllAttemptsReturnSameFixedDelay()`
Verifies that the `CreateLinear` factory method returns a `RetryPolicy` where all retry attempts return the same fixed delay.

### `CreateNoRetry_SetsZeroMaxRetriesAndDelay()`
Verifies that the `CreateNoRetry` factory method returns a `RetryPolicy` with `maxRetries` and `initialDelay` both set to zero.

### `CreateExponential_UsesDoubleBackoffMultiplier()`
Verifies that the `CreateExponential` factory method returns a `RetryPolicy` using a backoff multiplier of 2.0.

### `CanRetry_WhenBelowMaxRetries_ReturnsTrue()`
Verifies that the `CanRetry` method returns `true` when the current attempt number is less than `maxRetries`.

### `CanRetry_WhenAtMaxRetries_ReturnsFalse()`
Verifies that the `CanRetry` method returns `false` when the current attempt number equals `maxRetries`.

### `GenerateSagaId_ReturnsIdWithSagaPrefix()`
Verifies that the `GenerateSagaId` method returns a string prefixed with "saga-".

### `GenerateSagaId_EachCallReturnsUniqueId()`
Verifies that each call to `GenerateSagaId` returns a distinct identifier.

### `GenerateCorrelationId_ReturnsIdWithCorrPrefix()`
Verifies that the `GenerateCorrelationId` method returns a string prefixed with "corr-".

### `GenerateStepId_ReturnsIdWithStepPrefix()`
Verifies that the `GenerateStepId` method returns a string prefixed with "step-".

### `GenerateRequestId_ReturnsIdWithReqPrefix()`
Verifies that the `GenerateRequestId` method returns a string prefixed with "req-".

## Usage
