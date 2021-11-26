# IRateLimiter

The `IRateLimiter` interface defines a contract for rate limiting mechanisms that control the frequency of operations by managing token-based quotas. It is designed to prevent resource exhaustion or abuse by limiting the number of requests or actions that can occur within a specified time window. Implementations typically use a token bucket algorithm to track available tokens, refill them over time, and provide status information for monitoring and control.

## API

### AllowAsync
- **Purpose**: Determines whether a specified number of tokens can be consumed immediately.
- **Parameters**: None (implicitly consumes a default number of tokens, likely 1).
- **Return Value**: `Task<bool>` indicating if the operation is allowed.
- **Exceptions**: Throws `InvalidOperationException` if the rate limiter is in an invalid state.

### GetStatusAsync
- **Purpose**: Retrieves the current status of the rate limiter, including available tokens and refill timing.
- **Parameters**: None.
- **Return Value**: `Task<RateLimitStatus>` containing detailed state information.
- **Exceptions**: Throws `InvalidOperationException` if the rate limiter is not initialized.

### Reset
- **Purpose**: Resets the rate limiter to its initial state, clearing all consumed tokens and restoring available tokens to the total.
- **Parameters**: None.
- **Return Value**: `void`.
- **Exceptions**: Throws `NotSupportedException` if the implementation does not support resetting.

### TotalTokens
- **Purpose**: Gets the maximum number of tokens the rate limiter can hold.
- **Type**: `int`.

### AvailableTokens
- **Purpose**: Gets the current number of tokens available for consumption.
- **Type**: `int`.

### LastRefillTime
- **Purpose**: Gets the UTC timestamp of the last automatic token refill.
- **Type**: `DateTime`.

### TokenBucket
- **Purpose**: Provides direct access to the underlying token bucket implementation for advanced scenarios.
- **Type**: `TokenBucket`.

### Refill
- **Purpose**: Manually triggers a token refill based on the elapsed time since the last refill.
- **Parameters**: None.
- **Return Value**: `void`.
- **Exceptions**: Throws `InvalidOperationException` if the rate limiter is not properly configured.

### TryConsume
- **Purpose**: Attempts to consume a specified number of tokens without blocking.
- **Parameters**: `int tokens` (number of tokens to consume).
- **Return Value**: `bool` indicating success of the consumption attempt.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `tokens` is negative or exceeds `TotalTokens`.

### IsLimited
- **Purpose**: Indicates whether the rate limiter is currently restricting access due to exhausted tokens.
- **Type**: `bool`.

## Usage

### Example 1: Rate Limiting API Requests
```csharp
public class ApiService
{
    private readonly IRateLimiter _rateLimiter;

    public ApiService(IRateLimiter rateLimiter)
    {
        _rateLimiter = rateLimiter;
    }

    public async Task<bool> MakeRequestAsync()
    {
        if (await _rateLimiter.AllowAsync())
        {
            // Proceed with the API request
            return true;
        }
        // Rate limit exceeded; handle accordingly
        return false;
    }
}
```

### Example 2: Monitoring Rate Limit Status
```csharp
public class RateLimitMonitor
{
    private readonly IRateLimiter _rateLimiter;

    public RateLimitMonitor(IRateLimiter rateLimiter)
    {
        _rateLimiter = rateLimiter;
    }

    public async Task LogStatusAsync()
    {
        var status = await _rateLimiter.GetStatusAsync();
        Console.WriteLine($"Available Tokens: {status.AvailableTokens}");
        Console.WriteLine($"Last Refill: {status.LastRefillTime}");
    }
}
```

## Notes

- **Thread Safety**: Implementations must ensure thread-safe access to mutable state (e.g., `AvailableTokens`, `LastRefillTime`) when used in concurrent environments. Methods like `AllowAsync` and `TryConsume` may involve atomic operations to prevent race conditions.
- **Edge Cases**: 
  - Calling `TryConsume` with a token count exceeding `TotalTokens` will throw an exception.
  - `Reset` may not be supported by all implementations (e.g., distributed rate limiters).
  - `Refill` should be called periodically by the implementation to replenish tokens based on time elapsed.
- **TokenBucket Exposure**: The `TokenBucket` property allows direct manipulation but may bypass rate limiter safeguards. Use with caution.
