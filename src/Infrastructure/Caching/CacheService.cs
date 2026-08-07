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
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, int? maxKeyLength = null, int? maxValueSize = null, int? maxCacheSize = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    int GetCacheSize();

    /// <summary>
    /// Gets or creates a value in the cache with stampede protection.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(CacheKey key, Func<Task<T>> factory, TimeSpan? expiration = null, int? maxKeyLength = null, int? maxValueSize = null, int? maxCacheSize = null, CancellationToken cancellationToken = default);

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
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        cancellationToken.ThrowIfCancellationRequested();

        _lock.EnterReadLock();
        try
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.IsExpired())
                {
                    Interlocked.Increment(ref _misses);
                    return default;
                }
                Interlocked.Increment(ref _hits);
                return (T?)entry.Value;
            }
            Interlocked.Increment(ref _misses);
            return default;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, int? maxKeyLength = null, int? maxValueSize = null, int? maxCacheSize = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
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
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
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
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken cancellationToken = default)
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
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        cancellationToken.ThrowIfCancellationRequested();

        _lock.EnterReadLock();
        try
        {
            return _cache.ContainsKey(key) && !_cache[key].IsExpired();
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
    public async Task<T> GetOrCreateAsync<T>(CacheKey key, Func<Task<T>> factory, TimeSpan? expiration = null, int? maxKeyLength = null, int? maxValueSize = null, int? maxCacheSize = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(factory);
        cancellationToken.ThrowIfCancellationRequested();

        string stringKey = key.ToString();

        // 1. Try get
        T? cached = await GetAsync<T>(stringKey, cancellationToken);
        if (cached != null) return cached;

        // 2. Lock and re-check (Double-checked locking pattern)
        var semaphore = GetLock(stringKey);
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            cached = await GetAsync<T>(stringKey, cancellationToken);
            if (cached != null) return cached;

            // 3. Factory call
            T value = await factory();

            // 4. Set
            await SetAsync(stringKey, value, expiration, maxKeyLength, maxValueSize, maxCacheSize, cancellationToken);
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