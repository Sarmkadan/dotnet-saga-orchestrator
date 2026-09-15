# TokenBucketRateLimiter

`TokenBucketRateLimiter` is the in-memory implementation of `IRateLimiter`. It keeps an independent token bucket for each string key and synchronizes access so one instance can be shared across callers. Buckets refill over time at their configured capacity per second, up to that capacity.

The first operation that creates a bucket determines its capacity: `AllowAsync` uses `requestsPerSecond`, while `WaitAsync` uses `permits`. Later calls for the same key reuse that bucket until `Reset` removes it.

## Public API

### `TokenBucketRateLimiter()`

Creates an empty in-memory rate limiter. No buckets exist until an acquisition is attempted.

### `Task<bool> AllowAsync(string key, int requestsPerSecond)`

Attempts to acquire one token for `key` immediately. A new bucket starts with `requestsPerSecond` tokens. Returns `true` when a token is acquired and `false` when no token is currently available.

- `key` must not be null or empty.
- `requestsPerSecond` must be greater than zero.

### `Task<RateLimitStatus> GetStatusAsync(string key)`

Returns a snapshot containing `AvailableTokens`, `TotalTokens`, `LastRefillTime`, and `IsLimited`. If no bucket exists for the key, the result has zero available and total tokens, `DateTime.MinValue` as its last refill time, and `IsLimited` set to `false`.

Checking status does not itself refill the bucket; refilling occurs when an acquisition is attempted.

### `void Reset(string key)`

Removes the bucket for `key`. The next call to `AllowAsync` or `WaitAsync` creates a fresh bucket. Resetting a key that has no bucket has no effect.

### `Task<bool> WaitAsync(string key, int permits, TimeSpan timeout, CancellationToken cancellationToken = default)`

Repeatedly attempts to acquire `permits` tokens until it succeeds, the timeout expires, or cancellation is requested. Attempts are separated by delays of at most 100 milliseconds. Returns `true` after acquiring all requested permits, or `false` after timing out.

- `key` must not be null or empty.
- `permits` must be greater than zero.
- `timeout` must be positive.
- Cancellation is reported with `OperationCanceledException`.

## Usage

```csharp
using SagaOrchestrator.Infrastructure.RateLimiting;

IRateLimiter limiter = new TokenBucketRateLimiter();

if (await limiter.AllowAsync("orders-api", requestsPerSecond: 10))
{
    await SendOrderAsync();
}

// Wait up to two seconds for three permits from another keyed bucket.
bool acquired = await limiter.WaitAsync(
    "batch-worker",
    permits: 3,
    timeout: TimeSpan.FromSeconds(2),
    CancellationToken.None);

RateLimitStatus status = await limiter.GetStatusAsync("orders-api");
Console.WriteLine(status);

limiter.Reset("orders-api");
```

`TokenBucketRateLimiter` stores all state in memory. State is lost when the instance is discarded, and separate instances do not share limits.
