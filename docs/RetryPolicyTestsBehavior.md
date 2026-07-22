# RetryPolicyTestsBehavior

`RetryPolicyTestsBehavior` is a test class containing behavior-driven tests for retry policy implementations in the `dotnet-saga-orchestrator` project. It verifies the correctness of delay calculation, retry decision logic, and configuration handling for both exponential backoff and linear retry strategies, including jitter variations.

## API

### `CalculateDelay_SuccessFirstTry_NoRetryLogicApplied`
Verifies that when a retry is not needed (e.g., immediate success), no delay is calculated and no retry logic is triggered. This test ensures that the retry mechanism remains dormant when not required.

### `CalculateDelay_RetriesExhausted_ThrowsInvalidOperationException`
Validates that when the maximum number of retry attempts is reached, the retry policy throws an `InvalidOperationException` to signal exhaustion. This prevents further delay calculations or retries after all configured attempts are consumed.

### `CalculateDelay_DelayGrowsExponentially`
Ensures that when using exponential backoff, each retry delay increases by a factor of the base delay (e.g., 2x, 4x, 8x), starting from the initial delay. This test confirms the exponential growth pattern of delays across retry attempts.

### `CalculateDelay_DelayCappedAtMaxDelay`
Checks that the calculated delay does not exceed the configured maximum delay, even when exponential growth would otherwise exceed it. This ensures predictable upper bounds on retry timing.

### `CalculateDelay_WithJitter_AppliesRandomVariation`
Tests that when jitter is enabled, the calculated delay includes a random variation within a specified range, preventing synchronized retries across multiple clients. This test confirms that the delay is not purely deterministic when jitter is applied.

### `CanRetry_WithinMaxRetries_ReturnsTrue`
Verifies that the retry policy returns `true` when the current retry count is strictly less than the maximum allowed retries. This allows continuation of the retry loop under normal conditions.

### `CanRetry_AtMaxRetries_ReturnsFalse`
Ensures that when the current retry count equals the maximum allowed retries, the policy returns `false`, signaling that no further retries should be attempted. This prevents exceeding the retry limit.

### `CanRetry_BeyondMaxRetries_ReturnsFalse`
Confirms that when the current retry count exceeds the maximum allowed retries, the policy returns `false`. This acts as a safeguard against invalid retry states.

### `CreateExponentialWithJitter_JitterEnabled`
Validates that creating a retry policy with jitter enabled results in the correct configuration, including base delay, maximum delay, and jitter factor. This ensures the policy is instantiated with the expected parameters.

### `CreateLinear_DelaysGrowLinearly`
Tests that when using a linear retry strategy, each delay increases by a fixed amount (e.g., 100ms, 200ms, 300ms) based on the retry attempt number. This confirms the linear growth pattern of delays.

### `DefaultConstructor_UsesExpectedDefaultValues`
Ensures that when a retry policy is created using the default constructor, it initializes with sensible defaults (e.g., base delay of 100ms, maximum delay of 30 seconds, max retries of 3). This test confirms predictable behavior without explicit configuration.

### `CustomConstructor_SetsAllPropertiesCorrectly`
Verifies that when a retry policy is created using a custom constructor with explicit parameters, all properties (e.g., base delay, maximum delay, max retries, jitter factor) are set correctly. This ensures the constructor properly initializes the policy with the provided values.

## Usage

### Example 1: Testing Exponential Backoff with Jitter
```csharp
[Fact]
public void TestExponentialBackoffWithJitter()
{
    var policy = new RetryPolicy(
        baseDelay: TimeSpan.FromMilliseconds(100),
        maxDelay: TimeSpan.FromSeconds(30),
        maxRetries: 5,
        useJitter: true
    );

    var delay = policy.CalculateDelay(attempt: 3);
    Assert.InRange(delay.TotalMilliseconds, 400, 800); // 100 * 2^2 ± jitter
}
```

### Example 2: Validating Retry Exhaustion
```csharp
[Fact]
public void TestRetryExhaustion()
{
    var policy = new RetryPolicy(
        baseDelay: TimeSpan.FromMilliseconds(50),
        maxRetries: 2
    );

    Assert.True(policy.CanRetry(attempt: 1));
    Assert.True(policy.CanRetry(attempt: 2));
    Assert.False(policy.CanRetry(attempt: 2)); // At max retries
    Assert.Throws<InvalidOperationException>(() => policy.CalculateDelay(attempt: 3));
}
```

## Notes

- **Thread Safety**: The retry policy is designed to be stateless in its calculations, meaning `CalculateDelay` and `CanRetry` are thread-safe as long as the policy instance itself is not modified after construction. However, if jitter is used, the random number generator may introduce minor non-determinism, though this does not affect correctness.
- **Edge Cases**: The `CalculateDelay` method assumes `attempt` is non-negative; negative values may lead to unexpected behavior or exceptions. The `maxRetries` parameter must be non-negative, and `baseDelay` and `maxDelay` must be positive. The jitter factor, if used, should be a value between 0 and 1 to ensure meaningful variation.
- **Jitter Range**: When jitter is applied, the actual delay may occasionally exceed `maxDelay` due to rounding or floating-point precision, but this is acceptable as it remains within a small margin of the configured maximum.
