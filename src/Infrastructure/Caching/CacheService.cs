#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SagaOrchestrator.Infrastructure.Caching;

/// <summary>
/// In-memory caching service with TTL support and expiration policies.
/// Provides thread-safe cache operations for saga and definition caching.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task ClearAsync();
    Task<bool> ExistsAsync(string key);
    int GetCacheSize();

    /// <summary>
    /// Gets or creates a value in the cache with stampede protection.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(CacheKey key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Gets the cache hit and miss counters.
    /// </summary>
    (long Hits, long Misses) GetMetrics();
}

public class CacheService : ICacheService, IDisposable
{
    private readonly Dictionary<string, CacheEntry> _cache;
    private readonly ReaderWriterLockSlim _lock;
    private readonly Timer _cleanupTimer;
    private readonly SemaphoreSlim[] _locks;
    private readonly Random _random;
    private long _hits;
    private long _misses;

    public CacheService()
    {
        _cache = new();
        _lock = new();
        _locks = Enumerable.Range(0, 32).Select(_ => new SemaphoreSlim(1, 1)).ToArray();
        _random = new Random();
        // Cleanup expired entries every 5 minutes
        _cleanupTimer = new Timer(CleanupExpiredEntries, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    private SemaphoreSlim GetLock(string key) => _locks[Math.Abs(key.GetHashCode()) % _locks.Length];

    public async Task<T?> GetAsync<T>(string key)
    {
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

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        _lock.EnterWriteLock();
        try
        {
            var expiryTime = CalculateExpiry(expiration);
            _cache[key] = new CacheEntry(value, expiryTime);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public async Task RemoveAsync(string key)
    {
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

    public async Task ClearAsync()
    {
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

    public async Task<bool> ExistsAsync(string key)
    {
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

    public async Task<T> GetOrCreateAsync<T>(CacheKey key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(factory);

        string stringKey = key.ToString();

        // 1. Try get
        T? cached = await GetAsync<T>(stringKey);
        if (cached != null) return cached;

        // 2. Lock and re-check (Double-checked locking pattern)
        var semaphore = GetLock(stringKey);
        await semaphore.WaitAsync();
        try
        {
            cached = await GetAsync<T>(stringKey);
            if (cached != null) return cached;

            // 3. Factory call
            T value = await factory();

            // 4. Set
            await SetAsync(stringKey, value, expiration);
            return value;
        }
        finally
        {
            semaphore.Release();
        }
    }

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
