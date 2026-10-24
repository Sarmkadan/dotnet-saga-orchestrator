#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SagaOrchestrator.Infrastructure.Caching;

/// <summary>
/// In-memory caching service with TTL support and expiration policies.
/// Provides thread-safe cache operations for saga and definition caching.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets the cached value for <paramref name="key"/>, or the default value when missing or expired.
    /// </summary>
    /// <param name="key">Cache key; must not be null, empty, or whitespace.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is empty or consists only of white-space characters.</exception>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a value under <paramref name="key"/>, evicting an entry first when the cache is full.
    /// </summary>
    /// <param name="key">Cache key; must not be null, empty, or whitespace.</param>
    /// <param name="value">Value to store.</param>
    /// <param name="expiration">Time to live; defaults to one hour when null.</param>
    /// <param name="maxKeyLength">Overrides the default maximum key length.</param>
    /// <param name="maxValueSize">Overrides the default maximum serialized value size in bytes.</param>
    /// <param name="maxCacheSize">Overrides the default maximum number of cached items.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is empty or whitespace, exceeds the maximum key length, or the serialized value exceeds the maximum size.</exception>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, int? maxKeyLength = null, int? maxValueSize = null, int? maxCacheSize = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the entry for <paramref name="key"/>, if present.
    /// </summary>
    /// <param name="key">Cache key; must not be null, empty, or whitespace.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is empty or consists only of white-space characters.</exception>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a non-expired entry exists for <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Cache key; must not be null, empty, or whitespace.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is empty or consists only of white-space characters.</exception>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    int GetCacheSize();

    /// <summary>
    /// Gets or creates a value in the cache with stampede protection.
    /// </summary>
    /// <param name="key">Cache key; must not be null, empty, or whitespace.</param>
    /// <param name="factory">Produces the value when it is not cached; must not be null.</param>
    /// <param name="expiration">Time to live for a newly created value; defaults to one hour when null.</param>
    /// <param name="maxKeyLength">Overrides the default maximum key length.</param>
    /// <param name="maxValueSize">Overrides the default maximum serialized value size in bytes.</param>
    /// <param name="maxCacheSize">Overrides the default maximum number of cached items.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="factory"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is empty or whitespace, or the created value violates the size or key limits.</exception>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, int? maxKeyLength = null, int? maxValueSize = null, int? maxCacheSize = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the cache hit and miss counters.
    /// </summary>
    (long Hits, long Misses) GetMetrics();
}

