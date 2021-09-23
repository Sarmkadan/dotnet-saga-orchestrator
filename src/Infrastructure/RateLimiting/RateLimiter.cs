#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Infrastructure.RateLimiting;

/// <summary>
/// Token bucket rate limiter for API and service call throttling.
/// Implements sliding window rate limiting with configurable thresholds.
/// </summary>
public interface IRateLimiter
{
    Task<bool> AllowAsync(string key, int requestsPerSecond);
    Task<RateLimitStatus> GetStatusAsync(string key);
    void Reset(string key);
}

public class TokenBucketRateLimiter : IRateLimiter
{
    private readonly Dictionary<string, TokenBucket> _buckets = new();
    private readonly object _lock = new();

    public async Task<bool> AllowAsync(string key, int requestsPerSecond)
    {
        key = key.NotNullOrEmpty(nameof(key));
        requestsPerSecond.GreaterThan(0, nameof(requestsPerSecond));

        return await Task.Run(() =>
        {
            lock (_lock)
            {
                if (!_buckets.TryGetValue(key, out var bucket))
                {
                    bucket = new TokenBucket(requestsPerSecond);
                    _buckets[key] = bucket;
                }

                bucket.Refill(requestsPerSecond);
                return bucket.TryConsume();
            }
        });
    }

    public async Task<RateLimitStatus> GetStatusAsync(string key)
    {
        return await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_buckets.TryGetValue(key, out var bucket))
                {
                    return new RateLimitStatus
                    {
                        AvailableTokens = bucket.AvailableTokens,
                        TotalTokens = bucket.TotalTokens,
                        LastRefillTime = bucket.LastRefillTime,
                        IsLimited = bucket.AvailableTokens <= 0
                    };
                }

                return new RateLimitStatus { IsLimited = false };
            }
        });
    }

    public void Reset(string key)
    {
        lock (_lock)
        {
            _buckets.Remove(key);
        }
    }

    private class TokenBucket
    {
        public int TotalTokens { get; }
        public int AvailableTokens { get; private set; }
        public DateTime LastRefillTime { get; private set; }

        public TokenBucket(int capacity)
        {
            TotalTokens = capacity;
            AvailableTokens = capacity;
            LastRefillTime = DateTime.UtcNow;
        }

        public void Refill(int capacity)
        {
            var now = DateTime.UtcNow;
            var timeSinceLastRefill = (now - LastRefillTime).TotalSeconds;
            var tokensToAdd = (int)(capacity * timeSinceLastRefill);

            if (tokensToAdd > 0)
            {
                AvailableTokens = Math.Min(capacity, AvailableTokens + tokensToAdd);
                LastRefillTime = now;
            }
        }

        public bool TryConsume()
        {
            if (AvailableTokens > 0)
            {
                AvailableTokens--;
                return true;
            }
            return false;
        }
    }
}

public class RateLimitStatus
{
    public int AvailableTokens { get; set; }
    public int TotalTokens { get; set; }
    public DateTime LastRefillTime { get; set; }
    public bool IsLimited { get; set; }
}
