#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
}

public class CacheService : ICacheService
{
    private readonly Dictionary<string, CacheEntry> _cache;
    private readonly ReaderWriterLockSlim _lock;
    private readonly Timer _cleanupTimer;

    public CacheService()
    {
        _cache = new();
        _lock = new();
        // Cleanup expired entries every 5 minutes
        _cleanupTimer = new Timer(CleanupExpiredEntries, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        return await Task.Run(() =>
        {
            _lock.EnterReadLock();
            try
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    if (entry.IsExpired())
                    {
                        _lock.ExitReadLock();
                        _lock.EnterWriteLock();
                        try
                        {
                            _cache.Remove(key);
                            return default;
                        }
                        finally
                        {
                            _lock.ExitWriteLock();
                        }
                    }
                    return (T?)entry.Value;
                }
                return default;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        });
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        await Task.Run(() =>
        {
            _lock.EnterWriteLock();
            try
            {
                var expiryTime = expiration.HasValue
                    ? DateTime.UtcNow.Add(expiration.Value)
                    : DateTime.UtcNow.AddHours(1); // Default 1 hour

                _cache[key] = new CacheEntry(value, expiryTime);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        });
    }

    public async Task RemoveAsync(string key)
    {
        await Task.Run(() =>
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
        });
    }

    public async Task ClearAsync()
    {
        await Task.Run(() =>
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
        });
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await Task.Run(() =>
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
        });
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

            if (expiredKeys.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Cleaned up {expiredKeys.Count} expired cache entries");
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