/// <summary>
/// In-memory caching service with TTL support and expiration policies.
/// Provides thread-safe cache operations for saga and definition caching.
/// </summary>
public class CacheService : ICacheService, IDisposable
{
    private readonly Dictionary<string, CacheEntry> _cache;
    private readonly ReaderWriterLockSlim _lock;
    private readonly Timer _cleanupTimer;
    private readonly SemaphoreSlim[] _locks;
    private readonly Random _random;
    private long _hits;
    private long _misses;
    private readonly int _defaultMaxKeyLength;
    private readonly int _defaultMaxValueSize;
    private readonly int _defaultMaxCacheSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheService"/> class.
    /// </summary>
    /// <param name="defaultMaxKeyLength">Default maximum key length in characters.</param>
    /// <param name="defaultMaxValueSize">Default maximum value size in bytes (serialized JSON).</param>
    /// <param name="defaultMaxCacheSize">Default maximum number of items in the cache.</param>
    public CacheService(int defaultMaxKeyLength = 250, int defaultMaxValueSize = 102400, int defaultMaxCacheSize = 1000)
    {
        _cache = new();
        _lock = new();
        _locks = Enumerable.Range(0, 32).Select(_ => new SemaphoreSlim(1, 1)).ToArray();
        _random = new Random();
        // Cleanup expired entries every 5 minutes
        _cleanupTimer = new Timer(CleanupExpiredEntries, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        _defaultMaxKeyLength = defaultMaxKeyLength;
        _defaultMaxValueSize = defaultMaxValueSize;
        _defaultMaxCacheSize = defaultMaxCacheSize;
    }

    private SemaphoreSlim GetLock(string key) => _locks[Math.Abs(key.GetHashCode()) % _locks.Length];

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        _lock.EnterReadLock();
        try
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.IsExpired())
                {
                    Interlocked.Increment(ref _misses);
                    return Task.FromResult<T?>(default);
                }
                Interlocked.Increment(ref _hits);
                return Task.FromResult((T?)entry.Value);
            }
            Interlocked.Increment(ref _misses);
            return Task.FromResult<T?>(default);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, int? maxKeyLength = null, int? maxValueSize = null, int? maxCacheSize = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        var effectiveMaxKeyLength = maxKeyLength ?? _defaultMaxKeyLength;
        var effectiveMaxValueSize = maxValueSize ?? _defaultMaxValueSize;
        var effectiveMaxCacheSize = maxCacheSize ?? _defaultMaxCacheSize;

        if (key.Length > effectiveMaxKeyLength)
        {
            throw new ArgumentException($"Key length exceeds maximum allowed length of {effectiveMaxKeyLength}.", nameof(key));
        }

        if (effectiveMaxValueSize > 0 && value != null)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                var byteCount = Encoding.UTF8.GetByteCount(json);
                if (byteCount > effectiveMaxValueSize)
                {
                    throw new ArgumentException($"Value size ({byteCount} bytes) exceeds maximum allowed size of {effectiveMaxValueSize} bytes.", nameof(value));
                }
            }
            catch (NotSupportedException)
            {
                // If serialization fails, we cannot determine the size, so we skip the size check.
                // This can happen for types that are not serializable by System.Text.Json.
                // In a production system, you might want to handle this differently.
            }
        }

        _lock.EnterWriteLock();
        try
        {
            // Enforce maximum cache size by removing entries if necessary
            if (_cache.Count >= effectiveMaxCacheSize)
            {
                // Try to remove an expired entry first
                var expiredKey = _cache.FirstOrDefault(kvp => kvp.Value.IsExpired()).Key;
                if (!string.IsNullOrEmpty(expiredKey))
                {
                    _cache.Remove(expiredKey);
                }
                else
                {
                    // If no expired entries, remove the first entry (arbitrary choice)
                    var firstKey = _cache.Keys.First();
                    _cache.Remove(firstKey);
                }
            }

            var expiryTime = CalculateExpiry(expiration);
            _cache[key] = new CacheEntry(value, expiryTime);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        _lock.EnterWriteLock();
        try
        {
            _cache.Remove(key);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _lock.EnterWriteLock();
        try
        {
            _cache.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        _lock.EnterReadLock();
        try
        {
            return Task.FromResult(_cache.ContainsKey(key) && !_cache[key].IsExpired());
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc />
    public int GetCacheSize()
    {
        _lock.EnterReadLock();
        try
        {
            return _cache.Count;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc />
    public Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, int? maxKeyLength = null, int? maxValueSize = null, int? maxCacheSize = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        return GetOrCreateCoreAsync(key, factory, expiration, maxKeyLength, maxValueSize, maxCacheSize, cancellationToken);
    }

    private async Task<T> GetOrCreateCoreAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration, int? maxKeyLength, int? maxValueSize, int? maxCacheSize, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // 1. Try get
        T? cached = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
        if (cached != null) return cached;

        // 2. Lock and re-check (Double-checked locking pattern)
        var semaphore = GetLock(key);
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cached = await GetAsync<T>(key, cancellationToken).ConfigureAwait(false);
            if (cached != null) return cached;

            // 3. Factory call
            T value = await factory().ConfigureAwait(false);

            // 4. Set
            await SetAsync(key, value, expiration, maxKeyLength, maxValueSize, maxCacheSize, cancellationToken).ConfigureAwait(false);
            return value;
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <inheritdoc />
    public (long Hits, long Misses) GetMetrics()
    {
        return (Interlocked.Read(ref _hits), Interlocked.Read(ref _misses));
    }

    private DateTime CalculateExpiry(TimeSpan? expiration)
    {
        var ttl = expiration ?? TimeSpan.FromHours(1);
        // Add jitter: 0-5s
        var jitter = TimeSpan.FromMilliseconds(_random.Next(0, 5000));
        return DateTime.UtcNow.Add(ttl).Add(jitter);
    }

    private void CleanupExpiredEntries(object? state)
    {
        _lock.EnterWriteLock();
        try
        {
            var expiredKeys = _cache
                .Where(kvp => kvp.Value.IsExpired())
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cache.Remove(key);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
        _lock?.Dispose();
        foreach (var s in _locks) s.Dispose();
    }

    private class CacheEntry
    {
        public object? Value { get; }
        public DateTime ExpiryTime { get; }

        public CacheEntry(object? value, DateTime expiryTime)
        {
            Value = value;
            ExpiryTime = expiryTime;
        }

        public bool IsExpired() => DateTime.UtcNow > ExpiryTime;
    }
}
