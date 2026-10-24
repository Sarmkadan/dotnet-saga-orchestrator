#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SagaOrchestrator.Core.Extensions;

namespace SagaOrchestrator.Infrastructure.RateLimiting;

/// <summary>
/// Token bucket rate limiter for API and service call throttling.
/// Implements sliding window rate limiting with configurable thresholds.
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// Attempts to acquire a single permit from the bucket identified by <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Bucket identifier; must not be null, empty, or whitespace.</param>
    /// <param name="requestsPerSecond">Bucket capacity; must be greater than zero.</param>
    /// <returns>True when a permit was acquired; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is empty or consists only of white-space characters.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="requestsPerSecond"/> is not greater than zero.</exception>
    Task<bool> AllowAsync(string key, int requestsPerSecond);

    /// <summary>
    /// Gets a snapshot of the bucket identified by <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Bucket identifier; must not be null, empty, or whitespace.</param>
    /// <returns>The current status, or an unlimited status when no bucket exists for the key.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is empty or consists only of white-space characters.</exception>
    Task<RateLimitStatus> GetStatusAsync(string key);

    /// <summary>
    /// Removes the bucket identified by <paramref name="key"/>, if present.
    /// </summary>
    /// <param name="key">Bucket identifier; must not be null, empty, or whitespace.</param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is empty or consists only of white-space characters.</exception>
    void Reset(string key);

    /// <summary>
    /// Waits until a bucket permit is available or the timeout elapses.
    /// </summary>
    /// <param name="key">Bucket identifier; must not be null, empty, or whitespace.</param>
    /// <param name="permits">Number of permits to acquire; must be greater than zero.</param>
    /// <param name="timeout">Maximum time to wait; must be greater than zero.</param>
    /// <param name="cancellationToken">Token used to cancel the wait.</param>
    /// <returns>True when the permits were acquired; false when the timeout elapsed first.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is empty or consists only of white-space characters.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="permits"/> is not greater than zero, or <paramref name="timeout"/> is not positive.</exception>
    Task<bool> WaitAsync(string key, int permits, TimeSpan timeout, CancellationToken cancellationToken = default);
}

public class TokenBucketRateLimiter : IRateLimiter
{
    private readonly Dictionary<string, TokenBucket> _buckets = new();
    private readonly object _lock = new();

    /// <inheritdoc />
    public Task<bool> AllowAsync(string key, int requestsPerSecond)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        requestsPerSecond.GreaterThan(0, nameof(requestsPerSecond));

        return Task.Run(() =>
        {
            lock (_lock)
            {
                if (!_buckets.TryGetValue(key, out var bucket))
                {
                    bucket = new TokenBucket(requestsPerSecond);
                    _buckets[key] = bucket;
                }

                return bucket.TryAcquire(1);
            }
        });
    }

    /// <inheritdoc />
    public Task<RateLimitStatus> GetStatusAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return Task.Run(() =>
        {
            lock (_lock)
            {
                if (_buckets.TryGetValue(key, out var bucket))
                {
                    return RateLimitStatus.CreateSnapshot(
                bucket.AvailableTokens,
                bucket.TotalTokens,
                bucket.LastRefillTime,
                bucket.AvailableTokens <= 0
            );
                }

                return RateLimitStatus.NotLimited;
            }
        });
    }

    /// <inheritdoc />
    public void Reset(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        lock (_lock)
        {
            _buckets.Remove(key);
        }
    }

    /// <inheritdoc />
    public Task<bool> WaitAsync(string key, int permits, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        permits.GreaterThan(0, nameof(permits));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero((int)timeout.TotalMilliseconds, nameof(timeout));

        return WaitCoreAsync(key, permits, timeout, cancellationToken);
    }

    private async Task<bool> WaitCoreAsync(string key, int permits, TimeSpan timeout, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var deadline = DateTime.UtcNow.Add(timeout);
        var initialTimeout = TimeSpan.FromMilliseconds(Math.Min(100, timeout.TotalMilliseconds));

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (!_buckets.TryGetValue(key, out var bucket))
                    {
                        bucket = new TokenBucket(permits);
                        _buckets[key] = bucket;
                    }

                    return bucket.TryAcquire(permits);
                }
            });

            if (result)
            {
                return true;
            }

            var remainingTimeout = deadline - DateTime.UtcNow;
            if (remainingTimeout <= TimeSpan.Zero)
            {
                break;
            }

            var waitTime = TimeSpan.FromMilliseconds(Math.Min(initialTimeout.TotalMilliseconds, remainingTimeout.TotalMilliseconds));
            await Task.Delay(waitTime, cancellationToken);
        }

        return false;
    }

    private class TokenBucket
    {
        private int _availableTokens;
        private DateTime _lastRefillTime;
        private readonly int _totalTokens;
        private readonly object _stateLock = new();

        public int TotalTokens => _totalTokens;
        public int AvailableTokens => _availableTokens;
        public DateTime LastRefillTime => _lastRefillTime;

        public TokenBucket(int capacity)
        {
            _totalTokens = capacity;
            _availableTokens = capacity;
            _lastRefillTime = DateTime.UtcNow;
        }

        public bool TryAcquire(int tokens)
        {
            if (tokens <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tokens), "Tokens must be positive");
            }

            if (tokens > _totalTokens)
            {
                throw new ArgumentOutOfRangeException(nameof(tokens), $"Cannot acquire {tokens} tokens when bucket capacity is {_totalTokens}");
            }

            lock (_stateLock)
            {
                RefillInternal();
                if (_availableTokens >= tokens)
                {
                    _availableTokens -= tokens;
                    return true;
                }
                return false;
            }
        }

        private void RefillInternal()
        {
            var now = DateTime.UtcNow;
            var timeSinceLastRefill = (now - _lastRefillTime).TotalSeconds;
            var tokensToAdd = (int)(_totalTokens * timeSinceLastRefill);

            if (tokensToAdd > 0)
            {
                _availableTokens = Math.Min(_totalTokens, _availableTokens + tokensToAdd);
                _lastRefillTime = now;
            }
        }
    }
}

public sealed class RateLimitStatus
{
    public int AvailableTokens { get; }
    public int TotalTokens { get; }
    public DateTime LastRefillTime { get; }
    public bool IsLimited { get; }

    private RateLimitStatus(int availableTokens, int totalTokens, DateTime lastRefillTime, bool isLimited)
    {
        AvailableTokens = availableTokens;
        TotalTokens = totalTokens;
        LastRefillTime = lastRefillTime;
        IsLimited = isLimited;
    }

    internal static RateLimitStatus CreateSnapshot(int availableTokens, int totalTokens, DateTime lastRefillTime, bool isLimited)
    {
        return new RateLimitStatus(
            availableTokens,
            totalTokens,
            lastRefillTime,
            isLimited
        );
    }

    internal static RateLimitStatus NotLimited { get; } = new RateLimitStatus(0, 0, DateTime.MinValue, false);

    public override string ToString() =>
        IsLimited
            ? $"Rate limited: {AvailableTokens}/{TotalTokens} tokens available"
            : $"Within limit: {AvailableTokens}/{TotalTokens} tokens available";
}
